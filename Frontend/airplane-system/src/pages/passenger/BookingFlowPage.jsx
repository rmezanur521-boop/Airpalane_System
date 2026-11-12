import { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { Check, Plane, CreditCard, Tag } from 'lucide-react';
import PageWrapper     from '@/components/layout/PageWrapper';
import PassengerForm   from '@/components/booking/PassengerForm';
import FlightCard      from '@/components/flight/FlightCard';
import Button          from '@/components/ui/Button';
import Alert           from '@/components/ui/Alert';
import Input           from '@/components/ui/Input';
import bookingService  from '@/api/bookingService';
import paymentService  from '@/api/paymentService';
import userService     from '@/api/userService';
import { TRIP_TYPE, PASSENGER_TYPE, PAYMENT_METHOD, PAYMENT_METHOD_OPTIONS } from '@/utils/constants';
import { formatCurrency } from '@/utils/formatters';
import toast from 'react-hot-toast';
import Spinner from '@/components/ui/Spinner';
import StripeCardForm from '@/components/booking/StripeCardForm';
const STEPS = ['Review Flight', 'Passenger Details', 'Payment'];

const emptyPassenger = () => ({
  firstName: '', lastName: '',
  passengerType: PASSENGER_TYPE.ADULT,
  dateOfBirth: '', passportNumber: '',
  passportExpiry: '', passportCountry: '',
  mealPreference: '',
});

export default function BookingFlowPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const { flight, returnFlight, seatClass, passengers: paxCount } =
    location.state ?? {};

  const [step,        setStep]        = useState(0);
  const [passengers,  setPassengers]  = useState(() => {
    const total = (paxCount?.adults ?? 1) + (paxCount?.children ?? 0) + (paxCount?.infants ?? 0);
    return Array.from({ length: Math.max(total, 1) }, emptyPassenger);
  });
  const [savedUserPassenger, setSavedUserPassenger] = useState(null);
  const [isPrimaryTraveler,  setIsPrimaryTraveler]  = useState(true);

  const [promoCode,    setPromoCode]   = useState(location.state?.promoCode ?? '');
  const [promoResult,  setPromoResult] = useState(null);
  const [promoLoading, setPromoLoading] = useState(false);
  const [booking,      setBooking]     = useState(null);
  const [loading,      setLoading]     = useState(false);
  const [error,        setError]       = useState('');

  const [paymentMethod,   setPaymentMethod]   = useState(PAYMENT_METHOD.STRIPE);
  const [referenceNumber, setReferenceNumber] = useState('');
  const [paymentConfig,   setPaymentConfig]   = useState(null);
  const [clientSecret,    setClientSecret]   = useState('');
  const [intentLoading,   setIntentLoading]  = useState(false);

  // Flight guard
  useEffect(() => {
    if (!flight) navigate('/', { replace: true });
  }, [flight, navigate]);

  // Payment config
  useEffect(() => {
    paymentService.getPublicConfig()
      .then(({ data }) => setPaymentConfig(data))
      .catch(() => toast.error('Payment configuration could not be loaded.'));
  }, []);

  // Calculate estimated total cart price for promo validation and summary
  const calculateEstimatedTotal = () => {
    const getPrice = (f) => {
      if (!f) return 0;
      if (seatClass === 'Business') return f.businessBasePrice ?? f.totalPrice ?? 0;
      if (seatClass === 'First') return f.firstClassBasePrice ?? f.totalPrice ?? 0;
      return f.economyBasePrice ?? f.totalPrice ?? 0;
    };
    const out = Number(getPrice(flight));
    const ret = returnFlight ? Number(getPrice(returnFlight)) : 0;
    const count = Math.max(passengers.length, 1);
    return (out + ret) * count;
  };

  // Fetch logged-in user profile & passport for Passenger 1 auto-fill
  useEffect(() => {
    let isMounted = true;
    Promise.all([
      userService.getProfile().catch(() => ({ data: null })),
      userService.getPassport().catch(() => ({ data: null })),
    ]).then(([profileRes, passportRes]) => {
      if (!isMounted) return;
      const prof = profileRes?.data;
      const pass = passportRes?.data;

      if (prof) {
        const autoPassenger = {
          firstName: prof.firstName ?? '',
          lastName: prof.lastName ?? '',
          passengerType: PASSENGER_TYPE.ADULT,
          dateOfBirth: prof.dateOfBirth ? String(prof.dateOfBirth).slice(0, 10) : '',
          passportNumber: pass?.passportNumber ?? '',
          passportExpiry: pass?.expiryDate ? String(pass.expiryDate).slice(0, 10) : '',
          passportCountry: pass?.issuingCountry ?? '',
          mealPreference: '',
        };

        setSavedUserPassenger(autoPassenger);

        // Auto-fill Passenger 1 if currently empty
        setPassengers((prev) => {
          if (!prev || prev.length === 0) return [autoPassenger];
          const first = prev[0];
          const isEmpty = !first.firstName && !first.lastName && !first.passportNumber;
          if (isEmpty) {
            return [autoPassenger, ...prev.slice(1)];
          }
          return prev;
        });
      }
    });

    return () => { isMounted = false; };
  }, []);

  // Toggle Primary Traveler (I am Passenger 1)
  const handleTogglePrimary = (checked) => {
    setIsPrimaryTraveler(checked);
    if (checked && savedUserPassenger) {
      updatePassenger(0, { ...savedUserPassenger });
    } else {
      updatePassenger(0, emptyPassenger());
    }
  };

  // Auto-validate promo code when landing if provided via navigation state
  useEffect(() => {
    const code = (promoCode || '').trim().toUpperCase();
    if (!code || promoResult) return;
    const estTotal = calculateEstimatedTotal();
    paymentService.validatePromo(code, estTotal)
      .then(({ data }) => {
        setPromoResult(data);
        if (data.isValid) {
          toast.success(`Promo code ${data.code} applied! Save ${formatCurrency(data.discountAmount)}.`);
        }
      })
      .catch(() => {});
  }, [promoCode]);

  useEffect(() => {
    if (step !== 2 || !booking) return;
    if (paymentMethod !== PAYMENT_METHOD.STRIPE) return;
    if (clientSecret) return;

    setIntentLoading(true);
    paymentService.createPaymentIntent(booking.id)
      .then(({ data }) => setClientSecret(data.clientSecret))
      .catch((err) => setError(err.response?.data?.detail ?? 'Payment intent could not be created.'))
      .finally(() => setIntentLoading(false));
  }, [step, booking, paymentMethod, clientSecret]);

  if (!flight) return null;

  const updatePassenger = (i, data) =>
    setPassengers((p) => p.map((x, idx) => (idx === i ? data : x)));

  // ── Step 1 → 2: Create booking ────────────────────────────────────────────
  const handleCreateBooking = async () => {
    setError('');

    // Pre-flight client validations to prevent server rejections
    for (let i = 0; i < passengers.length; i++) {
      const p = passengers[i];
      if (!p.firstName?.trim() || !p.lastName?.trim()) {
        setError(`Please enter both first name and last name for Passenger ${i + 1}.`);
        return;
      }
      if (!p.dateOfBirth) {
        setError(`Please select date of birth for Passenger ${i + 1}.`);
        return;
      }
      if (p.passengerType !== PASSENGER_TYPE.INFANT) {
        if (!p.passportNumber?.trim()) {
          setError(`Passport number is required for Passenger ${i + 1}.`);
          return;
        }
        if (!p.passportExpiry) {
          setError(`Passport expiry date is required for Passenger ${i + 1}.`);
          return;
        }
        const expDate = new Date(p.passportExpiry);
        const minAllowed = new Date(Date.now() + 180 * 86400000);
        if (expDate <= minAllowed) {
          setError(`Passenger ${i + 1}'s passport must be valid for at least 6 months from today.`);
          return;
        }
      }
    }

    setLoading(true);
    try {
      const segments = returnFlight
        ? [
            { flightId: flight.id,       seatClass },
            { flightId: returnFlight.id, seatClass },
          ]
        : [{ flightId: flight.id, seatClass }];

      const tripType = returnFlight ? TRIP_TYPE.ROUND_TRIP : TRIP_TYPE.ONE_WAY;
      const activePromo = (promoResult?.isValid && promoCode.trim()) ? promoCode.trim().toUpperCase() : undefined;

      const { data } = await bookingService.createBooking({
        tripType,
        segments,
        passengers,
        promoCode: activePromo,
      });

      setBooking(data);
      setStep(2);
    } catch (err) {
      setError(err.response?.data?.detail ?? 'Failed to create booking.');
    } finally {
      setLoading(false);
    }
  };

  // ── Step 2 → 3: Pay via Stripe OR submit reference payment ────────────────
  const handlePayment = async () => {
    setError('');

    if (paymentMethod !== PAYMENT_METHOD.STRIPE && !referenceNumber.trim()) {
      setError('Please enter your transaction / reference number.');
      return;
    }

    setLoading(true);
    try {
      if (paymentMethod === PAYMENT_METHOD.STRIPE) {
        const { data: intent } = await paymentService.createPaymentIntent(booking.id);
        await paymentService.confirmPayment(intent.paymentIntentId);
        toast.success('Payment confirmed! 🎉');
      } else {
        await paymentService.createReferencePayment({
          bookingId: booking.id,
          referenceNumber: referenceNumber.trim(),
          amount: booking.totalAmount,
          currencyCode: 'USD',
          method: paymentMethod,
        });
        toast.success('Payment submitted! Awaiting admin approval.');
      }
      navigate(`/bookings/${booking.id}`, { replace: true });
    } catch (err) {
      setError(err.response?.data?.detail ?? 'Payment failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleValidatePromo = async () => {
    const code = promoCode.trim().toUpperCase();
    if (!code) {
      setPromoResult(null);
      return;
    }
    setPromoLoading(true);
    try {
      const estTotal = calculateEstimatedTotal();
      const { data } = await paymentService.validatePromo(code, estTotal);
      setPromoResult(data);
      if (data.isValid) {
        toast.success(`Promo code ${data.code} applied! You save ${formatCurrency(data.discountAmount)}.`);
      } else {
        toast.error(data.message ?? 'Invalid promo code');
      }
    } catch {
      toast.error('Could not validate promo code.');
    } finally {
      setPromoLoading(false);
    }
  };

  const handleReferencePayment = async () => {
  setError('');
  if (!referenceNumber.trim()) {
    setError('Please enter your transaction / reference number.');
    return;
  }
  setLoading(true);
  try {
    await paymentService.createReferencePayment({
      bookingId: booking.id,
      referenceNumber: referenceNumber.trim(),
      amount: booking.totalAmount,
      currencyCode: 'USD',
      method: paymentMethod,
    });
    toast.success('Payment submitted! Awaiting admin approval.');
    navigate(`/bookings/${booking.id}`, { replace: true });
  } catch (err) {
    setError(err.response?.data?.detail ?? 'Payment failed. Please try again.');
  } finally {
    setLoading(false);
  }
};

const handleStripeSuccess = async (paymentIntent) => {
  try {
    await paymentService.confirmPayment(paymentIntent.id);
    toast.success('Payment confirmed! 🎉');
    navigate(`/bookings/${booking.id}`, { replace: true });
  } catch (err) {
    // webhook ব্যাকআপ হিসেবে কনফার্ম করবে, তবু ইউজারকে জানাও
    toast.error('Payment succeeded, but confirmation sync-এ সমস্যা হয়েছে। Booking status-টা refresh করে দেখো।');
    navigate(`/bookings/${booking.id}`, { replace: true });
  }
};
const availableMethods = PAYMENT_METHOD_OPTIONS.filter(({ value }) => {
  if (!paymentConfig) return true;
  if (value === PAYMENT_METHOD.STRIPE) return paymentConfig.stripeEnabled;
  if (value === PAYMENT_METHOD.MOBILE_BANKING) return paymentConfig.bkashEnabled || paymentConfig.nagadEnabled;
  if (value === PAYMENT_METHOD.BANK_TRANSFER) return true; // ব্যাকএন্ডে এর কোনো "enable/disable" গেটওয়ে নেই, তাই সবসময় দেখাও
  return true;
});
  const totalPassengers = passengers.length;

  return (
    <PageWrapper>
      <div className="page-container py-10 max-w-4xl">
        {/* Stepper */}
        <div className="flex items-center gap-2 mb-10">
          {STEPS.map((s, i) => (
            <div key={s} className="flex items-center gap-2 flex-1">
              <div className={`h-8 w-8 rounded-full flex items-center justify-center text-sm font-bold
                flex-shrink-0 transition
                ${i < step
                  ? 'bg-brand-600 text-white'
                  : i === step
                  ? 'bg-brand-600 text-white ring-4 ring-brand-100'
                  : 'bg-slate-100 text-slate-400'}`}
              >
                {i < step ? <Check className="h-4 w-4" /> : i + 1}
              </div>
              <span className={`text-sm font-medium hidden sm:block
                ${i <= step ? 'text-slate-800' : 'text-slate-400'}`}>
                {s}
              </span>
              {i < STEPS.length - 1 && (
                <div className={`flex-1 h-px mx-2 ${i < step ? 'bg-brand-400' : 'bg-slate-200'}`} />
              )}
            </div>
          ))}
        </div>

        {/* ── Step 0: Review flight ───────────────────────────────────────── */}
        {step === 0 && (
          <div className="animate-fadeIn">
            <h2 className="section-title mb-4">Review Your Flight</h2>
            <FlightCard flight={flight} selectedClass={seatClass} />
            {returnFlight && (
              <>
                <h3 className="font-semibold text-slate-600 mt-6 mb-3">Return Flight</h3>
                <FlightCard flight={returnFlight} selectedClass={seatClass} />
              </>
            )}
            <div className="mt-6 flex justify-end">
              <Button onClick={() => setStep(1)}>
                Continue to Passengers
              </Button>
            </div>
          </div>
        )}

        {/* ── Step 1: Passengers ─────────────────────────────────────────── */}
        {step === 1 && (
          <div className="animate-fadeIn">
            <h2 className="section-title mb-6">Passenger Details</h2>
            {error && <Alert type="error" message={error} className="mb-4" />}

            {passengers.map((p, i) => (
              <PassengerForm
                key={i}
                index={i}
                passenger={p}
                onChange={updatePassenger}
                isPrimary={isPrimaryTraveler}
                onTogglePrimary={handleTogglePrimary}
                userProfileAvailable={!!savedUserPassenger}
                userFullName={savedUserPassenger ? `${savedUserPassenger.firstName} ${savedUserPassenger.lastName}`.trim() : ''}
              />
            ))}

            {/* Promo code */}
            <div className="card mb-6 border border-slate-100 shadow-sm">
              <div className="flex items-center gap-2 mb-3">
                <Tag className="h-4 w-4 text-brand-600" />
                <h3 className="font-semibold text-slate-700">Promo Code</h3>
              </div>
              <div className="flex gap-3">
                <Input
                  placeholder="Enter promo code (e.g. SUMMER25)"
                  value={promoCode}
                  onChange={(e) => {
                    setPromoCode(e.target.value.toUpperCase());
                    if (promoResult) setPromoResult(null);
                  }}
                  className="flex-1 font-mono tracking-wider uppercase"
                />
                <Button
                  variant="secondary"
                  onClick={handleValidatePromo}
                  loading={promoLoading}
                  disabled={!promoCode.trim()}
                >
                  Apply
                </Button>
              </div>

              {promoResult?.isValid && (
                <div className="flex items-center justify-between mt-3 p-3 rounded-xl bg-emerald-50 border border-emerald-200 text-emerald-800 text-xs font-semibold">
                  <span className="flex items-center gap-1.5">
                    🎉 Promo Code <strong>{promoResult.code}</strong> Applied! You save {formatCurrency(promoResult.discountAmount)}.
                  </span>
                  <button
                    type="button"
                    onClick={() => { setPromoCode(''); setPromoResult(null); }}
                    className="text-slate-400 hover:text-slate-600 transition underline font-normal"
                  >
                    Remove
                  </button>
                </div>
              )}

              {promoResult && !promoResult.isValid && (
                <p className="text-xs text-red-500 mt-2 font-medium">
                  {promoResult.message || 'Invalid or expired promo code.'}
                </p>
              )}
            </div>

            {/* Estimated Price Summary */}
            <div className="card mb-6 bg-slate-50 border border-slate-200/80 p-5 rounded-2xl">
              <h4 className="font-bold text-slate-800 text-sm mb-3">Fare & Order Summary</h4>
              <div className="space-y-2 text-sm text-slate-600">
                <div className="flex items-center justify-between">
                  <span>Passengers:</span>
                  <span className="font-semibold text-slate-800">{passengers.length} ({seatClass})</span>
                </div>
                <div className="flex items-center justify-between">
                  <span>Base Airfare Total:</span>
                  <span className="font-semibold text-slate-800">{formatCurrency(calculateEstimatedTotal())}</span>
                </div>
                {promoResult?.isValid && (
                  <div className="flex items-center justify-between text-emerald-700 font-semibold">
                    <span>Promo Discount ({promoResult.code}):</span>
                    <span>-{formatCurrency(promoResult.discountAmount)}</span>
                  </div>
                )}
                <div className="flex items-center justify-between text-base font-bold text-slate-900 border-t border-slate-200 pt-2.5 mt-2">
                  <span>Estimated Total:</span>
                  <span className="text-brand-700 text-xl font-extrabold">
                    {formatCurrency(calculateEstimatedTotal() - (promoResult?.isValid ? promoResult.discountAmount : 0))}
                  </span>
                </div>
              </div>
            </div>

            <div className="flex items-center justify-between">
              <Button variant="secondary" onClick={() => setStep(0)}>
                Back
              </Button>
              <Button onClick={handleCreateBooking} loading={loading} size="lg">
                Confirm Passengers & Continue
              </Button>
            </div>
          </div>
        )}

        {/* ── Step 2: Payment ────────────────────────────────────────────── */}
        {step === 2 && booking && (
          <div className="animate-fadeIn">
            <h2 className="section-title mb-6">Payment</h2>
            {error && <Alert type="error" message={error} className="mb-4" />}

            {/* Booking summary */}
            <div className="card mb-6">
              <h3 className="font-semibold text-slate-700 mb-4">Booking Summary</h3>
              <div className="flex flex-col gap-2 text-sm">
                <Row label="Booking Ref"   value={booking.bookingReference} />
                <Row label="Passengers"    value={totalPassengers} />
                <Row label="Seat Class"    value={seatClass} />
                {booking.discountAmount > 0 && (
                  <Row
                    label="Discount"
                    value={`-${formatCurrency(booking.discountAmount)}`}
                    valueClass="text-green-600"
                  />
                )}
                <div className="border-t border-slate-100 pt-2 mt-1">
                  <Row
                    label="Total"
                    value={formatCurrency(booking.totalAmount)}
                    labelClass="font-bold text-slate-800"
                    valueClass="font-bold text-slate-800 text-lg"
                  />
                </div>
              </div>
            </div>

            {/* Payment method selector */}
            <div className="card mb-6">
              <h3 className="font-semibold text-slate-700 mb-4">Choose Payment Method</h3>
              <div className="grid grid-cols-3 gap-3">
                {availableMethods.map(({ value, label }) => (
                  <button
                    key={value}
                    type="button"
                    onClick={() => setPaymentMethod(value)}
                    className={`p-3 rounded-xl border text-sm font-medium transition
                      ${paymentMethod === value
                        ? 'border-brand-600 bg-brand-50 text-brand-700'
                        : 'border-slate-200 text-slate-500 hover:border-slate-300'}`}
                  >
                    {label}
                  </button>
                ))}
              </div>
            </div>

            {/* Method-specific form — only ONE of these renders at a time */}
            {paymentMethod === PAYMENT_METHOD.STRIPE ? (
              <div className="card mb-6">
                <div className="flex items-center gap-2 mb-4">
                  <CreditCard className="h-5 w-5 text-brand-600" />
                  <h3 className="font-semibold text-slate-700">Card Details</h3>
                </div>
                {intentLoading || !clientSecret ? (
                  <div className="flex justify-center py-8"><Spinner /></div>
                ) : (
                  <StripeCardForm
                    publicKey={paymentConfig?.stripePublicKey}
                    clientSecret={clientSecret}
                    amount={booking.totalAmount}
                    onSuccess={handleStripeSuccess}
                    onError={(err) => setError(err.message ?? 'Payment failed.')}
                  />
                )}
              </div>
            ) : (
              <div className="card mb-6">
                <Input
                  label="Transaction / Reference Number"
                  value={referenceNumber}
                  onChange={(e) => setReferenceNumber(e.target.value)}
                  placeholder="e.g. bKash TrxID"
                />
                <Alert type="info" className="mt-3"
                  message="Your payment will be verified by our team before confirmation." />
              </div>
            )}

            <div className="flex items-center justify-between">
              <Button variant="secondary" onClick={() => setStep(1)}>Back</Button>
              {paymentMethod !== PAYMENT_METHOD.STRIPE && (
                <Button
                  onClick={handleReferencePayment}
                  loading={loading}
                  disabled={!referenceNumber.trim()}
                >
                  <CreditCard className="h-4 w-4" />
                  Submit for Review
                </Button>
              )}
            </div>
          </div>
        )}
      </div>
    </PageWrapper>
  );
}

function Row({ label, value, labelClass = 'text-slate-500', valueClass = 'text-slate-800 font-medium' }) {
  return (
    <div className="flex items-center justify-between">
      <span className={labelClass}>{label}</span>
      <span className={valueClass}>{value}</span>
    </div>
  );
}
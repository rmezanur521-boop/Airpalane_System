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
  const [promoCode,   setPromoCode]   = useState('');
  const [promoResult, setPromoResult] = useState(null);
  const [promoLoading, setPromoLoading] = useState(false);
  const [booking,     setBooking]     = useState(null);
  const [loading,     setLoading]     = useState(false);
  const [error,       setError]       = useState('');

  const [paymentMethod,   setPaymentMethod]   = useState(PAYMENT_METHOD.STRIPE);
  const [referenceNumber, setReferenceNumber] = useState('');
  const [paymentConfig, setPaymentConfig] = useState(null);
const [clientSecret, setClientSecret]   = useState('');
const [intentLoading, setIntentLoading] = useState(false);
  useEffect(() => {
    if (!flight) navigate('/', { replace: true });
  }, [flight, navigate]);
  useEffect(() => {
  paymentService.getPublicConfig()
    .then(({ data }) => setPaymentConfig(data))
    .catch(() => toast.error('Payment configuration লোড করা যায়নি।'));
}, []);

useEffect(() => {
  if (step !== 2 || !booking) return;
  if (paymentMethod !== PAYMENT_METHOD.STRIPE) return;
  if (clientSecret) return; // ইতিমধ্যে তৈরি থাকলে দ্বিতীয়বার কল করো না

  setIntentLoading(true);
  paymentService.createPaymentIntent(booking.id)
    .then(({ data }) => setClientSecret(data.clientSecret))
    .catch((err) => setError(err.response?.data?.detail ?? 'Payment intent তৈরি করা যায়নি।'))
    .finally(() => setIntentLoading(false));
}, [step, booking, paymentMethod, clientSecret]);
  if (!flight) return null;

  const updatePassenger = (i, data) =>
    setPassengers((p) => p.map((x, idx) => (idx === i ? data : x)));

  // ── Step 1 → 2: Create booking ────────────────────────────────────────────
  const handleCreateBooking = async () => {
    setError('');
    setLoading(true);
    try {
      const segments = returnFlight
        ? [
            { flightId: flight.id,       seatClass },
            { flightId: returnFlight.id, seatClass },
          ]
        : [{ flightId: flight.id, seatClass }];

      const tripType = returnFlight ? TRIP_TYPE.ROUND_TRIP : TRIP_TYPE.ONE_WAY;

      const { data } = await bookingService.createBooking({
        tripType,
        segments,
        passengers,
        promoCode: promoCode || undefined,
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
    if (!promoCode) return;
    setPromoLoading(true);
    try {
      const { data } = await paymentService.validatePromo(
        promoCode,
        flight.totalPrice ?? flight.economyBasePrice
      );
      setPromoResult(data);
      if (data.isValid) toast.success('Promo code applied!');
      else toast.error(data.message ?? 'Invalid promo code');
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
              />
            ))}

            {/* Promo code */}
            <div className="card mb-6">
              <div className="flex items-center gap-2 mb-3">
                <Tag className="h-4 w-4 text-brand-600" />
                <h3 className="font-semibold text-slate-700">Promo Code</h3>
              </div>
              <div className="flex gap-3">
                <Input
                  placeholder="Enter promo code"
                  value={promoCode}
                  onChange={(e) => setPromoCode(e.target.value.toUpperCase())}
                  className="flex-1"
                />
                <Button
                  variant="secondary"
                  onClick={handleValidatePromo}
                  loading={promoLoading}
                >
                  Apply
                </Button>
              </div>
              {promoResult?.isValid && (
                <p className="text-sm text-green-600 mt-2 font-medium">
                  Discount: {formatCurrency(promoResult.discountAmount)} applied!
                </p>
              )}
            </div>

            <div className="flex items-center justify-between">
              <Button variant="secondary" onClick={() => setStep(0)}>
                Back
              </Button>
              <Button onClick={handleCreateBooking} loading={loading}>
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
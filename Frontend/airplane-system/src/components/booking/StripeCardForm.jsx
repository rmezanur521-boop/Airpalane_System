import { useState } from 'react';
import { loadStripe } from '@stripe/stripe-js';
import {
  Elements, PaymentElement, useStripe, useElements,
} from '@stripe/react-stripe-js';
import { CreditCard } from 'lucide-react';
import Button from '@/components/ui/Button';
import Alert from '@/components/ui/Alert';
import { formatCurrency } from '@/utils/formatters';

// stripePromise একবারই তৈরি হবে (module scope), rerender-এ বারবার না
let stripePromiseCache = null;
function getStripePromise(publicKey) {
  if (!stripePromiseCache) {
    stripePromiseCache = loadStripe(publicKey);
  }
  return stripePromiseCache;
}

function InnerCardForm({ amount, onSuccess, onError }) {
  const stripe   = useStripe();
  const elements = useElements();
  const [submitting, setSubmitting] = useState(false);
  const [localError, setLocalError] = useState('');

  const handleConfirm = async () => {
    if (!stripe || !elements) return;

    setSubmitting(true);
    setLocalError('');

    try {
      const { error, paymentIntent } = await stripe.confirmPayment({
        elements,
        redirect: 'if_required', // card flow-এ সাধারণত redirect লাগে না
      });

      if (error) {
        setLocalError(error.message ?? 'পেমেন্ট confirm করা যায়নি।');
        onError?.(error);
        return;
      }

      if (paymentIntent?.status === 'succeeded') {
        onSuccess?.(paymentIntent);
      } else {
        setLocalError(`পেমেন্ট স্ট্যাটাস: ${paymentIntent?.status ?? 'unknown'}`);
      }
    } catch (err) {
      setLocalError('একটা সমস্যা হয়েছে, আবার চেষ্টা করো।');
      onError?.(err);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="flex flex-col gap-4">
      {localError && <Alert type="error" message={localError} />}
      <PaymentElement />
      <Button onClick={handleConfirm} loading={submitting} disabled={!stripe}>
        <CreditCard className="h-4 w-4" />
        Pay {formatCurrency(amount)}
      </Button>
    </div>
  );
}

export default function StripeCardForm({ publicKey, clientSecret, amount, onSuccess, onError }) {
  if (!publicKey || !clientSecret) return null;

  const stripePromise = getStripePromise(publicKey);

  return (
    <Elements
      stripe={stripePromise}
      options={{
        clientSecret,
        appearance: { theme: 'stripe' },
      }}
    >
      <InnerCardForm amount={amount} onSuccess={onSuccess} onError={onError} />
    </Elements>
  );
}
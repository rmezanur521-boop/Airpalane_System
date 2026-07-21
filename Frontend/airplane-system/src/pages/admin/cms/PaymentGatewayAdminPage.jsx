import { useEffect, useState } from 'react';
import Spinner from '@/components/ui/Spinner';
import GatewayCard from '@/components/cms/GatewayCard';
import paymentGatewayService from '@/api/cms/paymentGatewayService';
import toast from 'react-hot-toast';

export default function PaymentGatewayAdminPage() {
  const [settings, setSettings] = useState([]);
  const [loading, setLoading]   = useState(true);

  const load = () => {
    setLoading(true);
    paymentGatewayService.list()
      .then(({ data }) => setSettings(data ?? []))
      .catch(() => toast.error('Failed to load payment gateway settings.'))
      .finally(() => setLoading(false));
  };

  useEffect(load, []);

  const handleSaved = (updated) => {
    setSettings((prev) =>
      prev.map((s) => (s.provider === updated.provider ? updated : s))
    );
  };

  return (
    <div className="animate-fadeIn">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-slate-800">Payment Gateways</h1>
        <p className="text-slate-500 text-sm mt-1">
          Stripe, bKash ও Nagad-এর API কী ও সেটিংস এখান থেকে ম্যানেজ করো।
        </p>
      </div>

      {loading ? (
        <div className="flex justify-center py-16"><Spinner size="lg" /></div>
      ) : (
        <div className="flex flex-col gap-4 max-w-2xl">
          {settings.map((s) => (
            <GatewayCard key={s.provider} setting={s} onSaved={handleSaved} />
          ))}
        </div>
      )}
    </div>
  );
}
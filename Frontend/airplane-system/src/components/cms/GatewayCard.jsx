import { useState, useEffect } from 'react';
import { ChevronDown, Eye, EyeOff, CheckCircle2, XCircle } from 'lucide-react';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import Alert from '@/components/ui/Alert';
import toast from 'react-hot-toast';
import paymentGatewayService from '@/api/cms/paymentGatewayService';

// প্রতিটা provider-এর জন্য কোন কোন ফিল্ড দেখাতে হবে সেটা এখানে ডিফাইন করা
const FIELD_CONFIG = {
  Stripe: [
    { key: 'publicKey', label: 'Publishable Key', type: 'text', secret: false, placeholder: 'pk_test_...' },
    { key: 'secretKey', label: 'Secret Key', type: 'password', secret: true, placeholder: 'sk_test_...' },
    { key: 'webhookSecret', label: 'Webhook Secret', type: 'password', secret: true, placeholder: 'whsec_...' },
  ],
  Bkash: [
    { key: 'publicKey', label: 'App Key', type: 'text', secret: false, placeholder: 'App Key' },
    { key: 'secretKey', label: 'App Secret', type: 'password', secret: true, placeholder: 'App Secret' },
  ],
  Nagad: [
    { key: 'publicKey', label: 'Merchant ID', type: 'text', secret: false, placeholder: 'Merchant ID' },
    { key: 'secretKey', label: 'Merchant Private Key', type: 'password', secret: true, placeholder: 'Private Key' },
  ],
};

const PROVIDER_LABELS = {
  Stripe: 'Stripe',
  Bkash: 'bKash',
  Nagad: 'Nagad',
};

export default function GatewayCard({ setting, onSaved }) {
  const { provider } = setting;
  const fields = FIELD_CONFIG[provider] ?? [];

  const [isEnabled, setIsEnabled] = useState(setting.isEnabled);
  const [publicKey, setPublicKey] = useState(setting.publicKey ?? '');
  const [secretKey, setSecretKey] = useState('');       // সবসময় খালি শুরু হবে — কখনো actual value ভরা হবে না
  const [webhookSecret, setWebhookSecret] = useState('');
  const [showSecret, setShowSecret] = useState(false);
  const [showWebhook, setShowWebhook] = useState(false);
  const [saving, setSaving] = useState(false);
  const [expanded, setExpanded] = useState(false);

  // parent থেকে fresh data এলে (reload হলে) local state resync করো
  useEffect(() => {
    setIsEnabled(setting.isEnabled);
    setPublicKey(setting.publicKey ?? '');
    setSecretKey('');
    setWebhookSecret('');
  }, [setting]);

  const handleSave = async () => {
    setSaving(true);
    try {
      const payload = {
        isEnabled,
        publicKey: publicKey || null,
        // খালি রাখলে null পাঠাও — ব্যাকএন্ড তখন পুরনো secret অপরিবর্তিত রাখবে
        secretKey: secretKey.trim() || null,
        webhookSecret: webhookSecret.trim() || null,
      };
      const { data } = await paymentGatewayService.update(provider, payload);
      toast.success(`${PROVIDER_LABELS[provider]} settings saved.`);
      setSecretKey('');
      setWebhookSecret('');
      onSaved?.(data);
    } catch (err) {
      toast.error(err.response?.data?.detail ?? 'Failed to save settings.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="card">
      {/* Header */}
      <div className="flex items-center justify-between">
        <button
          type="button"
          onClick={() => setExpanded((p) => !p)}
          className="flex items-center gap-3 flex-1 text-left"
        >
          <div className={`h-9 w-9 rounded-xl flex items-center justify-center flex-shrink-0
            ${isEnabled ? 'bg-green-50' : 'bg-slate-100'}`}>
            {isEnabled
              ? <CheckCircle2 className="h-5 w-5 text-green-600" />
              : <XCircle className="h-5 w-5 text-slate-400" />}
          </div>
          <div>
            <h3 className="font-semibold text-slate-800">{PROVIDER_LABELS[provider] ?? provider}</h3>
            <p className="text-xs text-slate-400">
              {isEnabled ? 'Active' : 'Disabled'}
              {setting.hasSecretKey && ' · Secret key configured'}
            </p>
          </div>
          <ChevronDown className={`h-4 w-4 text-slate-400 ml-auto transition-transform
            ${expanded ? 'rotate-180' : ''}`} />
        </button>

        {/* Quick enable/disable toggle (always visible, no need to expand) */}
        <label className="ml-4 inline-flex items-center cursor-pointer flex-shrink-0">
          <input
            type="checkbox"
            checked={isEnabled}
            onChange={(e) => setIsEnabled(e.target.checked)}
            className="sr-only peer"
          />
          <div className="w-10 h-5 bg-slate-200 rounded-full peer peer-checked:bg-brand-600
            transition relative
            after:content-[''] after:absolute after:top-0.5 after:left-0.5
            after:bg-white after:rounded-full after:h-4 after:w-4 after:transition
            peer-checked:after:translate-x-5" />
        </label>
      </div>

      {/* Expandable form */}
      {expanded && (
        <div className="mt-5 pt-5 border-t border-slate-100 flex flex-col gap-4">
          {fields.map((f) =>
            f.secret ? (
              <div key={f.key} className="relative">
                <Input
                  label={f.label}
                  type={
                    (f.key === 'secretKey' ? showSecret : showWebhook)
                      ? 'text'
                      : 'password'
                  }
                  value={f.key === 'secretKey' ? secretKey : webhookSecret}
                  onChange={(e) =>
                    f.key === 'secretKey'
                      ? setSecretKey(e.target.value)
                      : setWebhookSecret(e.target.value)
                  }
                  placeholder={
                    (f.key === 'secretKey' ? setting.hasSecretKey : setting.hasWebhookSecret)
                      ? '•••••••••••• (রেখে দিতে খালি রাখো)'
                      : f.placeholder
                  }
                  className="pr-10"
                />
                <button
                  type="button"
                  onClick={() =>
                    f.key === 'secretKey'
                      ? setShowSecret((p) => !p)
                      : setShowWebhook((p) => !p)
                  }
                  className="absolute right-3 top-[38px] text-slate-400 hover:text-slate-600"
                >
                  {(f.key === 'secretKey' ? showSecret : showWebhook)
                    ? <EyeOff className="h-4 w-4" />
                    : <Eye className="h-4 w-4" />}
                </button>
              </div>
            ) : (
              <Input
                key={f.key}
                label={f.label}
                type={f.type}
                value={publicKey}
                onChange={(e) => setPublicKey(e.target.value)}
                placeholder={f.placeholder}
              />
            )
          )}

          <Alert
            type="info"
            message="Secret ফিল্ড খালি রাখলে আগের সেভ করা মান অপরিবর্তিত থাকবে।"
          />

          <div className="flex justify-end">
            <Button onClick={handleSave} loading={saving}>
              Save {PROVIDER_LABELS[provider] ?? provider} Settings
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}
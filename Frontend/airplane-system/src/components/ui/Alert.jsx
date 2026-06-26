import { AlertCircle, CheckCircle, Info, XCircle } from 'lucide-react';

const styles = {
  success: { wrap: 'bg-green-50 border-green-200 text-green-800', Icon: CheckCircle },
  error:   { wrap: 'bg-red-50 border-red-200 text-red-800',       Icon: XCircle },
  warning: { wrap: 'bg-yellow-50 border-yellow-200 text-yellow-800', Icon: AlertCircle },
  info:    { wrap: 'bg-blue-50 border-blue-200 text-blue-800',    Icon: Info },
};

export default function Alert({ type = 'info', title, message, className = '' }) {
  const { wrap, Icon } = styles[type] ?? styles.info;
  return (
    <div className={`flex gap-3 rounded-xl border p-4 ${wrap} ${className}`}>
      <Icon className="h-5 w-5 flex-shrink-0 mt-0.5" />
      <div>
        {title   && <p className="font-semibold text-sm">{title}</p>}
        {message && <p className="text-sm mt-0.5">{message}</p>}
      </div>
    </div>
  );
}
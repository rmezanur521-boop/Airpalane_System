const colorMap = {
  green:  'bg-green-100 text-green-700',
  red:    'bg-red-100 text-red-700',
  yellow: 'bg-yellow-100 text-yellow-700',
  blue:   'bg-blue-100 text-blue-700',
  sky:    'bg-sky-100 text-sky-700',
  slate:  'bg-slate-100 text-slate-700',
  purple: 'bg-purple-100 text-purple-700',
  brand:  'bg-brand-100 text-brand-700',
};

export default function Badge({ children, color = 'slate', className = '' }) {
  return (
    <span
      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium
        ${colorMap[color] ?? colorMap.slate} ${className}`}
    >
      {children}
    </span>
  );
}
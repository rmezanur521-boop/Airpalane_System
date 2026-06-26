export default function Select({
  label,
  error,
  options = [],
  className = '',
  containerClassName = '',
  placeholder,
  ...props
}) {
  return (
    <div className={`flex flex-col gap-1 ${containerClassName}`}>
      {label && (
        <label className="text-sm font-medium text-slate-700">{label}</label>
      )}
      <select
        className={`input-base ${error ? 'border-red-400 focus:ring-red-400' : ''} ${className}`}
        {...props}
      >
        {placeholder && (
          <option value="" disabled>
            {placeholder}
          </option>
        )}
        {options.map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
      {error && <p className="text-xs text-red-500">{error}</p>}
    </div>
  );
}
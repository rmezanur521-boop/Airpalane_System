import Spinner from './Spinner';

const variants = {
  primary:   'btn-primary',
  secondary: 'btn-secondary',
  danger:    'btn-danger',
  ghost:     'inline-flex items-center justify-center gap-2 text-slate-600 hover:text-slate-900 hover:bg-slate-100 font-medium text-sm rounded-xl px-4 py-2 transition duration-150',
};

const sizes = {
  sm: 'text-xs px-3 py-1.5',
  md: '',
  lg: 'text-base px-6 py-3',
};

export default function Button({
  children,
  variant  = 'primary',
  size     = 'md',
  loading  = false,
  className = '',
  ...props
}) {
  return (
    <button
      className={`${variants[variant]} ${sizes[size]} ${className}`}
      disabled={loading || props.disabled}
      {...props}
    >
      {loading && <Spinner size="sm" />}
      {children}
    </button>
  );
}
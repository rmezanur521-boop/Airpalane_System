import Input  from '@/components/ui/Input';
import Select from '@/components/ui/Select';
import { PASSENGER_TYPE } from '@/utils/constants';

const TYPE_OPTIONS = [
  { value: PASSENGER_TYPE.ADULT,  label: 'Adult (12+ yrs)' },
  { value: PASSENGER_TYPE.CHILD,  label: 'Child (2–11 yrs)' },
  { value: PASSENGER_TYPE.INFANT, label: 'Infant (under 2 yrs)' },
];

const MEAL_OPTIONS = [
  { value: '',           label: 'No preference' },
  { value: 'Standard',   label: 'Standard Meal' },
  { value: 'Halal',      label: 'Halal Meal' },
  { value: 'Vegetarian', label: 'Vegetarian' },
  { value: 'Vegan',      label: 'Vegan' },
  { value: 'Kosher',     label: 'Kosher' },
  { value: 'GlutenFree', label: 'Gluten-Free' },
];

const COUNTRY_OPTIONS = [
  { value: '',   label: 'Select passport country' },
  { value: 'BD', label: 'Bangladesh (BD)' },
  { value: 'US', label: 'United States (US)' },
  { value: 'GB', label: 'United Kingdom (GB)' },
  { value: 'AE', label: 'United Arab Emirates (AE)' },
  { value: 'SA', label: 'Saudi Arabia (SA)' },
  { value: 'IN', label: 'India (IN)' },
  { value: 'PK', label: 'Pakistan (PK)' },
  { value: 'CA', label: 'Canada (CA)' },
  { value: 'AU', label: 'Australia (AU)' },
  { value: 'SG', label: 'Singapore (SG)' },
  { value: 'MY', label: 'Malaysia (MY)' },
  { value: 'FR', label: 'France (FR)' },
  { value: 'DE', label: 'Germany (DE)' },
  { value: 'IT', label: 'Italy (IT)' },
  { value: 'JP', label: 'Japan (JP)' },
  { value: 'CN', label: 'China (CN)' },
  { value: 'ZA', label: 'South Africa (ZA)' },
  { value: 'NL', label: 'Netherlands (NL)' },
];

export default function PassengerForm({
  index,
  passenger,
  onChange,
  isPrimary,
  onTogglePrimary,
  userProfileAvailable,
  userFullName,
}) {
  const set = (k, v) => onChange(index, { ...passenger, [k]: v });

  // 6 months minimum expiry from today
  const minExpiry = new Date(Date.now() + 185 * 86400000).toISOString().slice(0, 10);
  const maxDob    = new Date().toISOString().slice(0, 10);

  // If passportCountry is not in COUNTRY_OPTIONS, dynamically add it so value is preserved
  const countryOptions = [...COUNTRY_OPTIONS];
  if (passenger.passportCountry && !countryOptions.some((c) => c.value === passenger.passportCountry)) {
    countryOptions.push({
      value: passenger.passportCountry,
      label: `${passenger.passportCountry} (Other)`,
    });
  }

  return (
    <div className="card mb-4 border border-slate-100 shadow-sm hover:shadow-md transition">
      {/* Passenger 1 Profile Auto-fill Banner */}
      {index === 0 && userProfileAvailable && (
        <div className="flex flex-col sm:flex-row sm:items-center justify-between p-3.5 mb-5 rounded-2xl bg-brand-50/80 border border-brand-200 gap-2">
          <label className="flex items-center gap-3 cursor-pointer text-sm font-semibold text-brand-900 select-none">
            <input
              type="checkbox"
              checked={isPrimary}
              onChange={(e) => onTogglePrimary?.(e.target.checked)}
              className="h-4 w-4 rounded border-brand-300 text-brand-600 focus:ring-brand-500 cursor-pointer"
            />
            <span>I am Passenger 1 (Use my saved profile & passport)</span>
          </label>
          {isPrimary && (
            <span className="text-xs text-brand-700 bg-white px-2.5 py-1 rounded-lg border border-brand-200 font-medium inline-flex items-center gap-1 shadow-sm w-fit">
              ✨ Auto-filled: {userFullName || 'Account Holder'}
            </span>
          )}
        </div>
      )}

      <div className="flex items-center justify-between mb-4">
        <h3 className="font-bold text-slate-800 text-base flex items-center gap-2">
          <span className="h-6 w-6 rounded-lg bg-brand-100 text-brand-700 text-xs font-bold flex items-center justify-center">
            {index + 1}
          </span>
          Passenger {index + 1}
          {index === 0 && <span className="text-xs font-medium text-slate-400">(Primary Traveler)</span>}
        </h3>
        <span className="text-xs font-medium text-slate-400 uppercase tracking-wider">
          {passenger.passengerType || 'Adult'}
        </span>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <Input
          label="First name"
          value={passenger.firstName}
          onChange={(e) => set('firstName', e.target.value)}
          placeholder="First / Given Name"
          required
        />
        <Input
          label="Last name"
          value={passenger.lastName}
          onChange={(e) => set('lastName', e.target.value)}
          placeholder="Last / Surname"
          required
        />
        <Select
          label="Passenger type"
          options={TYPE_OPTIONS}
          value={passenger.passengerType}
          onChange={(e) => set('passengerType', e.target.value)}
        />
        <div>
          <Input
            label="Date of birth"
            type="date"
            max={maxDob}
            value={passenger.dateOfBirth}
            onChange={(e) => set('dateOfBirth', e.target.value)}
            required
          />
        </div>

        <Input
          label="Passport number"
          value={passenger.passportNumber}
          onChange={(e) => set('passportNumber', e.target.value.toUpperCase())}
          placeholder="e.g. A12345678"
          required={passenger.passengerType !== PASSENGER_TYPE.INFANT}
        />

        <div>
          <Input
            label="Passport expiry"
            type="date"
            min={minExpiry}
            value={passenger.passportExpiry}
            onChange={(e) => set('passportExpiry', e.target.value)}
            required={passenger.passengerType !== PASSENGER_TYPE.INFANT}
          />
          <p className="text-[11px] text-slate-400 mt-1">Must be valid for at least 6 months</p>
        </div>

        <Select
          label="Passport issuing country"
          options={countryOptions}
          value={passenger.passportCountry}
          onChange={(e) => set('passportCountry', e.target.value)}
        />

        <Select
          label="Meal preference"
          options={MEAL_OPTIONS}
          value={passenger.mealPreference}
          onChange={(e) => set('mealPreference', e.target.value)}
        />
      </div>
    </div>
  );
}
import Input  from '@/components/ui/Input';
import Select from '@/components/ui/Select';
import { PASSENGER_TYPE } from '@/utils/constants';

const TYPE_OPTIONS = [
  { value: PASSENGER_TYPE.ADULT,  label: 'Adult' },
  { value: PASSENGER_TYPE.CHILD,  label: 'Child' },
  { value: PASSENGER_TYPE.INFANT, label: 'Infant' },
];

const MEAL_OPTIONS = [
  { value: '',          label: 'No preference' },
  { value: 'Standard', label: 'Standard' },
  { value: 'Vegetarian', label: 'Vegetarian' },
  { value: 'Vegan',    label: 'Vegan' },
  { value: 'Halal',    label: 'Halal' },
  { value: 'Kosher',   label: 'Kosher' },
  { value: 'GlutenFree', label: 'Gluten-Free' },
];

export default function PassengerForm({ index, passenger, onChange }) {
  const set = (k, v) => onChange(index, { ...passenger, [k]: v });

  return (
    <div className="card mb-4">
      <h3 className="font-semibold text-slate-700 mb-4">
        Passenger {index + 1}
      </h3>
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <Input
          label="First name"
          value={passenger.firstName}
          onChange={(e) => set('firstName', e.target.value)}
          placeholder="John"
          required
        />
        <Input
          label="Last name"
          value={passenger.lastName}
          onChange={(e) => set('lastName', e.target.value)}
          placeholder="Doe"
          required
        />
        <Select
          label="Passenger type"
          options={TYPE_OPTIONS}
          value={passenger.passengerType}
          onChange={(e) => set('passengerType', e.target.value)}
        />
        <Input
          label="Date of birth"
          type="date"
          value={passenger.dateOfBirth}
          onChange={(e) => set('dateOfBirth', e.target.value)}
          required
        />
        <Input
          label="Passport number"
          value={passenger.passportNumber}
          onChange={(e) => set('passportNumber', e.target.value)}
          placeholder="AB1234567"
        />
        <Input
          label="Passport expiry"
          type="date"
          value={passenger.passportExpiry}
          onChange={(e) => set('passportExpiry', e.target.value)}
        />
        <Input
          label="Passport country"
          value={passenger.passportCountry}
          onChange={(e) => set('passportCountry', e.target.value)}
          placeholder="US"
          maxLength={2}
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
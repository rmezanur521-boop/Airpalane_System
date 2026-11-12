import { useState, useRef, useEffect } from 'react';
import { Users, Armchair, ChevronDown, Plus, Minus } from 'lucide-react';

export default function PassengerClassSelector({
  adults = 1,
  children = 0,
  infants = 0,
  seatClass = 'Economy',
  onChange,
}) {
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef(null);

  const totalPassengers = adults + children + infants;

  const update = (field, delta) => {
    let nextAdults = adults;
    let nextChildren = children;
    let nextInfants = infants;

    if (field === 'adults') nextAdults = Math.max(1, Math.min(9, adults + delta));
    if (field === 'children') nextChildren = Math.max(0, Math.min(8, children + delta));
    if (field === 'infants') nextInfants = Math.max(0, Math.min(nextAdults, infants + delta));

    // Cap total at 9
    if (nextAdults + nextChildren + nextInfants > 9) return;
    // Infants cannot exceed adults
    if (nextInfants > nextAdults) nextInfants = nextAdults;

    onChange({
      adults: nextAdults,
      children: nextChildren,
      infants: nextInfants,
      seatClass,
    });
  };

  const handleClassSelect = (cls) => {
    onChange({ adults, children, infants, seatClass: cls });
  };

  // Close when clicking outside
  useEffect(() => {
    const handleClickOutside = (e) => {
      if (containerRef.current && !containerRef.current.contains(e.target)) {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const passengerLabel = `${totalPassengers} Traveler${totalPassengers > 1 ? 's' : ''}`;

  return (
    <div className="relative" ref={containerRef}>
      <label className="text-sm font-medium text-slate-700 flex items-center gap-1.5 mb-1">
        <Users className="h-3.5 w-3.5 text-brand-500" />
        Travelers & Cabin Class
      </label>

      {/* Trigger Button */}
      <button
        type="button"
        onClick={() => setIsOpen((prev) => !prev)}
        className="w-full input-base flex items-center justify-between text-left py-2.5 px-3.5 bg-white hover:border-brand-400 focus:ring-brand-400 transition"
      >
        <div className="flex items-center gap-2 truncate">
          <span className="font-semibold text-slate-800 text-sm">{passengerLabel}</span>
          <span className="text-slate-300">·</span>
          <span className="text-xs font-semibold text-brand-600 bg-brand-50 px-2 py-0.5 rounded-full border border-brand-100">
            {seatClass}
          </span>
        </div>
        <ChevronDown className={`h-4 w-4 text-slate-400 transition-transform ${isOpen ? 'rotate-180 text-brand-600' : ''}`} />
      </button>

      {/* Popover */}
      {isOpen && (
        <div className="absolute right-0 top-full mt-2 z-50 w-80 bg-white rounded-3xl shadow-2xl border border-slate-100 p-5 backdrop-blur-xl animate-in fade-in slide-in-from-top-2">
          {/* Cabin Class Selection */}
          <div className="mb-5">
            <div className="flex items-center gap-1.5 text-xs font-semibold uppercase tracking-wider text-slate-400 mb-2.5">
              <Armchair className="h-3.5 w-3.5" />
              Cabin Class
            </div>
            <div className="grid grid-cols-3 gap-1.5 p-1 bg-slate-100 rounded-xl">
              {['Economy', 'Business', 'First'].map((cls) => (
                <button
                  key={cls}
                  type="button"
                  onClick={() => handleClassSelect(cls)}
                  className={`py-1.5 text-xs font-medium rounded-lg transition ${
                    seatClass === cls
                      ? 'bg-white text-brand-700 shadow-sm font-bold'
                      : 'text-slate-600 hover:text-slate-900'
                  }`}
                >
                  {cls}
                </button>
              ))}
            </div>
          </div>

          {/* Passenger Counters */}
          <div className="space-y-4 mb-5 divide-y divide-slate-100">
            {/* Adults */}
            <div className="flex items-center justify-between pt-1">
              <div>
                <div className="font-semibold text-sm text-slate-800">Adults</div>
                <div className="text-xs text-slate-400">Age 12+ years</div>
              </div>
              <div className="flex items-center gap-3">
                <button
                  type="button"
                  onClick={() => update('adults', -1)}
                  disabled={adults <= 1}
                  className="h-8 w-8 rounded-full border border-slate-200 flex items-center justify-center text-slate-600 hover:bg-slate-100 disabled:opacity-30 disabled:cursor-not-allowed transition"
                >
                  <Minus className="h-3.5 w-3.5" />
                </button>
                <span className="w-5 text-center font-bold text-slate-800 text-sm">{adults}</span>
                <button
                  type="button"
                  onClick={() => update('adults', 1)}
                  disabled={totalPassengers >= 9}
                  className="h-8 w-8 rounded-full border border-slate-200 flex items-center justify-center text-slate-600 hover:bg-slate-100 disabled:opacity-30 disabled:cursor-not-allowed transition"
                >
                  <Plus className="h-3.5 w-3.5" />
                </button>
              </div>
            </div>

            {/* Children */}
            <div className="flex items-center justify-between pt-3">
              <div>
                <div className="font-semibold text-sm text-slate-800">Children</div>
                <div className="text-xs text-slate-400">Age 2–11 years</div>
              </div>
              <div className="flex items-center gap-3">
                <button
                  type="button"
                  onClick={() => update('children', -1)}
                  disabled={children <= 0}
                  className="h-8 w-8 rounded-full border border-slate-200 flex items-center justify-center text-slate-600 hover:bg-slate-100 disabled:opacity-30 disabled:cursor-not-allowed transition"
                >
                  <Minus className="h-3.5 w-3.5" />
                </button>
                <span className="w-5 text-center font-bold text-slate-800 text-sm">{children}</span>
                <button
                  type="button"
                  onClick={() => update('children', 1)}
                  disabled={totalPassengers >= 9}
                  className="h-8 w-8 rounded-full border border-slate-200 flex items-center justify-center text-slate-600 hover:bg-slate-100 disabled:opacity-30 disabled:cursor-not-allowed transition"
                >
                  <Plus className="h-3.5 w-3.5" />
                </button>
              </div>
            </div>

            {/* Infants */}
            <div className="flex items-center justify-between pt-3">
              <div>
                <div className="font-semibold text-sm text-slate-800">Infants</div>
                <div className="text-xs text-slate-400">Under 2 years (on lap)</div>
              </div>
              <div className="flex items-center gap-3">
                <button
                  type="button"
                  onClick={() => update('infants', -1)}
                  disabled={infants <= 0}
                  className="h-8 w-8 rounded-full border border-slate-200 flex items-center justify-center text-slate-600 hover:bg-slate-100 disabled:opacity-30 disabled:cursor-not-allowed transition"
                >
                  <Minus className="h-3.5 w-3.5" />
                </button>
                <span className="w-5 text-center font-bold text-slate-800 text-sm">{infants}</span>
                <button
                  type="button"
                  onClick={() => update('infants', 1)}
                  disabled={infants >= adults || totalPassengers >= 9}
                  className="h-8 w-8 rounded-full border border-slate-200 flex items-center justify-center text-slate-600 hover:bg-slate-100 disabled:opacity-30 disabled:cursor-not-allowed transition"
                >
                  <Plus className="h-3.5 w-3.5" />
                </button>
              </div>
            </div>
          </div>

          {/* Note & Done Button */}
          <div className="flex items-center justify-between pt-2 border-t border-slate-100">
            <span className="text-[11px] text-slate-400">Max 9 travelers</span>
            <button
              type="button"
              onClick={() => setIsOpen(false)}
              className="px-4 py-1.5 bg-brand-600 text-white rounded-xl text-xs font-semibold hover:bg-brand-700 transition"
            >
              Done
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

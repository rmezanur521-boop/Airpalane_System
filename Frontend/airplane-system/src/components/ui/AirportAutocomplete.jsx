import { useState, useEffect, useRef } from 'react';
import { Plane, MapPin, X, Loader2 } from 'lucide-react';
import airportService from '@/api/airportService';

export default function AirportAutocomplete({
  label,
  value = '', // IATA code, e.g. "JFK"
  onChange,   // (iataCode, airport) => void
  placeholder = 'City or Airport',
  required = false,
  error,
  className = '',
  containerClassName = '',
}) {
  const [searchTerm, setSearchTerm] = useState('');
  const [isOpen, setIsOpen] = useState(false);
  const [options, setOptions] = useState([]);
  const [loading, setLoading] = useState(false);
  const [selectedAirport, setSelectedAirport] = useState(null);
  const dropdownRef = useRef(null);
  const inputRef = useRef(null);

  // Sync selected airport display when value prop changes externally (e.g. swap or initial value)
  useEffect(() => {
    if (!value) {
      setSearchTerm('');
      setSelectedAirport(null);
      return;
    }

    if (selectedAirport && selectedAirport.iataCode === value) {
      setSearchTerm(`${selectedAirport.city} (${selectedAirport.iataCode})`);
      return;
    }

    // Lookup airport info for the given IATA code
    let isMounted = true;
    airportService.getAirportByIata(value)
      .then((res) => {
        if (isMounted && res.data) {
          setSelectedAirport(res.data);
          setSearchTerm(`${res.data.city} (${res.data.iataCode})`);
        }
      })
      .catch(() => {
        if (isMounted) setSearchTerm(value);
      });

    return () => { isMounted = false; };
  }, [value]);

  // Fetch airports matching query
  const searchAirports = async (term) => {
    setLoading(true);
    try {
      const res = await airportService.getAirports({ searchTerm: term?.trim() || '', pageSize: 8 });
      const items = res.data?.items ?? [];
      setOptions(items);
    } catch {
      setOptions([]);
    } finally {
      setLoading(false);
    }
  };

  // Debounced search on typing
  useEffect(() => {
    if (!isOpen) return;
    const timer = setTimeout(() => {
      searchAirports(searchTerm);
    }, 200);
    return () => clearTimeout(timer);
  }, [searchTerm, isOpen]);

  // Click outside listener
  useEffect(() => {
    const handleClickOutside = (e) => {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target)) {
        setIsOpen(false);
        // If user typed something without selecting, revert to selected airport
        if (selectedAirport) {
          setSearchTerm(`${selectedAirport.city} (${selectedAirport.iataCode})`);
        } else if (!value) {
          setSearchTerm('');
        }
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [selectedAirport, value]);

  const handleSelect = (airport) => {
    setSelectedAirport(airport);
    setSearchTerm(`${airport.city} (${airport.iataCode})`);
    onChange?.(airport.iataCode, airport);
    setIsOpen(false);
  };

  const handleClear = (e) => {
    e.stopPropagation();
    setSelectedAirport(null);
    setSearchTerm('');
    onChange?.('', null);
    inputRef.current?.focus();
  };

  const handleFocus = () => {
    setIsOpen(true);
    searchAirports(searchTerm);
  };

  return (
    <div className={`relative flex flex-col gap-1 ${containerClassName}`} ref={dropdownRef}>
      {label && (
        <label className="text-sm font-medium text-slate-700 flex items-center gap-1.5">
          <Plane className="h-3.5 w-3.5 text-brand-500 rotate-45" />
          {label}
        </label>
      )}

      <div className="relative">
        <input
          ref={inputRef}
          type="text"
          value={searchTerm}
          onChange={(e) => {
            setSearchTerm(e.target.value);
            if (!isOpen) setIsOpen(true);
          }}
          onFocus={handleFocus}
          placeholder={placeholder}
          required={required && !value}
          className={`input-base pl-9 pr-8 text-slate-800 font-medium ${
            error ? 'border-red-400 focus:ring-red-400' : ''
          } ${className}`}
        />

        <MapPin className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400 pointer-events-none" />

        {loading ? (
          <Loader2 className="absolute right-3 top-1/2 -translate-y-1/2 h-4 w-4 text-brand-500 animate-spin" />
        ) : searchTerm ? (
          <button
            type="button"
            onClick={handleClear}
            className="absolute right-2.5 top-1/2 -translate-y-1/2 p-1 text-slate-400 hover:text-slate-600 rounded-full hover:bg-slate-100 transition"
          >
            <X className="h-3.5 w-3.5" />
          </button>
        ) : null}
      </div>

      {error && <p className="text-xs text-red-500">{error}</p>}

      {/* Dropdown Menu */}
      {isOpen && (
        <div className="absolute left-0 right-0 top-full mt-1.5 z-50 bg-white rounded-2xl shadow-2xl border border-slate-100 py-2 max-h-72 overflow-y-auto backdrop-blur-md">
          {options.length > 0 ? (
            <div className="divide-y divide-slate-50">
              <div className="px-3 py-1.5 text-[11px] font-semibold text-slate-400 uppercase tracking-wider">
                {searchTerm ? 'Search Results' : 'Popular Airports'}
              </div>
              {options.map((airport) => (
                <button
                  key={airport.id || airport.iataCode}
                  type="button"
                  onClick={() => handleSelect(airport)}
                  className={`w-full px-3.5 py-2.5 text-left flex items-center justify-between gap-3 hover:bg-brand-50/80 transition-colors ${
                    value === airport.iataCode ? 'bg-brand-50 text-brand-700 font-semibold' : 'text-slate-700'
                  }`}
                >
                  <div className="flex items-center gap-3 min-w-0">
                    <div className="h-8 w-8 rounded-xl bg-slate-100 flex items-center justify-center text-slate-500 flex-shrink-0 group-hover:bg-brand-100">
                      <Plane className="h-4 w-4 text-brand-600" />
                    </div>
                    <div className="truncate">
                      <div className="font-semibold text-slate-800 text-sm leading-snug truncate">
                        {airport.city}, {airport.country}
                      </div>
                      <div className="text-xs text-slate-400 truncate">
                        {airport.name}
                      </div>
                    </div>
                  </div>
                  <span className="flex-shrink-0 px-2 py-0.5 text-xs font-mono font-bold rounded-md bg-slate-100 text-brand-700 border border-slate-200">
                    {airport.iataCode}
                  </span>
                </button>
              ))}
            </div>
          ) : (
            <div className="px-4 py-6 text-center text-sm text-slate-400">
              {loading ? 'Searching airports...' : 'No matching airports found'}
            </div>
          )}
        </div>
      )}
    </div>
  );
}

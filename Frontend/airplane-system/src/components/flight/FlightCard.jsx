import { Plane, Clock, Wifi } from 'lucide-react';
import { formatTime, formatDuration, formatCurrency } from '@/utils/formatters';
import Badge from '@/components/ui/Badge';
import { FLIGHT_STATUS_COLOR } from '@/utils/constants';

export default function FlightCard({ flight, onSelect, selectedClass }) {
  const priceKey = {
    Economy:  'economyBasePrice',
    Business: 'businessBasePrice',
    First:    'firstClassBasePrice',
  }[selectedClass] ?? 'economyBasePrice';

  const seatsKey = {
    Economy:  'availableEconomySeats',
    Business: 'availableBusinessSeats',
    First:    'availableFirstClassSeats',
  }[selectedClass] ?? 'availableEconomySeats';

  const price = flight.totalPrice ?? flight[priceKey];
  const seats = flight[seatsKey];
  const statusColor = FLIGHT_STATUS_COLOR[flight.status] ?? 'slate';

  return (
    <div className="card hover:shadow-md transition group">
      <div className="flex flex-col md:flex-row md:items-center gap-4">
        {/* Airline */}
        <div className="flex items-center gap-3 min-w-[160px]">
          {flight.airlineLogoUrl ? (
            <img
              src={flight.airlineLogoUrl}
              alt={flight.airlineName}
              className="h-10 w-10 rounded-lg object-contain border border-slate-100"
            />
          ) : (
            <div className="h-10 w-10 rounded-lg bg-brand-50 flex items-center justify-center">
              <Plane className="h-5 w-5 text-brand-500" />
            </div>
          )}
          <div>
            <p className="text-sm font-semibold text-slate-800">{flight.airlineName}</p>
            <p className="text-xs text-slate-400">{flight.flightNumber}</p>
          </div>
        </div>

        {/* Route */}
        <div className="flex-1 flex items-center gap-3">
          <div className="text-center">
            <p className="text-2xl font-bold text-slate-800">
              {formatTime(flight.departureTime)}
            </p>
            <p className="text-xs text-slate-500 font-medium">{flight.originIata}</p>
            <p className="text-xs text-slate-400">{flight.originCity}</p>
          </div>

          <div className="flex-1 flex flex-col items-center gap-1 min-w-0">
            <p className="text-xs text-slate-400 flex items-center gap-1">
              <Clock className="h-3 w-3" />
              {formatDuration(flight.durationMinutes)}
            </p>
            <div className="w-full flex items-center gap-1">
              <div className="flex-1 h-px bg-slate-200" />
              <Plane className="h-3.5 w-3.5 text-brand-400 rotate-90" />
              <div className="flex-1 h-px bg-slate-200" />
            </div>
            <p className="text-xs text-slate-400">
              {flight.stops === 0
                ? 'Nonstop'
                : `${flight.stops} stop${flight.stops > 1 ? 's' : ''}`}
            </p>
          </div>

          <div className="text-center">
            <p className="text-2xl font-bold text-slate-800">
              {formatTime(flight.arrivalTime)}
            </p>
            <p className="text-xs text-slate-500 font-medium">{flight.destinationIata}</p>
            <p className="text-xs text-slate-400">{flight.destinationCity}</p>
          </div>
        </div>

        {/* Price + action */}
        <div className="flex md:flex-col items-center md:items-end gap-3 md:min-w-[140px]">
          <div className="text-right">
            <p className="text-2xl font-extrabold text-brand-600">
              {formatCurrency(price)}
            </p>
            <p className="text-xs text-slate-400">{selectedClass ?? 'Economy'} · per person</p>
          </div>

          <div className="flex flex-col items-end gap-2">
            <Badge color={statusColor}>{flight.status}</Badge>
            {seats !== undefined && (
              <p className={`text-xs font-medium ${seats < 10 ? 'text-red-500' : 'text-slate-400'}`}>
                {seats} seats left
              </p>
            )}
            {onSelect && (
              <button
                onClick={() => onSelect(flight)}
                disabled={seats === 0 || flight.status === 'Cancelled'}
                className="btn-primary text-xs px-4 py-2 disabled:opacity-50"
              >
                Select
              </button>
            )}
          </div>
        </div>
      </div>

      {/* Extra info bar */}
      <div className="flex items-center gap-4 mt-4 pt-4 border-t border-slate-50 text-xs text-slate-400">
        <span className="flex items-center gap-1">
          <Wifi className="h-3 w-3" />
          {flight.aircraftModel ?? 'Aircraft TBC'}
        </span>
        {flight.gateNumber && (
          <span>Gate {flight.gateNumber}</span>
        )}
        <span className="ml-auto">
          Tax {flight.taxPercentage}% · Fee {formatCurrency(flight.airportFee)}
        </span>
      </div>
    </div>
  );
}
import { useCallback, useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { SlidersHorizontal, ArrowUpDown, Plane } from 'lucide-react';
import PageWrapper    from '@/components/layout/PageWrapper';
import FlightCard     from '@/components/flight/FlightCard';
import Spinner        from '@/components/ui/Spinner';
import Alert          from '@/components/ui/Alert';
import Button         from '@/components/ui/Button';
import flightService  from '@/api/flightService';
import { TRIP_TYPE }  from '@/utils/constants';
import { formatCurrency } from '@/utils/formatters';

export default function SearchResultsPage() {
  const [params]    = useSearchParams();
  const navigate    = useNavigate();

  const tripType    = params.get('tripType')   ?? TRIP_TYPE.ONE_WAY;
  const seatClass   = params.get('seatClass')  ?? 'Economy';
  const adults      = +(params.get('adults')   ?? 1);
  const children    = +(params.get('children') ?? 0);
  const infants     = +(params.get('infants')  ?? 0);
  const maxStopsQ   = +(params.get('maxStops') ?? 2);

  const [results,   setResults]   = useState([]);
  const [loading,   setLoading]   = useState(true);
  const [error,     setError]     = useState('');
  const [sortBy,    setSortBy]    = useState('price');
  const [sortDesc,  setSortDesc]  = useState(false);
  const [maxPrice,  setMaxPrice]  = useState('');
  const [maxStops,  setMaxStops]  = useState(maxStopsQ);

  const fetchResults = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const passengers = { adults, children, infants };
      let data;

      if (tripType === TRIP_TYPE.MULTI_CITY) {
        const legs = JSON.parse(params.get('legs') ?? '[]');
        const res  = await flightService.searchMultiCity({
          legs, passengers, seatClass,
        });
        data = res.data;
      } else if (tripType === TRIP_TYPE.ROUND_TRIP) {
        const res = await flightService.searchRoundTrip({
          originIata:      params.get('originIata'),
          destinationIata: params.get('destinationIata'),
          departureDate:   params.get('departureDate'),
          returnDate:      params.get('returnDate'),
          passengers,
          seatClass,
          maxStops,
          sortBy,
          sortDescending: sortDesc,
          maxPrice:       maxPrice ? +maxPrice : undefined,
        });
        data = res.data;
      } else {
        const res = await flightService.searchOneWay({
          originIata:      params.get('originIata'),
          destinationIata: params.get('destinationIata'),
          departureDate:   params.get('departureDate'),
          passengers,
          seatClass,
          maxStops,
          sortBy,
          sortDescending: sortDesc,
          maxPrice:       maxPrice ? +maxPrice : undefined,
        });
        data = res.data;
      }
      setResults(data ?? []);
    } catch (err) {
      setError(err.response?.data?.detail ?? 'Search failed. Please try again.');
    } finally {
      setLoading(false);
    }
  }, [params, sortBy, sortDesc, maxPrice, maxStops, tripType, adults, children, infants, seatClass]);

  useEffect(() => { fetchResults(); }, [fetchResults]);

  const handleSelect = (flight, returnFlight) => {
    const state = { flight, returnFlight, seatClass, passengers: { adults, children, infants } };
    navigate('/book', { state });
  };

  const isRoundTrip  = tripType === TRIP_TYPE.ROUND_TRIP;
  const isMultiCity  = tripType === TRIP_TYPE.MULTI_CITY;

  return (
    <PageWrapper>
      <div className="page-container py-8">
        {/* Header */}
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-slate-800">
            {isMultiCity
              ? 'Multi-City Results'
              : isRoundTrip
              ? `${params.get('originIata')} ⇄ ${params.get('destinationIata')}`
              : `${params.get('originIata')} → ${params.get('destinationIata')}`}
          </h1>
          <p className="text-slate-500 text-sm mt-1">
            {adults + children + infants} passenger{adults + children + infants > 1 ? 's' : ''} ·{' '}
            {seatClass} · {results.length} result{results.length !== 1 ? 's' : ''}
          </p>
        </div>

        <div className="flex flex-col lg:flex-row gap-6">
          {/* ── Filters sidebar ── */}
          <aside className="w-full lg:w-64 flex-shrink-0">
            <div className="card sticky top-24">
              <div className="flex items-center gap-2 mb-4">
                <SlidersHorizontal className="h-4 w-4 text-brand-600" />
                <h3 className="font-semibold text-slate-800">Filters</h3>
              </div>

              <div className="flex flex-col gap-4">
                <div className="flex flex-col gap-1">
                  <label className="text-xs font-semibold text-slate-500 uppercase tracking-wider">
                    Max price (USD)
                  </label>
                  <input
                    type="number"
                    min={0}
                    placeholder="Any"
                    value={maxPrice}
                    onChange={(e) => setMaxPrice(e.target.value)}
                    className="input-base"
                  />
                </div>

                <div className="flex flex-col gap-1">
                  <label className="text-xs font-semibold text-slate-500 uppercase tracking-wider">
                    Max stops
                  </label>
                  <div className="flex gap-2">
                    {[0, 1, 2].map((n) => (
                      <button
                        key={n}
                        type="button"
                        onClick={() => setMaxStops(n)}
                        className={`flex-1 py-1.5 rounded-lg text-sm font-medium border transition
                          ${maxStops === n
                            ? 'bg-brand-600 text-white border-brand-600'
                            : 'border-slate-200 text-slate-600 hover:border-brand-300'}`}
                      >
                        {n === 0 ? 'Direct' : n === 1 ? '1 stop' : '2+'}
                      </button>
                    ))}
                  </div>
                </div>

                <Button
                  onClick={fetchResults}
                  className="w-full"
                  size="sm"
                >
                  Apply Filters
                </Button>
              </div>
            </div>
          </aside>

          {/* ── Results ── */}
          <div className="flex-1 min-w-0">
            {/* Sort bar */}
            {!loading && results.length > 0 && (
              <div className="flex items-center gap-2 mb-4 flex-wrap">
                <ArrowUpDown className="h-4 w-4 text-slate-400" />
                <span className="text-sm text-slate-500">Sort by:</span>
                {['price', 'duration', 'departure'].map((s) => (
                  <button
                    key={s}
                    onClick={() => {
                      if (sortBy === s) setSortDesc((p) => !p);
                      else { setSortBy(s); setSortDesc(false); }
                    }}
                    className={`px-3 py-1 rounded-lg text-sm font-medium transition
                      ${sortBy === s
                        ? 'bg-brand-100 text-brand-700'
                        : 'text-slate-500 hover:bg-slate-100'}`}
                  >
                    {s.charAt(0).toUpperCase() + s.slice(1)}
                    {sortBy === s && (sortDesc ? ' ↓' : ' ↑')}
                  </button>
                ))}
              </div>
            )}

            {loading && (
              <div className="flex flex-col items-center py-24 gap-4">
                <Spinner size="lg" />
                <p className="text-slate-500">Searching flights…</p>
              </div>
            )}

            {!loading && error && (
              <Alert type="error" message={error} />
            )}

            {!loading && !error && results.length === 0 && (
              <div className="text-center py-24">
                <Plane className="h-12 w-12 text-slate-200 mx-auto mb-4" />
                <p className="text-slate-500 font-medium">No flights found</p>
                <p className="text-slate-400 text-sm mt-1">
                  Try adjusting your filters or search dates.
                </p>
                <Button
                  variant="secondary"
                  className="mt-6"
                  onClick={() => navigate('/')}
                >
                  Modify Search
                </Button>
              </div>
            )}

            {/* ONE-WAY results */}
            {!loading && !error && !isRoundTrip && !isMultiCity &&
              results.map((f) => (
                <div key={f.id} className="mb-4">
                  <FlightCard
                    flight={f}
                    selectedClass={seatClass}
                    onSelect={(fl) => handleSelect(fl)}
                  />
                </div>
              ))}

            {/* ROUND-TRIP results */}
            {!loading && !error && isRoundTrip &&
              results.map((r, i) => (
                <div key={i} className="card mb-4">
                  <div className="flex items-center gap-2 mb-3">
                    <span className="text-xs font-bold text-brand-600 uppercase tracking-wider
                                     bg-brand-50 px-2 py-1 rounded-lg">
                      Outbound
                    </span>
                    <span className="text-sm font-bold text-slate-700">
                      Total: {formatCurrency(r.totalPrice)}
                    </span>
                  </div>
                  <FlightCard flight={r.outboundFlight} selectedClass={seatClass} />
                  <div className="flex items-center gap-2 mt-4 mb-3">
                    <span className="text-xs font-bold text-sky-600 uppercase tracking-wider
                                     bg-sky-50 px-2 py-1 rounded-lg">
                      Return
                    </span>
                  </div>
                  <FlightCard flight={r.returnFlight} selectedClass={seatClass} />
                  <Button
                    className="w-full mt-4"
                    onClick={() => handleSelect(r.outboundFlight, r.returnFlight)}
                  >
                    Select Round Trip — {formatCurrency(r.totalPrice)}
                  </Button>
                </div>
              ))}

            {/* MULTI-CITY results */}
            {!loading && !error && isMultiCity &&
              results.map((r, i) => (
                <div key={i} className="card mb-4">
                  <div className="flex items-center justify-between mb-3">
                    <span className="text-xs font-bold text-purple-600 uppercase tracking-wider
                                     bg-purple-50 px-2 py-1 rounded-lg">
                      Multi-City Itinerary {i + 1}
                    </span>
                    <span className="font-bold text-slate-700">
                      {formatCurrency(r.totalPrice)}
                    </span>
                  </div>
                  <div className="flex flex-col gap-3">
                    {r.flights.map((f, fi) => (
                      <FlightCard key={fi} flight={f} selectedClass={seatClass} />
                    ))}
                  </div>
                  <Button
                    className="w-full mt-4"
                    onClick={() => handleSelect(r.flights[0], null)}
                  >
                    Select Itinerary — {formatCurrency(r.totalPrice)}
                  </Button>
                </div>
              ))}
          </div>
        </div>
      </div>
    </PageWrapper>
  );
}
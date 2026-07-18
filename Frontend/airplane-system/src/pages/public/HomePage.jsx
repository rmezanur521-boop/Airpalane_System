// src/pages/public/HomePage.jsx
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plane, ArrowLeftRight, Plus, Trash2, ArrowRight, Users, Armchair } from 'lucide-react';
import PageWrapper from '@/components/layout/PageWrapper';
import Button from '@/components/ui/Button';
import Input  from '@/components/ui/Input';
import Select from '@/components/ui/Select';
import { TRIP_TYPE, SEAT_CLASS_OPTIONS } from '@/utils/constants';
import { useCms } from '@/context/CmsContext';
import { buildCmsImageUrl } from '@/api/cms/cmsHelpers';

const TRIP_TABS = [
  { id: TRIP_TYPE.ONE_WAY,    label: 'One Way' },
  { id: TRIP_TYPE.ROUND_TRIP, label: 'Round Trip' },
  { id: TRIP_TYPE.MULTI_CITY, label: 'Multi-City' },
];

const EMPTY_LEG = { originIata: '', destinationIata: '', departureDate: '' };
const TODAY = new Date().toISOString().split('T')[0];

export default function HomePage() {
  const navigate = useNavigate();
  const cms = useCms();
  const [tripType, setTripType] = useState(TRIP_TYPE.ONE_WAY);

  const [form, setForm] = useState({
    originIata: '', destinationIata: '', departureDate: '', returnDate: '',
    adults: 1, children: 0, infants: 0, seatClass: 'Economy', maxStops: 2,
  });

  const [legs, setLegs] = useState([{ ...EMPTY_LEG }, { ...EMPTY_LEG }]);

  const setField = (k, v) => setForm((p) => ({ ...p, [k]: v }));
  const swapAirports = () => setForm((p) => ({ ...p, originIata: p.destinationIata, destinationIata: p.originIata }));
  const updateLeg = (i, k, v) => setLegs((prev) => prev.map((l, idx) => (idx === i ? { ...l, [k]: v } : l)));
  const addLeg    = () => setLegs((p) => [...p, { ...EMPTY_LEG }]);
  const removeLeg = (i) => setLegs((p) => p.filter((_, idx) => idx !== i));

  const handleSearch = (e) => {
    e.preventDefault();
    const params = new URLSearchParams();
    params.set('tripType', tripType);
    params.set('seatClass', form.seatClass);
    params.set('adults', form.adults);
    params.set('children', form.children);
    params.set('infants', form.infants);
    params.set('maxStops', form.maxStops);

    if (tripType === TRIP_TYPE.MULTI_CITY) {
      params.set('legs', JSON.stringify(legs));
    } else {
      params.set('originIata', form.originIata.toUpperCase());
      params.set('destinationIata', form.destinationIata.toUpperCase());
      params.set('departureDate', form.departureDate);
      if (tripType === TRIP_TYPE.ROUND_TRIP && form.returnDate) {
        params.set('returnDate', form.returnDate);
      }
    }
    navigate(`/search?${params.toString()}`);
  };

  const heroSlide = cms.hero?.[0];
  const heroTitle    = heroSlide?.title    || 'Your Journey Starts Here';
  const heroSubtitle = heroSlide?.subtitle || 'Search hundreds of flights to find the best deal';
  const heroBg       = buildCmsImageUrl(heroSlide?.backgroundImage);
  const overlay      = heroSlide?.overlayOpacity ?? 0.4;

  return (
    <PageWrapper>
      {/* Hero */}
      <section className="relative bg-gradient-to-br from-brand-700 via-brand-600 to-sky-500 overflow-hidden">
        {heroBg && (
          <div className="absolute inset-0 bg-cover bg-center" style={{ backgroundImage: `url(${heroBg})` }} />
        )}
        <div className="absolute inset-0 bg-black" style={{ opacity: heroBg ? overlay : 0 }} />
        {!heroBg && (
          <div className="absolute inset-0 opacity-10"
            style={{
              backgroundImage: `url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60'
                xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E
                %3Cg fill='%23ffffff' fill-opacity='1'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E
                %3C/g%3E%3C/g%3E%3C/svg%3E")`,
            }}
          />
        )}
        <div className="page-container py-16 relative">
          <div className="text-center text-white mb-10">
            <h1 className="text-4xl md:text-5xl font-extrabold mb-3 leading-tight">{heroTitle}</h1>
            <p className="text-brand-100 text-lg">{heroSubtitle}</p>
            {heroSlide?.buttonText && heroSlide?.buttonLink && (
              <a href={heroSlide.buttonLink} className="inline-flex items-center gap-2 mt-4 text-white font-medium underline underline-offset-4">
                {heroSlide.buttonText} <ArrowRight className="h-4 w-4" />
              </a>
            )}
          </div>

          {(heroSlide ? heroSlide.searchBoxEnabled !== false : true) && (
            <div className="bg-white rounded-3xl shadow-2xl p-6 md:p-8 max-w-4xl mx-auto">
              <div className="flex gap-1 bg-slate-100 rounded-xl p-1 mb-6 w-fit">
                {TRIP_TABS.map((t) => (
                  <button key={t.id} type="button" onClick={() => setTripType(t.id)}
                    className={`px-4 py-2 rounded-lg text-sm font-medium transition
                      ${tripType === t.id ? 'bg-white text-brand-600 shadow-sm' : 'text-slate-500 hover:text-slate-700'}`}>
                    {t.label}
                  </button>
                ))}
              </div>

              <form onSubmit={handleSearch}>
                {tripType !== TRIP_TYPE.MULTI_CITY && (
                  <div className="flex flex-col gap-4">
                    <div className="grid grid-cols-1 md:grid-cols-[1fr_auto_1fr] gap-2 items-end">
                      <Input label="From" placeholder="e.g. JFK" value={form.originIata}
                        onChange={(e) => setField('originIata', e.target.value.toUpperCase())} maxLength={3} required />
                      <button type="button" onClick={swapAirports}
                        className="hidden md:flex h-10 w-10 items-center justify-center rounded-xl border border-slate-200 text-slate-400 hover:text-brand-600 hover:border-brand-300 transition mb-0.5">
                        <ArrowLeftRight className="h-4 w-4" />
                      </button>
                      <Input label="To" placeholder="e.g. LAX" value={form.destinationIata}
                        onChange={(e) => setField('destinationIata', e.target.value.toUpperCase())} maxLength={3} required />
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <Input label="Departure date" type="date" min={TODAY} value={form.departureDate}
                        onChange={(e) => setField('departureDate', e.target.value)} required />
                      {tripType === TRIP_TYPE.ROUND_TRIP && (
                        <Input label="Return date" type="date" min={form.departureDate || TODAY} value={form.returnDate}
                          onChange={(e) => setField('returnDate', e.target.value)} required />
                      )}
                    </div>
                  </div>
                )}

                {tripType === TRIP_TYPE.MULTI_CITY && (
                  <div className="flex flex-col gap-3 mb-4">
                    {legs.map((leg, i) => (
                      <div key={i} className="grid grid-cols-1 md:grid-cols-[1fr_1fr_1fr_auto] gap-3 items-end p-4 rounded-xl bg-slate-50 border border-slate-100">
                        <Input label={`From ${i + 1}`} placeholder="IATA" value={leg.originIata}
                          onChange={(e) => updateLeg(i, 'originIata', e.target.value.toUpperCase())} maxLength={3} required />
                        <Input label="To" placeholder="IATA" value={leg.destinationIata}
                          onChange={(e) => updateLeg(i, 'destinationIata', e.target.value.toUpperCase())} maxLength={3} required />
                        <Input label="Date" type="date" min={TODAY} value={leg.departureDate}
                          onChange={(e) => updateLeg(i, 'departureDate', e.target.value)} required />
                        {legs.length > 2 && (
                          <button type="button" onClick={() => removeLeg(i)}
                            className="h-10 w-10 flex items-center justify-center rounded-xl text-red-400 hover:bg-red-50 hover:text-red-600 transition mb-0.5">
                            <Trash2 className="h-4 w-4" />
                          </button>
                        )}
                      </div>
                    ))}
                    {legs.length < 5 && (
                      <button type="button" onClick={addLeg}
                        className="flex items-center gap-2 text-sm text-brand-600 hover:text-brand-700 font-medium px-2">
                        <Plus className="h-4 w-4" />
                        Add another flight
                      </button>
                    )}
                  </div>
                )}

                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-4">
                  <div className="flex flex-col gap-1">
                    <label className="text-sm font-medium text-slate-700">Adults</label>
                    <input type="number" min={1} max={9} value={form.adults}
                      onChange={(e) => setField('adults', +e.target.value)} className="input-base" required />
                  </div>
                  <div className="flex flex-col gap-1">
                    <label className="text-sm font-medium text-slate-700">Children</label>
                    <input type="number" min={0} max={9} value={form.children}
                      onChange={(e) => setField('children', +e.target.value)} className="input-base" />
                  </div>
                  <div className="flex flex-col gap-1">
                    <label className="text-sm font-medium text-slate-700">Infants</label>
                    <input type="number" min={0} max={9} value={form.infants}
                      onChange={(e) => setField('infants', +e.target.value)} className="input-base" />
                  </div>
                  <Select label="Cabin class" options={SEAT_CLASS_OPTIONS} value={form.seatClass}
                    onChange={(e) => setField('seatClass', e.target.value)} />
                </div>

                <Button type="submit" className="w-full mt-6" size="lg">
                  <Plane className="h-5 w-5" />
                  Search Flights
                </Button>
              </form>
            </div>
          )}
        </div>
      </section>

      {/* Special Offers */}
      {cms.offers?.length > 0 && (
        <section className="page-container py-14">
          <h2 className="section-title mb-6">Special Offers</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {cms.offers.map((o) => (
              <a key={o.id} href={o.buttonLink || '#'} className="card overflow-hidden p-0 hover:shadow-md transition group">
                {o.offerImage && (
                  <div className="h-40 overflow-hidden">
                    <img src={buildCmsImageUrl(o.offerImage)} alt={o.title}
                      className="h-full w-full object-cover group-hover:scale-105 transition duration-300" />
                  </div>
                )}
                <div className="p-5">
                  <h3 className="font-semibold text-slate-800">{o.title}</h3>
                  {o.promoCode && (
                    <span className="inline-block mt-1 text-xs font-mono bg-brand-50 text-brand-700 px-2 py-0.5 rounded-md">
                      {o.promoCode}
                    </span>
                  )}
                  <p className="text-brand-600 font-bold mt-2">${Number(o.price ?? 0).toFixed(0)}</p>
                </div>
              </a>
            ))}
          </div>
        </section>
      )}

      {/* Popular Destinations */}
      {cms.destinations?.length > 0 && (
        <section className="page-container py-14">
          <h2 className="section-title mb-6">Popular Destinations</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {cms.destinations.map((d) => (
              <a key={d.id} href={d.buttonLink || '#'} className="card overflow-hidden p-0 hover:shadow-md transition group">
                {d.image && (
                  <div className="h-32 overflow-hidden">
                    <img src={buildCmsImageUrl(d.image)} alt={d.destinationName}
                      className="h-full w-full object-cover group-hover:scale-105 transition duration-300" />
                  </div>
                )}
                <div className="p-4">
                  <h3 className="font-semibold text-slate-800">{d.destinationName}</h3>
                  <p className="text-xs text-slate-400">{d.country}</p>
                  <p className="text-brand-600 font-bold mt-1 text-sm">From ${Number(d.startingPrice ?? 0).toFixed(0)}</p>
                </div>
              </a>
            ))}
          </div>
        </section>
      )}

      {/* Why Choose Us */}
      {cms.whyChooseUs?.length > 0 && (
        <section className="page-container py-14">
          <h2 className="section-title mb-6 text-center">Why Choose Us</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {cms.whyChooseUs.map((w) => (
              <div key={w.id} className="card text-center hover:shadow-md transition">
                <div className="h-12 w-12 rounded-2xl mx-auto mb-4 flex items-center justify-center"
                  style={{ backgroundColor: `${w.iconColor}1A` }}>
                  <i className={`fa ${w.icon}`} style={{ color: w.iconColor }} />
                </div>
                <h3 className="font-semibold text-slate-800 mb-2">{w.title}</h3>
                <p className="text-sm text-slate-500">{w.description}</p>
              </div>
            ))}
          </div>
        </section>
      )}

      {/* Fleet */}
      {cms.fleet?.length > 0 && (
        <section className="page-container py-14">
          <h2 className="section-title mb-6">Our Fleet</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
            {cms.fleet.map((f) => (
              <div key={f.id} className="card overflow-hidden p-0 hover:shadow-md transition">
                {f.image && (
                  <div className="h-40 overflow-hidden">
                    <img src={buildCmsImageUrl(f.image)} alt={f.aircraftName} className="h-full w-full object-cover" />
                  </div>
                )}
                <div className="p-5">
                  <h3 className="font-semibold text-slate-800">{f.aircraftName}</h3>
                  <p className="text-xs text-slate-400">{f.manufacturer}</p>
                  <div className="flex items-center gap-4 mt-3 text-sm text-slate-500">
                    <span className="flex items-center gap-1"><Armchair className="h-4 w-4" /> {f.seatCapacity}</span>
                    <span className="flex items-center gap-1"><Users className="h-4 w-4" /> {f.range}</span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </section>
      )}

      {/* Travel Services */}
      {cms.services?.length > 0 && (
        <section className="page-container py-14">
          <h2 className="section-title mb-6">Travel Services</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {cms.services.map((s) => (
              <a key={s.id} href={s.redirectUrl || '#'}
                target={s.isExternal ? '_blank' : undefined} rel={s.isExternal ? 'noreferrer' : undefined}
                className="card text-center hover:shadow-md transition">
                {s.image
                  ? <img src={buildCmsImageUrl(s.image)} alt={s.serviceName} className="h-10 w-10 mx-auto mb-3 object-contain" />
                  : <i className={`fa ${s.icon} text-2xl text-brand-600 mb-3 block`} />}
                <h3 className="font-medium text-slate-800 text-sm">{s.serviceName}</h3>
              </a>
            ))}
          </div>
        </section>
      )}
    </PageWrapper>
  );
}
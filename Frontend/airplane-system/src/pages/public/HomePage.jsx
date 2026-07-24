// src/pages/public/HomePage.jsx
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plane, ArrowLeftRight, Plus, Trash2, ArrowRight, Users, Armchair, ShieldCheck } from 'lucide-react';
import PageWrapper from '@/components/layout/PageWrapper';
import Button from '@/components/ui/Button';
import Input  from '@/components/ui/Input';
import Select from '@/components/ui/Select';
import Reveal from '@/components/ui/Reveal';
import SectionSkeleton from '@/components/ui/SectionSkeleton';
import { TRIP_TYPE, SEAT_CLASS_OPTIONS } from '@/utils/constants';
import { useCms } from '@/context/CmsContext';
import { buildCmsImageUrl } from '@/api/cms/cmsHelpers';
import { resolveIcon } from '@/utils/iconResolver';

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
  const heroTitle    = heroSlide?.title    || 'Fly Beyond Limits';
  const heroSubtitle = heroSlide?.subtitle || 'Search hundreds of routes and find your best deal in seconds';
  const heroBg       = buildCmsImageUrl(heroSlide?.backgroundImage);
  const overlay      = heroSlide?.overlayOpacity ?? 0.45;

  return (
    <PageWrapper>
      {/* ── Hero — Qatar Airways-inspired glass search card ───────────── */}
      <section className="relative bg-gradient-to-br from-brand-800 via-brand-600 to-sky-500">
  
  <div className="absolute inset-0 overflow-hidden pointer-events-none">
    {heroBg && (
      <div className="absolute inset-0 bg-cover bg-center scale-105" style={{ backgroundImage: `url(${heroBg})` }} />
    )}
    <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-black/20 to-black/10"
      style={{ opacity: heroBg ? overlay : 0.15 }} />
    <div className="absolute -top-24 -left-24 h-72 w-72 rounded-full bg-white/10 blur-3xl" />
    <div className="absolute -bottom-32 -right-16 h-96 w-96 rounded-full bg-sky-300/20 blur-3xl" />
  </div>

  <div className="page-container pt-16 pb-20 md:pb-24 relative z-10">
    <div className="text-center text-white mb-10 max-w-2xl mx-auto">
      <span className="inline-flex items-center gap-1.5 text-xs font-medium bg-white/15 backdrop-blur-sm px-3 py-1 rounded-full mb-4">
        <ShieldCheck className="h-3.5 w-3.5" /> Best Price Guarantee
      </span>
      <h1 className="text-4xl md:text-5xl font-extrabold mb-3 leading-tight tracking-tight">{heroTitle}</h1>
      <p className="text-brand-100 text-lg">{heroSubtitle}</p>
      {heroSlide?.buttonText && heroSlide?.buttonLink && (
        <a href={heroSlide.buttonLink}
          className="inline-flex items-center gap-2 mt-4 text-white font-medium underline underline-offset-4 hover:text-brand-100 transition">
          {heroSlide.buttonText} <ArrowRight className="h-4 w-4" />
        </a>
      )}
    </div>

    {(heroSlide ? heroSlide.searchBoxEnabled !== false : true) && (
      <div className="bg-white/90 backdrop-blur-xl rounded-3xl shadow-2xl ring-1 ring-black/5
                       p-6 md:p-8 max-w-4xl mx-auto relative z-20 -mb-16 md:-mb-20">
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
                        className="hidden md:flex h-10 w-10 items-center justify-center rounded-xl border border-slate-200 text-slate-400 hover:text-brand-600 hover:border-brand-300 hover:rotate-180 transition-all duration-300 mb-0.5">
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


      {cms.loading ? (
        <>
          <SectionSkeleton cards={3} />
          <SectionSkeleton cards={4} imageHeight="h-32" />
        </>
      ) : (
        <>
          {/* Special Offers */}
          {cms.offers?.length > 0 && (
            <Reveal>
              <section className="page-container py-14">
                <h2 className="section-title mb-6">Special Offers</h2>
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                  {cms.offers.map((o) => (
                    <a key={o.id} href={o.buttonLink || '#'}
                      className="group relative overflow-hidden rounded-2xl shadow-sm hover:shadow-xl transition-all duration-300">
                      {o.offerImage && (
                        <div className="h-48 overflow-hidden">
                          <img src={buildCmsImageUrl(o.offerImage)} alt={o.title}
                            className="h-full w-full object-cover group-hover:scale-110 transition-transform duration-500" />
                        </div>
                      )}
                      <div className="absolute inset-0 bg-gradient-to-t from-black/70 via-black/10 to-transparent" />
                      {o.promoCode && (
                        <span className="absolute top-3 left-3 bg-brand-600 text-white text-xs font-bold px-3 py-1 rounded-full shadow-lg">
                          {o.promoCode}
                        </span>
                      )}
                      <div className="absolute bottom-0 left-0 right-0 p-5 text-white">
                        <h3 className="font-semibold text-lg">{o.title}</h3>
                        <p className="text-brand-200 font-bold mt-1">${Number(o.price ?? 0).toFixed(0)}</p>
                      </div>
                    </a>
                  ))}
                </div>
              </section>
            </Reveal>
          )}

          {/* Popular Destinations */}
          {cms.destinations?.length > 0 && (
            <Reveal>
              <section className="page-container py-14">
                <h2 className="section-title mb-6">Popular Destinations</h2>
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
                  {cms.destinations.map((d) => (
                    <a key={d.id} href={d.buttonLink || '#'}
                      className="group relative h-64 overflow-hidden rounded-2xl shadow-sm hover:shadow-xl transition-all duration-300">
                      {d.image && (
                        <img src={buildCmsImageUrl(d.image)} alt={d.destinationName}
                          className="absolute inset-0 h-full w-full object-cover group-hover:scale-110 transition-transform duration-500" />
                      )}
                      <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/20 to-transparent group-hover:from-black/90 transition-all duration-300" />
                      <div className="absolute bottom-0 left-0 right-0 p-4 text-white">
                        <h3 className="font-semibold">{d.destinationName}</h3>
                        <p className="text-xs text-slate-300">{d.country}</p>
                        <p className="text-sm font-bold text-brand-300 mt-1">From ${Number(d.startingPrice ?? 0).toFixed(0)}</p>
                        <span className="inline-block mt-2 text-xs font-medium border border-white/40 rounded-full px-3 py-1
                          opacity-0 translate-y-2 group-hover:opacity-100 group-hover:translate-y-0 transition-all duration-300">
                          Explore →
                        </span>
                      </div>
                    </a>
                  ))}
                </div>
              </section>
            </Reveal>
          )}

          {/* Why Choose Us */}
          {cms.whyChooseUs?.length > 0 && (
            <Reveal>
              <section className="page-container py-14">
                <h2 className="section-title mb-6 text-center">Why Choose Us</h2>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
                  {cms.whyChooseUs.map((w) => {
                    const Icon = resolveIcon(w.icon);
                    return (
                      <div key={w.id}
                        className="card text-center transition-all duration-300 hover:shadow-xl hover:-translate-y-1
                                   hover:ring-2 hover:ring-offset-2"
                        style={{ '--tw-ring-color': `${w.iconColor}55` }}>
                        <div className="h-14 w-14 rounded-2xl mx-auto mb-4 flex items-center justify-center"
                          style={{ backgroundColor: `${w.iconColor}1A` }}>
                          <Icon className="h-7 w-7" style={{ color: w.iconColor }} />
                        </div>
                        <h3 className="font-semibold text-slate-800 mb-2">{w.title}</h3>
                        <p className="text-sm text-slate-500">{w.description}</p>
                      </div>
                    );
                  })}
                </div>
              </section>
            </Reveal>
          )}

          {/* Fleet */}
          {cms.fleet?.length > 0 && (
            <Reveal>
              <section className="page-container py-14">
                <h2 className="section-title mb-6">Our Fleet</h2>
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                  {cms.fleet.map((f) => (
                    <div key={f.id}
                      className="card overflow-hidden p-0 transition-all duration-300 hover:shadow-xl hover:-translate-y-1">
                      {f.image && (
                        <div className="h-44 overflow-hidden">
                          <img src={buildCmsImageUrl(f.image)} alt={f.aircraftName}
                            className="h-full w-full object-cover hover:scale-105 transition-transform duration-500" />
                        </div>
                      )}
                      <div className="p-5">
                        <h3 className="font-semibold text-slate-800">{f.aircraftName}</h3>
                        <p className="text-xs text-slate-400">{f.manufacturer}</p>
                        <div className="flex items-center gap-4 mt-3 text-sm text-slate-500">
                          <span className="flex items-center gap-1"><Armchair className="h-4 w-4" /> {f.seatCapacity} seats</span>
                          <span className="flex items-center gap-1"><Users className="h-4 w-4" /> {f.range}</span>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </section>
            </Reveal>
          )}

          {/* Travel Services */}
          {cms.services?.length > 0 && (
            <Reveal>
              <section className="page-container py-14">
                <h2 className="section-title mb-6">Travel Services</h2>
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
                  {cms.services.map((s) => {
                    const Icon = resolveIcon(s.icon);
                    return (
                      <a key={s.id} href={s.redirectUrl || '#'}
                        target={s.isExternal ? '_blank' : undefined} rel={s.isExternal ? 'noreferrer' : undefined}
                        className="text-center rounded-2xl p-6 bg-white/70 backdrop-blur-md border border-slate-100
                                   shadow-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300">
                        {s.image
                          ? <img src={buildCmsImageUrl(s.image)} alt={s.serviceName} className="h-10 w-10 mx-auto mb-3 object-contain" />
                          : <Icon className="h-8 w-8 mx-auto mb-3 text-brand-600" />}
                        <h3 className="font-medium text-slate-800 text-sm">{s.serviceName}</h3>
                        {s.description && (
                          <p className="text-xs text-slate-500 mt-1.5 leading-relaxed">{s.description}</p>
                        )}
                      </a>
                    );
                  })}
                </div>
              </section>
            </Reveal>
          )}
        </>
      )}
    </PageWrapper>
  );
}
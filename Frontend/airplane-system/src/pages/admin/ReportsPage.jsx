import { useEffect, useState } from 'react';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid,
  Tooltip, ResponsiveContainer, Cell,
} from 'recharts';
import { TrendingUp, DollarSign, Ticket, Route } from 'lucide-react';
import StatCard    from '@/components/ui/StatCard';
import Spinner     from '@/components/ui/Spinner';
import Alert       from '@/components/ui/Alert';
import Input       from '@/components/ui/Input';
import Button      from '@/components/ui/Button';
import adminService from '@/api/adminService';
import { formatCurrency, formatNumber } from '@/utils/formatters';

const TODAY    = new Date().toISOString().split('T')[0];
const MONTH_AGO = new Date(Date.now() - 30 * 86400000).toISOString().split('T')[0];

const COLORS = ['#0ea5e9', '#0284c7', '#0369a1', '#075985', '#0c4a6e', '#082f49'];

export default function ReportsPage() {
  const [revenue,  setRevenue]  = useState(null);
  const [bookings, setBookings] = useState(null);
  const [loading,  setLoading]  = useState(true);
  const [error,    setError]    = useState('');
  const [from,     setFrom]     = useState(MONTH_AGO);
  const [to,       setTo]       = useState(TODAY);

  const load = () => {
    setLoading(true); setError('');
    const f = from ? `${from}T00:00:00` : undefined;
    const t = to   ? `${to}T23:59:59`   : undefined;
    Promise.all([
      adminService.getRevenueReport(f, t),
      adminService.getBookingReport(f, t),
    ])
      .then(([rRes, bRes]) => {
        setRevenue(rRes.data);
        setBookings(bRes.data);
      })
      .catch(() => setError('Failed to load reports.'))
      .finally(() => setLoading(false));
  };

  useEffect(load, []);

  const routeData = (revenue?.topRoutes ?? []).map((r) => ({
    route:   `${r.originIata}→${r.destinationIata}`,
    revenue: r.revenue,
    count:   r.bookingCount,
  }));

  const airlineData = (revenue?.revenueByAirline ?? []).map((a) => ({
    name:    a.airlineName,
    revenue: a.revenue,
    count:   a.bookingCount,
  }));

  return (
    <div className="animate-fadeIn">
      {/* Header + date range */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-8">
        <div>
          <h1 className="text-2xl font-bold text-slate-800">Reports</h1>
          <p className="text-slate-500 text-sm mt-1">Revenue & booking analytics</p>
        </div>
        <div className="flex items-end gap-3 flex-wrap">
          <Input label="From" type="date" value={from}
            onChange={(e) => setFrom(e.target.value)} containerClassName="w-36" />
          <Input label="To"   type="date" value={to}
            onChange={(e) => setTo(e.target.value)}   containerClassName="w-36" />
          <Button onClick={load} size="sm" className="mb-0.5">Apply</Button>
        </div>
      </div>

      {error && <Alert type="error" message={error} className="mb-6" />}

      {loading ? (
        <div className="flex justify-center py-32"><Spinner size="lg" /></div>
      ) : (
        <>
          {/* Revenue stat cards */}
          {revenue && (
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
              <StatCard
                title="Total Revenue"
                value={formatCurrency(revenue.totalRevenue)}
                icon={DollarSign}
                color="green"
              />
              <StatCard
                title="Avg Ticket Price"
                value={formatCurrency(revenue.averageTicketPrice)}
                icon={TrendingUp}
                color="brand"
              />
              <StatCard
                title="Tickets Sold"
                value={formatNumber(revenue.totalTicketsSold)}
                icon={Ticket}
                color="purple"
              />
            </div>
          )}

          {/* Booking stat cards */}
          {bookings && (
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-8">
              {[
                { label: 'Total',     value: bookings.totalBookings,  color: 'brand' },
                { label: 'Confirmed', value: bookings.confirmed,       color: 'green' },
                { label: 'Cancelled', value: bookings.cancelled,       color: 'red' },
                { label: 'Refunded',  value: bookings.refunded,        color: 'purple' },
              ].map((s) => (
                <div key={s.label} className="card text-center">
                  <p className="text-3xl font-black text-slate-800">{s.value}</p>
                  <p className="text-sm text-slate-500 mt-1">{s.label}</p>
                  {s.label === 'Cancelled' && bookings.cancellationRate != null && (
                    <p className="text-xs text-red-400 mt-0.5">
                      Rate: {(bookings.cancellationRate * 100).toFixed(1)}%
                    </p>
                  )}
                </div>
              ))}
            </div>
          )}

          {/* Top routes chart */}
          {routeData.length > 0 && (
            <div className="card mb-6">
              <div className="flex items-center gap-2 mb-5">
                <Route className="h-5 w-5 text-brand-600" />
                <h2 className="font-semibold text-slate-800">Revenue by Top Routes</h2>
              </div>
              <ResponsiveContainer width="100%" height={280}>
                <BarChart data={routeData} layout="vertical">
                  <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" horizontal={false} />
                  <XAxis type="number" tick={{ fontSize: 11, fill: '#94a3b8' }}
                    tickFormatter={(v) => `$${(v / 1000).toFixed(0)}k`} />
                  <YAxis type="category" dataKey="route"
                    tick={{ fontSize: 11, fill: '#94a3b8' }} width={80} />
                  <Tooltip
                    formatter={(v) => formatCurrency(v)}
                    contentStyle={{ borderRadius: '12px', border: '1px solid #e2e8f0', fontSize: '13px' }}
                  />
                  <Bar dataKey="revenue" radius={[0, 6, 6, 0]}>
                    {routeData.map((_, i) => (
                      <Cell key={i} fill={COLORS[i % COLORS.length]} />
                    ))}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            </div>
          )}

          {/* Revenue by airline */}
          {airlineData.length > 0 && (
            <div className="card">
              <div className="flex items-center gap-2 mb-5">
                <DollarSign className="h-5 w-5 text-brand-600" />
                <h2 className="font-semibold text-slate-800">Revenue by Airline</h2>
              </div>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-slate-100">
                      <th className="text-left py-2 px-3 text-xs text-slate-500 font-semibold uppercase">
                        Airline
                      </th>
                      <th className="text-right py-2 px-3 text-xs text-slate-500 font-semibold uppercase">
                        Bookings
                      </th>
                      <th className="text-right py-2 px-3 text-xs text-slate-500 font-semibold uppercase">
                        Revenue
                      </th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-50">
                    {airlineData.map((a) => (
                      <tr key={a.name} className="hover:bg-slate-50 transition">
                        <td className="py-3 px-3 font-medium text-slate-800">{a.name}</td>
                        <td className="py-3 px-3 text-right text-slate-600">{a.count}</td>
                        <td className="py-3 px-3 text-right font-semibold text-brand-600">
                          {formatCurrency(a.revenue)}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
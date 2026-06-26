import { useEffect, useState } from 'react';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip,
  ResponsiveContainer, LineChart, Line,
} from 'recharts';
import {
  BookOpen, DollarSign, Users, Plane,
  Clock, XCircle, TrendingUp,
} from 'lucide-react';
import StatCard    from '@/components/ui/StatCard';
import Spinner     from '@/components/ui/Spinner';
import Alert       from '@/components/ui/Alert';
import Input       from '@/components/ui/Input';
import Button      from '@/components/ui/Button';
import adminService from '@/api/adminService';
import { formatCurrency, formatCurrency as fc } from '@/utils/formatters';

const TODAY    = new Date().toISOString().split('T')[0];
const WEEK_AGO = new Date(Date.now() - 7 * 86400000).toISOString().split('T')[0];

export default function AdminDashboardPage() {
  const [data,    setData]    = useState(null);
  const [loading, setLoading] = useState(true);
  const [error,   setError]   = useState('');
  const [from,    setFrom]    = useState(WEEK_AGO);
  const [to,      setTo]      = useState(TODAY);

  const load = () => {
    setLoading(true);
    setError('');
    adminService
      .getDashboard(from ? `${from}T00:00:00` : undefined, to ? `${to}T23:59:59` : undefined)
      .then(({ data: d }) => setData(d))
      .catch(() => setError('Failed to load dashboard data.'))
      .finally(() => setLoading(false));
  };

  useEffect(load, []);

  const chartData = (data?.revenueByLastSevenDays ?? []).map((d) => ({
    date:     d.date,
    revenue:  d.revenue,
    bookings: d.bookings,
  }));

  return (
    <div className="animate-fadeIn">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-8">
        <div>
          <h1 className="text-2xl font-bold text-slate-800">Dashboard</h1>
          <p className="text-slate-500 text-sm mt-1">Overview of your airline system</p>
        </div>
        <div className="flex items-end gap-3 flex-wrap">
          <Input
            label="From"
            type="date"
            value={from}
            onChange={(e) => setFrom(e.target.value)}
            containerClassName="w-36"
          />
          <Input
            label="To"
            type="date"
            value={to}
            onChange={(e) => setTo(e.target.value)}
            containerClassName="w-36"
          />
          <Button onClick={load} size="sm" className="mb-0.5">Apply</Button>
        </div>
      </div>

      {error && <Alert type="error" message={error} className="mb-6" />}

      {loading ? (
        <div className="flex justify-center py-32"><Spinner size="lg" /></div>
      ) : data && (
        <>
          {/* Stat cards */}
          <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4 mb-8">
            <StatCard
              title="Total Revenue"
              value={formatCurrency(data.totalRevenue)}
              subtitle={`Today: ${formatCurrency(data.revenueToday)}`}
              icon={DollarSign}
              color="green"
            />
            <StatCard
              title="Total Bookings"
              value={data.totalBookings}
              subtitle={`Confirmed: ${data.confirmedBookings}`}
              icon={BookOpen}
              color="brand"
            />
            <StatCard
              title="Total Users"
              value={data.totalUsers}
              icon={Users}
              color="purple"
            />
            <StatCard
              title="Active Flights"
              value={data.activeFlights}
              subtitle={`Delayed: ${data.delayedFlights}`}
              icon={Plane}
              color="sky"
            />
          </div>

          {/* Secondary stats */}
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
            <StatCard
              title="Pending Bookings"
              value={data.pendingBookings}
              icon={Clock}
              color="yellow"
            />
            <StatCard
              title="Cancelled Bookings"
              value={data.cancelledBookings}
              icon={XCircle}
              color="red"
            />
            <StatCard
              title="Cancelled Flights"
              value={data.cancelledFlights}
              icon={XCircle}
              color="red"
            />
          </div>

          {/* Charts */}
          {chartData.length > 0 && (
            <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
              {/* Revenue chart */}
              <div className="card">
                <div className="flex items-center gap-2 mb-5">
                  <TrendingUp className="h-5 w-5 text-brand-600" />
                  <h2 className="font-semibold text-slate-800">Revenue (Last 7 Days)</h2>
                </div>
                <ResponsiveContainer width="100%" height={240}>
                  <LineChart data={chartData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
                    <XAxis
                      dataKey="date"
                      tick={{ fontSize: 11, fill: '#94a3b8' }}
                      tickFormatter={(v) => v.slice(5)}
                    />
                    <YAxis
                      tick={{ fontSize: 11, fill: '#94a3b8' }}
                      tickFormatter={(v) => `$${(v / 1000).toFixed(0)}k`}
                    />
                    <Tooltip
                      formatter={(v) => formatCurrency(v)}
                      contentStyle={{
                        borderRadius: '12px',
                        border: '1px solid #e2e8f0',
                        fontSize: '13px',
                      }}
                    />
                    <Line
                      type="monotone"
                      dataKey="revenue"
                      stroke="#0ea5e9"
                      strokeWidth={2.5}
                      dot={{ fill: '#0ea5e9', r: 4 }}
                    />
                  </LineChart>
                </ResponsiveContainer>
              </div>

              {/* Bookings chart */}
              <div className="card">
                <div className="flex items-center gap-2 mb-5">
                  <BookOpen className="h-5 w-5 text-brand-600" />
                  <h2 className="font-semibold text-slate-800">Bookings (Last 7 Days)</h2>
                </div>
                <ResponsiveContainer width="100%" height={240}>
                  <BarChart data={chartData}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
                    <XAxis
                      dataKey="date"
                      tick={{ fontSize: 11, fill: '#94a3b8' }}
                      tickFormatter={(v) => v.slice(5)}
                    />
                    <YAxis tick={{ fontSize: 11, fill: '#94a3b8' }} />
                    <Tooltip
                      contentStyle={{
                        borderRadius: '12px',
                        border: '1px solid #e2e8f0',
                        fontSize: '13px',
                      }}
                    />
                    <Bar dataKey="bookings" fill="#0ea5e9" radius={[6, 6, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
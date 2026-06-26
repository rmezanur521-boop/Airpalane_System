import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Plane, BookOpen, Ticket, ArrowRight } from 'lucide-react';
import PageWrapper    from '@/components/layout/PageWrapper';
import Spinner        from '@/components/ui/Spinner';
import Badge          from '@/components/ui/Badge';
import { useAuth }    from '@/hooks/useAuth';
import bookingService from '@/api/bookingService';
import ticketService  from '@/api/ticketService';
import {
  formatDateTime, formatCurrency,
} from '@/utils/formatters';
import {
  BOOKING_STATUS_COLOR,
} from '@/utils/constants';

export default function DashboardPage() {
  const { user }                    = useAuth();
  const [bookings, setBookings]     = useState([]);
  const [loading,  setLoading]      = useState(true);

  useEffect(() => {
    bookingService
      .getMyBookings({ pageNumber: 1, pageSize: 5 })
      .then(({ data }) => setBookings(data.items ?? []))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  return (
    <PageWrapper>
      <div className="page-container py-10">
        {/* Greeting */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-slate-800">
            Hello, {user?.fullName?.split(' ')[0]} 👋
          </h1>
          <p className="text-slate-500 mt-1">
            Here's a summary of your travel activity.
          </p>
        </div>

        {/* Quick stats */}
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-10">
          {[
            {
              label: 'Total Bookings',
              value: bookings.length,
              Icon: BookOpen,
              color: 'text-brand-600 bg-brand-50',
            },
            {
              label: 'Confirmed',
              value: bookings.filter((b) => b.status === 'Confirmed').length,
              Icon: Ticket,
              color: 'text-green-600 bg-green-50',
            },
            {
              label: 'Upcoming Flights',
              value: bookings.filter(
                (b) =>
                  b.status === 'Confirmed' &&
                  b.segments?.some(
                    (s) => new Date(s.departureTime) > new Date()
                  )
              ).length,
              Icon: Plane,
              color: 'text-sky-600 bg-sky-50',
            },
          ].map((s) => (
            <div key={s.label} className="card flex items-center gap-4">
              <div className={`p-3 rounded-xl ${s.color}`}>
                <s.Icon className="h-6 w-6" />
              </div>
              <div>
                <p className="text-sm text-slate-500">{s.label}</p>
                <p className="text-3xl font-bold text-slate-800">{s.value}</p>
              </div>
            </div>
          ))}
        </div>

        {/* Quick actions */}
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-10">
          <Link
            to="/"
            className="card flex items-center gap-4 hover:shadow-md transition group"
          >
            <div className="p-3 rounded-xl bg-brand-50 text-brand-600 group-hover:bg-brand-100 transition">
              <Plane className="h-6 w-6" />
            </div>
            <div className="flex-1">
              <p className="font-semibold text-slate-800">Search Flights</p>
              <p className="text-sm text-slate-400">Find and book your next trip</p>
            </div>
            <ArrowRight className="h-5 w-5 text-slate-300 group-hover:text-brand-500 transition" />
          </Link>

          <Link
            to="/tickets"
            className="card flex items-center gap-4 hover:shadow-md transition group"
          >
            <div className="p-3 rounded-xl bg-sky-50 text-sky-600 group-hover:bg-sky-100 transition">
              <Ticket className="h-6 w-6" />
            </div>
            <div className="flex-1">
              <p className="font-semibold text-slate-800">My Tickets</p>
              <p className="text-sm text-slate-400">View and download boarding passes</p>
            </div>
            <ArrowRight className="h-5 w-5 text-slate-300 group-hover:text-sky-500 transition" />
          </Link>
        </div>

        {/* Recent bookings */}
        <div className="card">
          <div className="flex items-center justify-between mb-5">
            <h2 className="text-lg font-bold text-slate-800">Recent Bookings</h2>
            <Link
              to="/bookings"
              className="text-sm text-brand-600 hover:text-brand-700 font-medium flex items-center gap-1"
            >
              View all <ArrowRight className="h-4 w-4" />
            </Link>
          </div>

          {loading ? (
            <div className="flex justify-center py-10">
              <Spinner />
            </div>
          ) : bookings.length === 0 ? (
            <div className="text-center py-10 text-slate-400">
              <BookOpen className="h-10 w-10 mx-auto mb-3 opacity-30" />
              <p>No bookings yet.</p>
              <Link to="/" className="btn-primary mt-4 inline-flex">
                Book a Flight
              </Link>
            </div>
          ) : (
            <div className="flex flex-col divide-y divide-slate-50">
              {bookings.map((b) => (
                <Link
                  key={b.id}
                  to={`/bookings/${b.id}`}
                  className="flex items-center gap-4 py-3 hover:bg-slate-50 -mx-2 px-2 rounded-xl transition"
                >
                  <div className="h-10 w-10 rounded-xl bg-brand-50 flex items-center justify-center flex-shrink-0">
                    <Plane className="h-5 w-5 text-brand-500" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="font-semibold text-slate-800 text-sm">
                      {b.bookingReference}
                    </p>
                    <p className="text-xs text-slate-400">
                      {b.segments?.[0]?.originIata} →{' '}
                      {b.segments?.[b.segments.length - 1]?.destinationIata} ·{' '}
                      {formatDateTime(b.createdAt)}
                    </p>
                  </div>
                  <div className="flex flex-col items-end gap-1">
                    <Badge color={BOOKING_STATUS_COLOR[b.status] ?? 'slate'}>
                      {b.status}
                    </Badge>
                    <p className="text-sm font-bold text-slate-700">
                      {formatCurrency(b.totalAmount)}
                    </p>
                  </div>
                </Link>
              ))}
            </div>
          )}
        </div>
      </div>
    </PageWrapper>
  );
}
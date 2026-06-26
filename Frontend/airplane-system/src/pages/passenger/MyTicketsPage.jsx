import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Ticket, Search } from 'lucide-react';
import PageWrapper   from '@/components/layout/PageWrapper';
import Table         from '@/components/ui/Table';
import Badge         from '@/components/ui/Badge';
import bookingService from '@/api/bookingService';
import ticketService  from '@/api/ticketService';
import { formatDateTime } from '@/utils/formatters';

export default function MyTicketsPage() {
  const [tickets,  setTickets]  = useState([]);
  const [loading,  setLoading]  = useState(true);
  const [search,   setSearch]   = useState('');

  useEffect(() => {
    // Fetch recent bookings, then collect all tickets
    bookingService
      .getMyBookings({ pageNumber: 1, pageSize: 20 })
      .then(async ({ data }) => {
        const confirmed = (data.items ?? []).filter((b) => b.status === 'Confirmed');
        const all = await Promise.all(
          confirmed.map((b) =>
            ticketService
              .getTicketsByBooking(b.id)
              .then((r) => r.data)
              .catch(() => [])
          )
        );
        setTickets(all.flat());
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  const filtered = tickets.filter(
    (t) =>
      t.ticketNumber?.toLowerCase().includes(search.toLowerCase()) ||
      t.passengerName?.toLowerCase().includes(search.toLowerCase())
  );

  const columns = [
    { key: 'ticketNumber', header: 'Ticket #' },
    { key: 'passengerName', header: 'Passenger' },
    {
      key: 'route',
      header: 'Route',
      render: (t) => `${t.originIata} → ${t.destinationIata}`,
    },
    { key: 'flightNumber', header: 'Flight' },
    {
      key: 'departureTime',
      header: 'Departure',
      render: (t) => formatDateTime(t.departureTime),
    },
    {
      key: 'checkedIn',
      header: 'Check-in',
      render: (t) => (
        <Badge color={t.isCheckedIn ? 'green' : 'yellow'}>
          {t.isCheckedIn ? 'Checked In' : 'Pending'}
        </Badge>
      ),
    },
    {
      key: 'actions',
      header: '',
      render: (t) => (
        <Link
          to={`/tickets/${t.ticketNumber}`}
          className="text-brand-600 hover:text-brand-700 text-sm font-medium"
        >
          View
        </Link>
      ),
    },
  ];

  return (
    <PageWrapper>
      <div className="page-container py-10">
        <div className="flex items-center justify-between mb-6 flex-wrap gap-4">
          <div>
            <h1 className="section-title">My Tickets</h1>
            <p className="text-slate-500 text-sm mt-1">{filtered.length} ticket(s) found</p>
          </div>
          <div className="relative w-full sm:w-64">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <input
              className="input-base pl-9"
              placeholder="Search tickets…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
        </div>
        <Table columns={columns} data={filtered} loading={loading}
          emptyMessage="No tickets found." />
      </div>
    </PageWrapper>
  );
}
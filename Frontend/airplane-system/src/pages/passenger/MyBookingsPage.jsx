import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { BookOpen, Search } from 'lucide-react';
import PageWrapper    from '@/components/layout/PageWrapper';
import Table          from '@/components/ui/Table';
import Pagination     from '@/components/ui/Pagination';
import Badge          from '@/components/ui/Badge';
import Input          from '@/components/ui/Input';
import bookingService from '@/api/bookingService';
import { usePagination } from '@/hooks/usePagination';
import { useDebounce }   from '@/hooks/useDebounce';
import { formatDateTime, formatCurrency } from '@/utils/formatters';
import { BOOKING_STATUS_COLOR }           from '@/utils/constants';

export default function MyBookingsPage() {
  const [bookings, setBookings] = useState([]);
  const [total,    setTotal]    = useState(0);
  const [loading,  setLoading]  = useState(true);
  const [search,   setSearch]   = useState('');
  const debSearch               = useDebounce(search);
  const { pageNumber, pageSize, goToPage, resetPage } = usePagination(10);

  useEffect(() => { resetPage(); }, [debSearch]);

  useEffect(() => {
    setLoading(true);
    bookingService
      .getMyBookings({ pageNumber, pageSize, searchTerm: debSearch })
      .then(({ data }) => {
        setBookings(data.items ?? []);
        setTotal(data.totalPages ?? 1);
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [pageNumber, pageSize, debSearch]);

  const columns = [
    { key: 'bookingReference', header: 'Reference' },
    {
      key: 'route',
      header: 'Route',
      render: (b) => (
        <span className="font-medium">
          {b.segments?.[0]?.originIata ?? '—'} →{' '}
          {b.segments?.[b.segments.length - 1]?.destinationIata ?? '—'}
        </span>
      ),
    },
    {
      key: 'departureTime',
      header: 'Departure',
      render: (b) => formatDateTime(b.segments?.[0]?.departureTime),
    },
    {
      key: 'status',
      header: 'Status',
      render: (b) => (
        <Badge color={BOOKING_STATUS_COLOR[b.status] ?? 'slate'}>
          {b.status}
        </Badge>
      ),
    },
    {
      key: 'totalAmount',
      header: 'Amount',
      render: (b) => formatCurrency(b.totalAmount),
    },
    {
      key: 'actions',
      header: '',
      render: (b) => (
        <Link
          to={`/bookings/${b.id}`}
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
            <h1 className="section-title">My Bookings</h1>
            <p className="text-slate-500 text-sm mt-1">All your flight bookings</p>
          </div>
          <div className="relative w-full sm:w-64">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <input
              className="input-base pl-9"
              placeholder="Search reference…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
        </div>

        <Table columns={columns} data={bookings} loading={loading}
          emptyMessage="You have no bookings yet." />
        <Pagination pageNumber={pageNumber} totalPages={total} onPage={goToPage} />
      </div>
    </PageWrapper>
  );
}
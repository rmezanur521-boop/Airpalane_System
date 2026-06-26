import { useCallback, useEffect, useState } from 'react';
import { Search, Eye } from 'lucide-react';
import { Link } from 'react-router-dom';
import Table          from '@/components/ui/Table';
import Pagination     from '@/components/ui/Pagination';
import Badge          from '@/components/ui/Badge';
import bookingService from '@/api/bookingService';
import { usePagination } from '@/hooks/usePagination';
import { useDebounce }   from '@/hooks/useDebounce';
import { formatDateTime, formatCurrency } from '@/utils/formatters';
import { BOOKING_STATUS_COLOR }           from '@/utils/constants';

export default function BookingsAdminPage() {
  const [bookings, setBookings] = useState([]);
  const [total,    setTotal]    = useState(1);
  const [loading,  setLoading]  = useState(true);
  const [search,   setSearch]   = useState('');
  const debSearch               = useDebounce(search);
  const { pageNumber, pageSize, goToPage, resetPage } = usePagination(15);

  useEffect(() => { resetPage(); }, [debSearch]);

  const load = useCallback(() => {
    setLoading(true);
    bookingService
      .getAllBookings({ pageNumber, pageSize, searchTerm: debSearch })
      .then(({ data }) => {
        setBookings(data.items ?? []);
        setTotal(data.totalPages ?? 1);
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [pageNumber, pageSize, debSearch]);

  useEffect(load, [load]);

  const columns = [
    { key: 'bookingReference', header: 'Reference' },
    {
      key: 'route',
      header: 'Route',
      render: (b) =>
        `${b.segments?.[0]?.originIata ?? '?'} → ${b.segments?.[b.segments.length - 1]?.destinationIata ?? '?'}`,
    },
    { key: 'tripType', header: 'Type' },
    {
      key: 'status',
      header: 'Status',
      render: (b) => (
        <Badge color={BOOKING_STATUS_COLOR[b.status] ?? 'slate'}>{b.status}</Badge>
      ),
    },
    {
      key: 'totalAmount',
      header: 'Amount',
      render: (b) => formatCurrency(b.totalAmount),
    },
    {
      key: 'createdAt',
      header: 'Created',
      render: (b) => formatDateTime(b.createdAt),
    },
    {
      key: 'actions',
      header: '',
      render: (b) => (
        <Link
          to={`/bookings/${b.id}`}
          className="p-1.5 rounded-lg text-slate-400 hover:text-brand-600 hover:bg-brand-50
                     transition inline-flex"
          title="View booking"
        >
          <Eye className="h-4 w-4" />
        </Link>
      ),
    },
  ];

  return (
    <div className="animate-fadeIn">
      <div className="flex items-center justify-between mb-6 flex-wrap gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-800">Bookings</h1>
          <p className="text-slate-500 text-sm mt-1">All system bookings</p>
        </div>
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
          <input
            className="input-base pl-9 w-56"
            placeholder="Search reference…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
      </div>
      <Table columns={columns} data={bookings} loading={loading} />
      <Pagination pageNumber={pageNumber} totalPages={total} onPage={goToPage} />
    </div>
  );
}
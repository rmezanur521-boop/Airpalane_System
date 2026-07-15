import { useCallback, useEffect, useState } from 'react';
import { Search, CheckCircle, XCircle } from 'lucide-react';
import Table          from '@/components/ui/Table';
import Pagination     from '@/components/ui/Pagination';
import Badge          from '@/components/ui/Badge';
import Button         from '@/components/ui/Button';
import Modal          from '@/components/ui/Modal';
import Input          from '@/components/ui/Input';
import Alert          from '@/components/ui/Alert';
import paymentService from '@/api/paymentService';
import { usePagination } from '@/hooks/usePagination';
import { useDebounce }   from '@/hooks/useDebounce';
import { formatDateTime, formatCurrency } from '@/utils/formatters';
import { PAYMENT_STATUS_COLOR }           from '@/utils/constants';
import toast from 'react-hot-toast';

export default function PaymentsAdminPage() {
  const [payments, setPayments] = useState([]);
  const [total,    setTotal]    = useState(1);
  const [loading,  setLoading]  = useState(true);
  const [search,   setSearch]   = useState('');
  const debSearch               = useDebounce(search);
  const { pageNumber, pageSize, goToPage, resetPage } = usePagination(15);

  // Refund process modal
  const [refundModal,  setRefundModal]  = useState(false);
  const [refundTarget, setRefundTarget] = useState(null);
  const [denyReason,   setDenyReason]   = useState('');
  const [processing,   setProcessing]   = useState(false);
  // Reference payment review modal state
  const [reviewModal,  setReviewModal]  = useState(false);
  const [reviewTarget, setReviewTarget] = useState(null);
  const [rejectReason, setRejectReason] = useState('');
  const [reviewing,    setReviewing]    = useState(false);

  useEffect(() => { resetPage(); }, [debSearch]);

  const handleApprovePayment = async (approve) => {
    setReviewing(true);
    try {
      await paymentService.approveReferencePayment(reviewTarget.id, {
        approve,
        rejectionReason: approve ? undefined : rejectReason,
      });
      toast.success(approve ? 'Payment approved.' : 'Payment rejected.');
      setReviewModal(false);
      load();
    } catch {
      toast.error('Failed to review payment.');
    } finally {
      setReviewing(false);
    }
  };

  const load = useCallback(() => {
    setLoading(true);
    paymentService
      .getAllPayments({ pageNumber, pageSize, searchTerm: debSearch })
      .then(({ data }) => {
        setPayments(data.items ?? []);
        setTotal(data.totalPages ?? 1);
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [pageNumber, pageSize, debSearch]);

  useEffect(load, [load]);

  const handleProcessRefund = async (approve) => {
    setProcessing(true);
    try {
      await paymentService.processRefund(refundTarget.id, {
        approve,
        denialReason: approve ? undefined : denyReason,
      });
      toast.success(approve ? 'Refund approved.' : 'Refund denied.');
      setRefundModal(false);
      load();
    } catch {
      toast.error('Failed to process refund.');
    } finally {
      setProcessing(false);
    }
  };

  const columns = [
  { key: 'bookingReference', header: 'Booking Ref' },
  {
    key: 'method',                          
    header: 'Method',
    render: (p) => <Badge color="slate">{p.method}</Badge>,
  },
  {
    key: 'amount',
    header: 'Amount',
    render: (p) => formatCurrency(p.amount),
  },
  { key: 'currencyCode', header: 'Currency' },
  {
    key: 'status',
    header: 'Status',
    render: (p) => (
      <Badge color={PAYMENT_STATUS_COLOR[p.status] ?? 'slate'}>{p.status}</Badge>
    ),
  },
  {
    key: 'referenceNumber',              
    header: 'Reference No.',
    render: (p) => p.referenceNumber ?? '—',
  },
  {
    key: 'paidAt',
    header: 'Paid At',
    render: (p) => formatDateTime(p.paidAt),
  },
  {
    key: 'actions',
    header: '',
    render: (p) => {
      if (p.status === 'Pending' && p.method === 'Stripe') {
        return (
          <Button size="sm" variant="secondary"
            onClick={() => { setRefundTarget(p); setDenyReason(''); setRefundModal(true); }}>
            Process Refund
          </Button>
        );
      }
      if (p.status === 'PendingApproval') {
        return (
          <Button size="sm" variant="primary"
            onClick={() => { setReviewTarget(p); setRejectReason(''); setReviewModal(true); }}>
            Review Payment
          </Button>
        );
      }
      return null;
    },
  },
];
  return (
    <div className="animate-fadeIn">
      <div className="flex items-center justify-between mb-6 flex-wrap gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-800">Payments</h1>
          <p className="text-slate-500 text-sm mt-1">All system payments & refunds</p>
        </div>
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
          <input
            className="input-base pl-9 w-56"
            placeholder="Search…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
      </div>

      <Table columns={columns} data={payments} loading={loading} />
      <Pagination pageNumber={pageNumber} totalPages={total} onPage={goToPage} />

      {/* Refund Modal */}
      <Modal
        open={refundModal}
        onClose={() => setRefundModal(false)}
        title="Process Refund"
      >
        <p className="text-sm text-slate-600 mb-4">
          Refund amount: <strong>{formatCurrency(refundTarget?.amount)}</strong> for booking{' '}
          <strong>{refundTarget?.bookingReference}</strong>
        </p>
        <Input
          label="Denial reason (only if denying)"
          value={denyReason}
          onChange={(e) => setDenyReason(e.target.value)}
          placeholder="Reason for denial…"
        />
        <div className="flex gap-3 justify-end mt-6">
          <Button
            variant="danger"
            loading={processing}
            onClick={() => handleProcessRefund(false)}
          >
            <XCircle className="h-4 w-4" /> Deny
          </Button>
          <Button
            loading={processing}
            onClick={() => handleProcessRefund(true)}
          >
            <CheckCircle className="h-4 w-4" /> Approve Refund
          </Button>
        </div>
      </Modal>
      {/* Reference Payment Review Modal */}
      <Modal open={reviewModal} onClose={() => setReviewModal(false)} title="Review Payment">
        <p className="text-sm text-slate-600 mb-4">
          Reference: <strong>{reviewTarget?.referenceNumber}</strong> for{' '}
          <strong>{formatCurrency(reviewTarget?.amount)}</strong>
        </p>
        <Input
          label="Rejection reason (only if rejecting)"
          value={rejectReason}
          onChange={(e) => setRejectReason(e.target.value)}
          placeholder="Reason for rejection…"
        />
        <div className="flex gap-3 justify-end mt-6">
          <Button variant="danger" loading={reviewing} onClick={() => handleApprovePayment(false)}>
            <XCircle className="h-4 w-4" /> Reject
          </Button>
          <Button loading={reviewing} onClick={() => handleApprovePayment(true)}>
            <CheckCircle className="h-4 w-4" /> Approve
          </Button>
        </div>
      </Modal>
    </div>
  );
}
import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Plane, Download, XCircle } from 'lucide-react';
import PageWrapper    from '@/components/layout/PageWrapper';
import Spinner        from '@/components/ui/Spinner';
import Badge          from '@/components/ui/Badge';
import Button         from '@/components/ui/Button';
import Alert          from '@/components/ui/Alert';
import Modal          from '@/components/ui/Modal';
import bookingService from '@/api/bookingService';
import ticketService  from '@/api/ticketService';
import paymentService from '@/api/paymentService';
import { formatDateTime, formatCurrency, downloadBlob } from '@/utils/formatters';
import { BOOKING_STATUS_COLOR, PAYMENT_STATUS_COLOR }   from '@/utils/constants';
import toast from 'react-hot-toast';

export default function BookingDetailPage() {
  const { id }                      = useParams();
  const navigate                    = useNavigate();
  const [booking,  setBooking]      = useState(null);
  const [tickets,  setTickets]      = useState([]);
  const [loading,  setLoading]      = useState(true);
  const [cancelModal, setCancelModal] = useState(false);
  const [cancelReason, setCancelReason] = useState('');
  const [cancelling, setCancelling]   = useState(false);
  const [refundModal, setRefundModal]  = useState(false);
  const [refundReason, setRefundReason] = useState('');
  const [refunding, setRefunding]       = useState(false);

  const load = () => {
    setLoading(true);
    Promise.all([
      bookingService.getBookingById(id),
      ticketService.getTicketsByBooking(id),
    ])
      .then(([bRes, tRes]) => {
        setBooking(bRes.data);
        setTickets(tRes.data ?? []);
      })
      .catch(() => navigate('/bookings', { replace: true }))
      .finally(() => setLoading(false));
  };

  useEffect(load, [id]);

  const handleCancel = async () => {
    setCancelling(true);
    try {
      await bookingService.cancelBooking(id, cancelReason);
      toast.success('Booking cancelled.');
      setCancelModal(false);
      load();
    } catch (err) {
      toast.error(err.response?.data?.detail ?? 'Cancel failed.');
    } finally {
      setCancelling(false);
    }
  };

  const handleRefund = async () => {
    setRefunding(true);
    try {
      await paymentService.requestRefund(id, refundReason);
      toast.success('Refund requested.');
      setRefundModal(false);
    } catch (err) {
      toast.error(err.response?.data?.detail ?? 'Refund request failed.');
    } finally {
      setRefunding(false);
    }
  };

  const handleDownload = async (ticketNumber) => {
    try {
      const { data } = await ticketService.downloadTicket(ticketNumber);
      downloadBlob(data, `ticket-${ticketNumber}.pdf`);
    } catch {
      toast.error('Download failed.');
    }
  };

  const handleBoardingPass = async (ticketNumber) => {
    try {
      const { data } = await ticketService.getBoardingPass(ticketNumber);
      downloadBlob(data, `boarding-pass-${ticketNumber}.pdf`);
    } catch {
      toast.error('Download failed.');
    }
  };

  if (loading) {
    return (
      <PageWrapper>
        <div className="flex justify-center py-32"><Spinner size="lg" /></div>
      </PageWrapper>
    );
  }

  if (!booking) return null;

  const canCancel = ['PendingPayment', 'Confirmed'].includes(booking.status);
  const canRefund = booking.status === 'Cancelled' &&
    booking.payment?.status === 'Succeeded';

  return (
    <PageWrapper>
      <div className="page-container py-10 max-w-3xl">
        {/* Header */}
        <div className="flex items-start justify-between mb-6 flex-wrap gap-4">
          <div>
            <p className="text-sm text-slate-400 mb-1">Booking Reference</p>
            <h1 className="text-3xl font-black text-slate-800 tracking-tight">
              {booking.bookingReference}
            </h1>
          </div>
          <Badge color={BOOKING_STATUS_COLOR[booking.status] ?? 'slate'} className="text-sm px-3 py-1">
            {booking.status}
          </Badge>
        </div>

        {/* Segments */}
        <div className="card mb-4">
          <h2 className="font-semibold text-slate-700 mb-4">Flight Segments</h2>
          <div className="flex flex-col gap-4">
            {(booking.segments ?? []).map((s, i) => (
              <div key={s.id} className="flex items-center gap-4 p-4 rounded-xl bg-slate-50">
                <div className="h-10 w-10 rounded-xl bg-brand-50 flex items-center justify-center">
                  <Plane className="h-5 w-5 text-brand-500" />
                </div>
                <div className="flex-1">
                  <p className="font-semibold text-slate-800">
                    {s.originIata} → {s.destinationIata}
                  </p>
                  <p className="text-xs text-slate-400 mt-0.5">
                    {s.flightNumber} · {formatDateTime(s.departureTime)} → {formatDateTime(s.arrivalTime)}
                  </p>
                </div>
                <div className="text-right text-sm">
                  <p className="font-medium text-slate-700">{formatCurrency(s.segmentTotal)}</p>
                  <p className="text-xs text-slate-400">{s.seatClass}</p>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Passengers */}
        <div className="card mb-4">
          <h2 className="font-semibold text-slate-700 mb-4">Passengers</h2>
          <div className="flex flex-col gap-2">
            {(booking.passengers ?? []).map((p) => (
              <div key={p.id} className="flex items-center justify-between py-2
                border-b border-slate-50 last:border-0">
                <div>
                  <p className="text-sm font-medium text-slate-800">{p.fullName}</p>
                  <p className="text-xs text-slate-400">{p.passengerType}</p>
                </div>
                <p className="text-sm text-slate-600">
                  Seat: {p.seatNumber ?? 'Not selected'}
                </p>
              </div>
            ))}
          </div>
        </div>

        {/* Payment */}
        {booking.payment && (
          <div className="card mb-4">
            <h2 className="font-semibold text-slate-700 mb-4">Payment</h2>
            <div className="flex flex-col gap-2 text-sm">
              <Row label="Status">
                <Badge color={PAYMENT_STATUS_COLOR[booking.payment.status] ?? 'slate'}>
                  {booking.payment.status}
                </Badge>
              </Row>
              <Row label="Amount">
                <span className="font-bold text-slate-800">
                  {formatCurrency(booking.payment.amount)}
                </span>
              </Row>
              {booking.payment.paidAt && (
                <Row label="Paid at">{formatDateTime(booking.payment.paidAt)}</Row>
              )}
              {booking.payment.receiptUrl && (
                <Row label="Receipt">
                  <a href={booking.payment.receiptUrl} target="_blank" rel="noreferrer"
                    className="text-brand-600 hover:underline text-sm">
                    View receipt
                  </a>
                </Row>
              )}
            </div>
          </div>
        )}

        {/* Tickets */}
        {tickets.length > 0 && (
          <div className="card mb-6">
            <h2 className="font-semibold text-slate-700 mb-4">Tickets</h2>
            <div className="flex flex-col gap-3">
              {tickets.map((t) => (
                <div key={t.id}
                  className="flex items-center justify-between p-3 rounded-xl bg-slate-50">
                  <div>
                    <p className="text-sm font-semibold text-slate-800">{t.ticketNumber}</p>
                    <p className="text-xs text-slate-400">{t.passengerName}</p>
                  </div>
                  <div className="flex gap-2">
                    <Button size="sm" variant="secondary"
                      onClick={() => handleBoardingPass(t.ticketNumber)}>
                      <Download className="h-3 w-3" />
                      Boarding Pass
                    </Button>
                    <Button size="sm" variant="secondary"
                      onClick={() => handleDownload(t.ticketNumber)}>
                      <Download className="h-3 w-3" />
                      Ticket
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Actions */}
        <div className="flex gap-3 flex-wrap">
          {canCancel && (
            <Button variant="danger" onClick={() => setCancelModal(true)}>
              <XCircle className="h-4 w-4" />
              Cancel Booking
            </Button>
          )}
          {canRefund && (
            <Button variant="secondary" onClick={() => setRefundModal(true)}>
              Request Refund
            </Button>
          )}
        </div>
      </div>

      {/* Cancel Modal */}
      <Modal open={cancelModal} onClose={() => setCancelModal(false)} title="Cancel Booking">
        <Alert type="warning" message="This action cannot be undone." className="mb-4" />
        <div className="flex flex-col gap-1 mb-6">
          <label className="text-sm font-medium text-slate-700">Reason (optional)</label>
          <textarea
            className="input-base min-h-[80px] resize-none"
            value={cancelReason}
            onChange={(e) => setCancelReason(e.target.value)}
            placeholder="Tell us why you're cancelling…"
          />
        </div>
        <div className="flex gap-3 justify-end">
          <Button variant="secondary" onClick={() => setCancelModal(false)}>
            Keep Booking
          </Button>
          <Button variant="danger" loading={cancelling} onClick={handleCancel}>
            Yes, Cancel
          </Button>
        </div>
      </Modal>

      {/* Refund Modal */}
      <Modal open={refundModal} onClose={() => setRefundModal(false)} title="Request Refund">
        <div className="flex flex-col gap-1 mb-6">
          <label className="text-sm font-medium text-slate-700">Reason</label>
          <textarea
            className="input-base min-h-[80px] resize-none"
            value={refundReason}
            onChange={(e) => setRefundReason(e.target.value)}
            placeholder="Reason for refund…"
          />
        </div>
        <div className="flex gap-3 justify-end">
          <Button variant="secondary" onClick={() => setRefundModal(false)}>Cancel</Button>
          <Button loading={refunding} onClick={handleRefund}>Submit Request</Button>
        </div>
      </Modal>
    </PageWrapper>
  );
}

function Row({ label, children }) {
  return (
    <div className="flex items-center justify-between py-1.5 border-b border-slate-50 last:border-0">
      <span className="text-slate-500">{label}</span>
      <span>{children}</span>
    </div>
  );
}
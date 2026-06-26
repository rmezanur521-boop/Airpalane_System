import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { Download, QrCode, CheckCircle } from 'lucide-react';
import PageWrapper   from '@/components/layout/PageWrapper';
import Spinner       from '@/components/ui/Spinner';
import Badge         from '@/components/ui/Badge';
import Button        from '@/components/ui/Button';
import ticketService from '@/api/ticketService';
import { formatDateTime, downloadBlob } from '@/utils/formatters';
import toast from 'react-hot-toast';

export default function TicketDetailPage() {
  const { ticketNumber }          = useParams();
  const [ticket,   setTicket]     = useState(null);
  const [loading,  setLoading]    = useState(true);
  const [checkingIn, setCheckingIn] = useState(false);

  const load = () => {
    ticketService
      .getTicketByNumber(ticketNumber)
      .then(({ data }) => setTicket(data))
      .catch(() => {})
      .finally(() => setLoading(false));
  };

  useEffect(load, [ticketNumber]);

  const handleCheckIn = async () => {
    setCheckingIn(true);
    try {
      await ticketService.checkIn(ticketNumber);
      toast.success('Checked in successfully!');
      load();
    } catch (err) {
      toast.error(err.response?.data?.detail ?? 'Check-in failed.');
    } finally {
      setCheckingIn(false);
    }
  };

  const handleDownload = async (type) => {
    try {
      const { data } = type === 'boarding'
        ? await ticketService.getBoardingPass(ticketNumber)
        : await ticketService.downloadTicket(ticketNumber);
      downloadBlob(data, `${type}-${ticketNumber}.pdf`);
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

  if (!ticket) return null;

  return (
    <PageWrapper>
      <div className="page-container py-10 max-w-2xl">
        {/* Ticket card */}
        <div className="bg-white rounded-3xl shadow-xl overflow-hidden border border-slate-100">
          {/* Top band */}
          <div className="bg-gradient-to-r from-brand-600 to-sky-500 p-6 text-white">
            <div className="flex items-center justify-between mb-4">
              <div>
                <p className="text-brand-100 text-sm">Ticket Number</p>
                <p className="text-2xl font-black tracking-widest">{ticket.ticketNumber}</p>
              </div>
              <Badge color={ticket.isCheckedIn ? 'green' : 'yellow'} className="text-sm">
                {ticket.isCheckedIn ? 'Checked In' : 'Not Checked In'}
              </Badge>
            </div>
            <div className="flex items-end justify-between">
              <div>
                <p className="text-4xl font-black">{ticket.originIata}</p>
                <p className="text-brand-200 text-sm mt-1">{formatDateTime(ticket.departureTime)}</p>
              </div>
              <div className="text-center text-brand-200">
                <div className="text-2xl">✈</div>
                <div className="text-xs mt-1">{ticket.airlineName}</div>
              </div>
              <div className="text-right">
                <p className="text-4xl font-black">{ticket.destinationIata}</p>
                <p className="text-brand-200 text-sm mt-1">{formatDateTime(ticket.arrivalTime)}</p>
              </div>
            </div>
          </div>

          {/* Dotted divider */}
          <div className="flex items-center px-6">
            <div className="h-6 w-6 rounded-full bg-slate-50 border border-slate-100 -ml-9 flex-shrink-0" />
            <div className="flex-1 border-t-2 border-dashed border-slate-200" />
            <div className="h-6 w-6 rounded-full bg-slate-50 border border-slate-100 -mr-9 flex-shrink-0" />
          </div>

          {/* Details */}
          <div className="p-6 grid grid-cols-2 sm:grid-cols-3 gap-4">
            <Detail label="Passenger"  value={ticket.passengerName} />
            <Detail label="Flight"     value={ticket.flightNumber} />
            <Detail label="Airline"    value={ticket.airlineName} />
            <Detail label="Class"      value={ticket.seatClass} />
            <Detail label="Seat"       value={ticket.seatNumber ?? 'TBA'} />
            <Detail label="Booking Ref" value={ticket.bookingReference} />
            {ticket.isCheckedIn && (
              <Detail label="Checked In" value={formatDateTime(ticket.checkedInAt)} />
            )}
          </div>

          {/* QR placeholder */}
          <div className="flex justify-center pb-6">
            <div className="h-24 w-24 bg-slate-100 rounded-xl flex items-center justify-center">
              <QrCode className="h-12 w-12 text-slate-300" />
            </div>
          </div>
        </div>

        {/* Actions */}
        <div className="flex gap-3 mt-6 flex-wrap">
          {!ticket.isCheckedIn && (
            <Button onClick={handleCheckIn} loading={checkingIn}>
              <CheckCircle className="h-4 w-4" />
              Check In
            </Button>
          )}
          <Button variant="secondary" onClick={() => handleDownload('boarding')}>
            <Download className="h-4 w-4" />
            Boarding Pass
          </Button>
          <Button variant="secondary" onClick={() => handleDownload('ticket')}>
            <Download className="h-4 w-4" />
            Download Ticket
          </Button>
        </div>
      </div>
    </PageWrapper>
  );
}

function Detail({ label, value }) {
  return (
    <div>
      <p className="text-xs text-slate-400 uppercase tracking-wider mb-0.5">{label}</p>
      <p className="text-sm font-semibold text-slate-800">{value ?? '—'}</p>
    </div>
  );
}
import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { Download, QrCode, CheckCircle, ArrowLeft, Plane, ShieldCheck, Clock, Luggage } from 'lucide-react';
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
  const [downloading, setDownloading] = useState(null);

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
    setDownloading(type);
    try {
      const { data } = type === 'boarding'
        ? await ticketService.getBoardingPass(ticketNumber)
        : await ticketService.downloadTicket(ticketNumber);
      downloadBlob(data, `${type}-${ticketNumber}.pdf`);
      toast.success(`${type === 'boarding' ? 'Boarding Pass' : 'E-Ticket'} downloaded!`);
    } catch {
      toast.error('Download failed.');
    } finally {
      setDownloading(null);
    }
  };

  if (loading) {
    return (
      <PageWrapper>
        <div className="flex justify-center py-32"><Spinner size="lg" /></div>
      </PageWrapper>
    );
  }

  if (!ticket) {
    return (
      <PageWrapper>
        <div className="page-container py-16 text-center">
          <p className="text-slate-500 mb-4">Ticket not found or inaccessible.</p>
          <Link to="/tickets" className="text-brand-600 font-semibold hover:underline">
            Back to My Tickets
          </Link>
        </div>
      </PageWrapper>
    );
  }

  const qrData = `TKT:${ticket.ticketNumber}|PNR:${ticket.bookingReference}|FLT:${ticket.flightNumber}|PAX:${ticket.passengerName}`;
  const qrUrl = `https://api.qrserver.com/v1/create-qr-code/?size=180x180&data=${encodeURIComponent(qrData)}`;

  return (
    <PageWrapper>
      <div className="page-container py-10 max-w-3xl">
        <div className="mb-6 flex items-center justify-between">
          <Link to="/tickets" className="inline-flex items-center gap-1.5 text-sm text-slate-500 hover:text-slate-800 transition-colors font-medium">
            <ArrowLeft className="h-4 w-4" />
            Back to Tickets
          </Link>
          <div className="flex items-center gap-2">
            <Badge color={ticket.isCheckedIn ? 'green' : 'yellow'} className="text-sm px-3 py-1">
              {ticket.isCheckedIn ? 'Checked In' : 'Not Checked In'}
            </Badge>
          </div>
        </div>

        {/* E-Ticket Card - Styled in Brand Emerald */}
        <div className="bg-white rounded-3xl shadow-xl overflow-hidden border border-slate-200/80">
          {/* Top Flight Banner */}
          <div className="bg-gradient-to-r from-brand-700 via-brand-600 to-emerald-600 p-6 sm:p-8 text-white relative overflow-hidden">
            <div className="absolute right-0 top-0 translate-x-8 -translate-y-6 opacity-10 pointer-events-none">
              <Plane className="w-56 h-56 text-white" />
            </div>

            <div className="flex flex-wrap items-center justify-between gap-4 mb-6 relative z-10">
              <div>
                <p className="text-brand-100 text-xs font-semibold uppercase tracking-wider">Electronic Ticket Receipt</p>
                <p className="text-2xl sm:text-3xl font-black tracking-widest">{ticket.ticketNumber}</p>
              </div>
              <div className="bg-white/15 backdrop-blur-md px-3.5 py-1.5 rounded-xl border border-white/20">
                <p className="text-xs text-brand-100">Booking Reference (PNR)</p>
                <p className="text-base font-black tracking-wider text-white text-right">{ticket.bookingReference}</p>
              </div>
            </div>

            <div className="grid grid-cols-3 items-center relative z-10 pt-2">
              <div>
                <p className="text-3xl sm:text-5xl font-black">{ticket.originIata}</p>
                <p className="text-brand-100 text-xs sm:text-sm font-medium mt-1">{formatDateTime(ticket.departureTime)}</p>
              </div>
              <div className="text-center text-brand-200">
                <div className="inline-flex items-center justify-center p-2 rounded-full bg-white/10 backdrop-blur-sm border border-white/10">
                  <Plane className="h-6 w-6 text-white rotate-90" />
                </div>
                <div className="text-xs font-semibold text-white mt-1.5 tracking-wide">{ticket.airlineName}</div>
                <div className="text-[11px] text-brand-200">Flight {ticket.flightNumber}</div>
              </div>
              <div className="text-right">
                <p className="text-3xl sm:text-5xl font-black">{ticket.destinationIata}</p>
                <p className="text-brand-100 text-xs sm:text-sm font-medium mt-1">{formatDateTime(ticket.arrivalTime)}</p>
              </div>
            </div>
          </div>

          {/* Dotted Perforated Cut line */}
          <div className="flex items-center px-6 -my-3 relative z-10">
            <div className="h-6 w-6 rounded-full bg-slate-100 border border-slate-200 -ml-9 flex-shrink-0" />
            <div className="flex-1 border-t-2 border-dashed border-slate-300" />
            <div className="h-6 w-6 rounded-full bg-slate-100 border border-slate-200 -mr-9 flex-shrink-0" />
          </div>

          {/* Passenger & Ticket Details Grid */}
          <div className="p-6 sm:p-8 bg-white">
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-y-5 gap-x-4 mb-6">
              <Detail label="Passenger Name" value={ticket.passengerName} highlight />
              <Detail label="Cabin Class"    value={ticket.seatClass} />
              <Detail label="Seat Number"    value={ticket.seatNumber ?? 'Assigned at Check-in'} />
              <Detail label="Issue Date"     value={formatDateTime(ticket.issuedAt)} />
            </div>

            {/* Verification & QR Code Bar */}
            <div className="mt-6 pt-6 border-t border-slate-100 bg-slate-50/70 rounded-2xl p-5 flex flex-col sm:flex-row items-center justify-between gap-6">
              <div className="flex items-center gap-4">
                <div className="p-2 bg-white rounded-xl shadow-sm border border-slate-200">
                  <img
                    src={qrUrl}
                    alt="Ticket QR Code"
                    className="w-24 h-24 object-contain"
                    onError={(e) => {
                      // Fallback if offline
                      e.target.style.display = 'none';
                      e.target.parentNode.innerHTML = '<div class="w-24 h-24 flex items-center justify-center text-slate-400"><svg class="w-12 h-12" .../></div>';
                    }}
                  />
                </div>
                <div className="space-y-1">
                  <div className="inline-flex items-center gap-1.5 text-xs font-semibold text-emerald-700 bg-emerald-50 px-2 py-0.5 rounded-md border border-emerald-200">
                    <ShieldCheck className="h-3.5 w-3.5" />
                    Verified Official E-Ticket
                  </div>
                  <p className="text-xs text-slate-500">Scan at immigration, security, and gate scanners.</p>
                  <p className="text-[11px] text-slate-400 font-mono">PNR: {ticket.bookingReference} • TKT: {ticket.ticketNumber}</p>
                </div>
              </div>

              <div className="flex sm:flex-col gap-2 w-full sm:w-auto">
                <Button
                  variant="primary"
                  className="flex-1 sm:flex-none justify-center"
                  onClick={() => handleDownload('ticket')}
                  loading={downloading === 'ticket'}
                >
                  <Download className="h-4 w-4" />
                  E-Ticket (A4 PDF)
                </Button>
                <Button
                  variant="secondary"
                  className="flex-1 sm:flex-none justify-center"
                  onClick={() => handleDownload('boarding')}
                  loading={downloading === 'boarding'}
                >
                  <Download className="h-4 w-4" />
                  Boarding Pass (PDF)
                </Button>
              </div>
            </div>

            {/* Travel notice */}
            <div className="mt-5 grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs text-slate-500">
              <div className="flex items-start gap-2 bg-brand-50/50 border border-brand-100 rounded-xl p-3">
                <Clock className="h-4 w-4 text-brand-600 flex-shrink-0 mt-0.5" />
                <span>Check-in counters close strictly 60 minutes prior to departure. Boarding gate opens 45 mins prior.</span>
              </div>
              <div className="flex items-start gap-2 bg-brand-50/50 border border-brand-100 rounded-xl p-3">
                <Luggage className="h-4 w-4 text-brand-600 flex-shrink-0 mt-0.5" />
                <span>Standard baggage allowance included. Dangerous goods & power banks are cabin baggage only.</span>
              </div>
            </div>
          </div>

          {/* Bottom Action Footer */}
          {!ticket.isCheckedIn && (
            <div className="bg-slate-50 px-6 sm:px-8 py-4 border-t border-slate-100 flex items-center justify-between">
              <div>
                <p className="text-sm font-semibold text-slate-800">Online Check-in Available</p>
                <p className="text-xs text-slate-500">Confirm your seat and proceed straight to the gate.</p>
              </div>
              <Button onClick={handleCheckIn} loading={checkingIn}>
                <CheckCircle className="h-4 w-4" />
                Check In Now
              </Button>
            </div>
          )}
        </div>
      </div>
    </PageWrapper>
  );
}

function Detail({ label, value, highlight }) {
  return (
    <div>
      <p className="text-[11px] text-slate-400 uppercase tracking-wider mb-0.5 font-semibold">{label}</p>
      <p className={`text-sm font-bold ${highlight ? 'text-slate-900' : 'text-slate-700'}`}>{value ?? '—'}</p>
    </div>
  );
}

import { useCallback, useEffect, useState } from 'react';
import { Plus, Search, Edit2, Trash2, AlertTriangle } from 'lucide-react';
import Table        from '@/components/ui/Table';
import Pagination   from '@/components/ui/Pagination';
import Badge        from '@/components/ui/Badge';
import Button       from '@/components/ui/Button';
import Modal        from '@/components/ui/Modal';
import Input        from '@/components/ui/Input';
import Select       from '@/components/ui/Select';
import Alert        from '@/components/ui/Alert';
import flightService  from '@/api/flightService';
import airlineService from '@/api/airlineService';
import airportService from '@/api/airportService';
import routeService   from '@/api/routeService';
import adminService   from '@/api/adminService';import { usePagination } from '@/hooks/usePagination';
import { useDebounce }   from '@/hooks/useDebounce';
import { formatDateTime, formatCurrency } from '@/utils/formatters';
import { FLIGHT_STATUS_COLOR, FLIGHT_STATUS } from '@/utils/constants';
import toast from 'react-hot-toast';

const EMPTY_FORM = {
  flightNumber: '', airlineId: '', aircraftId: '', routeId: '',
  departureTime: '', arrivalTime: '',
  economyBasePrice: '', businessBasePrice: '', firstClassBasePrice: '',
  airportFee: '', taxPercentage: '', gateNumber: '',
};

const STATUS_OPTIONS = Object.values(FLIGHT_STATUS).map((s) => ({ value: s, label: s }));

export default function FlightsPage() {
  const [flights,   setFlights]   = useState([]);
  const [total,     setTotal]     = useState(1);
  const [loading,   setLoading]   = useState(true);
  const [search,    setSearch]    = useState('');
  const debSearch                 = useDebounce(search);
  const { pageNumber, pageSize, goToPage, resetPage } = usePagination(10);

  const [airlines,  setAirlines]  = useState([]);
  const [airports,  setAirports]  = useState([]);
  const [routes,    setRoutes]    = useState([]);
  // Modal state
  const [formModal,   setFormModal]   = useState(false);
  const [editFlight,  setEditFlight]  = useState(null);
  const [form,        setForm]        = useState(EMPTY_FORM);
  const [saving,      setSaving]      = useState(false);
  const [formError,   setFormError]   = useState('');

  // Status modal
  const [statusModal,  setStatusModal]  = useState(false);
  const [statusTarget, setStatusTarget] = useState(null);
  const [statusForm,   setStatusForm]   = useState({ status: '', gateNumber: '', delayReason: '' });
  const [updatingStatus, setUpdatingStatus] = useState(false);

  // Delete modal
  const [deleteModal,  setDeleteModal]  = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleting,     setDeleting]     = useState(false);

  // Alert modal
  const [alertModal,  setAlertModal]  = useState(false);
  const [alertTarget, setAlertTarget] = useState(null);
  const [alertCancel, setAlertCancel] = useState(false);
  const [sendingAlert, setSendingAlert] = useState(false);

  useEffect(() => {
  airlineService.getAirlines().then(({ data }) =>
    setAirlines((data ?? []).map((a) => ({ value: a.id, label: `${a.name} (${a.iataCode})` })))
  ).catch(() => {});
  airportService.getAirports({ pageSize: 200 }).then(({ data }) =>
    setAirports((data.items ?? []).map((a) => ({ value: a.iataCode, label: `${a.name} (${a.iataCode})` })))
  ).catch(() => {});
  routeService.getRoutes().then(({ data }) =>
    setRoutes((data ?? []).map((r) => ({ value: r.id, label: r.name })))
  ).catch(() => {});
}, []);

  useEffect(() => { resetPage(); }, [debSearch]);

  const load = useCallback(() => {
    setLoading(true);
    flightService
      .getFlights({ pageNumber, pageSize, searchTerm: debSearch })
      .then(({ data }) => {
        setFlights(data.items ?? []);
        setTotal(data.totalPages ?? 1);
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [pageNumber, pageSize, debSearch]);

  useEffect(load, [load]);

  const openCreate = () => {
    setEditFlight(null);
    setForm(EMPTY_FORM);
    setFormError('');
    setFormModal(true);
  };

  const openEdit = (f) => {
  setEditFlight(f);
  setForm({
    flightNumber:       f.flightNumber ?? '',
    airlineId:          f.airlineId  ?? '',
    aircraftId:         f.aircraftId ?? '',
    routeId:            f.routeId    ?? '',
    departureTime:      f.departureTime?.slice(0, 16) ?? '',
      arrivalTime:        f.arrivalTime?.slice(0, 16)   ?? '',
      economyBasePrice:   f.economyBasePrice   ?? '',
      businessBasePrice:  f.businessBasePrice  ?? '',
      firstClassBasePrice: f.firstClassBasePrice ?? '',
      airportFee:         f.airportFee         ?? '',
      taxPercentage:      f.taxPercentage       ?? '',
      gateNumber:         f.gateNumber          ?? '',
    });
    setFormError('');
    setFormModal(true);
  };

  const handleSave = async () => {
    setFormError('');
    setSaving(true);
    try {
      const payload = {
        ...form,
        economyBasePrice:    +form.economyBasePrice,
        businessBasePrice:   +form.businessBasePrice,
        firstClassBasePrice: +form.firstClassBasePrice,
        airportFee:          +form.airportFee,
        taxPercentage:       +form.taxPercentage,
        departureTime:       form.departureTime ? new Date(form.departureTime).toISOString() : '',
        arrivalTime:         form.arrivalTime   ? new Date(form.arrivalTime).toISOString()   : '',
      };
      if (editFlight) {
        await flightService.updateFlight(editFlight.id, payload);
        toast.success('Flight updated.');
      } else {
        await flightService.createFlight(payload);
        toast.success('Flight created.');
      }
      setFormModal(false);
      load();
    } catch (err) {
      setFormError(err.response?.data?.detail ?? 'Save failed.');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    setDeleting(true);
    try {
      await flightService.deleteFlight(deleteTarget.id);
      toast.success('Flight deleted.');
      setDeleteModal(false);
      load();
    } catch {
      toast.error('Delete failed.');
    } finally {
      setDeleting(false);
    }
  };

  const handleStatusUpdate = async () => {
    setUpdatingStatus(true);
    try {
      await flightService.updateFlightStatus(statusTarget.id, statusForm);
      toast.success('Status updated.');
      setStatusModal(false);
      load();
    } catch {
      toast.error('Status update failed.');
    } finally {
      setUpdatingStatus(false);
    }
  };

  const handleSendAlert = async () => {
    setSendingAlert(true);
    try {
      await adminService.sendFlightAlert(alertTarget.id, alertCancel);
      toast.success('Alert sent to passengers.');
      setAlertModal(false);
    } catch {
      toast.error('Failed to send alert.');
    } finally {
      setSendingAlert(false);
    }
  };

  const setField = (k) => (e) => setForm((p) => ({ ...p, [k]: e.target.value }));

  const columns = [
    { key: 'flightNumber', header: 'Flight #' },
    {
      key: 'route',
      header: 'Route',
      render: (f) => (
        <span className="font-medium">
          {f.originIata} → {f.destinationIata}
        </span>
      ),
    },
    { key: 'airlineName', header: 'Airline' },
    {
      key: 'departureTime',
      header: 'Departure',
      render: (f) => formatDateTime(f.departureTime),
    },
    {
      key: 'status',
      header: 'Status',
      render: (f) => (
        <Badge color={FLIGHT_STATUS_COLOR[f.status] ?? 'slate'}>{f.status}</Badge>
      ),
    },
    {
      key: 'economyBasePrice',
      header: 'Economy',
      render: (f) => formatCurrency(f.economyBasePrice),
    },
    {
      key: 'actions',
      header: '',
      render: (f) => (
        <div className="flex items-center gap-1">
          <button
            onClick={() => {
              setStatusTarget(f);
              setStatusForm({ status: f.status, gateNumber: f.gateNumber ?? '', delayReason: '' });
              setStatusModal(true);
            }}
            className="p-1.5 rounded-lg text-slate-400 hover:text-brand-600 hover:bg-brand-50 transition"
            title="Update Status"
          >
            <AlertTriangle className="h-4 w-4" />
          </button>
          <button
            onClick={() => { setAlertTarget(f); setAlertCancel(false); setAlertModal(true); }}
            className="p-1.5 rounded-lg text-slate-400 hover:text-yellow-600 hover:bg-yellow-50 transition text-xs font-bold"
            title="Send Alert"
          >
            ✉
          </button>
          <button
            onClick={() => openEdit(f)}
            className="p-1.5 rounded-lg text-slate-400 hover:text-brand-600 hover:bg-brand-50 transition"
          >
            <Edit2 className="h-4 w-4" />
          </button>
          <button
            onClick={() => { setDeleteTarget(f); setDeleteModal(true); }}
            className="p-1.5 rounded-lg text-slate-400 hover:text-red-600 hover:bg-red-50 transition"
          >
            <Trash2 className="h-4 w-4" />
          </button>
        </div>
      ),
    },
  ];

  return (
    <div className="animate-fadeIn">
      {/* Header */}
      <div className="flex items-center justify-between mb-6 flex-wrap gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-800">Flights</h1>
          <p className="text-slate-500 text-sm mt-1">Manage all flights</p>
        </div>
        <div className="flex items-center gap-3">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <input
              className="input-base pl-9 w-56"
              placeholder="Search flights…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
          <Button onClick={openCreate}>
            <Plus className="h-4 w-4" /> Add Flight
          </Button>
        </div>
      </div>

      <Table columns={columns} data={flights} loading={loading} />
      <Pagination pageNumber={pageNumber} totalPages={total} onPage={goToPage} />

      {/* ── Create / Edit Modal ─────────────────────────────────────────── */}
      <Modal
        open={formModal}
        onClose={() => setFormModal(false)}
        title={editFlight ? 'Edit Flight' : 'Add Flight'}
        size="lg"
      >
        {formError && <Alert type="error" message={formError} className="mb-4" />}
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Input label="Flight Number" value={form.flightNumber}
            onChange={setField('flightNumber')} placeholder="AA123" required />
          <Select label="Airline" options={airlines} value={form.airlineId}
            onChange={setField('airlineId')} placeholder="Select airline" />
          <Input label="Aircraft ID (UUID)" value={form.aircraftId}
            onChange={setField('aircraftId')} placeholder="UUID" />
          <Select label="Route" options={routes} value={form.routeId}
            onChange={setField('routeId')} placeholder="Select route" />
          <Input label="Departure Time" type="datetime-local" value={form.departureTime}
            onChange={setField('departureTime')} required />
          <Input label="Arrival Time" type="datetime-local" value={form.arrivalTime}
            onChange={setField('arrivalTime')} required />
          <Input label="Economy Price" type="number" value={form.economyBasePrice}
            onChange={setField('economyBasePrice')} placeholder="0.00" />
          <Input label="Business Price" type="number" value={form.businessBasePrice}
            onChange={setField('businessBasePrice')} placeholder="0.00" />
          <Input label="First Class Price" type="number" value={form.firstClassBasePrice}
            onChange={setField('firstClassBasePrice')} placeholder="0.00" />
          <Input label="Airport Fee" type="number" value={form.airportFee}
            onChange={setField('airportFee')} placeholder="0.00" />
          <Input label="Tax %" type="number" value={form.taxPercentage}
            onChange={setField('taxPercentage')} placeholder="0" />
          <Input label="Gate Number" value={form.gateNumber}
            onChange={setField('gateNumber')} placeholder="A12" />
        </div>
        <div className="flex gap-3 justify-end mt-6">
          <Button variant="secondary" onClick={() => setFormModal(false)}>Cancel</Button>
          <Button loading={saving} onClick={handleSave}>
            {editFlight ? 'Save Changes' : 'Create Flight'}
          </Button>
        </div>
      </Modal>

      {/* ── Status Modal ─────────────────────────────────────────────────── */}
      <Modal
        open={statusModal}
        onClose={() => setStatusModal(false)}
        title={`Update Status — ${statusTarget?.flightNumber}`}
      >
        <div className="flex flex-col gap-4">
          <Select
            label="New Status"
            options={STATUS_OPTIONS}
            value={statusForm.status}
            onChange={(e) => setStatusForm((p) => ({ ...p, status: e.target.value }))}
          />
          <Input
            label="Gate Number"
            value={statusForm.gateNumber}
            onChange={(e) => setStatusForm((p) => ({ ...p, gateNumber: e.target.value }))}
            placeholder="A12"
          />
          {statusForm.status === 'Delayed' && (
            <Input
              label="Delay Reason"
              value={statusForm.delayReason}
              onChange={(e) => setStatusForm((p) => ({ ...p, delayReason: e.target.value }))}
              placeholder="Weather, technical issue…"
            />
          )}
        </div>
        <div className="flex gap-3 justify-end mt-6">
          <Button variant="secondary" onClick={() => setStatusModal(false)}>Cancel</Button>
          <Button loading={updatingStatus} onClick={handleStatusUpdate}>Update Status</Button>
        </div>
      </Modal>

      {/* ── Delete Modal ─────────────────────────────────────────────────── */}
      <Modal open={deleteModal} onClose={() => setDeleteModal(false)} title="Delete Flight">
        <Alert
          type="error"
          message={`Are you sure you want to delete flight ${deleteTarget?.flightNumber}? This cannot be undone.`}
        />
        <div className="flex gap-3 justify-end mt-6">
          <Button variant="secondary" onClick={() => setDeleteModal(false)}>Cancel</Button>
          <Button variant="danger" loading={deleting} onClick={handleDelete}>Delete</Button>
        </div>
      </Modal>

      {/* ── Alert Modal ──────────────────────────────────────────────────── */}
      <Modal open={alertModal} onClose={() => setAlertModal(false)} title="Send Flight Alert">
        <p className="text-sm text-slate-600 mb-4">
          Send a notification to all passengers on flight{' '}
          <strong>{alertTarget?.flightNumber}</strong>.
        </p>
        <label className="flex items-center gap-3 cursor-pointer">
          <input
            type="checkbox"
            checked={alertCancel}
            onChange={(e) => setAlertCancel(e.target.checked)}
            className="h-4 w-4 rounded border-slate-300 text-brand-600"
          />
          <span className="text-sm text-slate-700">This is a cancellation alert</span>
        </label>
        <div className="flex gap-3 justify-end mt-6">
          <Button variant="secondary" onClick={() => setAlertModal(false)}>Cancel</Button>
          <Button loading={sendingAlert} onClick={handleSendAlert}>Send Alert</Button>
        </div>
      </Modal>
    </div>
  );
}
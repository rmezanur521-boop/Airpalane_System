import { useCallback, useEffect, useState } from 'react';
import { Plus, Edit2, Trash2, Search } from 'lucide-react';
import Table          from '@/components/ui/Table';
import Pagination     from '@/components/ui/Pagination';
import Button         from '@/components/ui/Button';
import Modal          from '@/components/ui/Modal';
import Input          from '@/components/ui/Input';
import Alert          from '@/components/ui/Alert';
import airportService from '@/api/airportService';
import { usePagination } from '@/hooks/usePagination';
import { useDebounce }   from '@/hooks/useDebounce';
import toast from 'react-hot-toast';

const EMPTY = {
  iataCode: '', icaoCode: '', name: '', city: '',
  country: '', countryCode: '', latitude: '', longitude: '', timeZone: '',
};

export default function AirportsAdminPage() {
  const [airports, setAirports] = useState([]);
  const [total,    setTotal]    = useState(1);
  const [loading,  setLoading]  = useState(true);
  const [search,   setSearch]   = useState('');
  const debSearch               = useDebounce(search);
  const { pageNumber, pageSize, goToPage, resetPage } = usePagination(15);

  const [modal,   setModal]   = useState(false);
  const [editing, setEditing] = useState(null);
  const [form,    setForm]    = useState(EMPTY);
  const [saving,  setSaving]  = useState(false);
  const [error,   setError]   = useState('');

  const [deleteModal,  setDeleteModal]  = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleting,     setDeleting]     = useState(false);

  useEffect(() => { resetPage(); }, [debSearch]);

  const load = useCallback(() => {
    setLoading(true);
    airportService
      .getAirports({ pageNumber, pageSize, searchTerm: debSearch })
      .then(({ data }) => {
        setAirports(data.items ?? []);
        setTotal(data.totalPages ?? 1);
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [pageNumber, pageSize, debSearch]);

  useEffect(load, [load]);

  const openCreate = () => {
    setEditing(null); setForm(EMPTY); setError(''); setModal(true);
  };

  const openEdit = (a) => {
    setEditing(a);
    setForm({
      iataCode: a.iataCode ?? '', icaoCode: a.icaoCode ?? '', name: a.name ?? '',
      city: a.city ?? '', country: a.country ?? '', countryCode: a.countryCode ?? '',
      latitude: a.latitude ?? '', longitude: a.longitude ?? '', timeZone: a.timeZone ?? '',
    });
    setError(''); setModal(true);
  };

  const handleSave = async () => {
    setError(''); setSaving(true);
    try {
      const payload = { ...form, latitude: +form.latitude, longitude: +form.longitude };
      if (editing) {
        await airportService.updateAirport(editing.id, payload);
        toast.success('Airport updated.');
      } else {
        await airportService.createAirport(payload);
        toast.success('Airport created.');
      }
      setModal(false); load();
    } catch (err) {
      setError(err.response?.data?.detail ?? 'Save failed.');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    setDeleting(true);
    try {
      await airportService.deleteAirport(deleteTarget.id);
      toast.success('Airport deleted.');
      setDeleteModal(false); load();
    } catch {
      toast.error('Delete failed.');
    } finally {
      setDeleting(false);
    }
  };

  const setF = (k) => (e) => setForm((p) => ({ ...p, [k]: e.target.value }));

  const columns = [
    { key: 'iataCode', header: 'IATA' },
    { key: 'icaoCode', header: 'ICAO' },
    { key: 'name',     header: 'Airport Name' },
    { key: 'city',     header: 'City' },
    { key: 'country',  header: 'Country' },
    { key: 'timeZone', header: 'Timezone' },
    {
      key: 'actions', header: '',
      render: (a) => (
        <div className="flex items-center gap-1">
          <button onClick={() => openEdit(a)}
            className="p-1.5 rounded-lg text-slate-400 hover:text-brand-600 hover:bg-brand-50 transition">
            <Edit2 className="h-4 w-4" />
          </button>
          <button onClick={() => { setDeleteTarget(a); setDeleteModal(true); }}
            className="p-1.5 rounded-lg text-slate-400 hover:text-red-600 hover:bg-red-50 transition">
            <Trash2 className="h-4 w-4" />
          </button>
        </div>
      ),
    },
  ];

  return (
    <div className="animate-fadeIn">
      <div className="flex items-center justify-between mb-6 flex-wrap gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-800">Airports</h1>
          <p className="text-slate-500 text-sm mt-1">Manage airport records</p>
        </div>
        <div className="flex items-center gap-3">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
            <input className="input-base pl-9 w-56" placeholder="Search airports…"
              value={search} onChange={(e) => setSearch(e.target.value)} />
          </div>
          <Button onClick={openCreate}><Plus className="h-4 w-4" /> Add Airport</Button>
        </div>
      </div>

      <Table columns={columns} data={airports} loading={loading} />
      <Pagination pageNumber={pageNumber} totalPages={total} onPage={goToPage} />

      {/* Create / Edit Modal */}
      <Modal open={modal} onClose={() => setModal(false)}
        title={editing ? 'Edit Airport' : 'Add Airport'} size="lg">
        {error && <Alert type="error" message={error} className="mb-4" />}
        <div className="grid grid-cols-2 gap-4">
          <Input label="IATA Code" value={form.iataCode} onChange={setF('iataCode')}
            placeholder="JFK" maxLength={3} required />
          <Input label="ICAO Code" value={form.icaoCode} onChange={setF('icaoCode')}
            placeholder="KJFK" maxLength={4} />
          <Input label="Airport Name" value={form.name} onChange={setF('name')}
            placeholder="John F. Kennedy International"
            className="col-span-2" containerClassName="col-span-2" required />
          <Input label="City" value={form.city} onChange={setF('city')} placeholder="New York" />
          <Input label="Country" value={form.country} onChange={setF('country')}
            placeholder="United States" />
          <Input label="Country Code" value={form.countryCode} onChange={setF('countryCode')}
            placeholder="US" maxLength={2} />
          <Input label="Timezone" value={form.timeZone} onChange={setF('timeZone')}
            placeholder="America/New_York" />
          <Input label="Latitude" type="number" value={form.latitude}
            onChange={setF('latitude')} placeholder="40.6413" />
          <Input label="Longitude" type="number" value={form.longitude}
            onChange={setF('longitude')} placeholder="-73.7781" />
        </div>
        <div className="flex gap-3 justify-end mt-6">
          <Button variant="secondary" onClick={() => setModal(false)}>Cancel</Button>
          <Button loading={saving} onClick={handleSave}>
            {editing ? 'Save Changes' : 'Create'}
          </Button>
        </div>
      </Modal>

      {/* Delete Modal */}
      <Modal open={deleteModal} onClose={() => setDeleteModal(false)} title="Delete Airport">
        <Alert type="error"
          message={`Delete ${deleteTarget?.name} (${deleteTarget?.iataCode})? This cannot be undone.`} />
        <div className="flex gap-3 justify-end mt-6">
          <Button variant="secondary" onClick={() => setDeleteModal(false)}>Cancel</Button>
          <Button variant="danger" loading={deleting} onClick={handleDelete}>Delete</Button>
        </div>
      </Modal>
    </div>
  );
}
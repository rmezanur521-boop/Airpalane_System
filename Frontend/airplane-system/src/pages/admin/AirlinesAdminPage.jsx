import { useEffect, useState } from 'react';
import { Plus, Edit2 } from 'lucide-react';
import Table          from '@/components/ui/Table';
import Button         from '@/components/ui/Button';
import Modal          from '@/components/ui/Modal';
import Input          from '@/components/ui/Input';
import Alert          from '@/components/ui/Alert';
import airlineService from '@/api/airlineService';
import toast from 'react-hot-toast';

const EMPTY = { iataCode: '', name: '', country: '', logoUrl: '', contactEmail: '' };

export default function AirlinesAdminPage() {
  const [airlines, setAirlines] = useState([]);
  const [loading,  setLoading]  = useState(true);
  const [modal,    setModal]    = useState(false);
  const [editing,  setEditing]  = useState(null);
  const [form,     setForm]     = useState(EMPTY);
  const [saving,   setSaving]   = useState(false);
  const [error,    setError]    = useState('');

  const load = () => {
    setLoading(true);
    airlineService.getAirlines()
      .then(({ data }) => setAirlines(data ?? []))
      .catch(() => {})
      .finally(() => setLoading(false));
  };

  useEffect(load, []);

  const openCreate = () => {
    setEditing(null); setForm(EMPTY); setError(''); setModal(true);
  };

  const openEdit = (a) => {
    setEditing(a);
    setForm({ iataCode: a.iataCode ?? '', name: a.name ?? '', country: a.country ?? '',
      logoUrl: a.logoUrl ?? '', contactEmail: a.contactEmail ?? '' });
    setError(''); setModal(true);
  };

  const handleSave = async () => {
    setError(''); setSaving(true);
    try {
      if (editing) {
        await airlineService.updateAirline(editing.id, form);
        toast.success('Airline updated.');
      } else {
        await airlineService.createAirline(form);
        toast.success('Airline created.');
      }
      setModal(false); load();
    } catch (err) {
      setError(err.response?.data?.detail ?? 'Save failed.');
    } finally {
      setSaving(false);
    }
  };

  const setF = (k) => (e) => setForm((p) => ({ ...p, [k]: e.target.value }));

  const columns = [
    { key: 'iataCode', header: 'IATA' },
    { key: 'name',     header: 'Name' },
    { key: 'country',  header: 'Country' },
    { key: 'contactEmail', header: 'Contact' },
    {
      key: 'actions', header: '',
      render: (a) => (
        <button onClick={() => openEdit(a)}
          className="p-1.5 rounded-lg text-slate-400 hover:text-brand-600 hover:bg-brand-50 transition">
          <Edit2 className="h-4 w-4" />
        </button>
      ),
    },
  ];

  return (
    <div className="animate-fadeIn">
      <div className="flex items-center justify-between mb-6 flex-wrap gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-800">Airlines</h1>
          <p className="text-slate-500 text-sm mt-1">Manage airline records</p>
        </div>
        <Button onClick={openCreate}><Plus className="h-4 w-4" /> Add Airline</Button>
      </div>

      <Table columns={columns} data={airlines} loading={loading} />

      <Modal open={modal} onClose={() => setModal(false)}
        title={editing ? 'Edit Airline' : 'Add Airline'}>
        {error && <Alert type="error" message={error} className="mb-4" />}
        <div className="flex flex-col gap-4">
          <Input label="IATA Code" value={form.iataCode} onChange={setF('iataCode')}
            placeholder="AA" maxLength={3} required />
          <Input label="Name" value={form.name} onChange={setF('name')}
            placeholder="American Airlines" required />
          <Input label="Country" value={form.country} onChange={setF('country')}
            placeholder="United States" />
          <Input label="Logo URL" type="url" value={form.logoUrl} onChange={setF('logoUrl')}
            placeholder="https://…" />
          <Input label="Contact Email" type="email" value={form.contactEmail}
            onChange={setF('contactEmail')} placeholder="ops@airline.com" />
        </div>
        <div className="flex gap-3 justify-end mt-6">
          <Button variant="secondary" onClick={() => setModal(false)}>Cancel</Button>
          <Button loading={saving} onClick={handleSave}>
            {editing ? 'Save Changes' : 'Create'}
          </Button>
        </div>
      </Modal>
    </div>
  );
}
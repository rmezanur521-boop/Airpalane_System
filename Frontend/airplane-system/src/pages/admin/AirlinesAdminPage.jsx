import { useEffect, useRef, useState } from 'react';
import { Plus, Edit2, ImagePlus, X, Building2 } from 'lucide-react';
import Table          from '@/components/ui/Table';
import Button         from '@/components/ui/Button';
import Modal          from '@/components/ui/Modal';
import Input          from '@/components/ui/Input';
import Alert          from '@/components/ui/Alert';
import airlineService from '@/api/airlineService';
import toast from 'react-hot-toast';

const EMPTY = { iataCode: '', name: '', country: '', contactEmail: '', contactPhone: '' };

export default function AirlinesAdminPage() {
  const [airlines, setAirlines] = useState([]);
  const [loading,  setLoading]  = useState(true);
  const [modal,    setModal]    = useState(false);
  const [editing,  setEditing]  = useState(null);
  const [form,     setForm]     = useState(EMPTY);
  const [saving,   setSaving]   = useState(false);
  const [error,    setError]    = useState('');

  // Logo state
  const [logoFile, setLogoFile] = useState(null);
  const [logoPreview, setLogoPreview] = useState('');

  // Gallery state — existing images (edit mode, synced live with the server)
  // and queued images (create mode, sent together with the create request).
  const [existingImages, setExistingImages] = useState([]);
  const [queuedImages, setQueuedImages] = useState([]);
  const [galleryBusy, setGalleryBusy] = useState(false);

  const logoInputRef = useRef(null);
  const galleryInputRef = useRef(null);

  const load = () => {
    setLoading(true);
    airlineService.getAirlines()
      .then(({ data }) => setAirlines(data ?? []))
      .catch(() => {})
      .finally(() => setLoading(false));
  };

  useEffect(load, []);

  const resetImageState = () => {
    setLogoFile(null);
    setLogoPreview('');
    setExistingImages([]);
    setQueuedImages([]);
    if (logoInputRef.current) logoInputRef.current.value = '';
    if (galleryInputRef.current) galleryInputRef.current.value = '';
  };

  const openCreate = () => {
    setEditing(null); setForm(EMPTY); setError('');
    resetImageState();
    setModal(true);
  };

  const openEdit = (a) => {
    setEditing(a);
    setForm({
      iataCode: a.iataCode ?? '', name: a.name ?? '', country: a.country ?? '',
      contactEmail: a.contactEmail ?? '', contactPhone: a.contactPhone ?? '',
    });
    setError('');
    resetImageState();
    setLogoPreview(a.logoUrl ?? '');
    setExistingImages(a.images ?? []);
    setModal(true);
  };

  const handleSave = async () => {
    setError(''); setSaving(true);
    try {
      if (editing) {
        await airlineService.updateAirline(editing.id, { ...form, logo: logoFile });
        toast.success('Airline updated.');
      } else {
        await airlineService.createAirline({ ...form, logo: logoFile, images: queuedImages });
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

  const handleLogoSelect = (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setLogoFile(file);
    setLogoPreview(URL.createObjectURL(file));
  };

  const handleGallerySelect = async (e) => {
    const files = Array.from(e.target.files ?? []);
    if (files.length === 0) return;

    if (editing) {
      // Edit mode: upload immediately so the gallery manager feels responsive.
      setGalleryBusy(true);
      try {
        const { data } = await airlineService.addAirlineImages(editing.id, files);
        setExistingImages(data.images ?? []);
        toast.success('Image(s) added.');
      } catch (err) {
        toast.error(err.response?.data?.detail ?? 'Image upload failed.');
      } finally {
        setGalleryBusy(false);
        if (galleryInputRef.current) galleryInputRef.current.value = '';
      }
    } else {
      // Create mode: queue locally, sent together with the create request.
      setQueuedImages((prev) => [...prev, ...files]);
      if (galleryInputRef.current) galleryInputRef.current.value = '';
    }
  };

  const handleDeleteExistingImage = async (imageId) => {
    setGalleryBusy(true);
    try {
      const { data } = await airlineService.deleteAirlineImage(editing.id, imageId);
      setExistingImages(data.images ?? []);
      toast.success('Image removed.');
    } catch (err) {
      toast.error(err.response?.data?.detail ?? 'Failed to remove image.');
    } finally {
      setGalleryBusy(false);
    }
  };

  const handleRemoveQueuedImage = (index) => {
    setQueuedImages((prev) => prev.filter((_, i) => i !== index));
  };

  const columns = [
    {
      key: 'logo', header: '',
      render: (a) => (
        <div className="h-9 w-9 rounded-lg bg-slate-100 border border-slate-200 flex items-center justify-center overflow-hidden">
          {a.logoUrl
            ? <img src={a.logoUrl} alt={a.name} className="h-full w-full object-cover" />
            : <Building2 className="h-4 w-4 text-slate-400" />}
        </div>
      ),
    },
    { key: 'iataCode', header: 'IATA' },
    { key: 'name',     header: 'Name' },
    { key: 'country',  header: 'Country' },
    { key: 'contactEmail', header: 'Contact' },
    {
      key: 'primaryImageUrl', header: 'Gallery',
      render: (a) =>
        a.primaryImageUrl
          ? <img src={a.primaryImageUrl} alt="" className="h-9 w-14 rounded-md object-cover border border-slate-200" />
          : <span className="text-slate-300 text-xs">—</span>,
    },
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
          <p className="text-slate-500 text-sm mt-1">Manage airline records, logos & gallery images</p>
        </div>
        <Button onClick={openCreate}><Plus className="h-4 w-4" /> Add Airline</Button>
      </div>

      <Table columns={columns} data={airlines} loading={loading} />

      <Modal open={modal} onClose={() => setModal(false)}
        title={editing ? 'Edit Airline' : 'Add Airline'}>
        {error && <Alert type="error" message={error} className="mb-4" />}
        <div className="flex flex-col gap-4">
          <Input label="IATA Code" value={form.iataCode} onChange={setF('iataCode')}
            placeholder="AA" maxLength={3} required disabled={!!editing} />
          <Input label="Name" value={form.name} onChange={setF('name')}
            placeholder="American Airlines" required />
          <Input label="Country" value={form.country} onChange={setF('country')}
            placeholder="United States" />
          <Input label="Contact Email" type="email" value={form.contactEmail}
            onChange={setF('contactEmail')} placeholder="ops@airline.com" />
          <Input label="Contact Phone" value={form.contactPhone}
            onChange={setF('contactPhone')} placeholder="+1 800 000 0000" />

          {/* ── Logo ─────────────────────────────────────────────────────── */}
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1.5">Logo</label>
            <div className="flex items-center gap-3">
              <div className="h-16 w-16 rounded-xl bg-slate-100 border border-slate-200 flex items-center justify-center overflow-hidden flex-shrink-0">
                {logoPreview
                  ? <img src={logoPreview} alt="Logo preview" className="h-full w-full object-cover" />
                  : <Building2 className="h-6 w-6 text-slate-400" />}
              </div>
              <input ref={logoInputRef} type="file" accept="image/*"
                onChange={handleLogoSelect}
                className="text-sm text-slate-600 file:mr-3 file:py-2 file:px-3 file:rounded-lg file:border-0 file:bg-brand-50 file:text-brand-700 file:text-sm file:font-medium hover:file:bg-brand-100" />
            </div>
          </div>

          {/* ── Gallery ──────────────────────────────────────────────────── */}
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1.5">Gallery Images</label>
            <div className="flex flex-wrap gap-3 mb-3">
              {(editing ? existingImages : queuedImages.map((f, i) => ({ id: i, imageUrl: URL.createObjectURL(f), isPrimary: i === 0 }))).map((img) => (
                <div key={img.id} className="relative h-20 w-20 rounded-lg overflow-hidden border border-slate-200 group">
                  <img src={img.imageUrl} alt="" className="h-full w-full object-cover" />
                  {img.isPrimary && (
                    <span className="absolute bottom-0 left-0 right-0 bg-brand-600/90 text-white text-[10px] text-center py-0.5">
                      Primary
                    </span>
                  )}
                  <button
                    type="button"
                    onClick={() => editing ? handleDeleteExistingImage(img.id) : handleRemoveQueuedImage(img.id)}
                    disabled={galleryBusy}
                    className="absolute top-1 right-1 bg-black/60 hover:bg-red-600 text-white rounded-full p-0.5 opacity-0 group-hover:opacity-100 transition">
                    <X className="h-3 w-3" />
                  </button>
                </div>
              ))}
            </div>
            <label className="inline-flex items-center gap-2 text-sm text-brand-600 hover:text-brand-700 cursor-pointer font-medium">
              <ImagePlus className="h-4 w-4" />
              {galleryBusy ? 'Uploading…' : 'Add images'}
              <input ref={galleryInputRef} type="file" accept="image/*" multiple
                onChange={handleGallerySelect} disabled={galleryBusy} className="hidden" />
            </label>
            <p className="text-xs text-slate-400 mt-1">
              {editing
                ? 'Changes to the gallery save immediately.'
                : 'These will be uploaded once you create the airline.'}
            </p>
          </div>
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
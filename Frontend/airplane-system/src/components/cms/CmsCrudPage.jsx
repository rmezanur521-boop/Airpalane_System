// src/components/cms/CmsCrudPage.jsx
import { useEffect, useState } from 'react';
import { Plus, Edit2, Trash2 } from 'lucide-react';
import toast from 'react-hot-toast';
import Modal from '@/components/ui/Modal';
import Button from '@/components/ui/Button';
import Alert from '@/components/ui/Alert';
import ReorderableTable from './ReorderableTable';
import { extractCmsError, buildCmsImageUrl } from '@/api/cms/cmsHelpers';

export default function CmsCrudPage({
  title, subtitle, service, emptyForm, columns, renderForm,
  imageConfig, orderField, addLabel = 'Add New',
}) {
  const [items, setItems]     = useState([]);
  const [loading, setLoading] = useState(true);
  const [modal, setModal]     = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm]       = useState(emptyForm);
  const [saving, setSaving]   = useState(false);
  const [error, setError]     = useState('');
  const [imageFile, setImageFile]       = useState(null);
  const [imagePreview, setImagePreview] = useState('');

  const load = () => {
    setLoading(true);
    service.list()
      .then(({ data }) => setItems(data ?? []))
      .catch((err) => toast.error(extractCmsError(err)))
      .finally(() => setLoading(false));
  };

  useEffect(load, []);

  const setField = (key) => (e) => {
    const value = e?.target
      ? (e.target.type === 'checkbox' ? e.target.checked : e.target.value)
      : e;
    setForm((p) => ({ ...p, [key]: value }));
  };

  const openCreate = () => {
    setEditing(null); setForm(emptyForm);
    setImageFile(null); setImagePreview(''); setError('');
    setModal(true);
  };

  const openEdit = (item) => {
    setEditing(item);
    setForm({ ...emptyForm, ...item });
    setImageFile(null);
    setImagePreview(imageConfig ? buildCmsImageUrl(item[imageConfig.urlField]) : '');
    setError('');
    setModal(true);
  };

  const handleImageSelect = (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setImageFile(file);
    setImagePreview(URL.createObjectURL(file));
  };

  const handleSave = async () => {
    setSaving(true); setError('');
    try {
      const { data: saved } = editing
        ? await service.update(editing.id, form)
        : await service.create(form);

      if (imageConfig && imageFile && saved?.id) {
        await service.uploadImage(saved.id, imageFile);
      }
      toast.success(editing ? 'Updated successfully.' : 'Created successfully.');
      setModal(false);
      load();
    } catch (err) {
      setError(extractCmsError(err));
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (item) => {
    const label = item.title ?? item.name ?? item.destinationName ?? item.aircraftName ?? item.serviceName ?? 'this item';
    if (!window.confirm(`Delete "${label}"? This cannot be undone.`)) return;
    try {
      await service.remove(item.id);
      toast.success('Deleted.');
      load();
    } catch (err) {
      toast.error(extractCmsError(err));
    }
  };

  const handleReorderSave = async (orderedItems) => {
    try {
      await service.reorder(orderedItems.map((it, i) => ({ id: it.id, order: i + 1 })));
      toast.success('Order saved.');
      setItems(orderedItems);
    } catch (err) {
      toast.error(extractCmsError(err));
      load();
    }
  };

  const allColumns = [
    ...columns,
    {
      key: 'actions', header: '',
      render: (item) => (
        <div className="flex items-center gap-1 justify-end">
          <button onClick={() => openEdit(item)}
            className="p-1.5 rounded-lg text-slate-400 hover:text-brand-600 hover:bg-brand-50 transition">
            <Edit2 className="h-4 w-4" />
          </button>
          <button onClick={() => handleDelete(item)}
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
          <h1 className="text-2xl font-bold text-slate-800">{title}</h1>
          {subtitle && <p className="text-slate-500 text-sm mt-1">{subtitle}</p>}
        </div>
        <Button onClick={openCreate}><Plus className="h-4 w-4" /> {addLabel}</Button>
      </div>

      <ReorderableTable
        columns={allColumns}
        data={items}
        loading={loading}
        orderField={orderField}
        onSaveOrder={handleReorderSave}
      />

      <Modal open={modal} onClose={() => setModal(false)}
        title={editing ? `Edit ${title}` : `Add ${title}`} size="lg">
        {error && <Alert type="error" message={error} className="mb-4" />}
        <div className="flex flex-col gap-4">
          {renderForm({ form, setField, editing })}

          {imageConfig && (
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1.5">{imageConfig.label}</label>
              <div className="flex items-center gap-3">
                <div className="h-16 w-24 rounded-xl bg-slate-100 border border-slate-200 flex items-center justify-center overflow-hidden flex-shrink-0">
                  {imagePreview
                    ? <img src={imagePreview} alt="" className="h-full w-full object-cover" />
                    : <span className="text-xs text-slate-400">No image</span>}
                </div>
                <input type="file" accept="image/*" onChange={handleImageSelect}
                  className="text-sm text-slate-600 file:mr-3 file:py-2 file:px-3 file:rounded-lg file:border-0 file:bg-brand-50 file:text-brand-700 file:text-sm file:font-medium hover:file:bg-brand-100" />
              </div>
              {!editing && (
                <p className="text-xs text-slate-400 mt-1">Image uploads right after the record is created.</p>
              )}
            </div>
          )}
        </div>
        <div className="flex gap-3 justify-end mt-6">
          <Button variant="secondary" onClick={() => setModal(false)}>Cancel</Button>
          <Button loading={saving} onClick={handleSave}>{editing ? 'Save Changes' : 'Create'}</Button>
        </div>
      </Modal>
    </div>
  );
}
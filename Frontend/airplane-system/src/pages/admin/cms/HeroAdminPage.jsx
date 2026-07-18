// src/pages/admin/cms/HeroAdminPage.jsx
import Input from '@/components/ui/Input';
import Select from '@/components/ui/Select';
import CmsCrudPage from '@/components/cms/CmsCrudPage';
import heroService from '@/api/cms/heroService';

const EMPTY = {
  title: '', subtitle: '', buttonText: '', buttonLink: '',
  searchBoxEnabled: true, overlayOpacity: 0.4, status: 'Active',
};

const STATUS_OPTIONS = [
  { value: 'Active', label: 'Active' },
  { value: 'Inactive', label: 'Inactive' },
];

export default function HeroAdminPage() {
  const columns = [
    { key: 'title', header: 'Title' },
    { key: 'buttonText', header: 'Button' },
    {
      key: 'status', header: 'Status',
      render: (r) => (
        <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${r.status === 'Active' ? 'bg-green-100 text-green-700' : 'bg-slate-100 text-slate-600'}`}>
          {r.status}
        </span>
      ),
    },
  ];

  return (
    <CmsCrudPage
      title="Hero Section"
      subtitle="Manage homepage hero banner slides"
      service={heroService}
      emptyForm={EMPTY}
      columns={columns}
      orderField="displayOrder"
      imageConfig={{ urlField: 'backgroundImage', label: 'Background Image' }}
      renderForm={({ form, setField }) => (
        <>
          <Input label="Title" value={form.title} onChange={setField('title')} maxLength={200} required />
          <Input label="Subtitle" value={form.subtitle} onChange={setField('subtitle')} />
          <div className="grid grid-cols-2 gap-4">
            <Input label="Button Text" value={form.buttonText} onChange={setField('buttonText')} />
            <Input label="Button Link" value={form.buttonLink} onChange={setField('buttonLink')} />
          </div>
          <div className="grid grid-cols-2 gap-4 items-end">
            <Input label="Overlay Opacity (0–1)" type="number" step="0.05" min="0" max="1"
              value={form.overlayOpacity} onChange={(e) => setField('overlayOpacity')(+e.target.value)} />
            <Select label="Status" options={STATUS_OPTIONS} value={form.status} onChange={setField('status')} />
          </div>
          <label className="flex items-center gap-2 text-sm text-slate-700">
            <input type="checkbox" checked={form.searchBoxEnabled} onChange={setField('searchBoxEnabled')}
              className="rounded border-slate-300 text-brand-600 focus:ring-brand-500" />
            Show search box on this slide
          </label>
        </>
      )}
    />
  );
}
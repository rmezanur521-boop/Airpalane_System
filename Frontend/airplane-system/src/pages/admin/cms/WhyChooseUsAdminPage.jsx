// src/pages/admin/cms/WhyChooseUsAdminPage.jsx
import Input from '@/components/ui/Input';
import Select from '@/components/ui/Select';
import CmsCrudPage from '@/components/cms/CmsCrudPage';
import whyChooseUsService from '@/api/cms/whyChooseUsService';

const EMPTY = { title: '', description: '', icon: '', iconColor: '#0057FF', status: 'Active' };
const STATUS_OPTIONS = [
  { value: 'Active', label: 'Active' },
  { value: 'Inactive', label: 'Inactive' },
];

export default function WhyChooseUsAdminPage() {
  const columns = [
    {
      key: 'icon', header: 'Icon',
      render: (r) => (
        <span className="inline-flex items-center gap-2">
          <span className="h-2.5 w-2.5 rounded-full inline-block" style={{ backgroundColor: r.iconColor }} />
          <code className="text-xs text-slate-500">{r.icon}</code>
        </span>
      ),
    },
    { key: 'title', header: 'Title' },
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
      title="Why Choose Us"
      subtitle="Manage the reasons/benefits shown on the homepage (no image — icon class name only)"
      service={whyChooseUsService}
      emptyForm={EMPTY}
      columns={columns}
      orderField="displayOrder"
      renderForm={({ form, setField }) => (
        <>
          <Input label="Title" value={form.title} onChange={setField('title')} maxLength={150} required />
          <Input label="Description" value={form.description} onChange={setField('description')} />
          <div className="grid grid-cols-2 gap-4 items-end">
            <Input label="Icon (e.g. fa-plane)" value={form.icon} onChange={setField('icon')} placeholder="fa-plane" />
            <Input label="Icon Color" type="color" value={form.iconColor} onChange={setField('iconColor')} />
          </div>
          <Select label="Status" options={STATUS_OPTIONS} value={form.status} onChange={setField('status')} />
        </>
      )}
    />
  );
}
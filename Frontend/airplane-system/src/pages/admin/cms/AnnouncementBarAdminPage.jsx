// src/pages/admin/cms/AnnouncementBarAdminPage.jsx
import Input from '@/components/ui/Input';
import Select from '@/components/ui/Select';
import CmsCrudPage from '@/components/cms/CmsCrudPage';
import announcementBarService from '@/api/cms/announcementBarService';

const EMPTY = {
  title: '', backgroundColor: '#FF0000', textColor: '#FFFFFF',
  startDate: '', endDate: '', status: 'Active',
};
const STATUS_OPTIONS = [
  { value: 'Active', label: 'Active' },
  { value: 'Inactive', label: 'Inactive' },
];

export default function AnnouncementBarAdminPage() {
  const columns = [
    {
      key: 'preview', header: 'Preview',
      render: (r) => (
        <span className="px-3 py-1 rounded-lg text-xs font-medium"
          style={{ backgroundColor: r.backgroundColor, color: r.textColor }}>
          {r.title}
        </span>
      ),
    },
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
      title="Announcement Bar"
      subtitle="Only one Active + currently in-date announcement shows on the site at a time"
      service={announcementBarService}
      emptyForm={EMPTY}
      columns={columns}
      orderField="priority"
      renderForm={({ form, setField }) => (
        <>
          <Input label="Title / Message" value={form.title} onChange={setField('title')} required />
          <div className="grid grid-cols-2 gap-4">
            <Input label="Background Color" type="color" value={form.backgroundColor} onChange={setField('backgroundColor')} />
            <Input label="Text Color" type="color" value={form.textColor} onChange={setField('textColor')} />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <Input label="Start Date" type="date" value={form.startDate?.slice(0, 10) ?? ''} onChange={setField('startDate')} />
            <Input label="End Date" type="date" value={form.endDate?.slice(0, 10) ?? ''} onChange={setField('endDate')} />
          </div>
          <Select label="Status" options={STATUS_OPTIONS} value={form.status} onChange={setField('status')} />
        </>
      )}
    />
  );
}
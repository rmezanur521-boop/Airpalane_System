// src/pages/admin/cms/TravelServicesAdminPage.jsx
import Input from '@/components/ui/Input';
import Select from '@/components/ui/Select';
import CmsCrudPage from '@/components/cms/CmsCrudPage';
import travelServiceService from '@/api/cms/travelServiceService';

const EMPTY = { serviceName: '', description: '', icon: '', redirectUrl: '', isExternal: false, status: 'Active' };
const STATUS_OPTIONS = [
  { value: 'Active', label: 'Active' },
  { value: 'Inactive', label: 'Inactive' },
];

export default function TravelServicesAdminPage() {
  const columns = [
    { key: 'serviceName', header: 'Service' },
    { key: 'description', header: 'Description', render: (r) => (
        <span className="text-slate-500 line-clamp-1 max-w-xs block">{r.description || '—'}</span>
      ) },
    { key: 'redirectUrl', header: 'Redirect URL' },
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
      title="Travel Services"
      subtitle="Manage service tiles shown on the homepage"
      service={travelServiceService}
      emptyForm={EMPTY}
      columns={columns}
      orderField="displayOrder"
      imageConfig={{ urlField: 'image', label: 'Service Image' }}
      renderForm={({ form, setField }) => (
        <>
          <Input label="Service Name" value={form.serviceName} onChange={setField('serviceName')} required />
          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-slate-700">Description</label>
            <textarea rows={3} className="input-base resize-none" value={form.description}
              onChange={setField('description')} placeholder="Short description shown under the service title" />
          </div>
          <Input label="Icon (e.g. fa-suitcase)" value={form.icon} onChange={setField('icon')} />
          <Input label="Redirect URL" value={form.redirectUrl} onChange={setField('redirectUrl')} maxLength={500} />
          <div className="grid grid-cols-2 gap-4 items-end">
            <Select label="Status" options={STATUS_OPTIONS} value={form.status} onChange={setField('status')} />
            <label className="flex items-center gap-2 text-sm text-slate-700 pb-2.5">
              <input type="checkbox" checked={form.isExternal} onChange={setField('isExternal')}
                className="rounded border-slate-300 text-brand-600 focus:ring-brand-500" />
              Opens in new tab (external link)
            </label>
          </div>
        </>
      )}
    />
  );
}
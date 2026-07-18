// src/pages/admin/cms/PopularDestinationsAdminPage.jsx
import Input from '@/components/ui/Input';
import Select from '@/components/ui/Select';
import CmsCrudPage from '@/components/cms/CmsCrudPage';
import popularDestinationService from '@/api/cms/popularDestinationService';

const EMPTY = { destinationName: '', country: '', startingPrice: 0, buttonLink: '', status: 'Active' };
const STATUS_OPTIONS = [
  { value: 'Active', label: 'Active' },
  { value: 'Inactive', label: 'Inactive' },
];

export default function PopularDestinationsAdminPage() {
  const columns = [
    { key: 'destinationName', header: 'Destination' },
    { key: 'country', header: 'Country' },
    { key: 'startingPrice', header: 'From', render: (r) => `$${Number(r.startingPrice ?? 0).toFixed(2)}` },
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
      title="Popular Destinations"
      subtitle="Manage destinations featured on the homepage"
      service={popularDestinationService}
      emptyForm={EMPTY}
      columns={columns}
      orderField="displayOrder"
      imageConfig={{ urlField: 'image', label: 'Destination Image' }}
      renderForm={({ form, setField }) => (
        <>
          <Input label="Destination Name" value={form.destinationName} onChange={setField('destinationName')} required />
          <Input label="Country" value={form.country} onChange={setField('country')} required />
          <Input label="Starting Price" type="number" min="0" step="0.01" value={form.startingPrice}
            onChange={(e) => setField('startingPrice')(+e.target.value)} />
          <Input label="Button Link" value={form.buttonLink} onChange={setField('buttonLink')} />
          <Select label="Status" options={STATUS_OPTIONS} value={form.status} onChange={setField('status')} />
        </>
      )}
    />
  );
}
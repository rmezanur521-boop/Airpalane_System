// src/pages/admin/cms/FleetAdminPage.jsx
import Input from '@/components/ui/Input';
import Select from '@/components/ui/Select';
import CmsCrudPage from '@/components/cms/CmsCrudPage';
import fleetService from '@/api/cms/fleetService';

const EMPTY = { aircraftName: '', manufacturer: '', seatCapacity: 100, range: '', status: 'Active' };
const STATUS_OPTIONS = [
  { value: 'Active', label: 'Active' },
  { value: 'Inactive', label: 'Inactive' },
];

export default function FleetAdminPage() {
  const columns = [
    { key: 'aircraftName', header: 'Aircraft' },
    { key: 'manufacturer', header: 'Manufacturer' },
    { key: 'seatCapacity', header: 'Seats' },
    { key: 'range', header: 'Range' },
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
      title="Fleet"
      subtitle="Manage the aircraft fleet showcase on the homepage"
      service={fleetService}
      emptyForm={EMPTY}
      columns={columns}
      orderField="displayOrder"
      imageConfig={{ urlField: 'image', label: 'Aircraft Image' }}
      renderForm={({ form, setField }) => (
        <>
          <Input label="Aircraft Name" value={form.aircraftName} onChange={setField('aircraftName')} required />
          <Input label="Manufacturer" value={form.manufacturer} onChange={setField('manufacturer')} />
          <div className="grid grid-cols-2 gap-4">
            <Input label="Seat Capacity" type="number" min="1" value={form.seatCapacity}
              onChange={(e) => setField('seatCapacity')(+e.target.value)} required />
            <Input label="Range" value={form.range} onChange={setField('range')} placeholder="5,500 km" />
          </div>
          <Select label="Status" options={STATUS_OPTIONS} value={form.status} onChange={setField('status')} />
        </>
      )}
    />
  );
}
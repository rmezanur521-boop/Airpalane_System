// src/pages/admin/cms/SpecialOffersAdminPage.jsx
import Input from '@/components/ui/Input';
import Select from '@/components/ui/Select';
import CmsCrudPage from '@/components/cms/CmsCrudPage';
import specialOfferService from '@/api/cms/specialOfferService';

const EMPTY = {
  title: '', description: '', price: 0, promoCode: '',
  startDate: '', endDate: '', buttonText: '', buttonLink: '',
  status: 'Active', featured: false,
};

const STATUS_OPTIONS = [
  { value: 'Active', label: 'Active' },
  { value: 'Inactive', label: 'Inactive' },
];

export default function SpecialOffersAdminPage() {
  const columns = [
    { key: 'title', header: 'Title' },
    { key: 'promoCode', header: 'Promo' },
    { key: 'price', header: 'Price', render: (r) => `$${Number(r.price ?? 0).toFixed(2)}` },
    {
      key: 'featured', header: 'Featured',
      render: (r) => (r.featured ? <span className="text-brand-600 text-xs font-semibold">★ Featured</span> : '—'),
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
      title="Special Offers"
      subtitle="Manage promotional offers shown on the homepage"
      service={specialOfferService}
      emptyForm={EMPTY}
      columns={columns}
      orderField="priority"
      imageConfig={{ urlField: 'offerImage', label: 'Offer Image' }}
      renderForm={({ form, setField }) => (
        <>
          <Input label="Title" value={form.title} onChange={setField('title')} required />
          <Input label="Description" value={form.description} onChange={setField('description')} />
          <div className="grid grid-cols-2 gap-4">
            <Input label="Price" type="number" min="0" step="0.01" value={form.price}
              onChange={(e) => setField('price')(+e.target.value)} />
            <Input label="Promo Code" value={form.promoCode} onChange={setField('promoCode')} />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <Input label="Start Date" type="date" value={form.startDate?.slice(0, 10) ?? ''} onChange={setField('startDate')} />
            <Input label="End Date" type="date" value={form.endDate?.slice(0, 10) ?? ''} onChange={setField('endDate')} />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <Input label="Button Text" value={form.buttonText} onChange={setField('buttonText')} />
            <Input label="Button Link" value={form.buttonLink} onChange={setField('buttonLink')} />
          </div>
          <div className="grid grid-cols-2 gap-4 items-end">
            <Select label="Status" options={STATUS_OPTIONS} value={form.status} onChange={setField('status')} />
            <label className="flex items-center gap-2 text-sm text-slate-700 pb-2.5">
              <input type="checkbox" checked={form.featured} onChange={setField('featured')}
                className="rounded border-slate-300 text-brand-600 focus:ring-brand-500" />
              Featured offer
            </label>
          </div>
        </>
      )}
    />
  );
}
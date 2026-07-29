// src/pages/admin/cms/WebsiteSettingsPage.jsx
import { useEffect, useState } from 'react';
import { Loader2, CheckCircle2 } from 'lucide-react';
import toast from 'react-hot-toast';
import Input from '@/components/ui/Input';
import Button from '@/components/ui/Button';
import navbarSettingsService from '@/api/cms/navbarSettingsService';
import footerSettingsService from '@/api/cms/footerSettingsService';
import homepageSettingsService from '@/api/cms/homepageSettingsService';
import smtpSettingsService from '@/api/cms/smtpSettingsService';
import { extractCmsError, buildCmsImageUrl } from '@/api/cms/cmsHelpers';

const TABS = [
  { key: 'navbar', label: 'Navbar' },
  { key: 'footer', label: 'Footer' },
  { key: 'homepage', label: 'Section Toggles' },
  { key: 'smtp', label: 'SMTP Settings' },
];

export default function WebsiteSettingsPage() {
  const [loading, setLoading]     = useState(true);
  const [activeTab, setActiveTab] = useState('navbar');
  const [navbar, setNavbar]       = useState(null);
  const [footer, setFooter]       = useState(null);
  const [homepage, setHomepage]   = useState(null);
  const [smtp, setSmtp]           = useState(null);

  useEffect(() => {
    Promise.all([
      navbarSettingsService.get(),
      footerSettingsService.get(),
      homepageSettingsService.get(),
      smtpSettingsService.get(),
    ])
      .then(([n, f, h, s]) => {
        setNavbar(n.data); setFooter(f.data); setHomepage(h.data); setSmtp(s.data);
      })
      .catch((err) => toast.error(extractCmsError(err)))
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <div className="flex items-center justify-center py-24">
        <Loader2 className="h-6 w-6 text-brand-500 animate-spin" />
      </div>
    );
  }

  return (
    <div className="animate-fadeIn">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-slate-800">Website Settings</h1>
        <p className="text-slate-500 text-sm mt-1">Manage navbar, footer, homepage visibility, and SMTP</p>
      </div>

      <div className="flex items-center gap-1 border-b border-slate-100 mb-6 flex-wrap">
        {TABS.map((tab) => (
          <button key={tab.key} onClick={() => setActiveTab(tab.key)}
            className={`px-4 py-2.5 text-sm font-medium rounded-t-xl transition border-b-2 -mb-px
              ${activeTab === tab.key ? 'border-brand-600 text-brand-600' : 'border-transparent text-slate-500 hover:text-slate-800'}`}>
            {tab.label}
          </button>
        ))}
      </div>

      <div className="card max-w-2xl">
        {activeTab === 'navbar' && <NavbarForm data={navbar} onSaved={setNavbar} />}
        {activeTab === 'footer' && <FooterForm data={footer} onSaved={setFooter} />}
        {activeTab === 'homepage' && <HomepageTogglesForm data={homepage} onSaved={setHomepage} />}
        {activeTab === 'smtp' && <SmtpForm data={smtp} onSaved={setSmtp} />}
      </div>
    </div>
  );
}

function NavbarForm({ data, onSaved }) {
  const [form, setForm]     = useState(data);
  const [saving, setSaving] = useState(false);

  const [logoFile, setLogoFile]       = useState(null);
  const [logoPreview, setLogoPreview] = useState(buildCmsImageUrl(data?.logo));

  const [faviconFile, setFaviconFile]       = useState(null);
  const [faviconPreview, setFaviconPreview] = useState(buildCmsImageUrl(data?.faviconPath));

  const setF = (k) => (e) => {
    const v = e.target.type === 'checkbox' ? e.target.checked : e.target.value;
    setForm((p) => ({ ...p, [k]: v }));
  };

  const handleLogo = (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setLogoFile(file);
    setLogoPreview(URL.createObjectURL(file));
  };

  const handleFavicon = (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setFaviconFile(file);
    setFaviconPreview(URL.createObjectURL(file));
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      const { data: updated } = await navbarSettingsService.update(form);
      let final = updated;
      if (logoFile) {
        const { data: withLogo } = await navbarSettingsService.uploadLogo(logoFile);
        final = withLogo;
      }
      if (faviconFile) {
        const { data: withFavicon } = await navbarSettingsService.uploadFavicon(faviconFile);
        final = withFavicon;
      }
      onSaved(final);
      toast.success('Navbar settings saved.');
    } catch (err) {
      toast.error(extractCmsError(err));
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="flex flex-col gap-4">
      <div>
        <label className="block text-sm font-medium text-slate-700 mb-1.5">Logo</label>
        <div className="flex items-center gap-3">
          <div className="h-14 w-14 rounded-xl bg-slate-100 border border-slate-200 flex items-center justify-center overflow-hidden">
            {logoPreview ? <img src={logoPreview} alt="" className="h-full w-full object-contain" /> : <span className="text-xs text-slate-400">Logo</span>}
          </div>
          <input type="file" accept="image/*" onChange={handleLogo}
            className="text-sm text-slate-600 file:mr-3 file:py-2 file:px-3 file:rounded-lg file:border-0 file:bg-brand-50 file:text-brand-700 file:text-sm file:font-medium hover:file:bg-brand-100" />
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-slate-700 mb-1.5">Favicon</label>
        <div className="flex items-center gap-3">
          <div className="h-14 w-14 rounded-xl bg-slate-100 border border-slate-200 flex items-center justify-center overflow-hidden">
            {faviconPreview ? <img src={faviconPreview} alt="" className="h-full w-full object-contain" /> : <span className="text-xs text-slate-400">Favicon</span>}
          </div>
          <input type="file" accept="image/png,image/x-icon,image/svg+xml" onChange={handleFavicon}
            className="text-sm text-slate-600 file:mr-3 file:py-2 file:px-3 file:rounded-lg file:border-0 file:bg-brand-50 file:text-brand-700 file:text-sm file:font-medium hover:file:bg-brand-100" />
        </div>
        <p className="text-xs text-slate-400 mt-1">Recommended: 32×32 or 64×64 .png/.ico file.</p>
      </div>

      <Input label="Company Name" value={form.companyName ?? ''} onChange={setF('companyName')} required />
      <Input label="Website URL" value={form.websiteUrl ?? ''} onChange={setF('websiteUrl')} placeholder="https://example.com" />

      <div className="grid grid-cols-2 gap-4">
        <Input label="Support Phone" value={form.supportPhone ?? ''} onChange={setF('supportPhone')} />
        <Input label="Support Email" type="email" value={form.supportEmail ?? ''} onChange={setF('supportEmail')} />
      </div>
      <div className="grid grid-cols-2 gap-3">
        {[
          ['showLogin', 'Show Login button'],
          ['showSignup', 'Show Signup button'],
          ['showLanguage', 'Show language switcher'],
          ['showCurrency', 'Show currency switcher'],
          ['announcementEnabled', 'Enable announcement bar'],
        ].map(([key, label]) => (
          <label key={key} className="flex items-center gap-2 text-sm text-slate-700">
            <input type="checkbox" checked={!!form[key]} onChange={setF(key)}
              className="rounded border-slate-300 text-brand-600 focus:ring-brand-500" />
            {label}
          </label>
        ))}
      </div>
      <div className="flex justify-end mt-2">
        <Button loading={saving} onClick={handleSave}>Save Navbar Settings</Button>
      </div>
    </div>
  );
}

const SOCIAL_FIELDS = [
  ['facebook', 'Facebook URL'],
  ['instagram', 'Instagram URL'],
  ['youtube', 'YouTube URL'],
  ['linkedIn', 'LinkedIn URL'],
  ['twitter', 'Twitter / X URL'],
];

function FooterForm({ data, onSaved }) {
  const [form, setForm]     = useState(data ?? {});
  const [saving, setSaving] = useState(false);
  const setF = (k) => (e) => setForm((p) => ({ ...p, [k]: e.target.value }));

  const handleSave = async () => {
    setSaving(true);
    try {
      const { data: updated } = await footerSettingsService.update(form);
      onSaved(updated);
      toast.success('Footer settings saved.');
    } catch (err) {
      toast.error(extractCmsError(err));
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="flex flex-col gap-4">
      <Input label="About" value={form.about ?? ''} onChange={setF('about')} />
      <Input label="Address" value={form.address ?? ''} onChange={setF('address')} />
      <div className="grid grid-cols-2 gap-4">
        <Input label="Phone" value={form.phone ?? ''} onChange={setF('phone')} />
        <Input label="Email" type="email" value={form.email ?? ''} onChange={setF('email')} />
      </div>

      <div>
        <h4 className="text-sm font-semibold text-slate-700 mb-2">Social Links</h4>
        <div className="grid grid-cols-2 gap-4">
          {SOCIAL_FIELDS.map(([key, label]) => (
            <Input key={key} label={label} value={form[key] ?? ''} onChange={setF(key)} placeholder="https://..." />
          ))}
        </div>
      </div>

      <Input label="Copyright Text" value={form.copyright ?? ''} onChange={setF('copyright')} />
      <div className="flex justify-end mt-2">
        <Button loading={saving} onClick={handleSave}>Save Footer Settings</Button>
      </div>
    </div>
  );
}

function HomepageTogglesForm({ data, onSaved }) {
  const [form, setForm]     = useState(data ?? {});
  const [saving, setSaving] = useState(false);
  const setF = (k) => (e) => setForm((p) => ({ ...p, [k]: e.target.checked }));

  const handleSave = async () => {
    setSaving(true);
    try {
      const { data: updated } = await homepageSettingsService.update(form);
      onSaved(updated);
      toast.success('Section visibility saved.');
    } catch (err) {
      toast.error(extractCmsError(err));
    } finally {
      setSaving(false);
    }
  };

  const sections = [
    ['showHero', 'Hero Banner'],
    ['showOffers', 'Special Offers'],
    ['showDestinations', 'Popular Destinations'],
    ['showFleet', 'Fleet'],
    ['showServices', 'Travel Services'],
    ['showWhyChooseUs', 'Why Choose Us'],
    ['showFooter', 'Footer'],
  ];

  return (
    <div className="flex flex-col gap-3">
      {sections.map(([key, label]) => (
        <label key={key}
          className="flex items-center justify-between px-4 py-3 rounded-xl border border-slate-100 hover:bg-slate-50 transition">
          <span className="text-sm font-medium text-slate-700">{label}</span>
          <input type="checkbox" checked={!!form[key]} onChange={setF(key)}
            className="h-4 w-4 rounded border-slate-300 text-brand-600 focus:ring-brand-500" />
        </label>
      ))}
      <div className="flex justify-end mt-3">
        <Button loading={saving} onClick={handleSave}>Save Section Toggles</Button>
      </div>
    </div>
  );
}

function SmtpForm({ data, onSaved }) {
  const [form, setForm]                     = useState({ ...data, smtpPassword: '' });
  const [changingPassword, setChangingPassword] = useState(false);
  const [saving, setSaving]                 = useState(false);
  const [error, setError]                   = useState('');

  const setF = (k) => (e) => setForm((p) => ({ ...p, [k]: e.target.value }));

  const handleSave = async () => {
    setSaving(true); setError('');
    try {
      const payload = {
        smtpHost: form.smtpHost,
        smtpPort: Number(form.smtpPort),
        smtpUsername: form.smtpUsername,
        smtpFromName: form.smtpFromName,
        smtpFromEmail: form.smtpFromEmail,
        ...(changingPassword && form.smtpPassword ? { smtpPassword: form.smtpPassword } : {}),
      };
      const { data: updated } = await smtpSettingsService.update(payload);
      onSaved(updated);
      setForm({ ...updated, smtpPassword: '' });
      setChangingPassword(false);
      toast.success('SMTP settings saved.');
    } catch (err) {
      setError(extractCmsError(err));
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="flex flex-col gap-4">
      {error && <p className="text-sm text-red-500">{error}</p>}
      <div className="grid sm:grid-cols-2 gap-4">
        <Input label="SMTP Host" value={form.smtpHost ?? ''} onChange={setF('smtpHost')} />
        <Input label="SMTP Port" type="number" value={form.smtpPort ?? ''} onChange={setF('smtpPort')} />
        <Input label="SMTP Username" value={form.smtpUsername ?? ''} onChange={setF('smtpUsername')} />
        <Input label="From Name" value={form.smtpFromName ?? ''} onChange={setF('smtpFromName')} />
        <Input label="From Email" type="email" value={form.smtpFromEmail ?? ''} onChange={setF('smtpFromEmail')} />

        <label className="block">
          <span className="text-sm font-medium text-slate-700">SMTP Password</span>
          <div className="mt-1.5">
            {changingPassword ? (
              <input className="input-base" type="password" autoComplete="new-password"
                placeholder="Enter new password" autoFocus
                value={form.smtpPassword ?? ''} onChange={setF('smtpPassword')} />
            ) : (
              <div className="flex items-center gap-3">
                <input className="input-base" type="password" value="••••••••" disabled readOnly />
                <button type="button" onClick={() => setChangingPassword(true)}
                  className="text-sm font-medium text-brand-600 hover:text-brand-700 whitespace-nowrap">
                  Change
                </button>
              </div>
            )}
          </div>
          <p className="text-xs text-slate-400 mt-1 flex items-center gap-1">
             {data?.isPasswordSet ? (
              <><CheckCircle2 className="h-3.5 w-3.5 text-green-500" />Password is set. Leave unchanged unless updating.</>
            ) : 'No password set yet.'}
          </p>
        </label>
      </div>
      <div className="flex justify-end mt-2">
        <Button loading={saving} onClick={handleSave}>Save SMTP Settings</Button>
      </div>
    </div>
  );
}
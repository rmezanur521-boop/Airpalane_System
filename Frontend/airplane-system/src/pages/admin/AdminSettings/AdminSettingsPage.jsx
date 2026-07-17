// src/pages/admin/AdminSettings/AdminSettingsPage.jsx

import { useEffect, useState } from 'react';
import { Loader2 } from 'lucide-react';
import toast from 'react-hot-toast';
import { adminSettingsApi, extractErrorMessage } from '@/api/adminSettingsApi';
import CompanyInfoForm from './CompanyInfoForm';
import LogoFaviconUploader from './LogoFaviconUploader';
import SmtpSettingsForm from './SmtpSettingsForm';

const TABS = [
  { key: 'company', label: 'Company Info' },
  { key: 'branding', label: 'Logo & Favicon' },
  { key: 'smtp', label: 'SMTP Settings' },
];

export default function AdminSettingsPage() {
  const [settings, setSettings] = useState(null);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState(null);
  const [activeTab, setActiveTab] = useState('company');

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setLoadError(null);

    adminSettingsApi
      .getSettings()
      .then((data) => {
        if (!cancelled) setSettings(data);
      })
      .catch((err) => {
        if (!cancelled) {
          const message = extractErrorMessage(err);
          setLoadError(message);
          toast.error(message);
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, []);

  // প্রতিটা successful save/upload/delete-এ পুরো updated DTO ফেরত আসে —
  // তাই আবার GET কল না করে সরাসরি local state update করা হয়।
  const handleUpdated = (updated) => setSettings(updated);

  if (loading) {
    return (
      <div className="flex items-center justify-center py-24">
        <Loader2 className="h-6 w-6 text-brand-500 animate-spin" />
      </div>
    );
  }

  if (loadError || !settings) {
    return (
      <div className="card max-w-lg mx-auto text-center py-10">
        <p className="text-sm font-medium text-slate-700">Couldn't load settings.</p>
        <p className="text-xs text-slate-400 mt-1">{loadError}</p>
      </div>
    );
  }

  return (
    <div className="page-container py-8 space-y-6">
      <div>
        <h1 className="section-title">Admin Settings</h1>
        <p className="text-sm text-slate-500 mt-1">
          Manage your company profile, branding, and outgoing email configuration.
        </p>
      </div>

      <div className="flex items-center gap-1 border-b border-slate-100">
        {TABS.map((tab) => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key)}
            className={`px-4 py-2.5 text-sm font-medium rounded-t-xl transition border-b-2 -mb-px
              ${
                activeTab === tab.key
                  ? 'border-brand-600 text-brand-600'
                  : 'border-transparent text-slate-500 hover:text-slate-800'
              }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      <div className="card">
        {activeTab === 'company' && (
          <CompanyInfoForm settings={settings} onUpdated={handleUpdated} />
        )}

        {activeTab === 'branding' && (
          <div className="divide-y divide-slate-100">
            <LogoFaviconUploader
              kind="logo"
              title="Company Logo"
              helperText="JPG, PNG, WEBP, GIF, ICO or SVG. Max 5 MB."
              currentUrl={settings.companyLogoUrl}
              onUpdated={handleUpdated}
            />
            <LogoFaviconUploader
              kind="favicon"
              title="Favicon"
              helperText="Shown in the browser tab. Max 5 MB."
              currentUrl={settings.faviconUrl}
              onUpdated={handleUpdated}
            />
          </div>
        )}

        {activeTab === 'smtp' && (
          <SmtpSettingsForm settings={settings} onUpdated={handleUpdated} />
        )}
      </div>
    </div>
  );
}
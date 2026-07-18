// src/components/layout/Footer.jsx
import { Plane, Facebook, Instagram, Phone, Mail, MapPin } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useCms } from '@/context/CmsContext';
import { buildCmsImageUrl } from '@/api/cms/cmsHelpers';

const quickLinks = [
  { to: '/',         label: 'Search Flights' },
  { to: '/bookings', label: 'My Bookings' },
  { to: '/tickets',  label: 'My Tickets' },
];

export default function Footer() {
  const { footer, navbar, cacheBust } = useCms();

  if (footer === null) return null; // showFooter:false হলে সম্পূর্ণ hide

  const companyName = navbar?.companyName || 'AirSystem';
  const logoUrl      = buildCmsImageUrl(navbar?.logo, cacheBust);

  return (
    <footer className="bg-slate-900 text-slate-300 mt-auto">
      <div className="page-container py-12 grid grid-cols-1 md:grid-cols-4 gap-10">
        {/* Company info */}
        <div>
          <div className="flex items-center gap-2 text-white font-bold mb-3">
            {logoUrl
              ? <img src={logoUrl} alt={companyName} className="h-8 w-auto object-contain" />
              : <Plane className="h-6 w-6 text-brand-400" />}
            <span>{companyName}</span>
          </div>
          <p className="text-sm text-slate-400 leading-relaxed">
            {footer?.about || 'Book your flights quickly, safely, and at the best prices.'}
          </p>
        </div>

        {/* Quick links */}
        <div>
          <h4 className="text-white font-semibold text-sm mb-4 uppercase tracking-wider">Quick Links</h4>
          <ul className="flex flex-col gap-2.5 text-sm">
            {quickLinks.map((l) => (
              <li key={l.to}>
                <Link to={l.to} className="hover:text-white transition">{l.label}</Link>
              </li>
            ))}
          </ul>
        </div>

        {/* Contact */}
        <div>
          <h4 className="text-white font-semibold text-sm mb-4 uppercase tracking-wider">Contact</h4>
          <ul className="flex flex-col gap-2.5 text-sm">
            {footer?.address && (
              <li className="flex items-start gap-2"><MapPin className="h-4 w-4 mt-0.5 flex-shrink-0" />{footer.address}</li>
            )}
            {footer?.phone && (
              <li><a href={`tel:${footer.phone}`} className="flex items-center gap-2 hover:text-white transition"><Phone className="h-4 w-4" />{footer.phone}</a></li>
            )}
            {footer?.email && (
              <li><a href={`mailto:${footer.email}`} className="flex items-center gap-2 hover:text-white transition"><Mail className="h-4 w-4" />{footer.email}</a></li>
            )}
          </ul>
        </div>

        {/* Social */}
        <div>
          <h4 className="text-white font-semibold text-sm mb-4 uppercase tracking-wider">Follow Us</h4>
          <div className="flex items-center gap-3">
            {footer?.facebook && (
              <a href={footer.facebook} target="_blank" rel="noreferrer"
                className="h-9 w-9 flex items-center justify-center rounded-full bg-slate-800 hover:bg-brand-600 hover:scale-110 transition-all duration-200">
                <Facebook className="h-4 w-4" />
              </a>
            )}
            {footer?.instagram && (
              <a href={footer.instagram} target="_blank" rel="noreferrer"
                className="h-9 w-9 flex items-center justify-center rounded-full bg-slate-800 hover:bg-brand-600 hover:scale-110 transition-all duration-200">
                <Instagram className="h-4 w-4" />
              </a>
            )}
          </div>
        </div>
      </div>

      <div className="border-t border-slate-800">
  <div className="page-container py-5 flex flex-col sm:flex-row items-center justify-between gap-3">
    <span className="text-xs text-slate-500">We accept:</span>
    <div className="flex items-center gap-2">
      {['Visa', 'Mastercard', 'bKash', 'Nagad'].map((m) => (
        <span key={m} className="text-xs font-semibold text-slate-300 bg-slate-800 px-3 py-1.5 rounded-lg">
          {m}
        </span>
      ))}
    </div>
  </div>
</div>

      <div className="border-t border-slate-800">
        <div className="page-container py-5 flex flex-col sm:flex-row items-center justify-between gap-2 text-xs text-slate-500">
          <p>{footer?.copyright || `© ${new Date().getFullYear()} ${companyName}. All rights reserved.`}</p>
        </div>
      </div>
    </footer>
  );
}
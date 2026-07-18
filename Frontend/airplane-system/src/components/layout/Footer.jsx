// src/components/layout/Footer.jsx
import { Plane, Facebook, Instagram, Phone, Mail } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useCms } from '@/context/CmsContext';

export default function Footer() {
  const { footer, navbar } = useCms();

  if (footer === null) {
    // showFooter: false থেকে backend null পাঠালে ফুটার সম্পূর্ণ hide
    return null;
  }

  const companyName = navbar?.companyName || 'AirSystem';

  return (
    <footer className="bg-white border-t border-slate-100 mt-auto">
      <div className="page-container py-8">
        <div className="flex flex-col md:flex-row items-center justify-between gap-4">
          <div className="flex items-center gap-2 text-brand-600 font-bold">
            <Plane className="h-5 w-5" />
            <span>{companyName}</span>
          </div>

          <div className="flex items-center gap-6 text-sm text-slate-500">
            <Link to="/" className="hover:text-slate-800 transition">Search Flights</Link>
            <Link to="/bookings" className="hover:text-slate-800 transition">My Bookings</Link>
            <Link to="/tickets" className="hover:text-slate-800 transition">My Tickets</Link>
          </div>

          <div className="flex items-center gap-3 text-slate-400">
            {footer?.phone && (
              <a href={`tel:${footer.phone}`} className="hover:text-brand-600 transition" title={footer.phone}>
                <Phone className="h-4 w-4" />
              </a>
            )}
            {footer?.email && (
              <a href={`mailto:${footer.email}`} className="hover:text-brand-600 transition" title={footer.email}>
                <Mail className="h-4 w-4" />
              </a>
            )}
            {footer?.facebook && (
              <a href={footer.facebook} target="_blank" rel="noreferrer" className="hover:text-brand-600 transition">
                <Facebook className="h-4 w-4" />
              </a>
            )}
            {footer?.instagram && (
              <a href={footer.instagram} target="_blank" rel="noreferrer" className="hover:text-brand-600 transition">
                <Instagram className="h-4 w-4" />
              </a>
            )}
          </div>
        </div>

        {footer?.address && (
          <p className="text-xs text-slate-400 text-center mt-4">{footer.address}</p>
        )}

        <p className="text-xs text-slate-400 text-center mt-2">
          {footer?.copyright || `© ${new Date().getFullYear()} ${companyName}. All rights reserved.`}
        </p>
      </div>
    </footer>
  );
}
// src/components/layout/Footer.jsx
import { Plane, Facebook, Instagram, Youtube, Linkedin, Twitter, Phone, Mail, MapPin } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useCms } from '@/context/CmsContext';
import { buildCmsImageUrl } from '@/api/cms/cmsHelpers';

const quickLinks = [
  { to: '/',         label: 'Search Flights' },
  { to: '/bookings', label: 'My Bookings' },
  { to: '/tickets',  label: 'My Tickets' },
  { to: '/privacy',  label: 'Privacy Policy' },
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
          <div className="flex items-center gap-3 flex-wrap">
            {[
              { key: 'facebook', href: footer?.facebook, Icon: Facebook },
              { key: 'instagram', href: footer?.instagram, Icon: Instagram },
              { key: 'youtube', href: footer?.youtube, Icon: Youtube },
              { key: 'linkedIn', href: footer?.linkedIn, Icon: Linkedin },
              { key: 'twitter', href: footer?.twitter, Icon: Twitter },
            ].filter((s) => s.href).map(({ key, href, Icon }) => (
              <a key={key} href={href} target="_blank" rel="noreferrer"
                className="h-9 w-9 flex items-center justify-center rounded-full bg-slate-800 hover:bg-brand-600 hover:scale-110 transition-all duration-200">
                <Icon className="h-4 w-4" />
              </a>
            ))}
          </div>
        </div>
      </div>

      <div className="border-t border-slate-800">
        <div className="page-container py-5 flex flex-col sm:flex-row items-center justify-between gap-3">
          <span className="text-xs text-slate-400 font-medium">We accept:</span>
          <div className="flex items-center gap-2.5 flex-wrap">
            {/* Visa */}
            <div className="h-8 px-2.5 bg-white rounded-lg flex items-center justify-center shadow-sm hover:scale-105 transition-transform" title="Visa">
              <svg viewBox="0 0 64 20" className="h-4 w-auto" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M26.2 19.5h-3.6l2.3-14h3.6l-2.3 14zm15.5-13.7c-.7-.3-1.9-.6-3.3-.6-3.6 0-6.2 1.9-6.2 4.7 0 2 1.8 3.2 3.2 3.9 1.4.7 1.9 1.1 1.9 1.8 0 1-.1.2-1.1 1.4-2.2 1.4-1.5 0-2.3-.2-3.5-.7l-.5-.2-.5 3.2c.9.4 2.5.8 4.2.8 3.9 0 6.5-1.9 6.5-4.9 0-1.6-1-2.9-3.1-3.9-1.3-.7-2.1-1.1-2.1-1.8 0-.6.7-1.2 2.1-1.2 1.2 0 2.1.3 2.8.6l.3.2.5-3.3zm11.6 0h-2.8c-.9 0-1.5.3-1.9 1.2l-5.4 12.5h3.8l.8-2.1h4.6l.4 2.1h3.3l-2.8-13.7zm-4.3 8.7 1.2-3.2c0 .03.24-.65.4-1.1l.2 1 .7 3.3h-2.5zm-18.7-8.7-3.4 9.6-.4-1.9c-.6-2.2-2.6-4.5-4.8-5.7l3.1 11.7h3.8l5.7-13.7h-4zm-5.6 0H18.9l-.1.3c4.7 1.2 7.8 4.1 9.1 7.6l-1.3-6.6c-.2-.9-.9-1.2-1.7-1.3z" fill="#1434CB"/>
                <path d="M20.6 5.8h-5.9l-.1.3c4.7 1.2 7.8 4.1 9.1 7.6l-1.3-6.6c-.2-.9-.9-1.2-1.8-1.3z" fill="#F7B600"/>
              </svg>
            </div>

            {/* Mastercard */}
            <div className="h-8 px-2.5 bg-white rounded-lg flex items-center justify-center shadow-sm hover:scale-105 transition-transform" title="Mastercard">
              <svg viewBox="0 0 36 24" className="h-5 w-auto" fill="none" xmlns="http://www.w3.org/2000/svg">
                <circle cx="12" cy="12" r="10" fill="#EB001B"/>
                <circle cx="24" cy="12" r="10" fill="#F79E1B"/>
                <path d="M18 5.3a9.96 9.96 0 0 0-3.6 6.7 9.96 9.96 0 0 0 3.6 6.7 9.96 9.96 0 0 0 3.6-6.7A9.96 9.96 0 0 0 18 5.3z" fill="#FF5F00"/>
              </svg>
            </div>

            {/* bKash */}
            <div className="h-8 px-2.5 bg-white rounded-lg flex items-center gap-1.5 justify-center shadow-sm hover:scale-105 transition-transform" title="bKash">
              <svg viewBox="0 0 45 42" className="h-5 w-auto" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M26.4 1.8L8.1 18.2l15.2 2.4L26.4 1.8z" fill="#E2136E"/>
                <path d="M8.1 18.2l18.4 1.6-7.2 8.8-11.2-10.4z" fill="#C2115E"/>
                <path d="M19.3 28.6l7.2-8.8 14.4 3.2-21.6 5.6z" fill="#E2136E"/>
                <path d="M26.5 19.8l14.4 3.2-6.4-15.2-8 12z" fill="#F23588"/>
                <path d="M34.5 7.8L26.4 1.8l2.4 10.4 5.7-4.4z" fill="#E2136E"/>
                <path d="M19.3 28.6l-7.2 8.8 12-4-4.8-4.8z" fill="#A80D50"/>
              </svg>
              <span className="font-extrabold text-[13px] tracking-tight text-[#E2136E] font-sans">bKash</span>
            </div>

            {/* Nagad */}
            <div className="h-8 px-2.5 bg-white rounded-lg flex items-center gap-1.5 justify-center shadow-sm hover:scale-105 transition-transform" title="Nagad">
              <svg viewBox="0 0 36 36" className="h-5 w-auto" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M7 25c0-7 5-14 12-18 5-3 11-1 11 4 0 5-4 11-10 14-5 4-10 5-13 0z" fill="#F7941D"/>
                <path d="M12 30c4 3 11 2 16-3 4-3 4-8 1-10-3-3-8-3-12 1-5 3-6 8-5 12z" fill="#ED1C24"/>
                <circle cx="18" cy="18" r="4.5" fill="#FEE900"/>
              </svg>
              <span className="font-black text-[13px] tracking-tight text-[#ED1C24] font-sans">Nagad</span>
            </div>
          </div>
        </div>
      </div>

      <div className="border-t border-slate-800">
        <div className="page-container py-5 flex flex-col sm:flex-row items-center justify-between gap-3 text-xs text-slate-500">
          <p>{footer?.copyright || `© ${new Date().getFullYear()} ${companyName}. All rights reserved.`}</p>
          <div className="flex items-center gap-4">
            <Link to="/privacy" className="text-slate-400 hover:text-white transition font-medium">
              Privacy Policy
            </Link>
          </div>
        </div>
      </div>
    </footer>
  );
}
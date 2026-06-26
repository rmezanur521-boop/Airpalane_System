import { Plane } from 'lucide-react';
import { Link } from 'react-router-dom';

export default function Footer() {
  return (
    <footer className="bg-white border-t border-slate-100 mt-auto">
      <div className="page-container py-8">
        <div className="flex flex-col md:flex-row items-center justify-between gap-4">
          <div className="flex items-center gap-2 text-brand-600 font-bold">
            <Plane className="h-5 w-5" />
            <span>AirSystem</span>
          </div>
          <div className="flex items-center gap-6 text-sm text-slate-500">
            <Link to="/" className="hover:text-slate-800 transition">
              Search Flights
            </Link>
            <Link to="/bookings" className="hover:text-slate-800 transition">
              My Bookings
            </Link>
            <Link to="/tickets" className="hover:text-slate-800 transition">
              My Tickets
            </Link>
          </div>
          <p className="text-xs text-slate-400">
            © {new Date().getFullYear()} AirSystem. All rights reserved.
          </p>
        </div>
      </div>
    </footer>
  );
}

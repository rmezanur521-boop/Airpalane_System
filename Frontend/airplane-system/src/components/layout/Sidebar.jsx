import { NavLink, useNavigate } from 'react-router-dom';
import {
  Plane, LayoutDashboard, Calendar, CreditCard, Users,
  Building2, MapPin, BarChart2, ScrollText, LogOut, X, Shield, Settings,
} from 'lucide-react';
import { useAuth } from '@/hooks/useAuth';
import { getInitials } from '@/utils/formatters';
import toast from 'react-hot-toast';

const adminLinks = [
  { to: '/admin',          label: 'Dashboard',   Icon: LayoutDashboard, exact: true },
  { to: '/admin/flights',  label: 'Flights',     Icon: Plane },
  { to: '/admin/bookings', label: 'Bookings',    Icon: Calendar },
  { to: '/admin/payments', label: 'Payments',    Icon: CreditCard,  adminOnly: true },
  { to: '/admin/users',    label: 'Users',       Icon: Users,       adminOnly: true },
  { to: '/admin/airlines', label: 'Airlines',    Icon: Building2,   adminOnly: true },
  { to: '/admin/airports', label: 'Airports',    Icon: MapPin,      adminOnly: true },
  { to: '/admin/reports',  label: 'Reports',     Icon: BarChart2,   adminOnly: true },
  { to: '/admin/audit-logs', label: 'Audit Logs', Icon: ScrollText, adminOnly: true },
  { to: '/admin/settings', label: 'Settings',    Icon: Settings,    adminOnly: true },
];

export default function Sidebar({ open, onClose }) {
  const { user, logout, isAdmin } = useAuth();
  const navigate                  = useNavigate();

  const handleLogout = async () => {
    await logout();
    toast.success('Logged out');
    navigate('/login');
  };

  const links = adminLinks.filter((l) => !l.adminOnly || isAdmin);

  const content = (
    <aside className="flex flex-col h-full bg-slate-900 text-white w-64">
      {/* Brand */}
      <div className="flex items-center justify-between px-6 py-5 border-b border-slate-700">
        <div className="flex items-center gap-2">
          <Plane className="h-6 w-6 text-brand-400" />
          <span className="font-bold text-lg">AirSystem</span>
        </div>
        {/* Close btn on mobile */}
        <button
          onClick={onClose}
          className="lg:hidden p-1 rounded-lg text-slate-400 hover:text-white hover:bg-slate-700 transition"
        >
          <X className="h-5 w-5" />
        </button>
      </div>

      {/* Role badge */}
      <div className="px-6 py-3 border-b border-slate-700">
        <div className="flex items-center gap-2">
          <Shield className="h-4 w-4 text-brand-400" />
          <span className="text-xs font-medium text-brand-400 uppercase tracking-wider">
            {user?.role} Panel
          </span>
        </div>
      </div>

      {/* Nav */}
      <nav className="flex-1 overflow-y-auto py-4 px-3 flex flex-col gap-1">
        {links.map(({ to, label, Icon, exact }) => (
          <NavLink
            key={to}
            to={to}
            end={exact}
            onClick={onClose}
            className={({ isActive }) =>
              `flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition
               ${isActive
                 ? 'bg-brand-600 text-white'
                 : 'text-slate-300 hover:text-white hover:bg-slate-800'}`
            }
          >
            <Icon className="h-5 w-5 flex-shrink-0" />
            {label}
          </NavLink>
        ))}
      </nav>

      {/* User section */}
      <div className="border-t border-slate-700 p-4">
        <div className="flex items-center gap-3 mb-3">
          <div className="h-9 w-9 rounded-full bg-brand-600 flex items-center justify-center text-white text-xs font-bold flex-shrink-0">
            {getInitials(user?.fullName ?? user?.email ?? '')}
          </div>
          <div className="min-w-0">
            <p className="text-sm font-medium text-white truncate">
              {user?.fullName}
            </p>
            <p className="text-xs text-slate-400 truncate">{user?.email}</p>
          </div>
        </div>
        <button
          onClick={handleLogout}
          className="w-full flex items-center gap-2 px-3 py-2 rounded-xl text-sm
                     text-slate-300 hover:text-white hover:bg-red-600 transition"
        >
          <LogOut className="h-4 w-4" />
          Log Out
        </button>
      </div>
    </aside>
  );

  return (
    <>
      {/* Desktop — always visible */}
      <div className="hidden lg:flex flex-shrink-0">{content}</div>

      {/* Mobile overlay */}
      {open && (
        <div className="lg:hidden fixed inset-0 z-50 flex">
          <div
            className="absolute inset-0 bg-black/50"
            onClick={onClose}
          />
          <div className="relative animate-slideIn">{content}</div>
        </div>
      )}
    </>
  );
}
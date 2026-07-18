// src/components/layout/Navbar.jsx
import { useState } from 'react';
import { Link, NavLink, useNavigate } from 'react-router-dom';
import {
  Plane, Menu, X, User, LogOut, Ticket,
  BookOpen, ChevronDown, LayoutDashboard,
} from 'lucide-react';
import { useAuth } from '@/hooks/useAuth';
import { useCms } from '@/context/CmsContext';
import { getInitials } from '@/utils/formatters';
import { buildCmsImageUrl } from '@/api/cms/cmsHelpers';
import toast from 'react-hot-toast';

const navLinks = [
  { to: '/',         label: 'Search Flights' },
  { to: '/bookings', label: 'My Bookings',   auth: true },
  { to: '/tickets',  label: 'My Tickets',    auth: true },
];

export default function Navbar() {
  const { user, logout, isAdmin, isAgent } = useAuth();
  const { navbar }                          = useCms();
  const navigate                            = useNavigate();
  const [menuOpen, setMenuOpen]             = useState(false);
  const [dropOpen, setDropOpen]             = useState(false);

  const companyName = navbar?.companyName || 'AirSystem';
  const logoUrl      = buildCmsImageUrl(navbar?.logo);
  const showLogin    = navbar?.showLogin ?? true;
  const showSignup   = navbar?.showSignup ?? true;

  const handleLogout = async () => {
    await logout();
    toast.success('Logged out successfully');
    navigate('/login');
  };

  const visibleLinks = navLinks.filter((l) => !l.auth || user);

  return (
    <nav className="sticky top-0 z-40 bg-white/80 backdrop-blur-md border-b border-slate-100 shadow-sm">
      <div className="page-container">
        <div className="flex items-center justify-between h-16">
          {/* Logo */}
          <Link to="/" className="flex items-center gap-2 text-brand-600 font-bold text-xl">
            {logoUrl
              ? <img src={logoUrl} alt={companyName} className="h-8 w-auto object-contain" />
              : <Plane className="h-7 w-7" />}
            <span>{companyName}</span>
          </Link>

          {/* Desktop Nav */}
          <div className="hidden md:flex items-center gap-1">
            {visibleLinks.map((l) => (
              <NavLink key={l.to} to={l.to} end={l.to === '/'}
                className={({ isActive }) =>
                  `px-4 py-2 rounded-xl text-sm font-medium transition
                   ${isActive ? 'bg-brand-50 text-brand-600' : 'text-slate-600 hover:text-slate-900 hover:bg-slate-50'}`
                }>
                {l.label}
              </NavLink>
            ))}
            {(isAdmin || isAgent) && (
              <NavLink to="/admin"
                className={({ isActive }) =>
                  `px-4 py-2 rounded-xl text-sm font-medium transition
                   ${isActive ? 'bg-brand-50 text-brand-600' : 'text-slate-600 hover:text-slate-900 hover:bg-slate-50'}`
                }>
                Admin Panel
              </NavLink>
            )}
          </div>

          {/* Right side */}
          <div className="hidden md:flex items-center gap-3">
            {user ? (
              <div className="relative">
                <button onClick={() => setDropOpen((p) => !p)}
                  className="flex items-center gap-2 px-3 py-2 rounded-xl hover:bg-slate-50 transition">
                  <div className="h-8 w-8 rounded-full bg-brand-600 flex items-center justify-center text-white text-xs font-bold">
                    {getInitials(user.fullName ?? user.email)}
                  </div>
                  <span className="text-sm font-medium text-slate-700">
                    {user.fullName?.split(' ')[0] ?? 'Account'}
                  </span>
                  <ChevronDown className="h-4 w-4 text-slate-400" />
                </button>

                {dropOpen && (
                  <>
                    <div className="fixed inset-0 z-10" onClick={() => setDropOpen(false)} />
                    <div className="absolute right-0 top-12 z-20 w-56 bg-white rounded-2xl shadow-xl border border-slate-100 py-1 animate-fadeIn">
                      <div className="px-4 py-3 border-b border-slate-50">
                        <p className="text-sm font-semibold text-slate-800">{user.fullName}</p>
                        <p className="text-xs text-slate-400 mt-0.5">{user.email}</p>
                      </div>
                      <DropItem to="/dashboard" Icon={LayoutDashboard} label="Dashboard" onClick={() => setDropOpen(false)} />
                      <DropItem to="/profile" Icon={User} label="Profile" onClick={() => setDropOpen(false)} />
                      <DropItem to="/bookings" Icon={BookOpen} label="My Bookings" onClick={() => setDropOpen(false)} />
                      <DropItem to="/tickets" Icon={Ticket} label="My Tickets" onClick={() => setDropOpen(false)} />
                      <div className="border-t border-slate-50 mt-1 pt-1">
                        <button onClick={handleLogout}
                          className="w-full flex items-center gap-3 px-4 py-2 text-sm text-red-600 hover:bg-red-50 transition">
                          <LogOut className="h-4 w-4" />
                          Log Out
                        </button>
                      </div>
                    </div>
                  </>
                )}
              </div>
            ) : (
              <div className="flex items-center gap-2">
                {showLogin && <Link to="/login" className="btn-secondary text-sm px-4 py-2">Log In</Link>}
                {showSignup && <Link to="/register" className="btn-primary text-sm px-4 py-2">Sign Up</Link>}
              </div>
            )}
          </div>

          {/* Mobile hamburger */}
          <button className="md:hidden p-2 rounded-xl text-slate-600 hover:bg-slate-100 transition"
            onClick={() => setMenuOpen((p) => !p)}>
            {menuOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
          </button>
        </div>
      </div>

      {/* Mobile Menu */}
      {menuOpen && (
        <div className="md:hidden border-t border-slate-100 bg-white animate-fadeIn">
          <div className="page-container py-4 flex flex-col gap-1">
            {visibleLinks.map((l) => (
              <NavLink key={l.to} to={l.to} end={l.to === '/'} onClick={() => setMenuOpen(false)}
                className={({ isActive }) =>
                  `px-4 py-2.5 rounded-xl text-sm font-medium transition
                   ${isActive ? 'bg-brand-50 text-brand-600' : 'text-slate-600 hover:bg-slate-50'}`
                }>
                {l.label}
              </NavLink>
            ))}
            {(isAdmin || isAgent) && (
              <NavLink to="/admin" onClick={() => setMenuOpen(false)}
                className={({ isActive }) =>
                  `px-4 py-2.5 rounded-xl text-sm font-medium transition
                   ${isActive ? 'bg-brand-50 text-brand-600' : 'text-slate-600 hover:bg-slate-50'}`
                }>
                Admin Panel
              </NavLink>
            )}
            <div className="border-t border-slate-100 mt-2 pt-2">
              {user ? (
                <button onClick={() => { handleLogout(); setMenuOpen(false); }}
                  className="w-full flex items-center gap-3 px-4 py-2.5 text-sm text-red-600 hover:bg-red-50 rounded-xl transition">
                  <LogOut className="h-4 w-4" />
                  Log Out
                </button>
              ) : (
                <div className="flex gap-2 px-2">
                  {showLogin && <Link to="/login" onClick={() => setMenuOpen(false)} className="btn-secondary flex-1 justify-center">Log In</Link>}
                  {showSignup && <Link to="/register" onClick={() => setMenuOpen(false)} className="btn-primary flex-1 justify-center">Sign Up</Link>}
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </nav>
  );
}

function DropItem({ to, Icon, label, onClick }) {
  return (
    <Link to={to} onClick={onClick} className="flex items-center gap-3 px-4 py-2 text-sm text-slate-700 hover:bg-slate-50 transition">
      <Icon className="h-4 w-4 text-slate-400" />
      {label}
    </Link>
  );
}
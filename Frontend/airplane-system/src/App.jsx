// src/App.jsx
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { AuthProvider } from '@/context/AuthContext';
import { CmsProvider } from '@/context/CmsContext';
import ProtectedRoute from '@/components/auth/ProtectedRoute';
import AdminLayout from '@/components/layout/AdminLayout';
import PaymentGatewayAdminPage from '@/pages/admin/cms/PaymentGatewayAdminPage';

// Auth pages
import LoginPage          from '@/pages/auth/LoginPage';
import RegisterPage       from '@/pages/auth/RegisterPage';
import ForgotPasswordPage from '@/pages/auth/ForgotPasswordPage';
import ResetPasswordPage  from '@/pages/auth/ResetPasswordPage';
import VerifyEmailPage    from '@/pages/auth/VerifyEmailPage';

// Public pages
import HomePage          from '@/pages/public/HomePage';
import SearchResultsPage from '@/pages/public/SearchResultsPage';
import NotFoundPage      from '@/pages/public/NotFoundPage';

// Passenger pages
import DashboardPage    from '@/pages/passenger/DashboardPage';
import MyBookingsPage   from '@/pages/passenger/MyBookingsPage';
import BookingDetailPage from '@/pages/passenger/BookingDetailPage';
import BookingFlowPage  from '@/pages/passenger/BookingFlowPage';
import MyTicketsPage    from '@/pages/passenger/MyTicketsPage';
import TicketDetailPage from '@/pages/passenger/TicketDetailPage';
import ProfilePage      from '@/pages/passenger/ProfilePage';

// Admin pages
import AdminDashboardPage from '@/pages/admin/AdminDashboardPage';
import FlightsPage        from '@/pages/admin/FlightsPage';
import BookingsAdminPage  from '@/pages/admin/BookingsAdminPage';
import PaymentsAdminPage  from '@/pages/admin/PaymentsAdminPage';
import UsersAdminPage     from '@/pages/admin/UsersAdminPage';
import AirlinesAdminPage  from '@/pages/admin/AirlinesAdminPage';
import AirportsAdminPage  from '@/pages/admin/AirportsAdminPage';
import ReportsPage        from '@/pages/admin/ReportsPage';
import AuditLogsPage      from '@/pages/admin/AuditLogsPage';
import AdminSettingsPage  from '@/pages/admin/AdminSettings/AdminSettingsPage';

// Website CMS pages
import HeroAdminPage              from '@/pages/admin/cms/HeroAdminPage';
import SpecialOffersAdminPage     from '@/pages/admin/cms/SpecialOffersAdminPage';
import PopularDestinationsAdminPage from '@/pages/admin/cms/PopularDestinationsAdminPage';
import WhyChooseUsAdminPage       from '@/pages/admin/cms/WhyChooseUsAdminPage';
import FleetAdminPage             from '@/pages/admin/cms/FleetAdminPage';
import TravelServicesAdminPage    from '@/pages/admin/cms/TravelServicesAdminPage';
import AnnouncementBarAdminPage   from '@/pages/admin/cms/AnnouncementBarAdminPage';
import WebsiteSettingsPage        from '@/pages/admin/cms/WebsiteSettingsPage';

const PASSENGER_ROLES = ['Passenger', 'Admin', 'Agent'];
const ADMIN_ROLES     = ['Admin', 'Agent'];
const ADMIN_ONLY      = ['Admin'];

function AdminRoute({ children }) {
  return (
    <ProtectedRoute roles={ADMIN_ROLES}>
      <AdminLayout>{children}</AdminLayout>
    </ProtectedRoute>
  );
}

function AdminOnlyRoute({ children }) {
  return (
    <ProtectedRoute roles={ADMIN_ONLY}>
      <AdminLayout>{children}</AdminLayout>
    </ProtectedRoute>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <CmsProvider>
        <AuthProvider>
          <Toaster
            position="top-right"
            toastOptions={{
              duration: 3500,
              style: { borderRadius: '12px', background: '#1e293b', color: '#f8fafc', fontSize: '14px' },
            }}
          />
          <Routes>
            {/* ── Public ──────────────────────────────────────────────── */}
            <Route path="/"             element={<HomePage />} />
            <Route path="/search"       element={<SearchResultsPage />} />
            <Route path="/login"        element={<LoginPage />} />
            <Route path="/register"     element={<RegisterPage />} />
            <Route path="/forgot-password" element={<ForgotPasswordPage />} />
            <Route path="/reset-password"  element={<ResetPasswordPage />} />
            <Route path="/verify-email"    element={<VerifyEmailPage />} />

            {/* ── Passenger ───────────────────────────────────────────── */}
            <Route path="/dashboard" element={<ProtectedRoute roles={PASSENGER_ROLES}><DashboardPage /></ProtectedRoute>} />
            <Route path="/book" element={<ProtectedRoute roles={PASSENGER_ROLES}><BookingFlowPage /></ProtectedRoute>} />
            <Route path="/bookings" element={<ProtectedRoute roles={PASSENGER_ROLES}><MyBookingsPage /></ProtectedRoute>} />
            <Route path="/bookings/:id" element={<ProtectedRoute roles={PASSENGER_ROLES}><BookingDetailPage /></ProtectedRoute>} />
            <Route path="/tickets" element={<ProtectedRoute roles={PASSENGER_ROLES}><MyTicketsPage /></ProtectedRoute>} />
            <Route path="/tickets/:ticketNumber" element={<ProtectedRoute roles={PASSENGER_ROLES}><TicketDetailPage /></ProtectedRoute>} />
            <Route path="/profile" element={<ProtectedRoute roles={PASSENGER_ROLES}><ProfilePage /></ProtectedRoute>} />

            {/* ── Admin ───────────────────────────────────────────────── */}
            <Route path="/admin"              element={<AdminRoute><AdminDashboardPage /></AdminRoute>} />
            <Route path="/admin/flights"      element={<AdminRoute><FlightsPage /></AdminRoute>} />
            <Route path="/admin/bookings"     element={<AdminRoute><BookingsAdminPage /></AdminRoute>} />
            <Route path="/admin/payments"     element={<AdminOnlyRoute><PaymentsAdminPage /></AdminOnlyRoute>} />
            <Route path="/admin/users"        element={<AdminOnlyRoute><UsersAdminPage /></AdminOnlyRoute>} />
            <Route path="/admin/airlines"     element={<AdminOnlyRoute><AirlinesAdminPage /></AdminOnlyRoute>} />
            <Route path="/admin/airports"     element={<AdminOnlyRoute><AirportsAdminPage /></AdminOnlyRoute>} />
            <Route path="/admin/reports"      element={<AdminOnlyRoute><ReportsPage /></AdminOnlyRoute>} />
            <Route path="/admin/audit-logs"   element={<AdminOnlyRoute><AuditLogsPage /></AdminOnlyRoute>} />
            <Route path="/admin/settings"     element={<AdminOnlyRoute><AdminSettingsPage /></AdminOnlyRoute>} />

            {/* ── Website CMS ─────────────────────────────────────────── */}
            <Route path="/admin/cms/hero"             element={<AdminOnlyRoute><HeroAdminPage /></AdminOnlyRoute>} />
            <Route path="/admin/cms/offers"           element={<AdminOnlyRoute><SpecialOffersAdminPage /></AdminOnlyRoute>} />
            <Route path="/admin/cms/destinations"     element={<AdminOnlyRoute><PopularDestinationsAdminPage /></AdminOnlyRoute>} />
            <Route path="/admin/cms/why-choose-us"    element={<AdminOnlyRoute><WhyChooseUsAdminPage /></AdminOnlyRoute>} />
            <Route path="/admin/cms/fleet"            element={<AdminOnlyRoute><FleetAdminPage /></AdminOnlyRoute>} />
            <Route path="/admin/cms/services"         element={<AdminOnlyRoute><TravelServicesAdminPage /></AdminOnlyRoute>} />
            <Route path="/admin/cms/announcements"    element={<AdminOnlyRoute><AnnouncementBarAdminPage /></AdminOnlyRoute>} />
            <Route path="/admin/cms/website-settings" element={<AdminOnlyRoute><WebsiteSettingsPage /></AdminOnlyRoute>} />
            <Route path="/admin/cms/payment-gateways" element={<AdminOnlyRoute><PaymentGatewayAdminPage /></AdminOnlyRoute>} />
            {/* ── Fallbacks ───────────────────────────────────────────── */}
            <Route path="/404" element={<NotFoundPage />} />
            <Route path="*"    element={<Navigate to="/404" replace />} />
          </Routes>
        </AuthProvider>
      </CmsProvider>
    </BrowserRouter>
  );
}
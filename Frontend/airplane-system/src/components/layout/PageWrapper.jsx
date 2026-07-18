// src/components/layout/PageWrapper.jsx
import Navbar from './Navbar';
import Footer from './Footer';
import { useCms } from '@/context/CmsContext';

export default function PageWrapper({ children, className = '' }) {
  const { announcement, navbar } = useCms();
  const showAnnouncement = navbar?.announcementEnabled && !!announcement;

  return (
    <div className="min-h-screen flex flex-col">
      {showAnnouncement && (
        <div
          className="text-center text-sm font-medium py-2 px-4"
          style={{ backgroundColor: announcement.backgroundColor, color: announcement.textColor }}
        >
          {announcement.title}
        </div>
      )}
      <Navbar />
      <main className={`flex-1 ${className}`}>{children}</main>
      <Footer />
    </div>
  );
}
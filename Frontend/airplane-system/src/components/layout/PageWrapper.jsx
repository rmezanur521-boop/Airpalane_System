import Navbar from './Navbar';
import Footer from './Footer';

export default function PageWrapper({ children, className = '' }) {
  return (
    <div className="min-h-screen flex flex-col">
      <Navbar />
      <main className={`flex-1 ${className}`}>{children}</main>
      <Footer />
    </div>
  );
}
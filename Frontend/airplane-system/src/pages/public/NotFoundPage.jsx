import { Link } from 'react-router-dom';
import { Plane } from 'lucide-react';

export default function NotFoundPage() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-brand-50 via-white to-sky-50
                    flex items-center justify-center p-4">
      <div className="text-center">
        <div className="inline-flex items-center justify-center h-20 w-20 rounded-2xl
                        bg-brand-600 text-white mb-6">
          <Plane className="h-12 w-12" />
        </div>
        <h1 className="text-7xl font-black text-brand-600 mb-2">404</h1>
        <h2 className="text-2xl font-bold text-slate-800 mb-3">Page not found</h2>
        <p className="text-slate-500 mb-8 max-w-sm mx-auto">
          Looks like this flight doesn't exist. Let's get you back on track.
        </p>
        <Link to="/" className="btn-primary">
          Back to Home
        </Link>
      </div>
    </div>
  );
}
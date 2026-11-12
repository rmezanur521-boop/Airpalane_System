import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { Plane, CheckCircle, XCircle } from 'lucide-react';
import authService from '@/api/authService';
import Spinner from '@/components/ui/Spinner';
import { useCms } from '@/context/CmsContext';
import { buildCmsImageUrl } from '@/api/cms/cmsHelpers';

export default function VerifyEmailPage() {
  const { navbar, cacheBust }  = useCms();
  const companyName            = navbar?.companyName || 'AirSystem';
  const logoUrl                = buildCmsImageUrl(navbar?.logo, cacheBust);

  const [params]          = useSearchParams();
  const [status, setStatus] = useState('loading'); // loading | success | error

  useEffect(() => {
    const token = params.get('token');
    const email = params.get('email');
    if (!token || !email) { setStatus('error'); return; }

    authService
      .verifyEmail(token, email)
      .then(() => setStatus('success'))
      .catch(() => setStatus('error'));
  }, [params]);

  return (
    <div className="min-h-screen bg-gradient-to-br from-brand-50 via-white to-emerald-50
                    flex items-center justify-center p-4">
      <div className="card shadow-xl shadow-slate-200/60 text-center max-w-md w-full">
        <Link to="/" className="inline-flex items-center justify-center mb-6 group" title="Return to home">
          {logoUrl ? (
            <img
              src={logoUrl}
              alt={companyName}
              className="h-16 w-auto object-contain max-h-16 drop-shadow-md group-hover:scale-105 transition-transform"
            />
          ) : (
            <div className="inline-flex items-center justify-center h-14 w-14 rounded-2xl bg-brand-600 text-white shadow-lg shadow-brand-500/25">
              <Plane className="h-8 w-8" />
            </div>
          )}
        </Link>

        {status === 'loading' && (
          <>
            <Spinner className="mx-auto mb-4" />
            <p className="text-slate-600">Verifying your email…</p>
          </>
        )}

        {status === 'success' && (
          <>
            <CheckCircle className="h-12 w-12 text-green-500 mx-auto mb-4" />
            <h2 className="text-xl font-bold text-slate-800 mb-2">Email Verified!</h2>
            <p className="text-slate-500 text-sm mb-6">
              Your account is now active. You can sign in.
            </p>
            <Link to="/login" className="btn-primary">
              Go to Login
            </Link>
          </>
        )}

        {status === 'error' && (
          <>
            <XCircle className="h-12 w-12 text-red-500 mx-auto mb-4" />
            <h2 className="text-xl font-bold text-slate-800 mb-2">Verification Failed</h2>
            <p className="text-slate-500 text-sm mb-6">
              The link is invalid or has expired.
            </p>
            <Link to="/login" className="btn-secondary">
              Back to Login
            </Link>
          </>
        )}
      </div>
    </div>
  );
}
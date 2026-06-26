import { useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { Plane, Eye, EyeOff } from 'lucide-react';
import authService from '@/api/authService';
import Button from '@/components/ui/Button';
import Input  from '@/components/ui/Input';
import Alert  from '@/components/ui/Alert';
import toast  from 'react-hot-toast';

export default function ResetPasswordPage() {
  const [params]              = useSearchParams();
  const navigate              = useNavigate();
  const [password, setPassword] = useState('');
  const [showPwd, setShowPwd]   = useState(false);
  const [loading, setLoading]   = useState(false);
  const [error, setError]       = useState('');

  const token = params.get('token') ?? '';
  const email = params.get('email') ?? '';

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await authService.resetPassword({ token, email, newPassword: password });
      toast.success('Password reset successfully!');
      navigate('/login', { replace: true });
    } catch (err) {
      setError(err.response?.data?.detail ?? 'Reset failed. The link may have expired.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-brand-50 via-white to-sky-50
                    flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center h-14 w-14 rounded-2xl
                          bg-brand-600 text-white mb-4">
            <Plane className="h-8 w-8" />
          </div>
          <h1 className="text-2xl font-bold text-slate-800">Reset your password</h1>
          <p className="text-slate-500 mt-1 text-sm">Choose a new strong password</p>
        </div>

        <div className="card shadow-xl shadow-slate-200/60">
          {error && <Alert type="error" message={error} className="mb-4" />}
          {(!token || !email) && (
            <Alert
              type="warning"
              message="Invalid or missing reset token. Please request a new link."
              className="mb-4"
            />
          )}

          <form onSubmit={handleSubmit} className="flex flex-col gap-4">
            <div className="flex flex-col gap-1">
              <label className="text-sm font-medium text-slate-700">New password</label>
              <div className="relative">
                <input
                  type={showPwd ? 'text' : 'password'}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Min. 8 characters"
                  required
                  minLength={8}
                  className="input-base pr-10"
                />
                <button
                  type="button"
                  onClick={() => setShowPwd((p) => !p)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600"
                >
                  {showPwd ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                </button>
              </div>
            </div>

            <Button
              type="submit"
              loading={loading}
              disabled={!token || !email}
              className="w-full"
            >
              Reset Password
            </Button>
          </form>

          <Link
            to="/login"
            className="block text-center mt-6 text-sm text-brand-600 hover:text-brand-700 font-medium"
          >
            Back to login
          </Link>
        </div>
      </div>
    </div>
  );
}
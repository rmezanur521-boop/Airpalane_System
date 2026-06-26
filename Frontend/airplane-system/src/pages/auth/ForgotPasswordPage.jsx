import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Plane, ArrowLeft } from 'lucide-react';
import authService from '@/api/authService';
import Button from '@/components/ui/Button';
import Input  from '@/components/ui/Input';
import Alert  from '@/components/ui/Alert';

export default function ForgotPasswordPage() {
  const [email, setEmail]     = useState('');
  const [loading, setLoading] = useState(false);
  const [sent, setSent]       = useState(false);
  const [error, setError]     = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await authService.forgotPassword(email);
      setSent(true);
    } catch {
      setError('Something went wrong. Please try again.');
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
          <h1 className="text-2xl font-bold text-slate-800">Forgot password?</h1>
          <p className="text-slate-500 mt-1 text-sm">
            Enter your email and we'll send a reset link
          </p>
        </div>

        <div className="card shadow-xl shadow-slate-200/60">
          {sent ? (
            <Alert
              type="success"
              title="Check your inbox"
              message="If an account exists for that email, we've sent a password reset link."
            />
          ) : (
            <>
              {error && <Alert type="error" message={error} className="mb-4" />}
              <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                <Input
                  label="Email address"
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="you@example.com"
                  required
                />
                <Button type="submit" loading={loading} className="w-full">
                  Send Reset Link
                </Button>
              </form>
            </>
          )}

          <Link
            to="/login"
            className="flex items-center justify-center gap-2 mt-6 text-sm text-slate-500 hover:text-slate-700 transition"
          >
            <ArrowLeft className="h-4 w-4" />
            Back to login
          </Link>
        </div>
      </div>
    </div>
  );
}
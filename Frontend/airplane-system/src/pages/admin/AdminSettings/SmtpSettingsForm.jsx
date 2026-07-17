// src/pages/admin/AdminSettings/SmtpSettingsForm.jsx

import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { CheckCircle2, Loader2, Save } from 'lucide-react';
import toast from 'react-hot-toast';
import { adminSettingsApi, extractErrorMessage, extractFieldError } from '@/api/adminSettingsApi';

const schema = z.object({
  smtpHost: z
    .string()
    .min(1, 'SMTP host is required.')
    .max(200, 'SMTP host must be at most 200 characters.'),
  smtpPort: z
    .coerce.number({ invalid_type_error: 'Port must be a number.' })
    .int('Port must be a whole number.')
    .min(1, 'Port must be between 1 and 65535.')
    .max(65535, 'Port must be between 1 and 65535.'),
  smtpUsername: z
    .string()
    .min(1, 'SMTP username is required.')
    .max(200, 'SMTP username must be at most 200 characters.'),
  smtpPassword: z
    .string()
    .refine((val) => val === '' || val.length >= 4, 'Password must be at least 4 characters.')
    .optional(),
  smtpFromName: z
    .string()
    .min(1, 'From name is required.')
    .max(200, 'From name must be at most 200 characters.'),
  smtpFromEmail: z
    .string()
    .min(1, 'From email is required.')
    .max(200, 'From email must be at most 200 characters.')
    .email('Enter a valid email address.'),
});

function toFormValues(s) {
  return {
    smtpHost: s.smtpHost,
    smtpPort: s.smtpPort,
    smtpUsername: s.smtpUsername,
    smtpPassword: '',
    smtpFromName: s.smtpFromName,
    smtpFromEmail: s.smtpFromEmail,
  };
}

export default function SmtpSettingsForm({ settings, onUpdated }) {
  const [changingPassword, setChangingPassword] = useState(false);

  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors, isDirty, isSubmitting },
  } = useForm({
    resolver: zodResolver(schema),
    defaultValues: toFormValues(settings),
  });

  useEffect(() => {
    reset(toFormValues(settings));
    setChangingPassword(false);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [settings.id, settings.updatedAt]);

  const onSubmit = async (values) => {
    try {
      const updated = await adminSettingsApi.updateSmtpSettings({
        smtpHost: values.smtpHost,
        smtpPort: values.smtpPort,
        smtpUsername: values.smtpUsername,
        smtpFromName: values.smtpFromName,
        smtpFromEmail: values.smtpFromEmail,
        // Password change করতে চাইলেই এবং কিছু লিখলেই তবে পাঠানো হবে,
        // নাহলে বাদ দেওয়া হবে যাতে backend আগের password রাখে।
        ...(changingPassword && values.smtpPassword ? { smtpPassword: values.smtpPassword } : {}),
      });
      onUpdated(updated);
      reset(toFormValues(updated));
      setChangingPassword(false);
      toast.success('SMTP settings saved.');
    } catch (err) {
      const fieldError = extractFieldError(err);
      if (fieldError && fieldError.field in schema.shape) {
        setError(fieldError.field, { message: fieldError.message });
      } else {
        toast.error(extractErrorMessage(err));
      }
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
      <div className="grid sm:grid-cols-2 gap-5">
        <Field label="SMTP Host" error={errors.smtpHost?.message}>
          <input className="input-base" {...register('smtpHost')} />
        </Field>

        <Field label="SMTP Port" error={errors.smtpPort?.message}>
          <input className="input-base" type="number" {...register('smtpPort')} />
        </Field>

        <Field label="SMTP Username" error={errors.smtpUsername?.message}>
          <input className="input-base" {...register('smtpUsername')} />
        </Field>

        <Field label="From Name" error={errors.smtpFromName?.message}>
          <input className="input-base" {...register('smtpFromName')} />
        </Field>

        <Field label="From Email" error={errors.smtpFromEmail?.message}>
          <input className="input-base" type="email" {...register('smtpFromEmail')} />
        </Field>

        <Field label="SMTP Password" error={errors.smtpPassword?.message}>
          {changingPassword ? (
            <input
              className="input-base"
              type="password"
              autoComplete="new-password"
              placeholder="Enter new password"
              autoFocus
              {...register('smtpPassword')}
            />
          ) : (
            <div className="flex items-center gap-3">
              <input className="input-base" type="password" value="••••••••" disabled readOnly />
              <button
                type="button"
                onClick={() => setChangingPassword(true)}
                className="text-sm font-medium text-brand-600 hover:text-brand-700 whitespace-nowrap"
              >
                Change
              </button>
            </div>
          )}
          <p className="text-xs text-slate-400 mt-1 flex items-center gap-1">
            {settings.isSmtpPasswordConfigured ? (
              <>
                <CheckCircle2 className="h-3.5 w-3.5 text-green-500" />
                Password is set. Leave unchanged unless you want to update it.
              </>
            ) : (
              'No password set yet.'
            )}
          </p>
        </Field>
      </div>

      <div className="flex items-center gap-3 pt-2">
        <button type="submit" disabled={isSubmitting} className="btn-primary">
          {isSubmitting ? <Loader2 className="h-4 w-4 animate-spin" /> : <Save className="h-4 w-4" />}
          Save Changes
        </button>
        {(isDirty || changingPassword) && !isSubmitting && (
          <span className="text-xs text-amber-600 font-medium">Unsaved changes</span>
        )}
      </div>
    </form>
  );
}

function Field({ label, error, children }) {
  return (
    <label className="block">
      <span className="text-sm font-medium text-slate-700">{label}</span>
      <div className="mt-1.5">{children}</div>
      {error && <p className="text-xs text-red-500 mt-1">{error}</p>}
    </label>
  );
}
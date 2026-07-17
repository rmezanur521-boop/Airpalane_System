// src/pages/admin/AdminSettings/CompanyInfoForm.jsx

import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Loader2, Save } from 'lucide-react';
import toast from 'react-hot-toast';
import { adminSettingsApi, extractErrorMessage, extractFieldError } from '@/api/adminSettingsApi';

const schema = z.object({
  companyName: z
    .string()
    .min(1, 'Company name is required.')
    .max(200, 'Company name must be at most 200 characters.'),
  supportEmail: z
    .string()
    .min(1, 'Support email is required.')
    .max(200, 'Support email must be at most 200 characters.')
    .email('Enter a valid email address.'),
  supportPhone: z
    .string()
    .min(1, 'Support phone is required.')
    .regex(/^\+?[0-9\s\-()]{7,20}$/, 'Enter a valid phone number.'),
  companyAddress: z
    .string()
    .min(1, 'Company address is required.')
    .max(500, 'Company address must be at most 500 characters.'),
  websiteUrl: z
    .string()
    .min(1, 'Website URL is required.')
    .max(300, 'Website URL must be at most 300 characters.')
    .refine(
      (val) => /^https?:\/\/.+/i.test(val),
      'Enter a valid URL starting with http:// or https://',
    ),
  footerText: z.string().max(500, 'Footer text must be at most 500 characters.').optional(),
});

function toFormValues(s) {
  return {
    companyName: s.companyName,
    supportEmail: s.supportEmail,
    supportPhone: s.supportPhone,
    companyAddress: s.companyAddress,
    websiteUrl: s.websiteUrl,
    footerText: s.footerText ?? '',
  };
}

export default function CompanyInfoForm({ settings, onUpdated }) {
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

  // settings prop বাইরে থেকে update হলে (যেমন Logo upload-এর পর) form-ও sync করে
  useEffect(() => {
    reset(toFormValues(settings));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [settings.id, settings.updatedAt]);

  const onSubmit = async (values) => {
    try {
      const updated = await adminSettingsApi.updateGeneralSettings({
        ...values,
        footerText: values.footerText || undefined,
      });
      onUpdated(updated);
      reset(toFormValues(updated));
      toast.success('Company info saved.');
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
        <Field label="Company Name" error={errors.companyName?.message}>
          <input className="input-base" {...register('companyName')} />
        </Field>

        <Field label="Support Email" error={errors.supportEmail?.message}>
          <input className="input-base" type="email" {...register('supportEmail')} />
        </Field>

        <Field label="Support Phone" error={errors.supportPhone?.message}>
          <input className="input-base" {...register('supportPhone')} />
        </Field>

        <Field label="Website URL" error={errors.websiteUrl?.message}>
          <input className="input-base" placeholder="https://" {...register('websiteUrl')} />
        </Field>
      </div>

      <Field label="Company Address" error={errors.companyAddress?.message}>
        <textarea className="input-base min-h-[80px] resize-y" {...register('companyAddress')} />
      </Field>

      <Field label="Footer Text" error={errors.footerText?.message} hint="Optional">
        <input className="input-base" {...register('footerText')} />
      </Field>

      <div className="flex items-center gap-3 pt-2">
        <button type="submit" disabled={isSubmitting} className="btn-primary">
          {isSubmitting ? <Loader2 className="h-4 w-4 animate-spin" /> : <Save className="h-4 w-4" />}
          Save Changes
        </button>
        {isDirty && !isSubmitting && (
          <span className="text-xs text-amber-600 font-medium">Unsaved changes</span>
        )}
      </div>
    </form>
  );
}

function Field({ label, error, hint, children }) {
  return (
    <label className="block">
      <span className="text-sm font-medium text-slate-700">{label}</span>
      {hint && <span className="text-xs text-slate-400 ml-2">{hint}</span>}
      <div className="mt-1.5">{children}</div>
      {error && <p className="text-xs text-red-500 mt-1">{error}</p>}
    </label>
  );
}
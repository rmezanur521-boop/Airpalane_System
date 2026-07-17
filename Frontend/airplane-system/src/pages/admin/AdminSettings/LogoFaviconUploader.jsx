// src/pages/admin/AdminSettings/LogoFaviconUploader.jsx

import { useRef, useState } from 'react';
import { ImageIcon, Loader2, Trash2, UploadCloud } from 'lucide-react';
import toast from 'react-hot-toast';
import { adminSettingsApi, buildFileUrl, extractErrorMessage } from '@/api/adminSettingsApi';

const ALLOWED_EXTENSIONS = ['jpg', 'jpeg', 'png', 'webp', 'gif', 'ico', 'svg'];
const ACCEPT_ATTR = ALLOWED_EXTENSIONS.map((ext) => `.${ext}`).join(',');
const MAX_SIZE_BYTES = 5 * 1024 * 1024; // 5 MB

function validateFile(file) {
  const ext = file.name.split('.').pop()?.toLowerCase() ?? '';
  if (!ALLOWED_EXTENSIONS.includes(ext)) {
    return 'Only image files (jpg, jpeg, png, webp, gif, ico, svg) are allowed.';
  }
  if (file.size > MAX_SIZE_BYTES) {
    return 'File is too large. Maximum size is 5 MB.';
  }
  return null;
}

export default function LogoFaviconUploader({ kind, title, helperText, currentUrl, onUpdated }) {
  const inputRef = useRef(null);
  const [isUploading, setIsUploading] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  const previewUrl = buildFileUrl(currentUrl);
  const busy = isUploading || isDeleting;

  const handlePick = () => inputRef.current?.click();

  const handleFileChange = async (e) => {
    const file = e.target.files?.[0];
    e.target.value = ''; // একই file পরে আবার select করার জন্য
    if (!file) return;

    const clientError = validateFile(file);
    if (clientError) {
      toast.error(clientError);
      return;
    }

    setIsUploading(true);
    try {
      const updated =
        kind === 'logo'
          ? await adminSettingsApi.uploadLogo(file)
          : await adminSettingsApi.uploadFavicon(file);
      onUpdated(updated);
      toast.success(`${title} uploaded successfully.`);
    } catch (err) {
      toast.error(extractErrorMessage(err));
    } finally {
      setIsUploading(false);
    }
  };

  const handleDelete = async () => {
    setIsDeleting(true);
    try {
      const updated =
        kind === 'logo'
          ? await adminSettingsApi.deleteLogo()
          : await adminSettingsApi.deleteFavicon();
      onUpdated(updated);
      toast.success(`${title} removed.`);
    } catch (err) {
      toast.error(extractErrorMessage(err));
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <div className="flex flex-col sm:flex-row sm:items-center gap-5 py-4">
      <div className="h-20 w-20 shrink-0 rounded-xl border border-slate-200 bg-slate-50 flex items-center justify-center overflow-hidden">
        {isUploading ? (
          <Loader2 className="h-6 w-6 text-brand-500 animate-spin" />
        ) : previewUrl ? (
          <img src={previewUrl} alt={`${title} preview`} className="h-full w-full object-contain" />
        ) : (
          <ImageIcon className="h-7 w-7 text-slate-300" />
        )}
      </div>

      <div className="flex-1 min-w-0">
        <p className="text-sm font-semibold text-slate-800">{title}</p>
        <p className="text-xs text-slate-400 mt-0.5">{helperText}</p>

        <div className="flex items-center gap-2 mt-3">
          <input
            ref={inputRef}
            type="file"
            accept={ACCEPT_ATTR}
            className="hidden"
            onChange={handleFileChange}
          />
          <button
            type="button"
            onClick={handlePick}
            disabled={busy}
            className="btn-secondary text-xs px-3 py-2"
          >
            <UploadCloud className="h-3.5 w-3.5" />
            {previewUrl ? 'Replace' : 'Upload'}
          </button>
          <button
            type="button"
            onClick={handleDelete}
            disabled={busy || !previewUrl}
            className="btn-danger text-xs px-3 py-2"
          >
            {isDeleting ? (
              <Loader2 className="h-3.5 w-3.5 animate-spin" />
            ) : (
              <Trash2 className="h-3.5 w-3.5" />
            )}
            Remove
          </button>
        </div>
      </div>
    </div>
  );
}
import { useEffect, useRef, useState } from 'react';
import { User, Shield, Camera } from 'lucide-react';
import PageWrapper  from '@/components/layout/PageWrapper';
import Input        from '@/components/ui/Input';
import Button       from '@/components/ui/Button';
import Alert        from '@/components/ui/Alert';
import userService  from '@/api/userService';
import { useAuth }  from '@/hooks/useAuth';
import { getInitials } from '@/utils/formatters';
import toast from 'react-hot-toast';

const EMPTY_PASSPORT = { passportNumber: '', issuingCountry: '', issuedDate: '', expiryDate: '' };

const todayIso = () => new Date().toISOString().slice(0, 10);

export default function ProfilePage() {
  const { user, updateUser }           = useAuth();
  const [profile, setProfile]          = useState({ firstName: '', lastName: '', phoneNumber: '' });
  const [passport, setPassport]        = useState(EMPTY_PASSPORT);
  const [savingProfile,  setSavingProfile]  = useState(false);
  const [savingPassport, setSavingPassport] = useState(false);
  const [profileError,   setProfileError]   = useState('');
  const [passportError,  setPassportError]  = useState('');

  // Avatar / profile-picture upload state
  const [avatarUrl, setAvatarUrl] = useState('');
  const [uploadingAvatar, setUploadingAvatar] = useState(false);
  const avatarInputRef = useRef(null);

  const loadProfile = () => {
    userService.getProfile().then(({ data }) => {
      setProfile({
        firstName:   data.firstName ?? '',
        lastName:    data.lastName  ?? '',
        phoneNumber: data.phoneNumber ?? '',
      });
      setAvatarUrl(data.profilePictureUrl ?? '');
      // Prefill passport fields from the existing record, if any — previously
      // this was never populated, so the form always looked empty even when
      // a passport had already been saved.
      setPassport(
        data.passport
          ? {
              passportNumber: data.passport.passportNumber ?? '',
              issuingCountry: data.passport.issuingCountry ?? '',
              issuedDate:     data.passport.issuedDate ?? '',
              expiryDate:     data.passport.expiryDate ?? '',
            }
          : EMPTY_PASSPORT
      );
    }).catch(() => {});
  };

  useEffect(loadProfile, []);

  const handleSaveProfile = async (e) => {
    e.preventDefault();
    setProfileError('');
    setSavingProfile(true);
    try {
      const { data } = await userService.updateProfile(profile);
      updateUser(data);
      toast.success('Profile updated!');
    } catch (err) {
      setProfileError(err.response?.data?.detail ?? 'Update failed.');
    } finally {
      setSavingProfile(false);
    }
  };

  const handleAvatarSelect = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setUploadingAvatar(true);
    try {
      const { data } = await userService.uploadProfileImage(file);
      setAvatarUrl(data.profilePictureUrl ?? '');
      updateUser(data);
      toast.success('Profile picture updated!');
    } catch (err) {
      toast.error(err.response?.data?.detail ?? 'Image upload failed.');
    } finally {
      setUploadingAvatar(false);
      if (avatarInputRef.current) avatarInputRef.current.value = '';
    }
  };

  const validatePassport = () => {
    if (!passport.passportNumber.trim()) return 'Passport number is required.';
    if (!passport.issuingCountry.trim()) return 'Issuing country is required.';
    if (!passport.issuedDate) return 'Issue date is required.';
    if (passport.issuedDate > todayIso()) return 'Issue date cannot be in the future.';
    if (!passport.expiryDate) return 'Expiry date is required.';
    if (passport.expiryDate <= passport.issuedDate) return 'Expiry date must be after the issue date.';
    return '';
  };

  const handleSavePassport = async (e) => {
    e.preventDefault();
    const validationError = validatePassport();
    if (validationError) {
      setPassportError(validationError);
      return;
    }
    setPassportError('');
    setSavingPassport(true);
    try {
      await userService.updatePassport(passport);
      toast.success('Passport updated!');
      loadProfile(); // re-fetch so the form reflects exactly what was persisted
    } catch (err) {
      setPassportError(err.response?.data?.detail ?? 'Update failed.');
    } finally {
      setSavingPassport(false);
    }
  };

  return (
    <PageWrapper>
      <div className="page-container py-10 max-w-2xl">
        <h1 className="section-title mb-8">My Profile</h1>

        {/* Avatar */}
        <div className="card mb-6 flex items-center gap-5">
          <div className="relative h-16 w-16 flex-shrink-0 group">
            <div className="h-16 w-16 rounded-2xl bg-brand-600 flex items-center justify-center
                            text-white text-2xl font-bold overflow-hidden">
              {avatarUrl
                ? <img src={avatarUrl} alt="Profile" className="h-full w-full object-cover" />
                : getInitials(user?.fullName ?? user?.email ?? '')}
            </div>
            <label className="absolute inset-0 flex items-center justify-center rounded-2xl
                              bg-black/50 opacity-0 group-hover:opacity-100 transition cursor-pointer">
              <Camera className="h-5 w-5 text-white" />
              <input ref={avatarInputRef} type="file" accept="image/*" className="hidden"
                onChange={handleAvatarSelect} disabled={uploadingAvatar} />
            </label>
          </div>
          <div>
            <p className="font-bold text-slate-800 text-lg">{user?.fullName}</p>
            <p className="text-slate-500 text-sm">{user?.email}</p>
            <p className="text-xs text-brand-600 font-medium mt-1">{user?.role}</p>
            <p className="text-xs text-slate-400 mt-1">
              {uploadingAvatar ? 'Uploading…' : 'Hover your photo to change it'}
            </p>
          </div>
        </div>

        {/* Profile form */}
        <div className="card mb-6">
          <div className="flex items-center gap-2 mb-5">
            <User className="h-5 w-5 text-brand-600" />
            <h2 className="font-semibold text-slate-800">Personal Information</h2>
          </div>
          {profileError && <Alert type="error" message={profileError} className="mb-4" />}
          <form onSubmit={handleSaveProfile} className="flex flex-col gap-4">
            <div className="grid grid-cols-2 gap-4">
              <Input
                label="First name"
                value={profile.firstName}
                onChange={(e) => setProfile((p) => ({ ...p, firstName: e.target.value }))}
                required
              />
              <Input
                label="Last name"
                value={profile.lastName}
                onChange={(e) => setProfile((p) => ({ ...p, lastName: e.target.value }))}
                required
              />
            </div>
            <Input
              label="Phone number"
              type="tel"
              value={profile.phoneNumber}
              onChange={(e) => setProfile((p) => ({ ...p, phoneNumber: e.target.value }))}
              placeholder="+1 555 000 0000"
            />
            <div className="flex justify-end">
              <Button type="submit" loading={savingProfile}>Save Profile</Button>
            </div>
          </form>
        </div>

        {/* Passport form */}
        <div className="card">
          <div className="flex items-center gap-2 mb-5">
            <Shield className="h-5 w-5 text-brand-600" />
            <h2 className="font-semibold text-slate-800">Passport Information</h2>
          </div>
          {passportError && <Alert type="error" message={passportError} className="mb-4" />}
          <form onSubmit={handleSavePassport} className="flex flex-col gap-4">
            <div className="grid grid-cols-2 gap-4">
              <Input
                label="Passport number"
                value={passport.passportNumber}
                onChange={(e) =>
                  setPassport((p) => ({ ...p, passportNumber: e.target.value }))
                }
                placeholder="AB1234567"
                maxLength={20}
                required
              />
              <Input
                label="Issuing country"
                value={passport.issuingCountry}
                onChange={(e) =>
                  setPassport((p) => ({ ...p, issuingCountry: e.target.value }))
                }
                placeholder="United States"
                maxLength={100}
                required
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <Input
                label="Issue date"
                type="date"
                value={passport.issuedDate}
                max={todayIso()}
                onChange={(e) =>
                  setPassport((p) => ({ ...p, issuedDate: e.target.value }))
                }
                required
              />
              <Input
                label="Expiry date"
                type="date"
                value={passport.expiryDate}
                min={passport.issuedDate || undefined}
                onChange={(e) =>
                  setPassport((p) => ({ ...p, expiryDate: e.target.value }))
                }
                required
              />
            </div>
            <div className="flex justify-end">
              <Button type="submit" loading={savingPassport}>Save Passport</Button>
            </div>
          </form>
        </div>
      </div>
    </PageWrapper>
  );
}
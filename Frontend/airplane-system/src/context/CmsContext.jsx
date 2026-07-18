// src/context/CmsContext.jsx
import { createContext, useContext, useEffect, useState, useCallback } from 'react';
import homepageService from '@/api/cms/homepageService';

const CmsContext = createContext(null);

export function CmsProvider({ children }) {
  const [data, setData]           = useState(null);
  const [loading, setLoading]     = useState(true);
  const [cacheBust, setCacheBust] = useState(Date.now());

  const load = useCallback(() => {
    setLoading(true);
    homepageService.get()
      .then(({ data }) => { setData(data); setCacheBust(Date.now()); })
      .catch(() => setData(null))
      .finally(() => setLoading(false));
  }, []);

  useEffect(load, [load]);

  return (
    <CmsContext.Provider value={{ ...data, loading, reload: load, cacheBust }}>
      {children}
    </CmsContext.Provider>
  );
}

export function useCms() {
  const ctx = useContext(CmsContext);
  if (!ctx) throw new Error('useCms must be used inside <CmsProvider>');
  return ctx;
}
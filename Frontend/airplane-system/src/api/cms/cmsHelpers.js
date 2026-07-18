// src/api/cms/cmsHelpers.js
import axiosInstance from "../axiosInstance";

const multipartConfig = { headers: { "Content-Type": undefined } };

/** একটা module-এর জন্য পুরো CRUD + image + reorder সার্ভিস বানিয়ে দেয়। */
export function buildCmsCrud(basePath) {
  return {
    list: () => axiosInstance.get(basePath),
    getById: (id) => axiosInstance.get(`${basePath}/${id}`),
    create: (payload) => axiosInstance.post(basePath, payload),
    update: (id, payload) => axiosInstance.put(`${basePath}/${id}`, payload),
    remove: (id) => axiosInstance.delete(`${basePath}/${id}`),
    uploadImage: (id, file) => {
      const fd = new FormData();
      fd.append("File", file);
      return axiosInstance.post(`${basePath}/${id}/image`, fd, multipartConfig);
    },
    reorder: (items) => axiosInstance.put(`${basePath}/reorder`, { items }),
  };
}

/** Single-record settings (Navbar / Footer / Homepage Settings) এর জন্য */
export function buildCmsSettings(basePath) {
  return {
    get: () => axiosInstance.get(basePath),
    update: (payload) => axiosInstance.put(basePath, payload),
    uploadLogo: (file) => {
      const fd = new FormData();
      fd.append("File", file);
      return axiosInstance.post(`${basePath}/logo`, fd, multipartConfig);
    },
  };
}

export function extractCmsError(err) {
  const status = err?.response?.status;
  if (status === 401 || status === 403)
    return "Access denied. Please log in as an admin.";
  const data = err?.response?.data;
  if (data?.errors) {
    const first = Object.values(data.errors)[0];
    return Array.isArray(first)
      ? first[0]
      : (data.message ?? "Validation failed.");
  }
  return (
    data?.message ?? err?.message ?? "Something went wrong. Please try again."
  );
}

export function buildCmsImageUrl(path) {
  if (!path) return "";
  if (path.startsWith("http")) return path;
  const host = import.meta.env.VITE_API_HOST ?? "";
  return `${host}${path}`;
}

import axiosInstance from "./axiosInstance";

const BASE = "/admin/settings";

export const adminSettingsApi = {
  getSettings: () => axiosInstance.get(BASE).then((r) => r.data),

  updateGeneralSettings: (payload) =>
    axiosInstance.put(BASE, payload).then((r) => r.data),

  updateSmtpSettings: (payload) =>
    axiosInstance.put(`${BASE}/smtp`, payload).then((r) => r.data),

  uploadLogo: (file) => {
    const formData = new FormData();
    formData.append("File", file);
    return axiosInstance
      .post(`${BASE}/logo`, formData, {
        headers: { "Content-Type": "multipart/form-data" },
      })
      .then((r) => r.data);
  },

  deleteLogo: () => axiosInstance.delete(`${BASE}/logo`).then((r) => r.data),

  uploadFavicon: (file) => {
    const formData = new FormData();
    formData.append("File", file);
    return axiosInstance
      .post(`${BASE}/favicon`, formData, {
        headers: { "Content-Type": "multipart/form-data" },
      })
      .then((r) => r.data);
  },

  deleteFavicon: () =>
    axiosInstance.delete(`${BASE}/favicon`).then((r) => r.data),
};

export function extractErrorMessage(error) {
  const status = error?.response?.status;
  const data = error?.response?.data;

  if (status === 401 || status === 403) {
    return "Access denied. Please log in as an admin.";
  }
  if (data?.message) return data.message;
  if (data?.detail) return data.detail;
  if (error?.message) return error.message;
  return "Something went wrong. Please try again.";
}

export function extractFieldError(error) {
  const data = error?.response?.data;
  if (data?.field && data?.message) {
    return { field: data.field, message: data.message };
  }
  return null;
}

export function buildFileUrl(path) {
  if (!path) return null;
  const host = import.meta.env.VITE_API_HOST ?? "";
  return `${host}${path}`;
}

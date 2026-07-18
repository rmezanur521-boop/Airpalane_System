// src/api/cms/footerSettingsService.js
import axiosInstance from "../axiosInstance";
const BASE = "/admin/cms/footer-settings";
export default {
  get: () => axiosInstance.get(BASE),
  update: (payload) => axiosInstance.put(BASE, payload),
};

// src/api/cms/homepageSettingsService.js
import axiosInstance from "../axiosInstance";
const BASE = "/admin/cms/homepage-settings";
export default {
  get: () => axiosInstance.get(BASE),
  update: (payload) => axiosInstance.put(BASE, payload),
};

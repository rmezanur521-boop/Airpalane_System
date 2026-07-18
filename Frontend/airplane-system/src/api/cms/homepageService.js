// src/api/cms/homepageService.js
// Public composite endpoint — auth লাগে না
import axiosInstance from "../axiosInstance";
export default {
  get: () => axiosInstance.get("/homepage"),
};

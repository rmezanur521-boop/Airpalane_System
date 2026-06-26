import axiosInstance from "./axiosInstance";

const authService = {
  register: (data) => axiosInstance.post("/auth/register", data),

  login: (data) => axiosInstance.post("/auth/login", data),

  refreshToken: (refreshToken) =>
    axiosInstance.post("/auth/refresh-token", { refreshToken }),

  revokeToken: (token) => axiosInstance.post("/auth/revoke-token", { token }),

  verifyEmail: (token, email) =>
    axiosInstance.post("/auth/verify-email", { token, email }),

  forgotPassword: (email) =>
    axiosInstance.post("/auth/forgot-password", { email }),

  resetPassword: (data) => axiosInstance.post("/auth/reset-password", data),

  getMe: () => axiosInstance.get("/auth/me"),
};

export default authService;

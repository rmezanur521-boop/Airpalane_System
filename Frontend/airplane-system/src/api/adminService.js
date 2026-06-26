import axiosInstance from "./axiosInstance";

const adminService = {
  getDashboard: (from, to) =>
    axiosInstance.get("/admin/dashboard", { params: { from, to } }),

  getRevenueReport: (from, to) =>
    axiosInstance.get("/admin/reports/revenue", { params: { from, to } }),

  getBookingReport: (from, to) =>
    axiosInstance.get("/admin/reports/bookings", { params: { from, to } }),

  getAuditLogs: (params) => axiosInstance.get("/admin/audit-logs", { params }),

  createAgent: (data) => axiosInstance.post("/admin/agents", data),

  sendFlightAlert: (flightId, isCancellation) =>
    axiosInstance.post("/admin/notifications/flight-alert", {
      flightId,
      isCancellation,
    }),
};

export default adminService;

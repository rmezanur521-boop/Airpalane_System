import axiosInstance from "./axiosInstance";

const paymentService = {
  createPaymentIntent: (bookingId) =>
    axiosInstance.post("/payments/intent", { bookingId }),

  confirmPayment: (paymentIntentId) =>
    axiosInstance.post("/payments/confirm", { paymentIntentId }),

  validatePromo: (code, cartTotal) =>
    axiosInstance.post("/payments/promo/validate", { code, cartTotal }),

  requestRefund: (bookingId, reason) =>
    axiosInstance.post("/payments/refund", { bookingId, reason }),

  getPaymentById: (id) => axiosInstance.get(`/payments/${id}`),

  processRefund: (id, data) =>
    axiosInstance.patch(`/payments/refund/${id}/process`, data),

  // Admin
  getAllPayments: (params) => axiosInstance.get("/payments/admin", { params }),
};

export default paymentService;

import axiosInstance from "@/api/axiosInstance";

const paymentGatewayService = {
  list: () => axiosInstance.get("/admin/payment-gateways"),
  update: (provider, data) =>
    axiosInstance.put(`/admin/payment-gateways/${provider}`, data),
};

export default paymentGatewayService;

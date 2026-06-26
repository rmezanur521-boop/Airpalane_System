import axiosInstance from "./axiosInstance";

const bookingService = {
  createBooking: (data) => axiosInstance.post("/bookings", data),

  getMyBookings: (params) => axiosInstance.get("/bookings", { params }),

  getBookingById: (id) => axiosInstance.get(`/bookings/${id}`),

  getBookingByReference: (reference) =>
    axiosInstance.get(`/bookings/reference/${reference}`),

  cancelBooking: (id, reason) =>
    axiosInstance.patch(`/bookings/${id}/cancel`, { reason }),

  selectSeat: (id, data) =>
    axiosInstance.post(`/bookings/${id}/select-seat`, data),

  // Admin
  getAllBookings: (params) => axiosInstance.get("/bookings/admin", { params }),
};

export default bookingService;

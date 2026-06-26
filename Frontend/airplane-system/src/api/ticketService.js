import axiosInstance from "./axiosInstance";

const ticketService = {
  getTicketsByBooking: (bookingId) =>
    axiosInstance.get(`/tickets/booking/${bookingId}`),

  getTicketByNumber: (ticketNumber) =>
    axiosInstance.get(`/tickets/${ticketNumber}`),

  downloadTicket: (ticketNumber) =>
    axiosInstance.get(`/tickets/${ticketNumber}/download`, {
      responseType: "blob",
    }),

  getBoardingPass: (ticketNumber) =>
    axiosInstance.get(`/tickets/${ticketNumber}/boarding-pass`, {
      responseType: "blob",
    }),

  checkIn: (ticketNumber) =>
    axiosInstance.post(`/tickets/${ticketNumber}/check-in`),
};

export default ticketService;

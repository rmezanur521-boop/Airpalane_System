import axiosInstance from "./axiosInstance";

const flightService = {
  // Public search
  searchOneWay: (data) => axiosInstance.post("/search/one-way", data),

  searchRoundTrip: (data) => axiosInstance.post("/search/round-trip", data),

  searchMultiCity: (data) => axiosInstance.post("/search/multi-city", data),

  // Flight CRUD (Admin/Agent)
  getFlights: (params) => axiosInstance.get("/flights", { params }),

  getFlightById: (id) => axiosInstance.get(`/flights/${id}`),

  createFlight: (data) => axiosInstance.post("/flights", data),

  updateFlight: (id, data) => axiosInstance.put(`/flights/${id}`, data),

  deleteFlight: (id) => axiosInstance.delete(`/flights/${id}`),

  getFlightSeats: (id) => axiosInstance.get(`/flights/${id}/seats`),

  updateFlightStatus: (id, data) =>
    axiosInstance.patch(`/flights/${id}/status`, data),
};

export default flightService;

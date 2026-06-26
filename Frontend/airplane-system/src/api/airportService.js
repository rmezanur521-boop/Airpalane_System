import axiosInstance from "./axiosInstance";

const airportService = {
  getAirports: (params) => axiosInstance.get("/airports", { params }),

  getAirportByIata: (iataCode) => axiosInstance.get(`/airports/${iataCode}`),

  createAirport: (data) => axiosInstance.post("/airports", data),

  updateAirport: (id, data) => axiosInstance.put(`/airports/${id}`, data),

  deleteAirport: (id) => axiosInstance.delete(`/airports/${id}`),
};

export default airportService;

import axiosInstance from "./axiosInstance";

const airlineService = {
  getAirlines: () => axiosInstance.get("/airlines"),

  getAirlineById: (id) => axiosInstance.get(`/airlines/${id}`),

  createAirline: (data) => axiosInstance.post("/airlines", data),

  updateAirline: (id, data) => axiosInstance.put(`/airlines/${id}`, data),
};

export default airlineService;

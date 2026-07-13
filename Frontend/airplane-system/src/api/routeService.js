import axiosInstance from "./axiosInstance";

const routeService = {
  getRoutes: () => axiosInstance.get("/routes"),
};

export default routeService;

import axiosInstance from "./axiosInstance";

const userService = {
  getProfile: () => axiosInstance.get("/users/profile"),

  updateProfile: (data) => axiosInstance.put("/users/profile", data),

  updatePassport: (data) => axiosInstance.put("/users/passport", data),

  // Admin
  getAllUsers: (params) => axiosInstance.get("/users", { params }),

  getUserById: (id) => axiosInstance.get(`/users/${id}`),

  deleteUser: (id) => axiosInstance.delete(`/users/${id}`),

  setUserActive: (id, isActive) =>
    axiosInstance.patch(`/users/${id}/activate`, { isActive }),
};

export default userService;

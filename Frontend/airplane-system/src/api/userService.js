import axiosInstance from "./axiosInstance";

const userService = {
  getProfile: () => axiosInstance.get("/users/profile"),

  updateProfile: (data) => axiosInstance.put("/users/profile", data),

  // axiosInstance defaults to "Content-Type: application/json" for every request.
  // For a FormData body we must clear that per-request (set to undefined, not
  // omitted) so axios lets the browser set the correct multipart boundary —
  // otherwise it silently JSON-stringifies the FormData (see airlineService.js
  // for the same fix / root-cause note).
  uploadProfileImage: (file) => {
    const fd = new FormData();
    fd.append("file", file); // must match the IFormFile parameter name "file" on UsersController.UploadProfileImage
    return axiosInstance.post("/users/profile/image", fd, {
      headers: { "Content-Type": undefined },
    });
  },
  getPassport: () => axiosInstance.get("/users/passport"),
  updatePassport: (data) => axiosInstance.put("/users/passport", data),

  // Admin
  getAllUsers: (params) => axiosInstance.get("/users", { params }),

  getUserById: (id) => axiosInstance.get(`/users/${id}`),

  deleteUser: (id) => axiosInstance.delete(`/users/${id}`),

  setUserActive: (id, isActive) =>
    axiosInstance.patch(`/users/${id}/activate`, { isActive }),
};

export default userService;

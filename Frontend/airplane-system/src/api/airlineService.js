import axiosInstance from "./axiosInstance";

const buildAirlineFormData = ({
  iataCode,
  name,
  country,
  contactEmail,
  contactPhone,
  logo,
  images,
}) => {
  const fd = new FormData();
  if (iataCode != null) fd.append("IataCode", iataCode);
  if (name != null) fd.append("Name", name);
  if (country != null) fd.append("Country", country);
  if (contactEmail) fd.append("ContactEmail", contactEmail);
  if (contactPhone) fd.append("ContactPhone", contactPhone);
  if (logo) fd.append("Logo", logo);
  (images || []).forEach((file) => fd.append("Images", file));
  return fd;
};

// axiosInstance sets a default "Content-Type: application/json" header for
// normal JSON calls. When the body is FormData, axios's own transformRequest
// only lets the browser set the correct "multipart/form-data; boundary=..."
// header if no Content-Type is already present — otherwise (as here) it
// silently JSON-stringifies the FormData instead of sending it as multipart,
// which is what was causing the 400s. So for every multipart call we must
// explicitly clear the header per-request (set to undefined, not omitted)
// so axios recomputes it correctly for this one request only.
const multipartConfig = { headers: { "Content-Type": undefined } };

const airlineService = {
  getAirlines: () => axiosInstance.get("/airlines"),

  getAirlineById: (id) => axiosInstance.get(`/airlines/${id}`),

  // `data` = { iataCode, name, country, contactEmail, contactPhone, logo (File), images (File[]) }
  createAirline: (data) =>
    axiosInstance.post(
      "/airlines",
      buildAirlineFormData(data),
      multipartConfig,
    ),

  updateAirline: (id, data) =>
    axiosInstance.put(
      `/airlines/${id}`,
      buildAirlineFormData(data),
      multipartConfig,
    ),

  addAirlineImages: (id, files) => {
    const fd = new FormData();
    (files || []).forEach((file) => fd.append("Images", file));
    return axiosInstance.post(`/airlines/${id}/images`, fd, multipartConfig);
  },

  deleteAirlineImage: (id, imageId) =>
    axiosInstance.delete(`/airlines/${id}/images/${imageId}`),
};

export default airlineService;

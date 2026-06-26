export const SEAT_CLASS = {
  ECONOMY: "Economy",
  BUSINESS: "Business",
  FIRST: "First",
};

export const SEAT_CLASS_OPTIONS = [
  { value: SEAT_CLASS.ECONOMY, label: "Economy" },
  { value: SEAT_CLASS.BUSINESS, label: "Business" },
  { value: SEAT_CLASS.FIRST, label: "First Class" },
];

export const TRIP_TYPE = {
  ONE_WAY: "OneWay",
  ROUND_TRIP: "RoundTrip",
  MULTI_CITY: "MultiCity",
};

export const BOOKING_STATUS = {
  PENDING_PAYMENT: "PendingPayment",
  CONFIRMED: "Confirmed",
  CANCELLED: "Cancelled",
  EXPIRED: "Expired",
  REFUNDED: "Refunded",
};

export const FLIGHT_STATUS = {
  SCHEDULED: "Scheduled",
  DELAYED: "Delayed",
  CANCELLED: "Cancelled",
  BOARDING: "Boarding",
  DEPARTED: "Departed",
  ARRIVED: "Arrived",
};

export const PAYMENT_STATUS = {
  PENDING: "Pending",
  SUCCEEDED: "Succeeded",
  FAILED: "Failed",
  CANCELLED: "Cancelled",
  REFUNDED: "Refunded",
};

export const REFUND_STATUS = {
  PENDING: "Pending",
  PROCESSED: "Processed",
  DENIED: "Denied",
};

export const PASSENGER_TYPE = {
  ADULT: "Adult",
  CHILD: "Child",
  INFANT: "Infant",
};

export const USER_ROLE = {
  PASSENGER: "Passenger",
  AGENT: "Agent",
  ADMIN: "Admin",
};

export const BOOKING_STATUS_COLOR = {
  PendingPayment: "yellow",
  Confirmed: "green",
  Cancelled: "red",
  Expired: "slate",
  Refunded: "purple",
};

export const FLIGHT_STATUS_COLOR = {
  Scheduled: "blue",
  Delayed: "yellow",
  Cancelled: "red",
  Boarding: "green",
  Departed: "sky",
  Arrived: "slate",
};

export const PAYMENT_STATUS_COLOR = {
  Pending: "yellow",
  Succeeded: "green",
  Failed: "red",
  Cancelled: "slate",
  Refunded: "purple",
};

import { format, formatDistanceToNow, parseISO } from "date-fns";

// ── Currency ─────────────────────────────────────────────────────────────────
export const formatCurrency = (amount, currency = "USD") => {
  if (amount == null) return "—";
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency,
    maximumFractionDigits: 2,
  }).format(amount);
};

// ── Dates ────────────────────────────────────────────────────────────────────
export const formatDate = (dateStr, fmt = "MMM dd, yyyy") => {
  if (!dateStr) return "—";
  try {
    return format(parseISO(dateStr), fmt);
  } catch {
    return dateStr;
  }
};

export const formatDateTime = (dateStr) =>
  formatDate(dateStr, "MMM dd, yyyy • HH:mm");

export const formatTime = (dateStr) => formatDate(dateStr, "HH:mm");

export const formatDateShort = (dateStr) => formatDate(dateStr, "dd MMM");

export const formatRelative = (dateStr) => {
  if (!dateStr) return "—";
  try {
    return formatDistanceToNow(parseISO(dateStr), { addSuffix: true });
  } catch {
    return dateStr;
  }
};

// ── Duration ─────────────────────────────────────────────────────────────────
export const formatDuration = (minutes) => {
  if (!minutes && minutes !== 0) return "—";
  const h = Math.floor(minutes / 60);
  const m = minutes % 60;
  if (h === 0) return `${m}m`;
  if (m === 0) return `${h}h`;
  return `${h}h ${m}m`;
};

// ── Names ────────────────────────────────────────────────────────────────────
export const getInitials = (name) => {
  if (!name) return "?";
  return name
    .split(" ")
    .map((n) => n[0])
    .slice(0, 2)
    .join("")
    .toUpperCase();
};

// ── Numbers ──────────────────────────────────────────────────────────────────
export const formatNumber = (n) => {
  if (n == null) return "—";
  return new Intl.NumberFormat("en-US").format(n);
};

// ── File download helper ─────────────────────────────────────────────────────
export const downloadBlob = (blob, filename) => {
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.setAttribute("download", filename);
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(url);
};

const AVATAR_COLORS = ['#0d6efd', '#6610f2', '#198754', '#d63384', '#fd7e14', '#20c997', '#6f42c1', '#0dcaf0'];

export function getInitials(name) {
  if (!name) {
    return '?';
  }

  const parts = name.trim().split(/\s+/);
  const first = parts[0]?.charAt(0) ?? '';
  const last = parts.length > 1 ? parts[parts.length - 1].charAt(0) : '';
  return `${first}${last}`.toUpperCase();
}

export function getAvatarColor(seed) {
  const text = String(seed ?? '');
  const total = [...text].reduce((sum, char) => sum + char.charCodeAt(0), 0);
  return AVATAR_COLORS[total % AVATAR_COLORS.length];
}

export function cleanParams(params) {
  return Object.fromEntries(
    Object.entries(params).filter(([, value]) => value !== '' && value !== null && value !== undefined),
  );
}

export function isValidEmail(value) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
}

export function isValidPhone(value) {
  return /^\+?[0-9\s-]{7,20}$/.test(value);
}

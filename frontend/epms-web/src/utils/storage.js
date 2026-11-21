const TOKEN_KEY = 'epms_token';
const USER_KEY = 'epms_user';
const EXPIRES_KEY = 'epms_expires_at';

export function getToken() {
  return localStorage.getItem(TOKEN_KEY);
}

export function saveSession(auth) {
  localStorage.setItem(TOKEN_KEY, auth.token);
  localStorage.setItem(USER_KEY, JSON.stringify(auth.user));
  if (auth.expiresAt) {
    localStorage.setItem(EXPIRES_KEY, auth.expiresAt);
  }
}

export function clearSession() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
  localStorage.removeItem(EXPIRES_KEY);
}

export function getSession() {
  const token = localStorage.getItem(TOKEN_KEY);
  const storedUser = localStorage.getItem(USER_KEY);
  const expiresAt = localStorage.getItem(EXPIRES_KEY);

  if (!token || !storedUser) {
    return null;
  }

  if (expiresAt && new Date(expiresAt) <= new Date()) {
    clearSession();
    return null;
  }

  try {
    return { token, user: JSON.parse(storedUser) };
  } catch {
    clearSession();
    return null;
  }
}

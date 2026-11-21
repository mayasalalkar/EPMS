import { useCallback, useMemo, useState } from 'react';
import authApi from '../api/authApi';
import { AuthContext } from './contexts';
import { clearSession, getSession, saveSession } from '../utils/storage';
import { ROLES } from '../utils/constants';

export function AuthProvider({ children }) {
  const [session, setSession] = useState(() => getSession());

  const startSession = useCallback((auth) => {
    saveSession(auth);
    setSession({ token: auth.token, user: auth.user });
    return auth.user;
  }, []);

  const login = useCallback(
    async (credentials) => {
      const auth = await authApi.login(credentials);
      return startSession(auth);
    },
    [startSession],
  );

  const register = useCallback(
    async (data) => {
      const auth = await authApi.register(data);
      return startSession(auth);
    },
    [startSession],
  );

  const logout = useCallback(() => {
    clearSession();
    setSession(null);
  }, []);

  const value = useMemo(
    () => ({
      user: session?.user ?? null,
      token: session?.token ?? null,
      isAuthenticated: Boolean(session),
      isAdmin: session?.user?.role === ROLES.admin,
      login,
      register,
      logout,
    }),
    [session, login, register, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

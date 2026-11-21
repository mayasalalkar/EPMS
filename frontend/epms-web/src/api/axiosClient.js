import axios from 'axios';
import { clearSession, getToken } from '../utils/storage';

export const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5080';

const axiosClient = axios.create({
  baseURL: `${API_URL}/api`,
  headers: {
    'Content-Type': 'application/json',
  },
});

axiosClient.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

axiosClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const isAuthRequest = error.config?.url?.startsWith('/auth/login') || error.config?.url?.startsWith('/auth/register');

    if (error.response?.status === 401 && !isAuthRequest) {
      clearSession();
      if (window.location.pathname !== '/login') {
        window.location.assign('/login?session=expired');
      }
    }

    return Promise.reject(error);
  },
);

export default axiosClient;

import axiosClient from './axiosClient';

const authApi = {
  login: (credentials) => axiosClient.post('/auth/login', credentials).then((response) => response.data),
  register: (data) => axiosClient.post('/auth/register', data).then((response) => response.data),
  me: () => axiosClient.get('/auth/me').then((response) => response.data),
};

export default authApi;

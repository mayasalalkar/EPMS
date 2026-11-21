import axiosClient from './axiosClient';

const dashboardApi = {
  getSummary: () => axiosClient.get('/dashboard').then((response) => response.data),
};

export default dashboardApi;

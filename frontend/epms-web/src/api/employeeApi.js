import axiosClient from './axiosClient';
import { cleanParams } from '../utils/helpers';

const employeeApi = {
  getAll: (params) => axiosClient.get('/employees', { params: cleanParams(params) }).then((response) => response.data),
  getById: (id) => axiosClient.get(`/employees/${id}`).then((response) => response.data),
  getDepartments: () => axiosClient.get('/employees/departments').then((response) => response.data),
  getLookup: () => axiosClient.get('/employees/lookup').then((response) => response.data),
  create: (data) => axiosClient.post('/employees', data).then((response) => response.data),
  update: (id, data) => axiosClient.put(`/employees/${id}`, data).then((response) => response.data),
  remove: (id) => axiosClient.delete(`/employees/${id}`),
};

export default employeeApi;

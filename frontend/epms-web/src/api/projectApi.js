import axiosClient from './axiosClient';
import { cleanParams } from '../utils/helpers';

const projectApi = {
  getAll: (params) => axiosClient.get('/projects', { params: cleanParams(params) }).then((response) => response.data),
  getById: (id) => axiosClient.get(`/projects/${id}`).then((response) => response.data),
  create: (data) => axiosClient.post('/projects', data).then((response) => response.data),
  update: (id, data) => axiosClient.put(`/projects/${id}`, data).then((response) => response.data),
  remove: (id) => axiosClient.delete(`/projects/${id}`),
  updateAssignments: (id, employeeIds) =>
    axiosClient.put(`/projects/${id}/assignments`, { employeeIds }).then((response) => response.data),
  updateProgress: (id, data) => axiosClient.patch(`/projects/${id}/progress`, data).then((response) => response.data),
};

export default projectApi;

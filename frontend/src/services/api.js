import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Product API
export const productApi = {
  getAll: (pageNumber = 1, pageSize = 10) =>
    api.get(`/api/products?pageNumber=${pageNumber}&pageSize=${pageSize}`),

  getById: (id) =>
    api.get(`/api/products/${id}`),

  create: (product) =>
    api.post('/api/products', product),

  update: (id, product) =>
    api.put(`/api/products/${id}`, product),

  delete: (id) =>
    api.delete(`/api/products/${id}`),
};

// Order API
export const orderApi = {
  getAll: (pageNumber = 1, pageSize = 10) =>
    api.get(`/api/orders?pageNumber=${pageNumber}&pageSize=${pageSize}`),

  getById: (id) =>
    api.get(`/api/orders/${id}`),

  create: (order) =>
    api.post('/api/orders', order),

  updateStatus: (id, status) =>
    api.put(`/api/orders/${id}/status?status=${status}`),
};

export default api;

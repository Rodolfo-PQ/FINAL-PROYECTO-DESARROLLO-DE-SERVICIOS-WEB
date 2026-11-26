import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '/api',
  headers: { 'Content-Type': 'application/json' },
  timeout: 10000,
  withCredentials: true,
});

let refreshRequest;

api.interceptors.request.use(config => {
  const token = localStorage.getItem('autofix_token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(response => response, async error => {
  const original = error.config;
  const authRoute = /\/auth\/(login|refresh|logout)/.test(original?.url || '');
  if (error.response?.status === 401 && original && !original._retry && !authRoute && localStorage.getItem('autofix_token')) {
    original._retry = true;
    try {
      if (!refreshRequest) {
        refreshRequest = api.post('/auth/refresh')
          .then(({ data }) => {
            localStorage.setItem('autofix_token', data.token);
            return data.token;
          })
          .finally(() => { refreshRequest = undefined; });
      }
      const token = await refreshRequest;
      original.headers.Authorization = `Bearer ${token}`;
      return api(original);
    } catch (refreshError) {
      localStorage.removeItem('autofix_token');
      localStorage.removeItem('autofix_usuario');
      localStorage.removeItem('autofix_rol');
      window.location.href = '/login';
      return Promise.reject(refreshError);
    }
  }
  if (error.response?.status === 401 && original && !authRoute) {
    localStorage.removeItem('autofix_token');
    localStorage.removeItem('autofix_usuario');
    localStorage.removeItem('autofix_rol');
    window.location.href = '/login';
  }
  return Promise.reject(error);
});

export function getErrorMessage(error) {
  if (error.response?.data?.errors) return Object.values(error.response.data.errors).flat().join(' ');
  return error.response?.data?.error || error.response?.data?.title || error.message || 'No fue posible completar la solicitud.';
}

export default api;

import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5000/api', // Asumiendo Gateway o Servicio de Identity
});

// Interceptor para añadir el Token en cada petición
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor para manejar Refresh Token
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response && error.response.status === 401 && !originalRequest._retry) {
      // SI es la petición de login, NO intentamos refrescar ni redirigir (evita pestañeo)
      if (originalRequest.url.includes('/Auth/login')) {
          return Promise.reject(error);
      }

      originalRequest._retry = true;
      const refreshToken = localStorage.getItem('refreshToken');
      const accessToken = localStorage.getItem('accessToken');

      try {
        const res = await axios.post('http://localhost:5000/api/Auth/refresh', {
          accessToken: accessToken,
          refreshToken: refreshToken
        });

        if (res.status === 200) {
          localStorage.setItem('accessToken', res.data.accessToken);
          localStorage.setItem('refreshToken', res.data.refreshToken);

          api.defaults.headers.common['Authorization'] = `Bearer ${res.data.accessToken}`;
          return api(originalRequest);
        }
      } catch (refreshError) {
        localStorage.clear();
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

export default api;

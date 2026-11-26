import api from './api';

// Mantiene las rutas en un solo lugar para reutilizar las operaciones CRUD.
const paths = {
  clientes: '/clientes',
  vehiculos: '/vehiculos',
  servicios: '/servicios',
};

// Cada función devuelve los datos HTTP o deja que Axios propague el error al formulario.
export const list = (resource) => api.get(paths[resource]).then(({ data }) => data);
export const create = (resource, input) => api.post(paths[resource], input).then(({ data }) => data);
export const update = (resource, id, input) => api.put(`${paths[resource]}/${id}`, input).then(({ data }) => data);
export const remove = (resource, id) => api.delete(`${paths[resource]}/${id}`);

import { createElement } from 'react';

// Configura columnas, campos, búsquedas y conversión de datos para el CRUD genérico.
export const entityConfig = {
  // Cliente no tiene relaciones seleccionables; su payload coincide con los campos editables.
  clientes: {
    title: 'Clientes',
    singular: 'cliente',
    icon: 'users',
    description: 'Personas y datos de contacto registrados en el taller.',
    columns: [
      { key: 'nombreCompleto', label: 'Cliente' },
      { key: 'email', label: 'Correo' },
      { key: 'telefono', label: 'Teléfono' },
      { key: 'fechaRegistro', label: 'Registro' },
    ],
    fields: [
      { name: 'Nombre', label: 'Nombre', required: true, minLength: 2, maxLength: 80 },
      { name: 'Apellido', label: 'Apellido', required: true, minLength: 2, maxLength: 80 },
      { name: 'Email', label: 'Correo electrónico', type: 'email', required: true, maxLength: 160 },
      { name: 'Telefono', label: 'Teléfono', type: 'tel', required: true, minLength: 7, maxLength: 30 },
    ],
    searchText: (item) => `${item.nombre} ${item.apellido} ${item.email} ${item.telefono}`,
    row: (item) => ({
      nombreCompleto: `${item.nombre} ${item.apellido}`,
      email: item.email,
      telefono: item.telefono,
      fechaRegistro: formatDate(item.fechaRegistro),
    }),
    payload: (form) => form,
  },
  // Vehículo requiere un propietario y transforma IDs/año desde los controles de texto.
  vehiculos: {
    title: 'Vehículos',
    singular: 'vehículo',
    icon: 'car',
    description: 'Vehículos vinculados a sus propietarios.',
    columns: [
      { key: 'placa', label: 'Placa' },
      { key: 'vehiculo', label: 'Vehículo' },
      { key: 'propietario', label: 'Propietario' },
      { key: 'anio', label: 'Año' },
      { key: 'color', label: 'Color' },
    ],
    fields: [
      { name: 'ClienteId', label: 'Propietario', type: 'client', required: true },
      { name: 'Placa', label: 'Placa', required: true, pattern: '[A-Za-z0-9 -]{5,12}', placeholder: 'ABC-123' },
      { name: 'Marca', label: 'Marca', required: true, minLength: 2, maxLength: 60 },
      { name: 'Modelo', label: 'Modelo', required: true, maxLength: 80 },
      { name: 'Anio', label: 'Año', type: 'number', required: true, min: 1900, max: 2100 },
      { name: 'Color', label: 'Color', required: true, minLength: 2, maxLength: 40 },
    ],
    searchText: (item, refs) => `${item.placa} ${item.marca} ${item.modelo} ${item.color} ${clientName(refs.clientes, item.clienteId)}`,
    row: (item, refs) => ({
      placa: createElement('strong', { className: 'plate-tag' }, item.placa),
      vehiculo: `${item.marca} ${item.modelo}`,
      propietario: clientName(refs.clientes, item.clienteId),
      anio: item.anio,
      color: item.color,
    }),
    payload: (form) => ({ ...form, ClienteId: Number(form.ClienteId), Anio: Number(form.Anio) }),
  },
  // Servicio usa las referencias para limitar vehículos y convierte fecha/costo al enviar.
  servicios: {
    title: 'Servicios',
    singular: 'servicio',
    icon: 'wrench',
    description: 'Órdenes de trabajo y seguimiento de mantenimiento.',
    columns: [
      { key: 'id', label: 'Orden' },
      { key: 'cliente', label: 'Cliente / vehículo' },
      { key: 'mecanico', label: 'Mecánico' },
      { key: 'equipo', label: 'Equipo asignado' },
      { key: 'descripcion', label: 'Descripción' },
      { key: 'tipo', label: 'Tipo' },
      { key: 'fecha', label: 'Ingreso' },
      { key: 'costo', label: 'Estimado' },
      { key: 'estado', label: 'Estado' },
    ],
    fields: [
      { name: 'ClienteId', label: 'Cliente', type: 'client', required: true },
      { name: 'VehiculoId', label: 'Vehículo', type: 'vehicle', required: true },
      { name: 'MecanicoId', label: 'Mecánico responsable', type: 'mechanic' },
      { name: 'MecanicoIds', label: 'Equipo de trabajo', type: 'mechanics' },
      { name: 'TipoMantenimiento', label: 'Tipo de mantenimiento', type: 'select', required: true, options: ['Preventivo', 'Correctivo', 'Diagnóstico', 'Revisión'] },
      { name: 'Descripcion', label: 'Descripción del trabajo', type: 'textarea', required: true, minLength: 5, maxLength: 500, full: true },
      { name: 'FechaIngreso', label: 'Fecha de ingreso', type: 'date', required: true },
      { name: 'CostoEstimado', label: 'Costo estimado', type: 'number', required: true, min: 0, max: 100000000, step: '0.01' },
      { name: 'Estado', label: 'Estado', type: 'select', required: true, options: ['Programado', 'En proceso', 'Finalizado', 'Cancelado'] },
    ],
    searchText: (item, refs) => `${item.descripcion} ${item.tipoMantenimiento} ${item.estado} ${clientName(refs.clientes, item.clienteId)} ${vehicleName(refs.vehiculos, item.vehiculoId)}`,
    row: (item, refs) => ({
      id: createElement('span', { className: 'order-id' }, `#${String(item.id).padStart(4, '0')}`),
      cliente: createElement('span', null,
        clientName(refs.clientes, item.clienteId),
        createElement('small', { className: 'd-block text-muted' }, vehicleName(refs.vehiculos, item.vehiculoId))),
      mecanico: refs.mecanicos.find((mecanico) => mecanico.id === item.mecanicoId)?.usuario || 'Sin asignar',
      equipo: (item.mecanicosAsignados || []).map((assignment) => refs.mecanicos.find((mecanico) => mecanico.id === assignment.mecanicoId)?.usuario || `#${assignment.mecanicoId}`).join(', ') || 'Sin asignar',
      descripcion: item.descripcion,
      tipo: item.tipoMantenimiento,
      fecha: formatDate(item.fechaIngreso),
      costo: new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(item.costoEstimado),
      estado: createElement('span', { className: `status-badge status-${item.estado.toLowerCase().replace(' ', '-')}` }, item.estado),
    }),
    payload: (form) => ({ ...form, ClienteId: Number(form.ClienteId), VehiculoId: Number(form.VehiculoId), MecanicoId: form.MecanicoId ? Number(form.MecanicoId) : null, MecanicoIds: form.MecanicoIds.map(Number), CostoEstimado: Number(form.CostoEstimado), FechaIngreso: new Date(`${form.FechaIngreso}T12:00:00`).toISOString() }),
  },
};

// Resuelve los nombres de relaciones para mostrar datos legibles en las tablas.
export function clientName(clients = [], id) {
  const client = clients.find((item) => item.id === id);
  return client ? `${client.nombre} ${client.apellido}` : 'Sin cliente';
}

export function vehicleName(vehicles = [], id) {
  const vehicle = vehicles.find((item) => item.id === id);
  return vehicle ? `${vehicle.marca} ${vehicle.modelo} · ${vehicle.placa}` : 'Sin vehículo';
}

// Presenta las fechas ISO del backend sin desplazar el día por la zona horaria.
export function formatDate(value) {
  return new Intl.DateTimeFormat('es-CO', { dateStyle: 'medium', timeZone: 'UTC' }).format(new Date(value));
}

import { useEffect, useMemo, useState } from 'react';
import { CarFront, ClipboardList, Wrench } from 'lucide-react';
import api, { getErrorMessage } from '../services/api';
import { formatDate } from '../services/entityConfig';

export function PortalCliente() {
  const [data, setData] = useState({ vehiculos: [], servicios: [] });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([api.get('/portal/mis-vehiculos'), api.get('/portal/mis-servicios')])
      .then(([vehiculos, servicios]) => setData({
        vehiculos: vehiculos.data,
        servicios: servicios.data,
      }))
      .catch((requestError) => setError(getErrorMessage(requestError)))
      .finally(() => setLoading(false));
  }, []);

  return (
    <section className="page-section">
      <div className="page-heading"><div><div className="eyebrow">PORTAL DEL CLIENTE</div><h1>Mis vehículos y servicios</h1><p>Consulta tus vehículos y el historial de trabajos del taller.</p></div></div>
      {error && <div className="state-panel state-error">{error}</div>}
      {loading ? <div className="state-panel">Cargando tu información...</div> : <>
        <div className="stats-grid">
          <article className="stat-card stat-green"><div className="stat-card-top"><span>Mis vehículos</span><CarFront size={19} /></div><div className="stat-value">{data.vehiculos.length}</div></article>
          <article className="stat-card stat-blue"><div className="stat-card-top"><span>Historial de servicios</span><Wrench size={19} /></div><div className="stat-value">{data.servicios.length}</div></article>
        </div>
        <section className="recent-panel mt-4">
          <div className="section-heading"><div><span className="eyebrow">MIS VEHÍCULOS</span><h2>Vehículos registrados</h2></div></div>
          {data.vehiculos.length ? <div className="table-responsive"><table className="table records-table align-middle"><thead><tr><th>Placa</th><th>Marca</th><th>Modelo</th><th>Año</th><th>Color</th></tr></thead><tbody>
            {data.vehiculos.map((vehiculo) => <tr key={vehiculo.id}><td>{vehiculo.placa}</td><td>{vehiculo.marca}</td><td>{vehiculo.modelo}</td><td>{vehiculo.anio}</td><td>{vehiculo.color}</td></tr>)}
          </tbody></table></div> : <div className="recent-empty">Aún no tienes vehículos registrados.</div>}
        </section>
        <section className="recent-panel mt-4">
          <div className="section-heading"><div><span className="eyebrow">HISTORIAL</span><h2>Mis órdenes de servicio</h2></div><ClipboardList size={19} className="heading-icon" /></div>
          {data.servicios.length ? <div className="table-responsive"><table className="table records-table align-middle"><thead><tr><th>Orden</th><th>Descripción</th><th>Tipo</th><th>Ingreso</th><th>Estado</th><th>Estimado</th></tr></thead><tbody>
            {data.servicios.map((servicio) => <tr key={servicio.id}><td>#{servicio.id}</td><td>{servicio.descripcion}</td><td>{servicio.tipoMantenimiento}</td><td>{formatDate(servicio.fechaIngreso)}</td><td>{servicio.estado}</td><td>{new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(servicio.costoEstimado)}</td></tr>)}
          </tbody></table></div> : <div className="recent-empty">Aún no tienes servicios registrados.</div>}
        </section>
      </>}
    </section>
  );
}

export function PortalMecanico() {
  const [data, setData] = useState({ clientes: [], servicios: [], vehiculos: [] });
  const [form, setForm] = useState({ clienteId: '', vehiculoId: '', descripcion: '', tipoMantenimiento: 'Diagnóstico', fechaIngreso: new Date().toISOString().slice(0, 10), costoEstimado: '0' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);

  async function loadData() {
    setLoading(true);
    setError('');
    try {
      const [clientes, servicios, vehiculos] = await Promise.all([
        api.get('/mecanico/mis-clientes'),
        api.get('/mecanico/mis-servicios'),
        api.get('/mecanico/mis-vehiculos'),
      ]);
      setData({ clientes: clientes.data, servicios: servicios.data, vehiculos: vehiculos.data });
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => { loadData(); }, []);

  const assignedVehicles = useMemo(() => data.vehiculos.filter((vehicle) => String(vehicle.clienteId) === String(form.clienteId)), [data.vehiculos, form.clienteId]);

  async function submitService(event) {
    event.preventDefault();
    setError('');
    try {
      await api.post('/mecanico/servicios', {
        clienteId: Number(form.clienteId),
        vehiculoId: Number(form.vehiculoId),
        descripcion: form.descripcion,
        tipoMantenimiento: form.tipoMantenimiento,
        fechaIngreso: new Date(`${form.fechaIngreso}T12:00:00`).toISOString(),
        costoEstimado: Number(form.costoEstimado),
        estado: 'Programado',
      });
      setForm((current) => ({ ...current, descripcion: '', costoEstimado: '0' }));
      await loadData();
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    }
  }

  return (
    <section className="page-section">
      <div className="page-heading"><div><div className="eyebrow">ESPACIO DEL MECÁNICO</div><h1>Mis asignaciones</h1><p>Consulta los clientes asignados y registra órdenes para sus vehículos.</p></div></div>
      {error && <div className="state-panel state-error">{error}</div>}
      {loading ? <div className="state-panel">Cargando tus asignaciones...</div> : <>
        {!data.clientes.length && <div className="state-panel">Aún no tienes asignaciones. El administrador debe asignarte una orden para habilitar clientes y vehículos.</div>}
        <div className="stats-grid">
          <article className="stat-card stat-orange"><div className="stat-card-top"><span>Clientes asignados</span><CarFront size={19} /></div><div className="stat-value">{data.clientes.length}</div></article>
          <article className="stat-card stat-blue"><div className="stat-card-top"><span>Órdenes asignadas</span><Wrench size={19} /></div><div className="stat-value">{data.servicios.length}</div></article>
        </div>
        {!!data.clientes.length && <section className="recent-panel mt-4">
          <div className="section-heading"><div><span className="eyebrow">REGISTRO</span><h2>Nueva orden de trabajo</h2></div></div>
          <form className="row g-3 p-3" onSubmit={submitService}>
            <div className="col-md-6"><label className="form-label" htmlFor="clienteId">Cliente asignado</label><select className="form-select" id="clienteId" required value={form.clienteId} onChange={(event) => setForm((current) => ({ ...current, clienteId: event.target.value, vehiculoId: '' }))}><option value="">Selecciona un cliente</option>{data.clientes.map((cliente) => <option key={cliente.id} value={cliente.id}>{cliente.nombre} {cliente.apellido}</option>)}</select></div>
            <div className="col-md-6"><label className="form-label" htmlFor="vehiculoId">Vehículo asignado</label><select className="form-select" id="vehiculoId" required value={form.vehiculoId} onChange={(event) => setForm((current) => ({ ...current, vehiculoId: event.target.value }))}><option value="">Selecciona un vehículo</option>{assignedVehicles.map((vehicle) => <option key={vehicle.id} value={vehicle.id}>{vehicle.marca} {vehicle.modelo} · {vehicle.placa}</option>)}</select></div>
            <div className="col-md-6"><label className="form-label" htmlFor="tipoMantenimiento">Tipo</label><select className="form-select" id="tipoMantenimiento" value={form.tipoMantenimiento} onChange={(event) => setForm((current) => ({ ...current, tipoMantenimiento: event.target.value }))}>{['Preventivo', 'Correctivo', 'Diagnóstico', 'Revisión'].map((type) => <option key={type}>{type}</option>)}</select></div>
            <div className="col-md-6"><label className="form-label" htmlFor="fechaIngreso">Fecha de ingreso</label><input className="form-control" id="fechaIngreso" type="date" required value={form.fechaIngreso} onChange={(event) => setForm((current) => ({ ...current, fechaIngreso: event.target.value }))} /></div>
            <div className="col-md-8"><label className="form-label" htmlFor="descripcion">Descripción</label><input className="form-control" id="descripcion" minLength="5" maxLength="500" required value={form.descripcion} onChange={(event) => setForm((current) => ({ ...current, descripcion: event.target.value }))} /></div>
            <div className="col-md-4"><label className="form-label" htmlFor="costoEstimado">Costo estimado</label><input className="form-control" id="costoEstimado" type="number" min="0" step="0.01" required value={form.costoEstimado} onChange={(event) => setForm((current) => ({ ...current, costoEstimado: event.target.value }))} /></div>
            <div className="col-12"><button className="button-primary" type="submit">Registrar servicio</button></div>
          </form>
        </section>}
        <section className="recent-panel mt-4">
          <div className="section-heading"><div><span className="eyebrow">SEGUIMIENTO</span><h2>Mis órdenes de servicio</h2></div></div>
          {data.servicios.length ? <div className="table-responsive"><table className="table records-table align-middle"><thead><tr><th>Orden</th><th>Cliente</th><th>Vehículo</th><th>Descripción</th><th>Estado</th></tr></thead><tbody>
            {data.servicios.map((servicio) => {
              const client = data.clientes.find((item) => item.id === servicio.clienteId);
              const vehicle = data.vehiculos.find((item) => item.id === servicio.vehiculoId);
              return <tr key={servicio.id}><td>#{servicio.id}</td><td>{client ? `${client.nombre} ${client.apellido}` : `#${servicio.clienteId}`}</td><td>{vehicle ? `${vehicle.marca} ${vehicle.modelo} · ${vehicle.placa}` : `#${servicio.vehiculoId}`}</td><td>{servicio.descripcion}</td><td>{servicio.estado}</td></tr>;
            })}
          </tbody></table></div> : <div className="recent-empty">No tienes órdenes asignadas todavía.</div>}
        </section>
      </>}
    </section>
  );
}

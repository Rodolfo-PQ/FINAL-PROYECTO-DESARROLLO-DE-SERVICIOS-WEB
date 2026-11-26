import { useMemo, useState } from 'react';
import { CarFront, Plus, Search, Trash2, UsersRound, Wrench, Pencil, X } from 'lucide-react';
import Swal from 'sweetalert2';
import useWorkshopData from '../hooks/useWorkshopData';
import { getErrorMessage } from '../services/api';
import { create, remove, update } from '../services/resources';
import { entityConfig } from '../services/entityConfig';

const icons = { users: UsersRound, car: CarFront, wrench: Wrench };

// Crea valores iniciales o adapta un registro del API al formato de los controles HTML.
function initialForm(record, fields) {
  if (!record) {
    const empty = Object.fromEntries(fields.map((field) => [field.name, '']));
    fields.filter((field) => field.type === 'mechanics').forEach((field) => { empty[field.name] = []; });
    if (empty.FechaIngreso !== undefined) empty.FechaIngreso = new Date().toISOString().slice(0, 10);
    if (empty.Estado !== undefined) empty.Estado = 'Programado';
    return empty;
  }
  return Object.fromEntries(fields.map((field) => {
    const key = field.name[0].toLowerCase() + field.name.slice(1);
    let value = field.type === 'mechanics'
      ? (record.mecanicosAsignados || []).map((assignment) => String(assignment.mecanicoId))
      : record[key] ?? '';
    if (field.type === 'date' && value) value = String(value).slice(0, 10);
    return [field.name, field.type === 'mechanics' ? value : value === '' ? '' : String(value)];
  }));
}

// Renderiza búsqueda, tabla, filtros y formulario para cualquiera de las entidades.
export default function RecordsPage({ resource }) {
  const config = entityConfig[resource];
  const Icon = icons[config.icon];
  const { data, loading, error, refresh } = useWorkshopData();
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState('Todos');
  const [editing, setEditing] = useState(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState(() => initialForm(null, config.fields));
  const [saving, setSaving] = useState(false);

  const rows = data[resource];
  // Aplica búsqueda de texto y, para órdenes, el filtro de estado seleccionado.
  const filtered = useMemo(() => rows.filter((item) => {
    const matchesSearch = config.searchText(item, data).toLocaleLowerCase('es').includes(search.toLocaleLowerCase('es'));
    const matchesStatus = resource !== 'servicios' || status === 'Todos' || item.estado === status;
    return matchesSearch && matchesStatus;
  }), [rows, data, search, status, config, resource]);

  function openForm(record = null) {
    setEditing(record);
    setForm(initialForm(record, config.fields));
    setModalOpen(true);
  }

  // Limpia el formulario y oculta el diálogo, tanto al cancelar como al terminar.
  function closeForm() {
    setEditing(null);
    setModalOpen(false);
    setForm(initialForm(null, config.fields));
  }

  // Valida el formulario, convierte los tipos del payload y ejecuta POST o PUT.
  async function handleSubmit(event) {
    event.preventDefault();
    if (!event.currentTarget.reportValidity()) return;
    setSaving(true);
    try {
      const payload = config.payload(form);
      if (editing) await update(resource, editing.id, payload);
      else await create(resource, payload);
      closeForm();
      refresh();
      await Swal.fire({ icon: 'success', title: editing ? 'Cambios guardados' : 'Registro creado', text: `El ${config.singular} se guardó correctamente.`, confirmButtonText: 'Entendido', confirmButtonColor: '#ef732e' });
    } catch (requestError) {
      await Swal.fire({ icon: 'error', title: 'No se pudo guardar', text: getErrorMessage(requestError), confirmButtonColor: '#ef732e' });
    } finally {
      setSaving(false);
    }
  }

  // Pide confirmación y comunica conflictos del servidor, como relaciones existentes.
  async function handleDelete(record) {
    const confirmation = await Swal.fire({
      icon: 'warning',
      title: `¿Eliminar ${config.singular}?`,
      text: 'Esta acción no se puede deshacer.',
      showCancelButton: true,
      confirmButtonText: 'Eliminar',
      cancelButtonText: 'Conservar',
      confirmButtonColor: '#c8463b',
      cancelButtonColor: '#66717c',
    });
    if (!confirmation.isConfirmed) return;
    try {
      await remove(resource, record.id);
      refresh();
      await Swal.fire({ icon: 'success', title: 'Registro eliminado', timer: 1300, showConfirmButton: false });
    } catch (requestError) {
      await Swal.fire({ icon: 'error', title: 'No se pudo eliminar', text: getErrorMessage(requestError), confirmButtonColor: '#ef732e' });
    }
  }

  // Genera el control correspondiente y limita vehículos al cliente elegido.
  function renderField(field) {
    const common = {
      id: field.name,
      name: field.name,
      value: form[field.name] ?? '',
      required: field.required,
      minLength: field.minLength,
      maxLength: field.maxLength,
      min: field.min,
      max: field.max,
      step: field.step,
      pattern: field.pattern,
      placeholder: field.placeholder,
      onChange: (event) => setForm((current) => ({ ...current, [field.name]: event.target.value, ...(field.name === 'ClienteId' && current.VehiculoId ? { VehiculoId: '' } : {}) })),
      className: 'form-control form-control-lg',
    };
    if (field.type === 'client') {
      return <select {...common}><option value="">Selecciona un cliente</option>{data.clientes.map((client) => <option key={client.id} value={client.id}>{client.nombre} {client.apellido}</option>)}</select>;
    }
    if (field.type === 'vehicle') {
      const vehicles = data.vehiculos.filter((vehicle) => String(vehicle.clienteId) === String(form.ClienteId));
      return <select {...common} disabled={!form.ClienteId}><option value="">Selecciona un vehículo</option>{vehicles.map((vehicle) => <option key={vehicle.id} value={vehicle.id}>{vehicle.marca} {vehicle.modelo} · {vehicle.placa}</option>)}</select>;
    }
    if (field.type === 'mechanic') {
      return <select {...common}><option value="">Sin asignar</option>{data.mecanicos.map((mechanic) => <option key={mechanic.id} value={mechanic.id}>{mechanic.usuario}</option>)}</select>;
    }
    if (field.type === 'mechanics') {
      return <select {...common} multiple size={Math.min(5, Math.max(2, data.mecanicos.length))} onChange={(event) => setForm((current) => ({ ...current, [field.name]: Array.from(event.target.selectedOptions, (option) => option.value) }))}>{data.mecanicos.map((mechanic) => <option key={mechanic.id} value={mechanic.id}>{mechanic.usuario}</option>)}</select>;
    }
    if (field.type === 'select') {
      return <select {...common}><option value="">Selecciona una opción</option>{field.options.map((option) => <option key={option} value={option}>{option}</option>)}</select>;
    }
    if (field.type === 'textarea') return <textarea {...common} rows={3} />;
    return <input {...common} type={field.type || 'text'} />;
  }

  return (
    <section className="page-section">
      <div className="page-heading">
        <div>
          <div className="eyebrow"><Icon size={14} /> ADMINISTRACIÓN</div>
          <h1>{config.title}</h1>
          <p>{config.description}</p>
        </div>
        <button className="button-primary" type="button" onClick={() => openForm()}><Plus size={17} /> Nuevo {config.singular}</button>
      </div>

      <div className="toolbar">
        <div className="search-field"><Search size={17} /><input aria-label={`Buscar ${config.title.toLowerCase()}`} placeholder="Buscar por nombre, placa o detalle..." value={search} onChange={(event) => setSearch(event.target.value)} /></div>
        {resource === 'servicios' && <select className="filter-select" aria-label="Filtrar por estado" value={status} onChange={(event) => setStatus(event.target.value)}><option>Todos</option><option>Programado</option><option>En proceso</option><option>Finalizado</option><option>Cancelado</option></select>}
        <span className="result-count">{loading || error ? '—' : `${filtered.length} ${filtered.length === 1 ? 'registro' : 'registros'}`}</span>
      </div>

      {loading ? <div className="state-panel"><span className="spinner-border spinner-border-sm" /> Cargando datos del taller...</div> : error ? <div className="state-panel state-error"><strong>La API no está disponible</strong><span>{error}</span><button className="button-outline" onClick={refresh}>Reintentar</button></div> : (
        <div className="table-frame">
          <div className="table-responsive">
            <table className="table records-table align-middle mb-0">
              <thead><tr>{config.columns.map((column) => <th key={column.key}>{column.label}</th>)}<th className="actions-heading">Acciones</th></tr></thead>
              <tbody>
                {filtered.map((item) => {
                  const values = config.row(item, data);
                  return <tr key={item.id}>{config.columns.map((column) => <td key={column.key}>{values[column.key]}</td>)}<td><div className="row-actions"><button className="icon-button" aria-label={`Editar ${config.singular}`} title="Editar" onClick={() => openForm(item)}><Pencil size={16} /></button><button className="icon-button danger" aria-label={`Eliminar ${config.singular}`} title="Eliminar" onClick={() => handleDelete(item)}><Trash2 size={16} /></button></div></td></tr>;
                })}
                {filtered.length === 0 && <tr><td colSpan={config.columns.length + 1}><div className="empty-state"><span className="empty-icon"><Icon size={22} /></span><strong>{rows.length ? 'No hay coincidencias' : `Aún no hay ${config.title.toLowerCase()}`}</strong><span>{rows.length ? 'Prueba con otra búsqueda o filtro.' : 'Crea el primer registro para empezar a organizar el taller.'}</span>{!rows.length && <button className="button-outline" onClick={() => openForm()}>Crear primer registro</button>}</div></td></tr>}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {modalOpen && <div className="modal-backdrop-custom" onClick={(event) => event.target === event.currentTarget && closeForm()} />}
      {modalOpen && <div className="record-modal" role="dialog" aria-modal="true" aria-labelledby="record-modal-title">
        <div className="modal-header-custom"><div><span className="eyebrow">{editing ? 'ACTUALIZAR REGISTRO' : 'NUEVO REGISTRO'}</span><h2 id="record-modal-title">{editing ? 'Editar' : 'Registrar'} {config.singular}</h2></div><button className="icon-button" aria-label="Cerrar" onClick={closeForm}><X size={19} /></button></div>
        <form onSubmit={handleSubmit}>
          <div className="modal-fields">
            {config.fields.map((field) => <div key={field.name} className={`field-group${field.full ? ' field-full' : ''}`}><label htmlFor={field.name}>{field.label}{field.required && <span className="required-mark"> *</span>}</label>{renderField(field)}</div>)}
          </div>
          <div className="modal-footer-custom"><button type="button" className="button-outline" onClick={closeForm}>Cancelar</button><button type="submit" className="button-primary" disabled={saving}>{saving && <span className="spinner-border spinner-border-sm" />}{saving ? 'Guardando...' : editing ? 'Guardar cambios' : 'Crear registro'}</button></div>
        </form>
      </div>}
    </section>
  );
}

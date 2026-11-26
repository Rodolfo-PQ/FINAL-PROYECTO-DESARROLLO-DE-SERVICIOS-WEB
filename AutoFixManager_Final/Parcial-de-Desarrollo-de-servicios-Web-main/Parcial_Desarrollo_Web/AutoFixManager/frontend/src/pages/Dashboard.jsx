import { ArrowUpRight, CalendarClock, CarFront, Check, CircleDashed, ClipboardList, UsersRound, Wrench } from 'lucide-react';
import { Link } from 'react-router-dom';
import useWorkshopData from '../hooks/useWorkshopData';
import { clientName, formatDate, vehicleName } from '../services/entityConfig';

const statusItems = [
  { name: 'Programado', className: 'scheduled', icon: CalendarClock },
  { name: 'En proceso', className: 'in-progress', icon: CircleDashed },
  { name: 'Finalizado', className: 'finished', icon: Check },
  { name: 'Cancelado', className: 'cancelled', icon: ClipboardList },
];

// Resume totales y estados usando únicamente registros recibidos desde la API.
export default function Dashboard() {
  const { data, loading, error, refresh } = useWorkshopData();
  // Calcula los indicadores de trabajo con el estado actual de las órdenes.
  const countByState = (state) => data.servicios.filter((item) => item.estado === state).length;
  const stats = [
    { label: 'Clientes registrados', value: data.clientes.length, icon: UsersRound, accent: 'orange', note: 'Directorio del taller' },
    { label: 'Vehículos', value: data.vehiculos.length, icon: CarFront, accent: 'green', note: 'Unidades registradas' },
    { label: 'Órdenes de servicio', value: data.servicios.length, icon: Wrench, accent: 'blue', note: 'Historial completo' },
  ];

  return (
    <section className="page-section dashboard-page">
      <div className="dashboard-intro">
        <div>
          <div className="eyebrow"><span className="eyebrow-line" /> CENTRO DE OPERACIONES</div>
          <h1>El taller, <span>en marcha.</span></h1>
          <p>Un vistazo al estado de tu operación y el trabajo que viene.</p>
        </div>
        <Link to="/servicios" className="button-primary"><PlusIcon /> Nueva orden</Link>
      </div>

      {error && <div className="api-banner"><span><strong>Sin conexión con la API.</strong> {error}</span><button onClick={refresh}>Reintentar</button></div>}
      <div className="stats-grid">
        {stats.map(({ label, value, icon: Icon, accent, note }) => <article className={`stat-card stat-${accent}`} key={label}>
          <div className="stat-card-top"><span>{label}</span><span className="stat-icon"><Icon size={19} /></span></div>
          <div className="stat-value">{loading || error ? '—' : value}</div>
          <div className="stat-note"><span className="stat-indicator" />{note}</div>
        </article>)}
      </div>

      <div className="dashboard-lower-grid">
        <section className="work-status-panel">
          <div className="section-heading"><div><span className="eyebrow">SEGUIMIENTO</span><h2>Estado de servicios</h2></div><Link to="/servicios" className="text-link">Ver órdenes <ArrowUpRight size={15} /></Link></div>
          <div className="status-list">
            {statusItems.map(({ name, className, icon: Icon }) => <div className="status-row" key={name}>
              <span className={`status-icon status-icon-${className}`}><Icon size={17} /></span>
              <span className="status-name">{name}</span>
              <span className="status-number">{loading || error ? '—' : countByState(name)}</span>
              <span className="status-track"><i style={{ width: `${loading || error || !data.servicios.length ? 0 : Math.max(5, countByState(name) / data.servicios.length * 100)}%` }} /></span>
            </div>)}
          </div>
          <div className="panel-footnote"><span /> Datos de la operación actual</div>
        </section>

        <section className="recent-panel">
          <div className="section-heading"><div><span className="eyebrow">ACTIVIDAD RECIENTE</span><h2>Últimas órdenes</h2></div><ClipboardList size={19} className="heading-icon" /></div>
          {loading ? <div className="recent-empty">Cargando actividad...</div> : error ? <div className="recent-empty"><strong>Actividad no disponible</strong><span>Conecta con la API para consultar las órdenes.</span></div> : data.servicios.length ? <div className="recent-list">{[...data.servicios].sort((a, b) => new Date(b.fechaCreacion) - new Date(a.fechaCreacion)).slice(0, 4).map((item) => <Link to="/servicios" className="recent-item" key={item.id}>
            <span className="recent-marker"><Wrench size={15} /></span>
            <span className="recent-copy"><strong>{clientName(data.clientes, item.clienteId)}</strong><small>{vehicleName(data.vehiculos, item.vehiculoId)}</small></span>
            <span className="recent-date">{formatDate(item.fechaIngreso)}</span>
          </Link>)}</div> : <div className="recent-empty"><span className="recent-empty-icon"><Wrench size={20} /></span><strong>Aún no hay órdenes</strong><span>Las órdenes nuevas aparecerán aquí.</span><Link to="/servicios">Crear primera orden <ArrowUpRight size={14} /></Link></div>}
        </section>
      </div>
    </section>
  );
}

function PlusIcon() {
  return <Wrench size={16} />;
}

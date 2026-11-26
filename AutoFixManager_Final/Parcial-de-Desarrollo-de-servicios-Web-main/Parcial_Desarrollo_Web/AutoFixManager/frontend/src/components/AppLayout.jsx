import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { Activity, CarFront, Gauge, UsersRound, Wrench, UserRoundCog } from 'lucide-react';
import Swal from 'sweetalert2';
import api from '../services/api';

// Elementos de navegación; `end` evita que Resumen quede activo en todas las rutas.
const adminLinks = [
  { to: '/', label: 'Resumen', icon: Gauge, end: true },
  { to: '/clientes', label: 'Clientes', icon: UsersRound },
  { to: '/vehiculos', label: 'Vehículos', icon: CarFront },
  { to: '/servicios', label: 'Servicios', icon: Wrench },
  { to: '/usuarios', label: 'Cuentas', icon: UserRoundCog },
];

// Título contextual mostrado en la barra superior según la ruta actual.
const pageNames = {
  '/': 'Vista general',
  '/clientes': 'Gestión de clientes',
  '/vehiculos': 'Parque automotor',
  '/servicios': 'Órdenes de trabajo',
  '/usuarios': 'Cuentas y roles',
};

// Proporciona navegación y cabecera comunes a todas las páginas.
export default function AppLayout() {
  const { pathname } = useLocation();
  const navigate = useNavigate();
  const role = localStorage.getItem('autofix_rol');
  const links = role === 'ADMIN'
    ? adminLinks
    : [{ to: '/', label: role === 'MECANICO' ? 'Mis asignaciones' : 'Mi portal', icon: role === 'MECANICO' ? Wrench : UsersRound, end: true }];
  const title = role === 'MECANICO' ? 'Órdenes asignadas' : role === 'CLIENTE' ? 'Historial de mi vehículo' : pageNames[pathname] || 'AutoFix Manager';
  const today = new Intl.DateTimeFormat('es-CO', { dateStyle: 'full' }).format(new Date());

  async function logout() {
    try {
      await api.post('/auth/logout');
    } catch {
      await Swal.fire({ icon: 'warning', title: 'Sesión local cerrada', text: 'No se pudo confirmar la revocación del refresh token en el servidor.' });
    } finally {
      localStorage.removeItem('autofix_token');
      localStorage.removeItem('autofix_usuario');
      localStorage.removeItem('autofix_rol');
      navigate('/login');
    }
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <NavLink to="/" className="brand-lockup">
          <span className="brand-mark"><Wrench size={20} strokeWidth={2.4} /></span>
          <span className="brand-type">AUTO<span>FIX</span><small>MANAGER</small></span>
        </NavLink>
        <div className="nav-caption">{role === 'ADMIN' ? 'TALLER' : 'MI CUENTA'}</div>
        <nav className="side-nav" aria-label="Navegación principal">
          {links.map(({ to, label, icon: Icon, end }) => (
            <NavLink key={to} to={to} end={end} aria-label={label} className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`}>
              <Icon size={18} strokeWidth={1.9} />
              <span>{label}</span>
              {label === 'Servicios' && <span className="nav-dot" />}
            </NavLink>
          ))}
        </nav>
        <div className="sidebar-foot">
          <div className="live-indicator"><span /> Interfaz activa</div>
          <span>AutoFix Manager <b>v1.0</b></span>
        </div>
      </aside>

      <main className="main-area">
        <header className="topbar">
          <div className="topbar-title"><Activity size={15} /> {title}</div>
          <div className="d-flex align-items-center gap-3"><span className="topbar-date">{localStorage.getItem('autofix_usuario')} · {localStorage.getItem('autofix_rol')}</span><button className="btn btn-sm btn-outline-secondary" onClick={logout}>Cerrar sesión</button><span className="topbar-date">{today}</span></div>
        </header>
        <div className="content-area"><Outlet /></div>
      </main>
    </div>
  );
}

import { Navigate, Route, Routes } from 'react-router-dom';
import AppLayout from './components/AppLayout';
import RecordsPage from './components/RecordsPage';
import Dashboard from './pages/Dashboard';
import Login from './pages/Login';
import { PortalCliente, PortalMecanico } from './pages/RolePortal';
import UsuariosPage from './pages/UsuariosPage';

function ProtectedLayout() {
  const role = localStorage.getItem('autofix_rol');
  return localStorage.getItem('autofix_token') && ['ADMIN', 'MECANICO', 'CLIENTE'].includes(role)
    ? <AppLayout />
    : <Navigate to="/login" replace />;
}

function RoleHome() {
  const role = localStorage.getItem('autofix_rol');
  if (role === 'CLIENTE') return <PortalCliente />;
  if (role === 'MECANICO') return <PortalMecanico />;
  return <Dashboard />;
}

function AdminOnly({ children }) {
  return localStorage.getItem('autofix_rol') === 'ADMIN' ? children : <Navigate to="/" replace />;
}

export default function App() {
  return <Routes>
    <Route path="/login" element={localStorage.getItem('autofix_token') ? <Navigate to="/" replace /> : <Login />} />
    <Route element={<ProtectedLayout />}>
      <Route index element={<RoleHome />} />
      <Route path="clientes" element={<AdminOnly><RecordsPage resource="clientes" /></AdminOnly>} />
      <Route path="vehiculos" element={<AdminOnly><RecordsPage resource="vehiculos" /></AdminOnly>} />
      <Route path="servicios" element={<AdminOnly><RecordsPage resource="servicios" /></AdminOnly>} />
      <Route path="usuarios" element={<AdminOnly><UsuariosPage /></AdminOnly>} />
    </Route>
    <Route path="*" element={<Navigate to="/" replace />} />
  </Routes>;
}

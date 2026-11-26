import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';
import { getErrorMessage } from '../services/api';

export default function Login() {
  const [nombreUsuario, setNombreUsuario] = useState('admin');
  const [contrasena, setContrasena] = useState('');
  const [error, setError] = useState('');
  const [cargando, setCargando] = useState(false);
  const navigate = useNavigate();

  async function iniciarSesion(event) {
    event.preventDefault();
    setError('');
    setCargando(true);
    try {
      const { data } = await api.post('/auth/login', { nombreUsuario, contrasena });
      localStorage.setItem('autofix_token', data.token);
      localStorage.setItem('autofix_usuario', data.usuario);
      localStorage.setItem('autofix_rol', data.rol);
      navigate('/', { replace: true });
    } catch (e) {
      setError(getErrorMessage(e));
    } finally {
      setCargando(false);
    }
  }

  return <main className="container d-flex align-items-center justify-content-center min-vh-100">
    <form className="card shadow-sm p-4" style={{ width: '100%', maxWidth: 420 }} onSubmit={iniciarSesion}>
      <div className="mb-4"><h2 className="fw-bold mb-1">AutoFix Manager</h2><p className="text-muted mb-0">Inicia sesión con tu cuenta del taller</p></div>
      <label className="form-label" htmlFor="usuario">Usuario</label>
      <input id="usuario" className="form-control mb-3" value={nombreUsuario} onChange={e => setNombreUsuario(e.target.value)} required />
      <label className="form-label" htmlFor="contrasena">Contraseña</label>
      <input id="contrasena" type="password" className="form-control mb-3" value={contrasena} onChange={e => setContrasena(e.target.value)} required />
      {error && <div className="alert alert-danger py-2">{error}</div>}
      <button className="btn btn-dark w-100" disabled={cargando}>{cargando ? 'Ingresando...' : 'Iniciar sesión'}</button>
    </form>
  </main>;
}

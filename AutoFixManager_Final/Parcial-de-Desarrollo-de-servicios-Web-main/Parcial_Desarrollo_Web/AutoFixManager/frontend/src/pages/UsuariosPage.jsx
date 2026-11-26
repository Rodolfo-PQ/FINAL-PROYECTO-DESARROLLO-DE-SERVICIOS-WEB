import { useEffect, useState } from 'react';
import { Trash2, UsersRound } from 'lucide-react';
import api, { getErrorMessage } from '../services/api';

const emptyForm = { nombreUsuario: '', contrasena: '', rol: 'MECANICO', clienteId: '' };

export default function UsuariosPage() {
  const [usuarios, setUsuarios] = useState([]);
  const [clientes, setClientes] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');
  const [loading, setLoading] = useState(true);

  async function loadUsers() {
    setLoading(true);
    setError('');
    try {
      const [usersResponse, clientsResponse] = await Promise.all([
        api.get('/auth/usuarios'),
        api.get('/clientes'),
      ]);
      setUsuarios(usersResponse.data);
      setClientes(clientsResponse.data);
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => { loadUsers(); }, []);

  async function createUser(event) {
    event.preventDefault();
    setError('');
    setMessage('');
    try {
      await api.post('/auth/usuarios', {
        nombreUsuario: form.nombreUsuario,
        contrasena: form.contrasena,
        rol: form.rol,
        clienteId: form.rol === 'CLIENTE' ? Number(form.clienteId) : null,
      });
      setForm(emptyForm);
      setMessage('La cuenta se creó correctamente.');
      await loadUsers();
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    }
  }

  async function deleteUser(user) {
    if (!window.confirm(`¿Eliminar la cuenta ${user.usuario}?`)) return;
    setError('');
    setMessage('');
    try {
      await api.delete(`/auth/usuarios/${user.id}`);
      setMessage('La cuenta se eliminó correctamente.');
      await loadUsers();
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    }
  }

  return (
    <section className="page-section">
      <div className="page-heading"><div><div className="eyebrow"><UsersRound size={14} /> ADMINISTRACIÓN</div><h1>Cuentas y roles</h1><p>Registra accesos para mecánicos y clientes del taller.</p></div></div>
      {error && <div className="state-panel state-error">{error}</div>}
      {message && <div className="alert alert-success">{message}</div>}
      <section className="recent-panel mb-4">
        <div className="section-heading"><div><span className="eyebrow">NUEVA CUENTA</span><h2>Registrar usuario</h2></div></div>
        <form className="row g-3 p-3" onSubmit={createUser}>
          <div className="col-md-4"><label className="form-label" htmlFor="nombreUsuario">Usuario</label><input id="nombreUsuario" className="form-control" minLength="4" maxLength="80" required value={form.nombreUsuario} onChange={(event) => setForm((current) => ({ ...current, nombreUsuario: event.target.value }))} /></div>
          <div className="col-md-4"><label className="form-label" htmlFor="contrasena">Contraseña inicial</label><input id="contrasena" className="form-control" type="password" minLength="8" required value={form.contrasena} onChange={(event) => setForm((current) => ({ ...current, contrasena: event.target.value }))} /></div>
          <div className="col-md-4"><label className="form-label" htmlFor="rol">Rol</label><select id="rol" className="form-select" value={form.rol} onChange={(event) => setForm((current) => ({ ...current, rol: event.target.value, clienteId: '' }))}><option value="MECANICO">MECANICO</option><option value="CLIENTE">CLIENTE</option></select></div>
          {form.rol === 'CLIENTE' && <div className="col-md-6"><label className="form-label" htmlFor="clienteId">Cliente asociado</label><select id="clienteId" className="form-select" required value={form.clienteId} onChange={(event) => setForm((current) => ({ ...current, clienteId: event.target.value }))}><option value="">Selecciona un cliente</option>{clientes.map((cliente) => <option key={cliente.id} value={cliente.id}>{cliente.nombre} {cliente.apellido}</option>)}</select></div>}
          <div className="col-12"><button className="button-primary" type="submit">Crear cuenta</button></div>
        </form>
      </section>
      <section className="recent-panel">
        <div className="section-heading"><div><span className="eyebrow">USUARIOS</span><h2>Cuentas registradas</h2></div></div>
        {loading ? <div className="state-panel">Cargando usuarios...</div> : <div className="table-responsive"><table className="table records-table align-middle"><thead><tr><th>Usuario</th><th>Rol</th><th>Cliente asociado</th><th>Acciones</th></tr></thead><tbody>
          {usuarios.map((user) => {
            const cliente = clientes.find((item) => item.id === user.clienteId);
            return <tr key={user.id}><td>{user.usuario}</td><td>{user.rol}</td><td>{cliente ? `${cliente.nombre} ${cliente.apellido}` : '—'}</td><td>{user.rol !== 'ADMIN' && <button className="icon-button danger" title="Eliminar cuenta" aria-label={`Eliminar ${user.usuario}`} onClick={() => deleteUser(user)}><Trash2 size={16} /></button>}</td></tr>;
          })}
          {!usuarios.length && <tr><td colSpan="4">No hay cuentas registradas.</td></tr>}
        </tbody></table></div>}
      </section>
    </section>
  );
}

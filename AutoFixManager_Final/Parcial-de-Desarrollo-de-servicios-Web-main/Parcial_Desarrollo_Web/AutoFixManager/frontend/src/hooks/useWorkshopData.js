import { useEffect, useState } from 'react';
import { list } from '../services/resources';
import api from '../services/api';

// Carga los tres recursos reales y comparte referencias para dashboard y formularios.
export default function useWorkshopData() {
  const [data, setData] = useState({ clientes: [], vehiculos: [], servicios: [], mecanicos: [] });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [revision, setRevision] = useState(0);

  useEffect(() => {
    let active = true;
    setLoading(true);
    setError('');
    // Espera las tres respuestas para evitar mostrar relaciones parcialmente cargadas.
    Promise.all([list('clientes'), list('vehiculos'), list('servicios'), api.get('/auth/usuarios')])
      .then(([clientes, vehiculos, servicios, { data: usuarios }]) => {
        if (active) setData({ clientes, vehiculos, servicios, mecanicos: usuarios.filter((usuario) => usuario.rol === 'MECANICO') });
      })
      .catch((requestError) => {
        if (active) setError(requestError.message || 'No se pudo conectar con la API.');
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    // Ignora una respuesta tardía si la página se desmontó o se solicitó una recarga.
    return () => { active = false; };
  }, [revision]);

  return { data, loading, error, refresh: () => setRevision((value) => value + 1) };
}

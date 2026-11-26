import { describe, expect, it } from 'vitest';
import { clientName, formatDate, vehicleName } from './entityConfig';

describe('entityConfig helpers', () => {
  it('formats complete client and vehicle names', () => {
    expect(clientName([{ id: 2, nombre: 'Ana', apellido: 'Pérez' }], 2)).toBe('Ana Pérez');
    expect(clientName([], 2)).toBe('Sin cliente');
    expect(vehicleName([{ id: 3, marca: 'Toyota', modelo: 'Corolla', placa: 'ABC-123' }], 3)).toBe('Toyota Corolla · ABC-123');
  });

  it('does not shift an ISO date to another day', () => {
    expect(formatDate('2026-11-25T12:00:00Z')).not.toContain('24');
  });
});

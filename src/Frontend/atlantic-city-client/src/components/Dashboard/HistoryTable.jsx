import { Clock, Database } from 'lucide-react';
import { formatearEstado } from '../../utils/formatters';

export function HistoryTable({ datosHistorial, progresoLotes, onVerDetalle }) {
    return (
        <div className="card">
            <h3><Clock size={20} /> Historial de Lotes Procesados</h3>
            <div className="table-wrapper">
                <table>
                    <thead>
                        <tr>
                            <th>Archivo</th>
                            <th>Usuario</th>
                            <th>Estado</th>
                            <th>Progreso</th>
                            <th>Fecha</th>
                            <th>Acción</th>
                        </tr>
                    </thead>
                    <tbody>
                        {datosHistorial.length === 0 ? (
                            <tr><td colSpan="6" className="empty-state">No hay historial disponible</td></tr>
                        ) : (
                            datosHistorial.map((h, i) => {
                                const batchId = h.Id || h.id;
                                const estadoRaw = (h.Status || h.status || '').toLowerCase();
                                const estaEnProceso = estadoRaw.includes('proceso') || estadoRaw === 'pendiente' || estadoRaw === 'pending';
                                const registrosEnVivo = progresoLotes[batchId];
                                const estaEnProcesoActivo = registrosEnVivo !== undefined;
                                const totalFinal = h.TotalRecords || h.totalRecords || 0;
                                
                                // Si hay progreso en vivo, forzamos el estado visual a "En Proceso"
                                const st = estaEnProcesoActivo 
                                    ? { texto: 'En Proceso', clase: 'en-proceso' }
                                    : formatearEstado(h.Status || h.status);
                                
                                return (
                                    <tr key={i}>
                                        <td><strong>{h.FileName || h.fileName}</strong></td>
                                        <td>{h.UserEmail || h.userEmail}</td>
                                        <td>
                                            <span className={`badge ${st.clase}`}>{st.texto}</span>
                                        </td>
                                        <td className="progress-cell">
                                            {estaEnProcesoActivo ? (
                                                <span className="progress-live">
                                                    <span className="pulse-dot"></span>
                                                    <span className="counter">{registrosEnVivo.procesados}</span>
                                                    <span className="separator">/</span>
                                                    <span className="total">{registrosEnVivo.total}</span>
                                                </span>
                                            ) : totalFinal > 0 ? (
                                                <span className="progress-final">{totalFinal} registros</span>
                                            ) : (
                                                <span className="progress-waiting">—</span>
                                            )}
                                        </td>
                                        <td className="p-period">
                                            {(h.CreatedAt || h.createdAt) 
                                                ? new Date(h.CreatedAt || h.createdAt).toLocaleString() 
                                                : 'N/A'}
                                        </td>
                                        <td>
                                            <button 
                                                onClick={() => onVerDetalle(h)}
                                                className="btn-view"
                                                title="Ver contenido"
                                            >
                                                <Database size={16} />
                                            </button>
                                        </td>
                                    </tr>
                                );
                            })
                        )}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

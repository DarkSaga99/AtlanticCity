import { X } from 'lucide-react';
import { formatearEstado } from '../../utils/formatters';

export function BatchDetailView({ lote, items, cargando, onClose }) {
    const st = formatearEstado(lote?.Status || lote?.status);
    
    return (
        <div className="card full-width">
            <div className="card-header">
                <div className="title-with-back">
                    <button onClick={onClose} className="btn-icon-back"><X size={20} /></button>
                    <h3>Contenido de: {lote?.FileName || lote?.fileName}</h3>
                </div>
                <div className="batch-stats-summary">
                    <div className="stat-pill total">
                        <span>Total:</span> <strong>{lote?.TotalRecords || lote?.totalRecords || 0}</strong>
                    </div>
                    <div className="stat-pill success">
                        <span>Éxito:</span> <strong>{lote?.SuccessCount || lote?.successCount || 0}</strong>
                    </div>
                    <div className="stat-pill error">
                        <span>Error:</span> <strong>{lote?.ErrorCount || lote?.errorCount || 0}</strong>
                    </div>
                </div>
                <span className={`badge ${st.clase}`}>{st.texto}</span>
            </div>
            <div className="table-wrapper" style={{ maxHeight: '60vh' }}>
                {cargando ? (
                    <div className="empty-state">Cargando registros...</div>
                ) : (
                    <table>
                        <thead>
                            <tr>
                                <th>Producto</th>
                                <th>Periodo</th>
                                <th>Estado</th>
                            </tr>
                        </thead>
                        <tbody>
                            {items.length === 0 ? (
                                <tr><td colSpan="3" className="empty-state">No hay registros en este lote</td></tr>
                            ) : (
                                items.map((row, i) => {
                                    const rowSt = formatearEstado(row.Estado || row.estado);
                                    return (
                                        <tr key={i}>
                                            <td>
                                                <div className="prod-cell">
                                                    <span className="p-code">{row.CodigoProducto || row.codigoProducto}</span>
                                                    <span className="p-name">{row.Nombre || row.nombre}</span>
                                                </div>
                                            </td>
                                            <td className="p-period">{row.period || row.Period || row.periodo || row.Periodo}</td>
                                            <td>
                                                <span className={`badge ${rowSt.clase}`}>{rowSt.texto}</span>
                                            </td>
                                        </tr>
                                    );
                                })
                            )}
                        </tbody>
                    </table>
                )}
            </div>
        </div>
    );
}

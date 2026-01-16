import { History } from 'lucide-react';
import { formatearEstado } from '../../utils/formatters';

export function TrackingTable({ datosTrazabilidad }) {
    return (
        <div className="card full-width">
            <div className="card-header">
                <h3><History size={20} /> Seguimiento Directo de Productos</h3>
            </div>
            <div className="table-wrapper">
                <table>
                    <thead>
                        <tr>
                            <th>Producto</th>
                            <th>Periodo</th>
                            <th>Estado</th>
                        </tr>
                    </thead>
                    <tbody>
                        {datosTrazabilidad.length === 0 ? (
                            <tr><td colSpan="3" className="empty-state">No hay registros hoy</td></tr>
                        ) : (
                            datosTrazabilidad.map((row, i) => {
                                const st = formatearEstado(row.Estado || row.estado);
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
                                            <span className={`badge ${st.clase}`}>{st.texto}</span>
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

import { ShieldAlert } from 'lucide-react';

export function AlertsTable({ datosAlertas }) {
    return (
        <div className="card full-width">
            <div className="card-header">
                <h3><ShieldAlert size={20} /> Log de Auditoría y Alertas</h3>
            </div>
            <div className="table-wrapper">
                <table>
                    <thead>
                        <tr>
                            <th>Acción</th>
                            <th>Detalles</th>
                            <th>Usuario</th>
                            <th>Fecha</th>
                        </tr>
                    </thead>
                    <tbody>
                        {datosAlertas.length === 0 ? (
                            <tr><td colSpan="4" className="empty-state">No hay alertas registradas</td></tr>
                        ) : (
                            datosAlertas.map((a, i) => (
                                <tr key={i}>
                                    <td>
                                        <span className={`badge ${a.Action?.toLowerCase().replace(/ /g, '-')}`}>
                                            {a.Action || a.action}
                                        </span>
                                    </td>
                                    <td style={{ maxWidth: '400px' }}>{a.Details || a.details}</td>
                                    <td>{a.User || a.user || 'Sistema'}</td>
                                    <td className="p-period">
                                        {(a.CreatedAt || a.createdAt) 
                                            ? new Date(a.CreatedAt || a.createdAt).toLocaleString() 
                                            : 'N/A'}
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

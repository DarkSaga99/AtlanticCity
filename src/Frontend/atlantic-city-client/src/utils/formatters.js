// Helper para normalizar estados al español
export const formatearEstado = (estadoRaw) => {
    if (!estadoRaw) return { texto: 'Desconocido', clase: '' };
    const e = estadoRaw.toLowerCase();
    
    if (e === 'pending' || e === 'pendiente') return { texto: 'Pendiente', clase: 'pendiente' };
    if (e === 'inprogress' || e === 'en proceso' || e.includes('proce')) return { texto: 'En Proceso', clase: 'en-proceso' };
    if (e === 'finished' || e === 'finalizado' || e === 'terminado') return { texto: 'Finalizado', clase: 'finalizado' };
    if (e === 'notified' || e === 'notificado') return { texto: 'Notificado', clase: 'notificado' };
    if (e === 'rechazado' || e === 'rejected') return { texto: 'Rechazado', clase: 'rechazado' };
    if (e === 'error') return { texto: 'Error', clase: 'error' };
    
    return { texto: estadoRaw, clase: e.replace(' ', '-') };
};

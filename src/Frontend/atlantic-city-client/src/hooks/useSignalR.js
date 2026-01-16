import { useState, useEffect, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';

export function useSignalR(isAuthenticated, onRefresh) {
    const [progresoLotes, setProgresoLotes] = useState({});
    const [notificacion, setNotificacion] = useState(null);

    useEffect(() => {
        if (!isAuthenticated) return;

        const conexion = new signalR.HubConnectionBuilder()
            .withUrl("http://localhost:5000/hub-notificaciones")
            .withAutomaticReconnect()
            .build();

        conexion.start().then(() => {
            conexion.on("ArchivoProcesado", (mensaje) => {
                // Actualizar progreso en tiempo real
                if (mensaje.idMensaje || mensaje.IdMensaje) {
                    const batchId = mensaje.idMensaje || mensaje.IdMensaje;
                    const procesados = mensaje.registrosProcesados || mensaje.RegistrosProcesados || 0;
                    const total = mensaje.totalRegistros || mensaje.TotalRegistros || 0;
                    
                    setProgresoLotes(prev => ({
                        ...prev,
                        [batchId]: { procesados, total }
                    }));
                }
                
                // Mensaje final
                if (mensaje.esCompleto === true) {
                    const batchId = mensaje.idMensaje || mensaje.IdMensaje;
                    
                    setProgresoLotes(prev => {
                        const newState = { ...prev };
                        delete newState[batchId];
                        return newState;
                    });
                    
                    if (mensaje.esRechazado) {
                        setNotificacion({
                            titulo: "Carga Rechazada",
                            texto: mensaje.motivoRechazo,
                            tipo: "error"
                        });
                    } else {
                        setNotificacion({
                            titulo: "Procesamiento Finalizado",
                            texto: `El archivo '${mensaje.nombreArchivo}' se procesó exitosamente.`,
                            tipo: "success"
                        });
                    }
                    onRefresh?.();
                }
            });
        }).catch(err => console.error("Error en WebSocket:", err));

        return () => conexion.stop();
    }, [isAuthenticated, onRefresh]);

    // Auto-limpiar notificaciones
    useEffect(() => {
        if (notificacion) {
            const timer = setTimeout(() => setNotificacion(null), 5000);
            return () => clearTimeout(timer);
        }
    }, [notificacion]);

    const clearNotificacion = useCallback(() => setNotificacion(null), []);

    return {
        progresoLotes,
        notificacion,
        setNotificacion,
        clearNotificacion
    };
}

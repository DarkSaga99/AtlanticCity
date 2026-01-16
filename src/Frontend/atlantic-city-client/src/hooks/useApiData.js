import { useState, useCallback } from 'react';
import api from '../api/authApi';

export function useApiData() {
    const [datosTrazabilidad, setDatosTrazabilidad] = useState([]);
    const [datosHistorial, setDatosHistorial] = useState([]);
    const [datosAlertas, setDatosAlertas] = useState([]);

    const obtenerSeguimiento = useCallback(async () => {
        try {
            const res = await api.get('/Archivos/seguimiento');
            setDatosTrazabilidad(res.data || []);
        } catch (e) {
            console.warn("Error cargando seguimiento");
        }
    }, []);

    const obtenerHistorial = useCallback(async () => {
        try {
            const res = await api.get('/Archivos/historial');
            setDatosHistorial(res.data || []);
        } catch (e) {
            console.warn("Error cargando historial");
        }
    }, []);

    const obtenerAlertas = useCallback(async () => {
        try {
            const res = await api.get('/Archivos/alertas');
            setDatosAlertas(res.data || []);
        } catch (e) {
            console.warn("Error cargando alertas");
        }
    }, []);

    const refrescarTodo = useCallback(() => {
        obtenerSeguimiento();
        obtenerHistorial();
        obtenerAlertas();
    }, [obtenerSeguimiento, obtenerHistorial, obtenerAlertas]);

    const obtenerProductosLote = useCallback(async (loteId) => {
        try {
            const res = await api.get(`/Archivos/${loteId}/productos`);
            return res.data || [];
        } catch (e) {
            console.error("Error al cargar detalle del lote");
            return [];
        }
    }, []);

    return {
        datosTrazabilidad,
        datosHistorial,
        datosAlertas,
        obtenerSeguimiento,
        obtenerHistorial,
        obtenerAlertas,
        refrescarTodo,
        obtenerProductosLote
    };
}

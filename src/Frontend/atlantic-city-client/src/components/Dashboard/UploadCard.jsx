import { useState } from 'react';
import { Upload, FileSpreadsheet } from 'lucide-react';
import api from '../../api/authApi';

export function UploadCard({ onSuccess, onError }) {
    const [archivoSeleccionado, setArchivoSeleccionado] = useState(null);
    const [progreso, setProgreso] = useState(0);
    const [statusTexto, setStatusTexto] = useState("");

    const descargarPlantilla = () => {
        const contenido = "CodigoProducto,Nombre,Periodo\nPROD001,Laptop Gamer,2024-01\nPROD002,Monitor 4K,2024-01";
        const blob = new Blob([contenido], { type: 'text/csv' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = "Plantilla_Excel_Atlantic.csv";
        a.click();
    };

    const manejarSubida = async () => {
        if (!archivoSeleccionado) return;
        
        const MAX_SIZE = 5 * 1024 * 1024; // 5MB
        if (archivoSeleccionado.size > MAX_SIZE) {
            onError?.({
                titulo: "Archivo muy grande",
                texto: `El límite configurado es de 5MB. Tu archivo pesa ${(archivoSeleccionado.size / (1024 * 1024)).toFixed(2)}MB.`,
                tipo: "error"
            });
            return;
        }

        setProgreso(10);
        const formData = new FormData();
        formData.append('archivo', archivoSeleccionado);
        
        try {
            setStatusTexto("Subiendo archivo...");
            const storedEmail = localStorage.getItem('email') || localStorage.getItem('username');
            await api.post(`/Archivos/subir?correoUsuario=${encodeURIComponent(storedEmail)}`, formData, {
                onUploadProgress: (p) => setProgreso(Math.round((p.loaded * 100) / p.total))
            });
            
            onSuccess?.({
                titulo: "Archivo Recibido",
                texto: "Se ha iniciado el procesamiento en segundo plano. Puedes continuar usando la plataforma.",
                tipo: "success"
            });

            setArchivoSeleccionado(null);
            setProgreso(0);
            setStatusTexto("");

        } catch (err) {
            const mensajeError = err.response?.data?.Error || err.response?.data?.error || "Fallo en la subida";
            onError?.({
                titulo: "Error de Subida",
                texto: mensajeError,
                tipo: "error"
            });
            setProgreso(0);
            setStatusTexto("");
        }
    };

    return (
        <div className="card">
            <div className="card-header">
                <h3><Upload size={18} /> Nueva Carga</h3>
                <button onClick={descargarPlantilla} className="btn-download">Excel Plantilla</button>
            </div>

            <div className={`upload-zone ${archivoSeleccionado ? 'has-file' : ''}`}>
                <input 
                    type="file" 
                    onChange={(e) => setArchivoSeleccionado(e.target.files[0])} 
                    accept=".csv,.xlsx" 
                />
                <div className="upload-content">
                    <div className="circle-icon"><FileSpreadsheet size={28} /></div>
                    <p className="primary-text">
                        {archivoSeleccionado ? archivoSeleccionado.name : "Subir Archivo"}
                    </p>
                    {!archivoSeleccionado && <p className="secondary-text">Click para seleccionar o arrastra aquí</p>}
                </div>
            </div>

            {progreso > 0 && (
                <div className="upload-progress">
                    <div className="progress-info">
                        <span>{statusTexto || `Subiendo: ${progreso}%`}</span>
                    </div>
                    <div className="progress-track">
                        <div className="progress-fill" style={{ width: `${progreso}%` }}></div>
                    </div>
                </div>
            )}

            <button onClick={manejarSubida} disabled={!archivoSeleccionado || progreso > 0} className="primary-btn">
                {progreso > 0 ? "Enviando..." : "Iniciar Proceso"}
            </button>
        </div>
    );
}

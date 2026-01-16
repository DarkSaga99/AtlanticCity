import { useState, useEffect, useCallback } from 'react';
import { motion, AnimatePresence } from 'framer-motion';

// Hooks personalizados
import { useAuth } from './hooks/useAuth';
import { useSignalR } from './hooks/useSignalR';
import { useApiData } from './hooks/useApiData';

// Componentes
import { LoginScreen } from './components/Auth/LoginScreen';
import { Sidebar } from './components/Layout/Sidebar';
import { Header } from './components/Layout/Header';
import { Toast } from './components/Common/Toast';
import { UploadCard } from './components/Dashboard/UploadCard';
import { HistoryTable } from './components/Dashboard/HistoryTable';
import { TrackingTable } from './components/Dashboard/TrackingTable';
import { AlertsTable } from './components/Dashboard/AlertsTable';
import { BatchDetailView } from './components/Dashboard/BatchDetailView';

import './App.css';

function App() {
    // Hooks personalizados
    const auth = useAuth();
    const apiData = useApiData();
    
    // Estados locales de UI
    const [view, setView] = useState('dashboard');
    const [isLogin, setIsLogin] = useState(true);
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [localUsername, setLocalUsername] = useState(auth.username);
    
    // Estado para detalle de lote
    const [loteSeleccionado, setLoteSeleccionado] = useState(null);
    const [itemsLote, setItemsLote] = useState([]);
    const [cargandoDetalle, setCargandoDetalle] = useState(false);
    
    // Estado para notificaciones manuales (desde UploadCard)
    const [notificacionManual, setNotificacionManual] = useState(null);

    // SignalR hook con callback de refresco
    const signalR = useSignalR(auth.isAuthenticated, apiData.refrescarTodo);
    
    // Combinar notificaciones de SignalR y manuales
    const notificacionActiva = signalR.notificacion || notificacionManual;
    
    const cerrarNotificacion = useCallback(() => {
        signalR.clearNotificacion();
        setNotificacionManual(null);
    }, [signalR]);

    // Cargar datos al autenticarse
    useEffect(() => {
        if (auth.isAuthenticated) {
            apiData.refrescarTodo();
        }
    }, [auth.isAuthenticated]);

    // Cargar datos según la vista
    useEffect(() => {
        if (auth.isAuthenticated) {
            if (view === 'dashboard') apiData.obtenerHistorial();
            if (view === 'history') apiData.obtenerSeguimiento();
            if (view === 'alerts') apiData.obtenerAlertas();
        }
    }, [view, auth.isAuthenticated]);

    // Limpiar notificación manual después de 5s
    useEffect(() => {
        if (notificacionManual) {
            const timer = setTimeout(() => setNotificacionManual(null), 5000);
            return () => clearTimeout(timer);
        }
    }, [notificacionManual]);

    // Handler para ver detalle de lote
    const verDetalleLote = async (lote) => {
        setLoteSeleccionado(lote);
        setCargandoDetalle(true);
        setView('batchDetail');
        const items = await apiData.obtenerProductosLote(lote.Id || lote.id);
        setItemsLote(items);
        setCargandoDetalle(false);
    };

    // Handler para subida exitosa
    const handleUploadSuccess = (notif) => {
        setNotificacionManual(notif);
        apiData.refrescarTodo();
    };

    // VISTA DE LOGIN
    if (!auth.isAuthenticated) {
        return (
            <>
                <LoginScreen
                    username={localUsername}
                    setUsername={setLocalUsername}
                    email={email}
                    setEmail={setEmail}
                    password={password}
                    setPassword={setPassword}
                    error={auth.error}
                    isLogin={isLogin}
                    setIsLogin={setIsLogin}
                    onLogin={auth.login}
                    onRegister={auth.register}
                    setError={auth.setError}
                    setNotification={setNotificacionManual}
                />
                <Toast notificacion={notificacionActiva} onClose={cerrarNotificacion} />
            </>
        );
    }

    // VISTA PRINCIPAL (AUTENTICADO)
    return (
        <div className="app-shell">
            <Sidebar view={view} setView={setView} onLogout={auth.logout} />

            <main className="app-main">
                <Header username={auth.username} />

                <div className="content-container">
                    <AnimatePresence mode="wait">
                        {view === 'dashboard' && (
                            <motion.div 
                                key="dash" 
                                initial={{ opacity: 0, x: 20 }} 
                                animate={{ opacity: 1, x: 0 }} 
                                exit={{ opacity: 0, x: -20 }} 
                                className="dashboard-layout"
                            >
                                <section className="upload-section">
                                    <UploadCard 
                                        onSuccess={handleUploadSuccess} 
                                        onError={setNotificacionManual} 
                                    />
                                </section>

                                <section className="monitoring-section">
                                    <HistoryTable 
                                        datosHistorial={apiData.datosHistorial}
                                        progresoLotes={signalR.progresoLotes}
                                        onVerDetalle={verDetalleLote}
                                    />
                                </section>
                            </motion.div>
                        )}

                        {view === 'history' && (
                            <motion.div 
                                key="hist" 
                                initial={{ opacity: 0, x: 20 }} 
                                animate={{ opacity: 1, x: 0 }} 
                                exit={{ opacity: 0, x: -20 }} 
                                className="history-layout"
                            >
                                <TrackingTable datosTrazabilidad={apiData.datosTrazabilidad} />
                            </motion.div>
                        )}

                        {view === 'alerts' && (
                            <motion.div 
                                key="alert" 
                                initial={{ opacity: 0, x: 20 }} 
                                animate={{ opacity: 1, x: 0 }} 
                                exit={{ opacity: 0, x: -20 }} 
                                className="alerts-layout"
                            >
                                <AlertsTable datosAlertas={apiData.datosAlertas} />
                            </motion.div>
                        )}

                        {view === 'batchDetail' && (
                            <motion.div 
                                key="detail" 
                                initial={{ opacity: 0, x: 20 }} 
                                animate={{ opacity: 1, x: 0 }} 
                                exit={{ opacity: 0, x: -20 }} 
                                className="history-layout"
                            >
                                <BatchDetailView 
                                    lote={loteSeleccionado}
                                    items={itemsLote}
                                    cargando={cargandoDetalle}
                                    onClose={() => setView('dashboard')}
                                />
                            </motion.div>
                        )}
                    </AnimatePresence>
                </div>
            </main>

            <Toast notificacion={notificacionActiva} onClose={cerrarNotificacion} />
        </div>
    );
}

export default App;

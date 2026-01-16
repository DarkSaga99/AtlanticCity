import { LayoutDashboard, Clock, ShieldAlert, LogOut } from 'lucide-react';

export function Sidebar({ view, setView, onLogout }) {
    return (
        <aside className="app-sidebar">
            <div className="sidebar-brand">Atlantic City</div>
            <nav className="nav-group">
                <button 
                    onClick={() => setView('dashboard')} 
                    className={`nav-link ${view === 'dashboard' ? 'active' : ''}`}
                >
                    <LayoutDashboard size={20} />
                    <span>Panel</span>
                </button>
                <button 
                    onClick={() => setView('history')} 
                    className={`nav-link ${view === 'history' ? 'active' : ''}`}
                >
                    <Clock size={20} />
                    <span>Historial</span>
                </button>
                <button 
                    onClick={() => setView('alerts')} 
                    className={`nav-link ${view === 'alerts' ? 'active' : ''}`}
                >
                    <ShieldAlert size={20} />
                    <span>Alertas</span>
                </button>
            </nav>
            <div className="sidebar-bottom">
                <button onClick={onLogout} className="logout-action">
                    <LogOut size={18} />
                    <span>Cerrar Sesión</span>
                </button>
            </div>
        </aside>
    );
}

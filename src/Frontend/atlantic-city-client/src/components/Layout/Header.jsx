import { User } from 'lucide-react';

export function Header({ username }) {
    return (
        <header className="main-header">
            <h1>Portal de Administración</h1>
            <div className="user-profile">
                <User size={20} color="#94a3b8" />
                <span>{username || 'Usuario'}</span>
                <div className="avatar">{username ? username[0].toUpperCase() : 'U'}</div>
            </div>
        </header>
    );
}

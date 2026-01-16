import { useState } from 'react';
import api from '../api/authApi';

export function useAuth() {
    const [token, setToken] = useState(localStorage.getItem('accessToken'));
    const [username, setUsername] = useState(localStorage.getItem('username') || '');
    const [error, setError] = useState('');

    const login = async (user, pass) => {
        setError('');
        try {
            const res = await api.post('/Auth/login', { username: user, password: pass });
            const { accessToken, refreshToken, email: userEmail } = res.data;
            localStorage.setItem('accessToken', accessToken);
            localStorage.setItem('refreshToken', refreshToken);
            localStorage.setItem('username', user);
            localStorage.setItem('email', userEmail);
            setToken(accessToken);
            setUsername(user);
            return { success: true };
        } catch (err) {
            const msg = err.response?.data;
            const errorMsg = typeof msg === 'string' ? msg : (msg?.Error || msg?.error || 'Credenciales inválidas');
            setError(errorMsg);
            return { success: false, error: errorMsg };
        }
    };

    const register = async (user, email, pass) => {
        setError('');
        try {
            await api.post('/Auth/register', { username: user, email, password: pass });
            return { success: true };
        } catch (err) {
            const msg = err.response?.data;
            const errorMsg = typeof msg === 'string' ? msg : (msg?.Error || msg?.error || 'Error en el registro');
            setError(errorMsg);
            return { success: false, error: errorMsg };
        }
    };

    const logout = () => {
        localStorage.clear();
        setToken(null);
        setUsername('');
    };

    const isAuthenticated = !!token;

    return {
        token,
        username,
        error,
        setError,
        login,
        register,
        logout,
        isAuthenticated
    };
}

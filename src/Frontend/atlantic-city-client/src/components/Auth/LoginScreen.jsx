import { motion } from 'framer-motion';
import { User, Lock, LogIn, Building2, Mail } from 'lucide-react';

export function LoginScreen({ 
    username, 
    setUsername, 
    email, 
    setEmail, 
    password, 
    setPassword, 
    error, 
    isLogin, 
    setIsLogin, 
    onLogin, 
    onRegister,
    setError 
}) {
    const handleSubmit = async (e) => {
        e.preventDefault();
        if (isLogin) {
            await onLogin(username, password);
        } else {
            const result = await onRegister(username, email, password);
            if (result.success) {
                setIsLogin(true);
                setEmail('');
            }
        }
    };

    const toggleMode = () => {
        setIsLogin(!isLogin);
        setError('');
        setUsername('');
        setEmail('');
        setPassword('');
    };

    return (
        <div className="login-screen">
            <motion.div initial={{ y: 20, opacity: 0 }} animate={{ y: 0, opacity: 1 }} className="login-box">
                <div className="login-header">
                    <div className="login-logo"><Building2 size={40} /></div>
                    <h1>Atlantic City</h1>
                    <p>{isLogin ? "Plataforma de Carga Masiva" : "Crear Nueva Cuenta"}</p>
                </div>

                <form onSubmit={handleSubmit} className="login-form">
                    <div className="form-field">
                        <label><User size={16} /> {isLogin ? "Usuario" : "Ingrese Usuario"}</label>
                        <input 
                            type="text" 
                            value={username} 
                            onChange={e => setUsername(e.target.value)} 
                            placeholder={isLogin ? "Ingresa tu usuario" : "Nombre de usuario"} 
                            required 
                        />
                    </div>
                    {!isLogin && (
                        <div className="form-field">
                            <label><Mail size={16} /> Correo Electrónico</label>
                            <input 
                                type="email" 
                                value={email} 
                                onChange={e => setEmail(e.target.value)} 
                                placeholder="ejemplo@correo.com" 
                                required 
                            />
                        </div>
                    )}
                    <div className="form-field">
                        <label><Lock size={16} /> {isLogin ? "Contraseña" : "Ingrese Contraseña"}</label>
                        <input 
                            type="password" 
                            value={password} 
                            onChange={e => setPassword(e.target.value)} 
                            placeholder="••••••••" 
                            required 
                        />
                    </div>
                    {error && <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="error-alert">{error}</motion.div>}
                    <button type="submit" className="login-btn">
                        <span>{isLogin ? "Ingresar al Sistema" : "Registrar Cuenta"}</span>
                        {isLogin ? <LogIn size={20} /> : <User size={20} />}
                    </button>
                </form>

                <div className="login-toggle" style={{ marginTop: '1.5rem', textAlign: 'center', fontSize: '0.9rem' }}>
                    {isLogin ? (
                        <p>¿No tienes cuenta? <button onClick={toggleMode} className="btn-link">Regístrate aquí</button></p>
                    ) : (
                        <p>¿Ya tienes cuenta? <button onClick={toggleMode} className="btn-link">Inicia sesión</button></p>
                    )}
                </div>
            </motion.div>
        </div>
    );
}

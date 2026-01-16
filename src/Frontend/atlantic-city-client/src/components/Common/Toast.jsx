import { motion, AnimatePresence } from 'framer-motion';
import { CheckCircle, ShieldAlert, X } from 'lucide-react';

export function Toast({ notificacion, onClose }) {
    return (
        <AnimatePresence>
            {notificacion && (
                <motion.div 
                    initial={{ y: -50, opacity: 0 }} 
                    animate={{ y: 0, opacity: 1 }} 
                    exit={{ y: -50, opacity: 0 }} 
                    className={`floating-toast ${notificacion.tipo}`}
                >
                    {notificacion.tipo === 'success' 
                        ? <CheckCircle color="#10b981" size={24} /> 
                        : <ShieldAlert color="#ef4444" size={24} />
                    }
                    <div className="toast-body">
                        <strong style={{ color: notificacion.tipo === 'success' ? '#10b981' : '#ef4444' }}>
                            {notificacion.titulo}
                        </strong>
                        <p>{notificacion.texto}</p>
                    </div>
                    <button onClick={onClose} className="toast-close">
                        <X size={18} />
                    </button>
                </motion.div>
            )}
        </AnimatePresence>
    );
}

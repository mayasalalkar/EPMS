import { useCallback, useMemo, useState } from 'react';
import { ToastContext } from './contexts';
import ToastContainer from '../components/ToastContainer';

const TOAST_DURATION = 4000;

export function ToastProvider({ children }) {
  const [toasts, setToasts] = useState([]);

  const removeToast = useCallback((id) => {
    setToasts((current) => current.filter((toast) => toast.id !== id));
  }, []);

  const showToast = useCallback(
    (type, message) => {
      const id = `${Date.now()}-${Math.random()}`;
      setToasts((current) => [...current, { id, type, message }]);
      setTimeout(() => removeToast(id), TOAST_DURATION);
    },
    [removeToast],
  );

  const value = useMemo(
    () => ({
      success: (message) => showToast('success', message),
      error: (message) => showToast('danger', message),
      info: (message) => showToast('primary', message),
    }),
    [showToast],
  );

  return (
    <ToastContext.Provider value={value}>
      {children}
      <ToastContainer toasts={toasts} onClose={removeToast} />
    </ToastContext.Provider>
  );
}

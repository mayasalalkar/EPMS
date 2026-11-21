const TOAST_ICONS = {
  success: 'bi-check-circle-fill',
  danger: 'bi-exclamation-octagon-fill',
  primary: 'bi-info-circle-fill',
};

function ToastContainer({ toasts, onClose }) {
  return (
    <div className="toast-container position-fixed top-0 end-0 p-3">
      {toasts.map((toast) => (
        <div key={toast.id} className={`toast show border-0 text-bg-${toast.type}`} role="alert" aria-live="assertive">
          <div className="d-flex">
            <div className="toast-body d-flex align-items-center gap-2">
              <i className={`bi ${TOAST_ICONS[toast.type]}`}></i>
              <span>{toast.message}</span>
            </div>
            <button
              type="button"
              className="btn-close btn-close-white me-2 m-auto"
              aria-label="Close"
              onClick={() => onClose(toast.id)}
            ></button>
          </div>
        </div>
      ))}
    </div>
  );
}

export default ToastContainer;

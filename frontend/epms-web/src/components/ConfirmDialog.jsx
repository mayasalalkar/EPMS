import Modal from './Modal';

function ConfirmDialog({
  show,
  title = 'Are you sure?',
  message,
  confirmText = 'Delete',
  variant = 'danger',
  loading = false,
  onConfirm,
  onCancel,
}) {
  if (!show) {
    return null;
  }

  const handleClose = () => {
    if (!loading) {
      onCancel();
    }
  };

  return (
    <Modal title={title} onClose={handleClose}>
      <div className="modal-body">
        <div className="d-flex gap-3">
          <div className={`stat-icon flex-shrink-0 bg-${variant}-subtle text-${variant}`}>
            <i className="bi bi-exclamation-triangle"></i>
          </div>
          <p className="mb-0 text-secondary">{message}</p>
        </div>
      </div>
      <div className="modal-footer">
        <button type="button" className="btn btn-light" onClick={handleClose} disabled={loading}>
          Cancel
        </button>
        <button type="button" className={`btn btn-${variant}`} onClick={onConfirm} disabled={loading}>
          {loading && <span className="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>}
          {confirmText}
        </button>
      </div>
    </Modal>
  );
}

export default ConfirmDialog;

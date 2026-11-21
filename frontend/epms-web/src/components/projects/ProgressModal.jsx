import Modal from '../Modal';
import ProgressForm from './ProgressForm';
import StatusBadge from '../StatusBadge';

function ProgressModal({ project, onClose, onSaved }) {
  return (
    <Modal title="Update Progress" onClose={onClose}>
      <div className="modal-body">
        <div className="d-flex align-items-center justify-content-between gap-2 mb-3">
          <span className="fw-semibold">{project.title}</span>
          <StatusBadge status={project.status} />
        </div>
        <ProgressForm project={project} onSaved={onSaved} onCancel={onClose} />
      </div>
    </Modal>
  );
}

export default ProgressModal;

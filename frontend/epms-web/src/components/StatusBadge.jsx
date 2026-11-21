import { getStatusLabel } from '../utils/constants';

const STATUS_STYLES = {
  Pending: 'bg-warning-subtle text-warning-emphasis',
  InProgress: 'bg-primary-subtle text-primary-emphasis',
  Completed: 'bg-success-subtle text-success-emphasis',
};

function StatusBadge({ status }) {
  return (
    <span className={`badge rounded-pill fw-medium ${STATUS_STYLES[status] ?? 'bg-secondary-subtle text-secondary-emphasis'}`}>
      {getStatusLabel(status)}
    </span>
  );
}

export default StatusBadge;

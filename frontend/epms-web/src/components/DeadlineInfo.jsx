import { daysUntil } from '../utils/formatDate';

function DeadlineInfo({ project }) {
  if (project.status === 'Completed') {
    return (
      <span className="small text-success">
        <i className="bi bi-check2-circle me-1"></i>
        Completed
      </span>
    );
  }

  const days = daysUntil(project.deadline);

  if (days < 0) {
    return (
      <span className="small text-danger fw-medium">
        <i className="bi bi-exclamation-circle me-1"></i>
        Overdue by {Math.abs(days)} {Math.abs(days) === 1 ? 'day' : 'days'}
      </span>
    );
  }

  if (days === 0) {
    return (
      <span className="small text-warning-emphasis fw-medium">
        <i className="bi bi-alarm me-1"></i>
        Due today
      </span>
    );
  }

  return (
    <span className={`small ${days <= 7 ? 'text-warning-emphasis' : 'text-secondary'}`}>
      <i className="bi bi-clock me-1"></i>
      {days} {days === 1 ? 'day' : 'days'} left
    </span>
  );
}

export default DeadlineInfo;

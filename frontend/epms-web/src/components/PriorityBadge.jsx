const PRIORITY_STYLES = {
  Low: { className: 'bg-secondary-subtle text-secondary-emphasis', icon: 'bi-arrow-down' },
  Medium: { className: 'bg-info-subtle text-info-emphasis', icon: 'bi-dash' },
  High: { className: 'bg-danger-subtle text-danger-emphasis', icon: 'bi-arrow-up' },
};

function PriorityBadge({ priority }) {
  const style = PRIORITY_STYLES[priority] ?? PRIORITY_STYLES.Low;

  return (
    <span className={`badge rounded-pill fw-medium ${style.className}`}>
      <i className={`bi ${style.icon} me-1`}></i>
      {priority}
    </span>
  );
}

export default PriorityBadge;

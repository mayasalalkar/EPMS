function EmptyState({ icon = 'bi-inbox', title = 'Nothing here yet', message, action }) {
  return (
    <div className="text-center py-5 px-3">
      <div className="empty-icon mx-auto mb-3">
        <i className={`bi ${icon}`}></i>
      </div>
      <h6 className="fw-semibold mb-1">{title}</h6>
      {message && <p className="text-secondary small mb-3">{message}</p>}
      {action}
    </div>
  );
}

export default EmptyState;

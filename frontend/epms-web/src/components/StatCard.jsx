function StatCard({ title, value, icon, variant = 'primary' }) {
  return (
    <div className="card h-100">
      <div className="card-body d-flex align-items-center gap-3">
        <div className={`stat-icon bg-${variant}-subtle text-${variant}`}>
          <i className={`bi ${icon}`}></i>
        </div>
        <div>
          <div className="text-secondary small fw-medium">{title}</div>
          <div className="fs-4 fw-semibold lh-sm">{value}</div>
        </div>
      </div>
    </div>
  );
}

export default StatCard;

function ProgressBar({ value = 0, showLabel = true }) {
  const variant = value >= 100 ? 'bg-success' : 'bg-primary';

  return (
    <div className="d-flex align-items-center gap-2">
      <div
        className="progress progress-thin flex-grow-1"
        role="progressbar"
        aria-valuenow={value}
        aria-valuemin="0"
        aria-valuemax="100"
      >
        <div className={`progress-bar ${variant}`} style={{ width: `${value}%` }}></div>
      </div>
      {showLabel && <span className="small text-secondary progress-label">{value}%</span>}
    </div>
  );
}

export default ProgressBar;

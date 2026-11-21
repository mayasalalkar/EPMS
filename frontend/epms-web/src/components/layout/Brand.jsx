function Brand({ large = false }) {
  return (
    <div className="d-flex align-items-center gap-2">
      <div className={`brand-logo ${large ? 'brand-logo-lg' : ''}`}>
        <i className="bi bi-diagram-3"></i>
      </div>
      <div className="lh-sm">
        <div className={`fw-bold ${large ? 'fs-4' : 'fs-5'}`}>EPMS</div>
        <div className="text-secondary brand-subtitle">Employee &amp; Project Management</div>
      </div>
    </div>
  );
}

export default Brand;

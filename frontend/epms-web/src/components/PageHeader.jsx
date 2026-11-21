function PageHeader({ title, subtitle, actions }) {
  return (
    <div className="d-flex flex-column flex-sm-row align-items-sm-center justify-content-between gap-3 mb-4">
      <div>
        <h4 className="fw-semibold mb-1">{title}</h4>
        {subtitle && <p className="text-secondary mb-0">{subtitle}</p>}
      </div>
      {actions && <div className="d-flex gap-2">{actions}</div>}
    </div>
  );
}

export default PageHeader;

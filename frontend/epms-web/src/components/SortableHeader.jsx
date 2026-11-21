function SortableHeader({ label, field, sortBy, sortDir, onSort, className = '' }) {
  const isActive = sortBy === field;
  let icon = 'bi-arrow-down-up text-body-tertiary';
  if (isActive) {
    icon = sortDir === 'asc' ? 'bi-sort-up' : 'bi-sort-down';
  }

  return (
    <th scope="col" className={className} aria-sort={isActive ? (sortDir === 'asc' ? 'ascending' : 'descending') : undefined}>
      <button type="button" className="btn btn-link p-0 text-reset text-decoration-none fw-semibold"
        onClick={() => onSort(field)}>
        {label}<i className={`bi ${icon} ms-1`} aria-hidden="true"></i>
      </button>
    </th>
  );
}

export default SortableHeader;

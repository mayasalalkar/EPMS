import { PAGE_SIZE_OPTIONS } from '../utils/constants';

function getPageNumbers(page, totalPages) {
  const visible = 5;
  let start = Math.max(1, page - Math.floor(visible / 2));
  const end = Math.min(totalPages, start + visible - 1);
  start = Math.max(1, end - visible + 1);

  const pages = [];
  for (let number = start; number <= end; number += 1) {
    pages.push(number);
  }
  return pages;
}

function Pagination({ page, pageSize, totalCount, totalPages, onPageChange, onPageSizeChange }) {
  const from = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
  const to = Math.min(page * pageSize, totalCount);
  const pages = getPageNumbers(page, totalPages);

  return (
    <div className="d-flex flex-column flex-md-row align-items-center justify-content-between gap-3">
      <div className="d-flex align-items-center gap-2 text-secondary small">
        <span>Rows per page</span>
        <select
          className="form-select form-select-sm w-auto"
          value={pageSize}
          onChange={(event) => onPageSizeChange(Number(event.target.value))}
          aria-label="Rows per page"
        >
          {PAGE_SIZE_OPTIONS.map((size) => (
            <option key={size} value={size}>
              {size}
            </option>
          ))}
        </select>
        <span className="ms-2">
          Showing {from}-{to} of {totalCount}
        </span>
      </div>

      {totalPages > 1 && (
        <nav aria-label="Pagination">
          <ul className="pagination pagination-sm mb-0">
            <li className={`page-item ${page === 1 ? 'disabled' : ''}`}>
              <button type="button" className="page-link" onClick={() => onPageChange(page - 1)} aria-label="Previous">
                <i className="bi bi-chevron-left"></i>
              </button>
            </li>
            {pages.map((number) => (
              <li key={number} className={`page-item ${number === page ? 'active' : ''}`}>
                <button type="button" className="page-link" onClick={() => onPageChange(number)}>
                  {number}
                </button>
              </li>
            ))}
            <li className={`page-item ${page === totalPages ? 'disabled' : ''}`}>
              <button type="button" className="page-link" onClick={() => onPageChange(page + 1)} aria-label="Next">
                <i className="bi bi-chevron-right"></i>
              </button>
            </li>
          </ul>
        </nav>
      )}
    </div>
  );
}

export default Pagination;

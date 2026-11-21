import { useMemo, useState } from 'react';
import Avatar from '../Avatar';

function EmployeePicker({ employees, selectedIds, onChange, loading }) {
  const [search, setSearch] = useState('');

  const filteredEmployees = useMemo(() => {
    const term = search.trim().toLowerCase();
    if (!term) {
      return employees;
    }
    return employees.filter(
      (employee) => employee.name.toLowerCase().includes(term) || employee.email.toLowerCase().includes(term),
    );
  }, [employees, search]);

  const toggleEmployee = (id) => {
    if (selectedIds.includes(id)) {
      onChange(selectedIds.filter((selectedId) => selectedId !== id));
    } else {
      onChange([...selectedIds, id]);
    }
  };

  return (
    <div className="border rounded-3">
      <div className="p-2 border-bottom d-flex align-items-center gap-2">
        <div className="input-group input-group-sm">
          <span className="input-group-text bg-white">
            <i className="bi bi-search"></i>
          </span>
          <input
            type="search"
            className="form-control"
            placeholder="Search employees"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            aria-label="Search employees"
          />
        </div>
        <span className="badge bg-primary-subtle text-primary-emphasis text-nowrap">{selectedIds.length} selected</span>
        {selectedIds.length > 0 && (
          <button type="button" className="btn btn-link btn-sm text-decoration-none p-0" onClick={() => onChange([])}>
            Clear
          </button>
        )}
      </div>

      <div className="employee-picker p-2">
        {loading && <div className="text-secondary small p-2">Loading employees...</div>}
        {!loading && filteredEmployees.length === 0 && (
          <div className="text-secondary small p-2">No employees found.</div>
        )}
        {!loading &&
          filteredEmployees.map((employee) => (
            <label
              key={employee.id}
              className="employee-option d-flex align-items-center gap-2 px-2 py-1 rounded-2"
              htmlFor={`assign-${employee.id}`}
            >
              <input
                id={`assign-${employee.id}`}
                type="checkbox"
                className="form-check-input mt-0"
                checked={selectedIds.includes(employee.id)}
                onChange={() => toggleEmployee(employee.id)}
              />
              <Avatar name={employee.name} seed={employee.id} size="sm" />
              <span className="lh-sm">
                <span className="d-block small fw-medium">{employee.name}</span>
                <span className="d-block text-secondary picker-email">{employee.email}</span>
              </span>
            </label>
          ))}
      </div>
    </div>
  );
}

export default EmployeePicker;

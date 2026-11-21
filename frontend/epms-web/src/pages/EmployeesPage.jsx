import { useEffect, useState } from 'react';
import employeeApi from '../api/employeeApi';
import Avatar from '../components/Avatar';
import ConfirmDialog from '../components/ConfirmDialog';
import ListState from '../components/ListState';
import PageHeader from '../components/PageHeader';
import SortableHeader from '../components/SortableHeader';
import EmployeeFormModal from '../components/employees/EmployeeFormModal';
import usePagedList from '../hooks/usePagedList';
import useToast from '../hooks/useToast';
import { getErrorMessage } from '../utils/errors';

const columns = [['Name', 'name'], ['Email', 'email'], ['Phone', 'phone'],
  ['Department', 'department'], ['Designation', 'designation']];

export default function EmployeesPage() {
  const list = usePagedList(employeeApi.getAll, { sortBy: 'name', department: '' });
  const toast = useToast();
  const [departments, setDepartments] = useState([]);
  const [departmentsError, setDepartmentsError] = useState('');
  const [form, setForm] = useState(null);
  const [deleting, setDeleting] = useState(null);
  const [removing, setRemoving] = useState(false);
  const [revision, setRevision] = useState(0);

  useEffect(() => {
    let active = true;
    employeeApi.getDepartments()
      .then((items) => { if (active) { setDepartments(items); setDepartmentsError(''); } })
      .catch((error) => { if (active) setDepartmentsError(getErrorMessage(error)); });
    return () => { active = false; };
  }, [revision]);

  function refresh() {
    list.refresh();
    setRevision((value) => value + 1);
  }

  async function remove() {
    setRemoving(true);
    try {
      await employeeApi.remove(deleting.id);
      toast.success(`${deleting.name} was deleted.`);
      setDeleting(null);
      refresh();
    } catch (error) {
      toast.error(getErrorMessage(error));
    } finally {
      setRemoving(false);
    }
  }

  return (
    <>
      <PageHeader title="Employees" subtitle="Manage your employee directory."
        actions={<button className="btn btn-primary" onClick={() => setForm({ employee: null })}>
          <i className="bi bi-person-plus me-2" aria-hidden="true" />Add employee
        </button>} />
      <section className="card workspace-panel">
        <div className="card-body">
          <div className="row g-2 mb-3">
            <div className="col-md-7">
              <label className="visually-hidden" htmlFor="employee-search">Search employees</label>
              <input id="employee-search" type="search" className="form-control" placeholder="Search employees"
                value={list.search} onChange={(event) => list.changeSearch(event.target.value)} />
            </div>
            <div className="col-md-5">
              <label className="visually-hidden" htmlFor="employee-department">Department</label>
              <select id="employee-department" className="form-select" value={list.query.department}
                onChange={(event) => list.update({ department: event.target.value })}>
                <option value="">All departments</option>
                {[...new Set([list.query.department, ...departments].filter(Boolean))].map((name) =>
                  <option key={name}>{name}</option>)}
              </select>
            </div>
          </div>
          {departmentsError && <p className="text-danger small" role="alert">Departments: {departmentsError}</p>}
          <ListState list={list} emptyTitle="No employees match your search" emptyMessage="Try another search or department.">
            <div className="table-responsive">
              <table className="table align-middle mb-0">
                <thead><tr>
                  {columns.map(([label, field]) => <SortableHeader key={field} label={label} field={field}
                    sortBy={list.query.sortBy} sortDir={list.query.sortDir} onSort={list.sort} />)}
                  <th scope="col" className="text-end">Actions</th>
                </tr></thead>
                <tbody>{list.data?.items.map((employee) => (
                  <tr key={employee.id}>
                    <td><div className="d-flex align-items-center gap-2"><Avatar name={employee.name} />{employee.name}</div></td>
                    <td>{employee.email}</td><td>{employee.phone || '—'}</td>
                    <td>{employee.department}</td><td>{employee.designation}</td>
                    <td className="text-end text-nowrap">
                      <button className="btn btn-sm btn-outline-secondary me-1" onClick={() => setForm({ employee })}>Edit</button>
                      <button className="btn btn-sm btn-outline-danger" onClick={() => setDeleting(employee)}>Delete</button>
                    </td>
                  </tr>
                ))}</tbody>
              </table>
            </div>
          </ListState>
        </div>
      </section>
      {form && <EmployeeFormModal employee={form.employee} departments={departments} onClose={() => setForm(null)}
        onSaved={(employee, isEdit) => {
          setForm(null);
          toast.success(`${employee.name} was ${isEdit ? 'updated' : 'added'}.`);
          refresh();
        }} />}
      <ConfirmDialog show={Boolean(deleting)} title="Delete employee?"
        message={`Removing ${deleting?.name ?? 'this employee'} also removes their login and assignments.`}
        loading={removing} onConfirm={remove} onCancel={() => setDeleting(null)} />
    </>
  );
}

import { useEffect, useState } from 'react';
import employeeApi from '../api/employeeApi';
import projectApi from '../api/projectApi';
import AvatarGroup from '../components/AvatarGroup';
import ConfirmDialog from '../components/ConfirmDialog';
import DeadlineInfo from '../components/DeadlineInfo';
import ListState from '../components/ListState';
import PageHeader from '../components/PageHeader';
import PriorityBadge from '../components/PriorityBadge';
import ProgressBar from '../components/ProgressBar';
import SortableHeader from '../components/SortableHeader';
import StatusBadge from '../components/StatusBadge';
import ProjectFormModal from '../components/projects/ProjectFormModal';
import usePagedList from '../hooks/usePagedList';
import useToast from '../hooks/useToast';
import { PROJECT_PRIORITIES, PROJECT_STATUSES } from '../utils/constants';
import { getErrorMessage } from '../utils/errors';

const columns = [['Project', 'title'], ['Deadline', 'deadline'], ['Status', 'status'],
  ['Priority', 'priority'], ['Progress', 'progressPercent']];

export default function ProjectsPage() {
  const list = usePagedList(projectApi.getAll, { sortBy: 'title', status: '', priority: '' });
  const toast = useToast();
  const [form, setForm] = useState(null);
  const [lookup, setLookup] = useState({ items: [], loading: true, error: '' });
  const [deleting, setDeleting] = useState(null);
  const [removing, setRemoving] = useState(false);
  const formOpen = form !== null;

  useEffect(() => {
    if (!formOpen) return;
    let active = true;
    employeeApi.getLookup()
      .then((items) => { if (active) setLookup({ items, loading: false, error: '' }); })
      .catch((error) => { if (active) setLookup({ items: [], loading: false, error: getErrorMessage(error) }); });
    return () => { active = false; };
  }, [formOpen]);

  function openForm(project) {
    setLookup({ items: [], loading: true, error: '' });
    setForm({ project });
  }

  async function remove() {
    setRemoving(true);
    try {
      await projectApi.remove(deleting.id);
      toast.success(`${deleting.title} was deleted.`);
      setDeleting(null);
      list.refresh();
    } catch (error) {
      toast.error(getErrorMessage(error));
    } finally {
      setRemoving(false);
    }
  }

  return (
    <>
      <PageHeader title="Projects" subtitle="Plan work, assign employees, and track delivery."
        actions={<button className="btn btn-primary" onClick={() => openForm(null)}>
          <i className="bi bi-plus-lg me-2" aria-hidden="true" />Add project
        </button>} />
      <section className="card workspace-panel">
        <div className="card-body">
          <div className="row g-2 mb-3">
            <div className="col-md-6">
              <label className="visually-hidden" htmlFor="project-search">Search projects</label>
              <input id="project-search" type="search" className="form-control" placeholder="Search projects" value={list.search}
                onChange={(event) => list.changeSearch(event.target.value)} />
            </div>
            {[['status', 'Status', PROJECT_STATUSES], ['priority', 'Priority', PROJECT_PRIORITIES]].map(([key, label, options]) => (
              <div className="col-6 col-md-3" key={key}>
                <label className="visually-hidden" htmlFor={`filter-${key}`}>{label}</label>
                <select id={`filter-${key}`} className="form-select" value={list.query[key]}
                  onChange={(event) => list.update({ [key]: event.target.value })}>
                  <option value="">All {key === 'status' ? 'statuses' : 'priorities'}</option>
                  {options.map(({ value, label: text }) => <option key={value} value={value}>{text}</option>)}
                </select>
              </div>
            ))}
          </div>
          <ListState list={list} emptyTitle="No projects match your filters" emptyMessage="Clear a filter or search for something else.">
            <div className="table-responsive">
              <table className="table align-middle mb-0">
                <thead><tr>
                  {columns.map(([label, field]) => <SortableHeader key={field} label={label} field={field}
                    sortBy={list.query.sortBy} sortDir={list.query.sortDir} onSort={list.sort} />)}
                  <th scope="col">Assigned</th><th scope="col" className="text-end">Actions</th>
                </tr></thead>
                <tbody>{list.data?.items.map((project) => (
                  <tr key={project.id}>
                    <td className="fw-semibold">{project.title}</td>
                    <td className="text-nowrap"><DeadlineInfo project={project} /></td>
                    <td><StatusBadge status={project.status} /></td>
                    <td><PriorityBadge priority={project.priority} /></td>
                    <td className="table-progress"><ProgressBar value={project.progressPercent} /></td>
                    <td><AvatarGroup people={project.assignedEmployees} /></td>
                    <td className="text-end text-nowrap">
                      <button className="btn btn-sm btn-outline-secondary me-1" onClick={() => openForm(project)}>Edit</button>
                      <button className="btn btn-sm btn-outline-danger" onClick={() => setDeleting(project)}>Delete</button>
                    </td>
                  </tr>
                ))}</tbody>
              </table>
            </div>
          </ListState>
        </div>
      </section>
      {form && <ProjectFormModal project={form.project} employees={lookup.items} employeesLoading={lookup.loading}
        employeesError={lookup.error} onClose={() => setForm(null)} onSaved={(project, isEdit) => {
          setForm(null);
          toast.success(`${project.title} was ${isEdit ? 'updated' : 'added'}.`);
          list.refresh();
        }} />}
      <ConfirmDialog show={Boolean(deleting)} title="Delete project?"
        message={`Delete ${deleting?.title ?? 'this project'} and its assignments? This cannot be undone.`}
        loading={removing} onConfirm={remove} onCancel={() => setDeleting(null)} />
    </>
  );
}

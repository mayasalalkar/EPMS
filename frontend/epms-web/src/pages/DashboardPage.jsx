import { useEffect, useState } from 'react';
import { ArcElement, Chart as ChartJS, Legend, Tooltip } from 'chart.js';
import { Doughnut } from 'react-chartjs-2';
import dashboardApi from '../api/dashboardApi';
import DeadlineInfo from '../components/DeadlineInfo';
import EmptyState from '../components/EmptyState';
import Loader from '../components/Loader';
import PageHeader from '../components/PageHeader';
import StatCard from '../components/StatCard';
import useAuth from '../hooks/useAuth';
import { getErrorMessage } from '../utils/errors';

ChartJS.register(ArcElement, Legend, Tooltip);

export default function DashboardPage() {
  const { isAdmin } = useAuth();
  const [data, setData] = useState(null);
  const [error, setError] = useState('');
  const [revision, setRevision] = useState(0);

  useEffect(() => {
    let active = true;
    dashboardApi.getSummary()
      .then((result) => { if (active) setData(result); })
      .catch((failure) => { if (active) setError(getErrorMessage(failure)); });
    return () => { active = false; };
  }, [revision]);

  const stats = data ? [
    ...(isAdmin ? [['Employees', data.totalEmployees, 'bi-people', 'primary']] : []),
    ['Projects', data.totalProjects, 'bi-kanban', 'primary'],
    ['Completed', data.completedProjects, 'bi-check2-circle', 'success'],
    ['In progress', data.inProgressProjects, 'bi-arrow-repeat', 'info'],
    ['Pending', data.pendingProjects, 'bi-hourglass-split', 'warning'],
    ['Overdue', data.overdueProjects, 'bi-exclamation-circle', 'danger'],
  ] : [];

  return (
    <>
      <PageHeader title={isAdmin ? 'Dashboard' : 'My Dashboard'} subtitle={isAdmin
        ? 'Track project progress, deadlines, and employee assignments.'
        : 'See the status and upcoming deadlines of your assigned projects.'} />
      {error ? <div className="alert alert-danger" role="alert">{error}{' '}
        <button className="btn btn-sm btn-outline-danger ms-2"
          onClick={() => { setError(''); setRevision((value) => value + 1); }}>Retry</button>
      </div> : !data ? <Loader /> : (
        <>
          <div className="row row-cols-2 row-cols-md-3 g-3 mb-4">
            {stats.map(([title, value, icon, variant]) => (
              <div className="col" key={title}><StatCard {...{ title, value, icon, variant }} /></div>
            ))}
          </div>
          <div className="row g-3 mb-4">
            <section className="col-lg-5">
              <div className="card workspace-panel h-100"><div className="card-body">
                <h2 className="h5 mb-3">Project status overview</h2>
                {data.totalProjects > 0 ? <div className="status-chart">
                  <Doughnut role="img"
                    aria-label={`${data.completedProjects} completed, ${data.inProgressProjects} in progress, ${data.pendingProjects} pending`}
                    options={{ maintainAspectRatio: false, plugins: { legend: { position: 'bottom' } } }}
                    data={{
                      labels: ['Completed', 'In progress', 'Pending'],
                      datasets: [{ data: [data.completedProjects, data.inProgressProjects, data.pendingProjects],
                        backgroundColor: ['#18805e', '#285ec7', '#cf8517'], borderWidth: 0 }],
                    }} />
                </div> : <EmptyState title="No projects yet" />}
              </div></div>
            </section>
            <section className="col-lg-7">
              <div className="card workspace-panel h-100"><div className="card-body">
                <h2 className="h5 mb-3">Upcoming deadlines</h2>
                {data.upcomingDeadlines.length ? <ul className="list-group list-group-flush">
                  {data.upcomingDeadlines.map((project) => <li key={project.id}
                    className="list-group-item px-0 d-flex justify-content-between align-items-center gap-2">
                    <span className="fw-medium">{project.title}</span><DeadlineInfo project={project} />
                  </li>)}
                </ul> : <EmptyState title="No upcoming deadlines" />}
              </div></div>
            </section>
          </div>
          {isAdmin && <section className="card workspace-panel"><div className="card-body">
            <h2 className="h5 mb-3">Employee assignment summary</h2>
            {data.employeeAssignments.length ? <div className="table-responsive">
              <table className="table align-middle mb-0">
                <thead><tr>{['Employee', 'Department', 'Total', 'Pending', 'In progress', 'Completed'].map((column) =>
                  <th scope="col" key={column}>{column}</th>)}</tr></thead>
                <tbody>{data.employeeAssignments.map((row) => <tr key={row.employeeId}>
                  <th scope="row">{row.name}</th><td>{row.department}</td><td>{row.totalProjects}</td>
                  <td>{row.pendingProjects}</td><td>{row.inProgressProjects}</td><td>{row.completedProjects}</td>
                </tr>)}</tbody>
              </table>
            </div> : <EmptyState title="No employees yet" />}
          </div></section>}
        </>
      )}
    </>
  );
}

import projectApi from '../api/projectApi';
import DeadlineInfo from '../components/DeadlineInfo';
import ListState from '../components/ListState';
import PageHeader from '../components/PageHeader';
import PriorityBadge from '../components/PriorityBadge';
import ProgressBar from '../components/ProgressBar';
import StatusBadge from '../components/StatusBadge';
import ProgressForm from '../components/projects/ProgressForm';
import usePagedList from '../hooks/usePagedList';

export default function MyProjectsPage() {
  const list = usePagedList(projectApi.getAll, { sortBy: 'deadline', pageSize: 10 });
  return (
    <>
      <PageHeader title="My Projects" subtitle="View and update the progress of projects assigned to you." />
      <ListState list={list} emptyTitle="No projects assigned" emptyMessage="Projects will appear here when an administrator assigns them to you.">
        <div className="row row-cols-1 row-cols-xl-2 g-3">
          {list.data?.items.map((project) => (
            <div className="col" key={project.id}>
              <article className="card workspace-panel h-100">
                <div className="card-body d-flex flex-column">
                  <div className="d-flex gap-2 mb-2">
                    <StatusBadge status={project.status} /><PriorityBadge priority={project.priority} />
                  </div>
                  <h2 className="h5">{project.title}</h2>
                  <p className="text-secondary flex-grow-1">{project.description}</p>
                  <div className="small text-secondary mb-2"><DeadlineInfo project={project} /></div>
                  <ProgressBar value={project.progressPercent} />
                  <div className="border-top mt-3 pt-3">
                    <ProgressForm project={project} onSaved={list.refresh} />
                  </div>
                </div>
              </article>
            </div>
          ))}
        </div>
      </ListState>
    </>
  );
}

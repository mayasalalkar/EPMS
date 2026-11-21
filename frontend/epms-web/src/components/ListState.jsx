import EmptyState from './EmptyState';
import Loader from './Loader';
import Pagination from './Pagination';

export default function ListState({ list, children, emptyTitle, emptyMessage }) {
  const { data, error, loading, update } = list;
  if (loading) return <Loader />;
  if (error) {
    return <div className="alert alert-danger" role="alert">{error}{' '}
      <button className="btn btn-sm btn-outline-danger ms-2" onClick={list.refresh}>Retry</button>
    </div>;
  }
  if (!data) return null;

  return (
    <>
      {data.items.length ? children : <EmptyState title={emptyTitle} message={emptyMessage} />}
      {data.totalCount > 0 && (
        <div className="mt-3">
          <Pagination {...data} onPageChange={(page) => update({ page })}
            onPageSizeChange={(pageSize) => update({ pageSize })} />
        </div>
      )}
    </>
  );
}

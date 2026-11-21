import useAuth from '../../hooks/useAuth';
import Avatar from '../Avatar';

function Topbar({ onMenuClick }) {
  const { user, logout } = useAuth();
  const firstName = user?.name?.split(' ')[0] ?? '';

  return (
    <header className="app-topbar px-3 px-lg-4 py-2">
      <div className="d-flex align-items-center justify-content-between gap-3">
        <div className="d-flex align-items-center gap-2">
          <button type="button" className="btn btn-light d-lg-none" aria-label="Open menu" onClick={onMenuClick}>
            <i className="bi bi-list fs-5"></i>
          </button>
          <span className="d-lg-none fw-bold fs-5">EPMS</span>
          <span className="d-none d-lg-inline text-secondary">
            Welcome back, <span className="fw-semibold text-body">{firstName}</span>
          </span>
        </div>

        <div className="d-flex align-items-center gap-3">
          <div className="d-flex align-items-center gap-2">
            <Avatar name={user?.name} seed={user?.id} />
            <div className="d-none d-sm-block lh-sm">
              <div className="fw-semibold small">{user?.name}</div>
              <div className={`badge ${user?.role === 'Admin' ? 'text-bg-primary' : 'text-bg-secondary'}`}>{user?.role}</div>
            </div>
          </div>
          <button type="button" className="btn btn-outline-secondary btn-sm" onClick={logout} title="Sign out">
            <i className="bi bi-box-arrow-right"></i>
            <span className="d-none d-md-inline ms-1">Sign out</span>
          </button>
        </div>
      </div>
    </header>
  );
}

export default Topbar;

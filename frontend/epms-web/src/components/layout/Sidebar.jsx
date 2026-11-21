import { NavLink } from 'react-router-dom';
import useAuth from '../../hooks/useAuth';
import Brand from './Brand';

const ADMIN_LINKS = [
  { to: '/', label: 'Dashboard', icon: 'bi-grid-1x2', end: true },
  { to: '/employees', label: 'Employees', icon: 'bi-people' },
  { to: '/projects', label: 'Projects', icon: 'bi-kanban' },
];

const EMPLOYEE_LINKS = [
  { to: '/', label: 'Dashboard', icon: 'bi-grid-1x2', end: true },
  { to: '/my-projects', label: 'My Projects', icon: 'bi-briefcase' },
];

function Sidebar({ open, onClose }) {
  const { isAdmin, logout } = useAuth();
  const links = isAdmin ? ADMIN_LINKS : EMPLOYEE_LINKS;

  return (
    <aside className={`app-sidebar ${open ? 'open' : ''}`}>
      <div className="d-flex align-items-center justify-content-between px-3 py-3 border-bottom">
        <Brand />
        <button type="button" className="btn-close d-lg-none" aria-label="Close menu" onClick={onClose}></button>
      </div>

      <nav className="flex-grow-1 p-3">
        <div className="text-uppercase text-secondary fw-semibold small px-2 mb-2 sidebar-heading">Menu</div>
        <ul className="nav flex-column gap-1">
          {links.map((link) => (
            <li key={link.to} className="nav-item">
              <NavLink to={link.to} end={link.end} className="sidebar-link" onClick={onClose}>
                <i className={`bi ${link.icon}`}></i>
                <span>{link.label}</span>
              </NavLink>
            </li>
          ))}
        </ul>
      </nav>

      <div className="p-3 border-top">
        <button type="button" className="sidebar-link w-100 border-0 bg-transparent" onClick={logout}>
          <i className="bi bi-box-arrow-left"></i>
          <span>Sign out</span>
        </button>
      </div>
    </aside>
  );
}

export default Sidebar;

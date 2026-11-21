import { Link } from 'react-router-dom';
import Footer from '../components/layout/Footer';

function NotFoundPage() {
  return (
    <div className="auth-page">
      <div className="text-center">
        <div className="display-1 fw-bold text-primary">404</div>
        <h4 className="fw-semibold">Page not found</h4>
        <p className="text-secondary mb-4">The page you are looking for does not exist or has been moved.</p>
        <Link to="/" className="btn btn-primary">
          <i className="bi bi-house me-2"></i>
          Back to dashboard
        </Link>
        <Footer className="mt-4" />
      </div>
    </div>
  );
}

export default NotFoundPage;

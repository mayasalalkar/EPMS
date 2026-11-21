import { useState } from 'react';
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import Brand from '../components/layout/Brand';
import Footer from '../components/layout/Footer';
import useAuth from '../hooks/useAuth';
import useToast from '../hooks/useToast';
import { getErrorMessage } from '../utils/errors';
import { isValidEmail } from '../utils/helpers';

function LoginPage() {
  const { login } = useAuth();
  const toast = useToast();
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams] = useSearchParams();

  const [values, setValues] = useState({ email: '', password: '' });
  const [errors, setErrors] = useState({});
  const [serverError, setServerError] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  const sessionExpired = searchParams.get('session') === 'expired';

  const handleChange = (event) => {
    const { name, value } = event.target;
    setValues((current) => ({ ...current, [name]: value }));
    setErrors((current) => ({ ...current, [name]: undefined }));
  };

  const validate = () => {
    const nextErrors = {};
    if (!values.email.trim()) {
      nextErrors.email = 'Email is required.';
    } else if (!isValidEmail(values.email.trim())) {
      nextErrors.email = 'Enter a valid email address.';
    }
    if (!values.password) {
      nextErrors.password = 'Password is required.';
    }
    return nextErrors;
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setServerError('');

    const validationErrors = validate();
    setErrors(validationErrors);
    if (Object.keys(validationErrors).length > 0) {
      return;
    }

    setSubmitting(true);
    try {
      const user = await login({ email: values.email.trim(), password: values.password });
      toast.success(`Welcome back, ${user.name}!`);
      navigate(location.state?.from?.pathname ?? '/', { replace: true });
    } catch (error) {
      setServerError(getErrorMessage(error, 'Unable to sign in. Please try again.'));
      setSubmitting(false);
    }
  };

  return (
    <main className="login-page">
      <section className="login-overview" aria-label="About EPMS">
        <div className="login-overview-content">
          <Brand large />
          <div className="login-intro">
            <p className="login-eyebrow mb-3">Team workspace</p>
            <p className="login-headline mb-3">See who’s working on what.</p>
            <p className="login-description mb-4">Manage your employee directory, assign projects, and track progress in one place.</p>
            <ul className="login-features list-unstyled m-0">
              <li><i className="bi bi-people" aria-hidden="true" />Employee directory</li>
              <li><i className="bi bi-kanban" aria-hidden="true" />Project assignments</li>
              <li><i className="bi bi-bar-chart" aria-hidden="true" />Progress dashboards</li>
            </ul>
          </div>
        </div>
      </section>
      <section className="login-panel" aria-labelledby="login-heading">
        <div className="login-form">
          <p className="login-eyebrow text-primary mb-2">Welcome back</p>
          <h1 id="login-heading" className="mb-2">Sign in</h1>
          <p className="text-secondary mb-4">Use your email and password to access your workspace.</p>
          {sessionExpired && !serverError && (
            <div className="alert alert-warning py-2 small" role="alert">
              <i className="bi bi-clock-history me-2"></i>
              Your session has expired. Please sign in again.
            </div>
          )}
          {serverError && (
            <div className="alert alert-danger py-2 small" role="alert">
              <i className="bi bi-exclamation-circle me-2"></i>
              {serverError}
            </div>
          )}

          <form onSubmit={handleSubmit} noValidate>
            <div className="mb-3">
              <label htmlFor="login-email" className="form-label">
                Email
              </label>
              <div className="input-group has-validation">
                <span className="input-group-text bg-white">
                  <i className="bi bi-envelope"></i>
                </span>
                <input
                  id="login-email"
                  name="email"
                  type="email"
                  className={`form-control ${errors.email ? 'is-invalid' : ''}`}
                  placeholder="name@company.com"
                  value={values.email}
                  onChange={handleChange}
                  autoComplete="email"
                  autoFocus
                />
                {errors.email && <div className="invalid-feedback">{errors.email}</div>}
              </div>
            </div>

            <div className="mb-4">
              <label htmlFor="login-password" className="form-label">
                Password
              </label>
              <div className="input-group has-validation">
                <span className="input-group-text bg-white">
                  <i className="bi bi-lock"></i>
                </span>
                <input
                  id="login-password"
                  name="password"
                  type={showPassword ? 'text' : 'password'}
                  className={`form-control ${errors.password ? 'is-invalid' : ''}`}
                  placeholder="Enter your password"
                  value={values.password}
                  onChange={handleChange}
                  autoComplete="current-password"
                />
                <button
                  type="button"
                  className="btn btn-outline-secondary"
                  onClick={() => setShowPassword((current) => !current)}
                  aria-label={showPassword ? 'Hide password' : 'Show password'}
                >
                  <i className={`bi ${showPassword ? 'bi-eye-slash' : 'bi-eye'}`}></i>
                </button>
                {errors.password && <div className="invalid-feedback">{errors.password}</div>}
              </div>
            </div>

            <button type="submit" className="btn btn-primary w-100" disabled={submitting}>
              {submitting && <span className="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>}
              Sign in
            </button>
          </form>

          <p className="text-secondary small mt-4 mb-0">
            New here? <Link to="/register">Create an account</Link>
          </p>
        </div>
        <Footer className="login-footer" />
      </section>
    </main>
  );
}

export default LoginPage;

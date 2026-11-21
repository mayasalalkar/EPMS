import Brand from './Brand';
import Footer from './Footer';

function AuthLayout({ title, subtitle, children, width = 'auth-card' }) {
  return (
    <div className="auth-page">
      <div className={`w-100 ${width}`}>
        <div className="d-flex justify-content-center mb-4">
          <Brand large />
        </div>
        <div className="card shadow-sm">
          <div className="card-body p-4 p-md-5">
            <h4 className="fw-semibold mb-1">{title}</h4>
            {subtitle && <p className="text-secondary mb-4">{subtitle}</p>}
            {children}
          </div>
        </div>
        <Footer className="mt-3" />
      </div>
    </div>
  );
}

export default AuthLayout;

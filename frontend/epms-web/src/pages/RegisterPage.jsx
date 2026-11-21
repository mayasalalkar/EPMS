import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import AuthLayout from '../components/layout/AuthLayout';
import useAuth from '../hooks/useAuth';
import useToast from '../hooks/useToast';
import { getErrorMessage, getFieldErrors } from '../utils/errors';
import { isValidEmail, isValidPhone } from '../utils/helpers';

const INITIAL_VALUES = {
  name: '',
  email: '',
  password: '',
  confirmPassword: '',
  phone: '',
  department: '',
  designation: '',
};

function validate(values) {
  const errors = {};

  if (!values.name.trim()) {
    errors.name = 'Name is required.';
  } else if (values.name.trim().length > 100) {
    errors.name = 'Name cannot exceed 100 characters.';
  }

  if (!values.email.trim()) {
    errors.email = 'Email is required.';
  } else if (!isValidEmail(values.email.trim())) {
    errors.email = 'Enter a valid email address.';
  }

  if (!values.password) {
    errors.password = 'Password is required.';
  } else if (values.password.length < 8) {
    errors.password = 'Password must be at least 8 characters.';
  } else if (new TextEncoder().encode(values.password).length > 72) {
    errors.password = 'Password must be at most 72 UTF-8 bytes.';
  }

  if (!values.confirmPassword) {
    errors.confirmPassword = 'Please confirm your password.';
  } else if (values.confirmPassword !== values.password) {
    errors.confirmPassword = 'Passwords do not match.';
  }

  if (values.phone.trim() && !isValidPhone(values.phone.trim())) {
    errors.phone = 'Enter a valid phone number (7-20 digits).';
  }

  return errors;
}

function RegisterPage() {
  const { register } = useAuth();
  const toast = useToast();
  const navigate = useNavigate();

  const [values, setValues] = useState(INITIAL_VALUES);
  const [errors, setErrors] = useState({});
  const [serverError, setServerError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const handleChange = (event) => {
    const { name, value } = event.target;
    setValues((current) => ({ ...current, [name]: value }));
    setErrors((current) => ({ ...current, [name]: undefined }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setServerError('');

    const validationErrors = validate(values);
    setErrors(validationErrors);
    if (Object.keys(validationErrors).length > 0) {
      return;
    }

    setSubmitting(true);
    try {
      const user = await register({
        name: values.name.trim(),
        email: values.email.trim(),
        password: values.password,
        phone: values.phone.trim() || null,
        department: values.department.trim(),
        designation: values.designation.trim(),
      });
      toast.success(`Account created. Welcome, ${user.name}!`);
      navigate('/', { replace: true });
    } catch (error) {
      setErrors(getFieldErrors(error));
      setServerError(getErrorMessage(error, 'Unable to create your account.'));
      setSubmitting(false);
    }
  };

  const renderField = (name, label, options = {}) => {
    const { type = 'text', required = false, placeholder, autoComplete, autoFocus } = options;
    return (
      <div className="mb-3">
        <label htmlFor={`register-${name}`} className="form-label">
          {label} {required ? <span className="text-danger">*</span> : <span className="text-secondary small">(optional)</span>}
        </label>
        <input
          id={`register-${name}`}
          name={name}
          type={type}
          className={`form-control ${errors[name] ? 'is-invalid' : ''}`}
          value={values[name]}
          onChange={handleChange}
          placeholder={placeholder}
          autoComplete={autoComplete}
          autoFocus={autoFocus}
        />
        {errors[name] && <div className="invalid-feedback">{errors[name]}</div>}
      </div>
    );
  };

  return (
    <AuthLayout title="Create an account" subtitle="Register as an employee to view and update your projects." width="auth-card-wide">
      {serverError && (
        <div className="alert alert-danger py-2 small" role="alert">
          <i className="bi bi-exclamation-circle me-2"></i>
          {serverError}
        </div>
      )}

      <form onSubmit={handleSubmit} noValidate>
        {renderField('name', 'Full name', { required: true, placeholder: 'e.g. Priya Sharma', autoComplete: 'name', autoFocus: true })}
        {renderField('email', 'Email', { type: 'email', required: true, placeholder: 'name@company.com', autoComplete: 'email' })}

        <div className="row">
          <div className="col-md-6">
            {renderField('password', 'Password', { type: 'password', required: true, placeholder: 'Min. 8 characters', autoComplete: 'new-password' })}
          </div>
          <div className="col-md-6">
            {renderField('confirmPassword', 'Confirm password', {
              type: 'password',
              required: true,
              placeholder: 'Re-enter password',
              autoComplete: 'new-password',
            })}
          </div>
        </div>

        {renderField('phone', 'Phone', { type: 'tel', placeholder: '+91 98765 43210', autoComplete: 'tel' })}

        <div className="row">
          <div className="col-md-6">{renderField('department', 'Department', { placeholder: 'e.g. Engineering' })}</div>
          <div className="col-md-6">{renderField('designation', 'Designation', { placeholder: 'e.g. Developer' })}</div>
        </div>

        <button type="submit" className="btn btn-primary w-100 mt-2" disabled={submitting}>
          {submitting && <span className="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>}
          Create account
        </button>
      </form>

      <p className="text-center text-secondary small mt-4 mb-0">
        Already have an account? <Link to="/login">Sign in</Link>
      </p>
    </AuthLayout>
  );
}

export default RegisterPage;

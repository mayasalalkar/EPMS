import { useState } from 'react';
import Modal from '../Modal';
import employeeApi from '../../api/employeeApi';
import { getErrorMessage, getFieldErrors } from '../../utils/errors';
import { isValidEmail, isValidPhone } from '../../utils/helpers';

const EMPTY_FORM = {
  name: '',
  email: '',
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
  } else if (values.email.trim().length > 150) {
    errors.email = 'Email cannot exceed 150 characters.';
  }

  if (!values.phone.trim()) {
    errors.phone = 'Phone is required.';
  } else if (!isValidPhone(values.phone.trim())) {
    errors.phone = 'Enter a valid phone number (7-20 digits).';
  }

  if (!values.department.trim()) {
    errors.department = 'Department is required.';
  } else if (values.department.trim().length > 100) {
    errors.department = 'Department cannot exceed 100 characters.';
  }

  if (!values.designation.trim()) {
    errors.designation = 'Designation is required.';
  } else if (values.designation.trim().length > 100) {
    errors.designation = 'Designation cannot exceed 100 characters.';
  }

  return errors;
}

function EmployeeFormModal({ employee, departments, onClose, onSaved }) {
  const isEdit = Boolean(employee);
  const [values, setValues] = useState(() =>
    employee
      ? {
          name: employee.name,
          email: employee.email,
          phone: employee.phone,
          department: employee.department,
          designation: employee.designation,
        }
      : EMPTY_FORM,
  );
  const [errors, setErrors] = useState({});
  const [serverError, setServerError] = useState('');
  const [saving, setSaving] = useState(false);

  const handleChange = (event) => {
    const { name, value } = event.target;
    setValues((current) => ({ ...current, [name]: value }));
    setErrors((current) => ({ ...current, [name]: undefined }));
  };

  const handleClose = () => {
    if (!saving) {
      onClose();
    }
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setServerError('');

    const validationErrors = validate(values);
    setErrors(validationErrors);
    if (Object.keys(validationErrors).length > 0) {
      return;
    }

    const payload = Object.fromEntries(Object.entries(values).map(([key, value]) => [key, value.trim()]));

    setSaving(true);
    try {
      const saved = isEdit ? await employeeApi.update(employee.id, payload) : await employeeApi.create(payload);
      onSaved(saved, isEdit);
    } catch (error) {
      const message = getErrorMessage(error);
      const fieldErrors = getFieldErrors(error);
      if (error.response?.status === 409 && message.toLowerCase().includes('email')) {
        fieldErrors.email = message;
      }
      setErrors(fieldErrors);
      setServerError(message);
      setSaving(false);
    }
  };

  const renderField = (name, label, type = 'text', extraProps = {}) => (
    <div className="mb-3">
      <label htmlFor={`employee-${name}`} className="form-label">
        {label} <span className="text-danger">*</span>
      </label>
      <input
        id={`employee-${name}`}
        name={name}
        type={type}
        className={`form-control ${errors[name] ? 'is-invalid' : ''}`}
        value={values[name]}
        onChange={handleChange}
        {...extraProps}
      />
      {errors[name] && <div className="invalid-feedback">{errors[name]}</div>}
    </div>
  );

  return (
    <Modal title={isEdit ? 'Edit Employee' : 'Add Employee'} onClose={handleClose}>
      <form onSubmit={handleSubmit} noValidate>
        <div className="modal-body">
          {serverError && (
            <div className="alert alert-danger py-2 small" role="alert">
              <i className="bi bi-exclamation-circle me-2"></i>
              {serverError}
            </div>
          )}

          {renderField('name', 'Full name', 'text', { placeholder: 'e.g. Priya Sharma', autoFocus: true })}
          {renderField('email', 'Email', 'email', { placeholder: 'name@company.com' })}
          {renderField('phone', 'Phone', 'tel', { placeholder: '+91 98765 43210' })}

          <div className="row">
            <div className="col-md-6">
              {renderField('department', 'Department', 'text', {
                list: 'department-options',
                placeholder: 'e.g. Engineering',
              })}
              <datalist id="department-options">
                {departments.map((department) => (
                  <option key={department} value={department} />
                ))}
              </datalist>
            </div>
            <div className="col-md-6">{renderField('designation', 'Designation', 'text', { placeholder: 'e.g. Developer' })}</div>
          </div>
        </div>
        <div className="modal-footer">
          <button type="button" className="btn btn-light" onClick={handleClose} disabled={saving}>
            Cancel
          </button>
          <button type="submit" className="btn btn-primary" disabled={saving}>
            {saving && <span className="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>}
            {isEdit ? 'Save changes' : 'Add employee'}
          </button>
        </div>
      </form>
    </Modal>
  );
}

export default EmployeeFormModal;

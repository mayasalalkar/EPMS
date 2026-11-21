import { useState } from 'react';
import Modal from '../Modal';
import EmployeePicker from './EmployeePicker';
import projectApi from '../../api/projectApi';
import { getErrorMessage, getFieldErrors } from '../../utils/errors';
import { toInputDate } from '../../utils/formatDate';
import { PROJECT_PRIORITIES, PROJECT_STATUSES } from '../../utils/constants';

function getInitialValues(project) {
  if (!project) {
    return {
      title: '',
      description: '',
      startDate: new Date().toISOString().slice(0, 10),
      deadline: '',
      status: 'Pending',
      priority: 'Medium',
      progressPercent: 0,
      employeeIds: [],
    };
  }

  return {
    title: project.title,
    description: project.description ?? '',
    startDate: toInputDate(project.startDate),
    deadline: toInputDate(project.deadline),
    status: project.status,
    priority: project.priority,
    progressPercent: project.progressPercent,
    employeeIds: project.assignedEmployees.map((employee) => employee.id),
  };
}

function validate(values) {
  const errors = {};

  if (!values.title.trim()) {
    errors.title = 'Title is required.';
  } else if (values.title.trim().length > 150) {
    errors.title = 'Title cannot exceed 150 characters.';
  }

  if (!values.description.trim()) {
    errors.description = 'Description is required.';
  } else if (values.description.length > 2000) {
    errors.description = 'Description cannot exceed 2000 characters.';
  }

  if (!values.startDate) {
    errors.startDate = 'Start date is required.';
  }

  if (!values.deadline) {
    errors.deadline = 'Deadline is required.';
  } else if (values.startDate && values.deadline < values.startDate) {
    errors.deadline = 'Deadline cannot be before the start date.';
  }

  const progress = Number(values.progressPercent);
  if (!Number.isInteger(progress) || progress < 0 || progress > 100) {
    errors.progressPercent = 'Progress must be between 0 and 100.';
  }

  if (values.employeeIds.length === 0) {
    errors.employeeIds = 'Select at least one employee.';
  }

  return errors;
}

function ProjectFormModal({ project, employees, employeesLoading, employeesError, onClose, onSaved }) {
  const isEdit = Boolean(project);
  const [values, setValues] = useState(() => getInitialValues(project));
  const [errors, setErrors] = useState({});
  const [serverError, setServerError] = useState('');
  const [saving, setSaving] = useState(false);

  const updateValue = (name, value) => {
    setValues((current) => {
      const next = { ...current, [name]: value };
      if (name === 'status' && value === 'Completed') {
        next.progressPercent = 100;
      }
      return next;
    });
    setErrors((current) => ({ ...current, [name]: undefined }));
  };

  const handleChange = (event) => {
    const { name, value } = event.target;
    updateValue(name, name === 'progressPercent' ? Number(value) : value);
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

    const payload = {
      ...values,
      title: values.title.trim(),
      description: values.description.trim(),
      progressPercent: Number(values.progressPercent),
    };

    setSaving(true);
    try {
      const saved = isEdit ? await projectApi.update(project.id, payload) : await projectApi.create(payload);
      onSaved(saved, isEdit);
    } catch (error) {
      setErrors(getFieldErrors(error));
      setServerError(getErrorMessage(error));
      setSaving(false);
    }
  };

  return (
    <Modal title={isEdit ? 'Edit Project' : 'Create Project'} size="lg" onClose={handleClose}>
      <form id="project-form" className="modal-body" onSubmit={handleSubmit} noValidate>
          {serverError && (
            <div className="alert alert-danger py-2 small" role="alert">
              <i className="bi bi-exclamation-circle me-2"></i>
              {serverError}
            </div>
          )}

          <div className="mb-3">
            <label htmlFor="project-title" className="form-label">
              Title <span className="text-danger">*</span>
            </label>
            <input
              id="project-title"
              name="title"
              className={`form-control ${errors.title ? 'is-invalid' : ''}`}
              value={values.title}
              onChange={handleChange}
              placeholder="e.g. Customer Portal Revamp"
              autoFocus
            />
            {errors.title && <div className="invalid-feedback">{errors.title}</div>}
          </div>

          <div className="mb-3">
            <label htmlFor="project-description" className="form-label">
              Description
            </label>
            <textarea
              id="project-description"
              name="description"
              rows="3"
              className={`form-control ${errors.description ? 'is-invalid' : ''}`}
              value={values.description}
              onChange={handleChange}
              placeholder="Short summary of the project scope"
            ></textarea>
            {errors.description && <div className="invalid-feedback">{errors.description}</div>}
          </div>

          <div className="row g-3 mb-3">
            <div className="col-md-6">
              <label htmlFor="project-start" className="form-label">
                Start date <span className="text-danger">*</span>
              </label>
              <input
                id="project-start"
                name="startDate"
                type="date"
                className={`form-control ${errors.startDate ? 'is-invalid' : ''}`}
                value={values.startDate}
                onChange={handleChange}
              />
              {errors.startDate && <div className="invalid-feedback">{errors.startDate}</div>}
            </div>
            <div className="col-md-6">
              <label htmlFor="project-deadline" className="form-label">
                Deadline <span className="text-danger">*</span>
              </label>
              <input
                id="project-deadline"
                name="deadline"
                type="date"
                min={values.startDate || undefined}
                className={`form-control ${errors.deadline ? 'is-invalid' : ''}`}
                value={values.deadline}
                onChange={handleChange}
              />
              {errors.deadline && <div className="invalid-feedback">{errors.deadline}</div>}
            </div>
          </div>

          <div className="row g-3 mb-3">
            <div className="col-md-4">
              <label htmlFor="project-status" className="form-label">
                Status
              </label>
              <select id="project-status" name="status" className="form-select" value={values.status} onChange={handleChange}>
                {PROJECT_STATUSES.map((item) => (
                  <option key={item.value} value={item.value}>
                    {item.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="col-md-4">
              <label htmlFor="project-priority" className="form-label">
                Priority
              </label>
              <select
                id="project-priority"
                name="priority"
                className="form-select"
                value={values.priority}
                onChange={handleChange}
              >
                {PROJECT_PRIORITIES.map((item) => (
                  <option key={item.value} value={item.value}>
                    {item.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="col-md-4">
              <label htmlFor="project-progress" className="form-label d-flex justify-content-between">
                <span>Progress</span>
                <span className="text-primary fw-medium">{values.progressPercent}%</span>
              </label>
              <input
                id="project-progress"
                name="progressPercent"
                type="range"
                min="0"
                max="100"
                step="5"
                className={`form-range ${errors.progressPercent ? 'is-invalid' : ''}`}
                value={values.progressPercent}
                onChange={handleChange}
                disabled={values.status === 'Completed'}
              />
              {errors.progressPercent && <div className="invalid-feedback d-block">{errors.progressPercent}</div>}
            </div>
          </div>

          <div>
            <label className="form-label">Assign employees</label>
            {employeesError && <p className="text-danger small" role="alert">{employeesError}</p>}
            <EmployeePicker
              employees={employees}
              selectedIds={values.employeeIds}
              loading={employeesLoading}
              onChange={(ids) => updateValue('employeeIds', ids)}
            />
            {errors.employeeIds && <div className="invalid-feedback d-block">{errors.employeeIds}</div>}
          </div>
      </form>
      <div className="modal-footer">
          <button type="button" className="btn btn-light" onClick={handleClose} disabled={saving}>
            Cancel
          </button>
          <button type="submit" form="project-form" className="btn btn-primary"
            disabled={saving || employeesLoading || Boolean(employeesError)}>
            {saving && <span className="spinner-border spinner-border-sm me-2" aria-hidden="true"></span>}
            {isEdit ? 'Save changes' : 'Save project'}
          </button>
      </div>
    </Modal>
  );
}

export default ProjectFormModal;

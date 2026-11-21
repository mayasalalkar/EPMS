import { useState } from 'react';
import projectApi from '../../api/projectApi';
import useToast from '../../hooks/useToast';
import { getErrorMessage } from '../../utils/errors';
import { PROJECT_STATUSES } from '../../utils/constants';

function ProgressForm({ project, onSaved, onCancel }) {
  const toast = useToast();
  const [status, setStatus] = useState(project.status);
  const [progress, setProgress] = useState(project.progressPercent);
  const [saving, setSaving] = useState(false);

  const hasChanges = status !== project.status || progress !== project.progressPercent;
  const validProgress = Number.isInteger(progress) && progress >= 0 && progress <= 100;
  const isCompleted = status === 'Completed';

  const handleStatusChange = (event) => {
    const nextStatus = event.target.value;
    setStatus(nextStatus);
    if (nextStatus === 'Completed') {
      setProgress(100);
    }
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (!validProgress) return;
    setSaving(true);
    try {
      const updated = await projectApi.updateProgress(project.id, { status, progressPercent: progress });
      toast.success(`Progress updated for "${updated.title}".`);
      setStatus(updated.status);
      setProgress(updated.progressPercent);
      onSaved(updated);
    } catch (error) {
      toast.error(getErrorMessage(error));
    } finally {
      setSaving(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <div className="row g-3 align-items-end">
        <div className="col-sm-5">
          <label htmlFor={`status-${project.id}`} className="form-label small fw-medium mb-1">
            Status
          </label>
          <select
            id={`status-${project.id}`}
            className="form-select form-select-sm"
            value={status}
            onChange={handleStatusChange}
            disabled={saving}
          >
            {PROJECT_STATUSES.map((item) => (
              <option key={item.value} value={item.value}>
                {item.label}
              </option>
            ))}
          </select>
        </div>
        <div className="col-sm-7">
          <label htmlFor={`progress-${project.id}`} className="form-label small fw-medium mb-1 d-flex justify-content-between">
            <span>Progress</span>
            <span className="text-primary">{progress}%</span>
          </label>
          <input
            id={`progress-${project.id}`}
            type="range"
            className="form-range"
            min="0"
            max="100"
            step="1"
            value={progress}
            aria-valuetext={`${progress}%`}
            onChange={(event) => setProgress(event.target.valueAsNumber)}
            disabled={saving || isCompleted}
          />
        </div>
      </div>
      <div className="d-flex justify-content-end gap-2 mt-3">
        {onCancel && (
          <button type="button" className="btn btn-light btn-sm" onClick={onCancel} disabled={saving}>
            Cancel
          </button>
        )}
        <button type="submit" className="btn btn-primary btn-sm" disabled={saving || !hasChanges || !validProgress}>
          {saving ? (
            <span className="spinner-border spinner-border-sm me-1" aria-hidden="true"></span>
          ) : (
            <i className="bi bi-check2 me-1"></i>
          )}
          Save progress
        </button>
      </div>
    </form>
  );
}

export default ProgressForm;

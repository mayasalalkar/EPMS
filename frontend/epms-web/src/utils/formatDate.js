const DAY_IN_MS = 24 * 60 * 60 * 1000;

export function parseDate(value) {
  if (!value) {
    return null;
  }

  const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(value);
  if (match) {
    return new Date(Number(match[1]), Number(match[2]) - 1, Number(match[3]));
  }

  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? null : date;
}

export function formatDate(value) {
  const date = parseDate(value);
  if (!date) {
    return '-';
  }

  return date.toLocaleDateString('en-IN', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  });
}

export function toInputDate(value) {
  return value ? value.slice(0, 10) : '';
}

export function daysUntil(value) {
  const target = parseDate(value);
  if (!target) {
    return null;
  }

  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return Math.round((target.getTime() - today.getTime()) / DAY_IN_MS);
}

export function isOverdue(project) {
  if (!project || project.status === 'Completed') {
    return false;
  }

  const days = daysUntil(project.deadline);
  return days !== null && days < 0;
}

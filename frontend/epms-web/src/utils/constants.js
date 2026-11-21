export const PROJECT_STATUSES = [
  { value: 'Pending', label: 'Pending' },
  { value: 'InProgress', label: 'In Progress' },
  { value: 'Completed', label: 'Completed' },
];

export const PROJECT_PRIORITIES = [
  { value: 'Low', label: 'Low' },
  { value: 'Medium', label: 'Medium' },
  { value: 'High', label: 'High' },
];

export const PAGE_SIZE_OPTIONS = [5, 10, 20, 50];

export const ROLES = {
  admin: 'Admin',
  employee: 'Employee',
};

export function getStatusLabel(status) {
  return PROJECT_STATUSES.find((item) => item.value === status)?.label ?? status;
}

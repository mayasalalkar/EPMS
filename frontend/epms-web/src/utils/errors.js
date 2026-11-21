const DEFAULT_MESSAGE = 'Something went wrong. Please try again.';

export function getErrorMessage(error, fallback = DEFAULT_MESSAGE) {
  if (!error?.response) {
    return 'Unable to reach the server. Please check your connection.';
  }

  const data = error.response.data;

  if (data?.detail || data?.message) {
    return data.detail || data.message;
  }

  if (data?.errors) {
    const firstError = Object.values(data.errors).flat()[0];
    if (firstError) {
      return firstError;
    }
  }

  if (data?.title) {
    return data.title;
  }

  return fallback;
}

export function getFieldErrors(error) {
  const errors = error?.response?.data?.errors;
  if (!errors) {
    return {};
  }

  return Object.entries(errors).reduce((result, [key, messages]) => {
    const fieldName = key.replace(/^\$\./, '').split('.').pop();
    if (!fieldName) {
      return result;
    }
    const normalized = fieldName.charAt(0).toLowerCase() + fieldName.slice(1);
    result[normalized] = Array.isArray(messages) ? messages[0] : messages;
    return result;
  }, {});
}

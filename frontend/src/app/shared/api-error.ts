import { HttpErrorResponse } from '@angular/common/http';

export function readApiError(err: unknown): string {
  if (!(err instanceof HttpErrorResponse)) {
    return 'Something went wrong.';
  }

  const body = err.error;

  // Our service errors: { errors: string[] }
  if (Array.isArray(body?.errors) && body.errors.length) {
    return body.errors.join(' ');
  }

  // DataAnnotations / ModelState: { errors: { Field: string[] } }
  if (body?.errors && typeof body.errors === 'object') {
    const messages = Object.values(body.errors as Record<string, string[]>)
      .flat()
      .filter((m) => typeof m === 'string' && m.length > 0);
    if (messages.length) {
      return messages.join(' ');
    }
  }

  if (typeof body?.detail === 'string' && body.detail) {
    return body.detail;
  }
  if (err.status === 0) {
    return 'Cannot reach the API. Is the backend running on port 5080?';
  }
  return err.message || 'Request failed.';
}

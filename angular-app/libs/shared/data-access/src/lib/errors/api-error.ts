import { HttpErrorResponse } from '@angular/common/http';

export type ApiErrorKind = 'network' | 'client' | 'server';

const DEFAULT_MESSAGES: Record<ApiErrorKind, string> = {
  network: 'Unable to reach the server. Check your connection and try again.',
  client: 'The request could not be completed.',
  server: 'The server encountered an error. Please try again later.',
};

/**
 * Normalized API failure. Controllers return a JSON string or string array; framework-generated
 * 4xx bodies (ProblemDetails objects) are not parsed and fall back to the default message.
 */
export class ApiError extends Error {
  readonly status: number;
  readonly messages: readonly string[];
  readonly kind: ApiErrorKind;

  constructor(status: number, messages: readonly string[], kind: ApiErrorKind) {
    super(messages.join(' '));
    this.name = 'ApiError';
    this.status = status;
    this.messages = messages;
    this.kind = kind;
  }

  /**
   * Normalizes any thrown value. Non-HTTP values carry no status, so they are treated as
   * status 0 (network) with the default message.
   */
  static from(error: unknown): ApiError {
    if (error instanceof ApiError) {
      return error;
    }
    const status = error instanceof HttpErrorResponse ? error.status : 0;
    const kind = kindForStatus(status);
    const body = error instanceof HttpErrorResponse ? error.error : null;
    const messages = status === 0 ? [] : extractMessages(body);
    return new ApiError(
      status,
      messages.length > 0 ? messages : [DEFAULT_MESSAGES[kind]],
      kind,
    );
  }
}

function kindForStatus(status: number): ApiErrorKind {
  if (status === 0) return 'network';
  return status >= 500 ? 'server' : 'client';
}

function extractMessages(body: unknown): string[] {
  if (typeof body === 'string') {
    return parseTextBody(body);
  }
  if (Array.isArray(body)) {
    return body.filter(isNonEmptyString);
  }
  return [];
}

/** A text-typed response body may itself be JSON (`"msg"` or `["a","b"]`) or plain text. */
function parseTextBody(text: string): string[] {
  const trimmed = text.trim();
  if (trimmed === '') return [];
  try {
    const parsed: unknown = JSON.parse(trimmed);
    if (typeof parsed === 'string' || Array.isArray(parsed)) {
      return extractMessages(parsed);
    }
  } catch {
    // Not JSON: fall through and treat as plain text.
  }
  return [trimmed];
}

function isNonEmptyString(value: unknown): value is string {
  return typeof value === 'string' && value.trim() !== '';
}

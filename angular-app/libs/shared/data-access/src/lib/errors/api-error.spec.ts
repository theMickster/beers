import { HttpErrorResponse } from '@angular/common/http';
import { ApiError } from './api-error';

const http = (status: number, error: unknown) =>
  new HttpErrorResponse({ status, error });

describe('ApiError.from', () => {
  it('uses a default message for an empty body', () => {
    for (const body of [null, '', '   ']) {
      const error = ApiError.from(http(400, body));
      expect(error.status).toBe(400);
      expect(error.messages).toHaveLength(1);
      expect(error.messages[0]).not.toBe('');
    }
  });

  it('reads a string body', () => {
    expect(ApiError.from(http(400, 'Name is required')).messages).toEqual([
      'Name is required',
    ]);
  });

  it('reads a JSON-encoded string body', () => {
    expect(ApiError.from(http(400, '"Name is required"')).messages).toEqual([
      'Name is required',
    ]);
  });

  it('reads a string array body', () => {
    expect(ApiError.from(http(400, ['a', 'b'])).messages).toEqual(['a', 'b']);
  });

  it('reads a JSON-encoded string array body', () => {
    expect(ApiError.from(http(422, '["a","b"]')).messages).toEqual(['a', 'b']);
  });

  it('drops non-string and blank array entries', () => {
    expect(ApiError.from(http(400, ['a', 1, '', ' ', 'b'])).messages).toEqual([
      'a',
      'b',
    ]);
  });

  it('falls back to the default message for a ProblemDetails object body', () => {
    const error = ApiError.from(http(404, { title: 'Not Found', status: 404 }));
    expect(error.messages).toEqual(['The request could not be completed.']);
  });

  it('falls back to plain text when a body is not valid JSON', () => {
    expect(ApiError.from(http(400, '{oops')).messages).toEqual(['{oops']);
  });

  it('maps status 0 to network', () => {
    const error = ApiError.from(http(0, new ProgressEvent('error')));
    expect(error.kind).toBe('network');
    expect(error.status).toBe(0);
    expect(error.messages).toEqual([
      'Unable to reach the server. Check your connection and try again.',
    ]);
  });

  it('maps 4xx to client', () => {
    expect(ApiError.from(http(404, 'nope')).kind).toBe('client');
  });

  it('maps 5xx to server', () => {
    const error = ApiError.from(http(503, null));
    expect(error.kind).toBe('server');
    expect(error.status).toBe(503);
    expect(error.messages).toEqual([
      'The server encountered an error. Please try again later.',
    ]);
  });

  it('returns an existing ApiError unchanged', () => {
    const original = new ApiError(400, ['x'], 'client');
    expect(ApiError.from(original)).toBe(original);
  });

  it('treats a non-HTTP value as a network-kind error with a default message', () => {
    const error = ApiError.from(new TypeError('boom'));
    expect(error.kind).toBe('network');
    expect(error.messages).toHaveLength(1);
  });
});

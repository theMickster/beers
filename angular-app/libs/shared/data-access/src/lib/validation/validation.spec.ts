import { firstValueFrom, of } from 'rxjs';
import { ApiError } from '../errors/api-error';
import {
  isBlank,
  isEmptyGuid,
  isGuid,
  isObject,
  MAX_PAGE_SIZE,
  normalizeId,
  REQUEST_BODY_REQUIRED,
  whenValid,
} from './validation';

const GUID = '3f2504e0-4f89-11d3-9a0c-0305e82c3301';

describe('isGuid', () => {
  it('accepts a GUID in any letter case', () => {
    expect(isGuid(GUID)).toBe(true);
    expect(isGuid(GUID.toUpperCase())).toBe(true);
  });

  it.each(['', ' ', 'brewer-1', 'a/b c', `${GUID}0`, ` ${GUID}`])(
    'rejects the string %j',
    (value) => {
      expect(isGuid(value)).toBe(false);
    },
  );

  it.each([null, undefined, 42, {}, []])(
    'rejects the non-string %j',
    (value) => {
      expect(isGuid(value)).toBe(false);
    },
  );
});

describe('isEmptyGuid', () => {
  it('is true only for the empty GUID', () => {
    expect(isEmptyGuid('00000000-0000-0000-0000-000000000000')).toBe(true);
    expect(isEmptyGuid(GUID)).toBe(false);
    expect(isEmptyGuid('')).toBe(false);
  });
});

describe('isObject', () => {
  it.each([{}, [], new Date()])('is true for %j', (value) => {
    expect(isObject(value)).toBe(true);
  });

  it.each([null, undefined, 'text', 42, true])('is false for %j', (value) => {
    expect(isObject(value)).toBe(false);
  });
});

describe('normalizeId', () => {
  it('lowercases a string id', () => {
    expect(normalizeId(GUID.toUpperCase())).toBe(GUID);
  });

  it('stringifies a non-string so it never matches a real id', () => {
    expect(normalizeId(42 as unknown as string)).toBe('42');
    expect(normalizeId(null as unknown as string)).toBe('null');
    expect(normalizeId(undefined as unknown as string)).toBe('undefined');
  });
});

describe('isBlank', () => {
  it.each(['', '   ', '\t\n'])('is true for the string %j', (value) => {
    expect(isBlank(value)).toBe(true);
  });

  it.each([null, undefined, 42, {}])(
    'is true for the non-string %j',
    (value) => {
      expect(isBlank(value)).toBe(true);
    },
  );

  it('is false for a string with visible characters', () => {
    expect(isBlank('a')).toBe(false);
    expect(isBlank('  a  ')).toBe(false);
  });
});

describe('constants', () => {
  it('caps the page size at the server limit', () => {
    expect(MAX_PAGE_SIZE).toBe(50);
  });

  it('words the missing body message', () => {
    expect(REQUEST_BODY_REQUIRED).toBe('Request body is required.');
  });
});

describe('whenValid', () => {
  it('runs the request when there are no violations', async () => {
    const request = vi.fn(() => of('ok'));

    expect(await firstValueFrom(whenValid([], request))).toBe('ok');
    expect(request).toHaveBeenCalledTimes(1);
  });

  it('fails with a client ApiError and does not run the request on a violation', async () => {
    const request = vi.fn(() => of('ok'));

    const error: unknown = await firstValueFrom(
      whenValid(['Name is required.', 'Title is required.'], request),
    ).catch((caught: unknown) => caught);

    expect(error).toBeInstanceOf(ApiError);
    expect(error).toMatchObject({
      status: 400,
      kind: 'client',
      messages: ['Name is required.', 'Title is required.'],
    });
    expect(request).not.toHaveBeenCalled();
  });
});

import { TestBed } from '@angular/core/testing';
import { API_BASE_URL } from './api-base-url';

describe('API_BASE_URL', () => {
  it('resolves the provided value', () => {
    TestBed.configureTestingModule({
      providers: [
        { provide: API_BASE_URL, useValue: 'https://example.test/api/v1' },
      ],
    });
    expect(TestBed.inject(API_BASE_URL)).toBe('https://example.test/api/v1');
  });

  it('throws when no provider is registered', () => {
    expect(() => TestBed.inject(API_BASE_URL)).toThrow();
  });
});

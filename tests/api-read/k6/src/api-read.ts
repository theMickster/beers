import crypto from 'k6/crypto';
import http, { RefinedResponse } from 'k6/http';
import { check, sleep } from 'k6';
import type { Options } from 'k6/options';

type TestMode = 'smoke' | 'load' | 'stress';

type ModeConfig = {
  vus: number;
  duration: string;
};

const mode = ((__ENV.TEST_MODE ?? 'smoke').toLowerCase() as TestMode);
const baseUrl = __ENV.BASE_URL ?? 'http://host.docker.internal:8080';

const scenariosByMode: Record<TestMode, ModeConfig> = {
  smoke: { vus: Number(__ENV.VUS ?? 1), duration: __ENV.DURATION ?? '30s' },
  load: { vus: Number(__ENV.VUS ?? 25), duration: __ENV.DURATION ?? '5m' },
  stress: { vus: Number(__ENV.VUS ?? 75), duration: __ENV.DURATION ?? '3m' }
};

const selected = scenariosByMode[mode] ?? scenariosByMode.smoke;

export const options: Options = {
  vus: selected.vus,
  duration: selected.duration,
  thresholds: {
    http_req_failed: [__ENV.THRESHOLD_ERRORS ?? 'rate<0.05'],
    http_req_duration: [__ENV.THRESHOLD_P95 ?? 'p(95)<1500']
  }
};

function get(path: string): RefinedResponse<'text'> {
  return http.get(`${baseUrl}${path}`, {
    headers: {
      Accept: 'application/json'
    },
    tags: { endpoint: path }
  });
}

function post(path: string, body: unknown): RefinedResponse<'text'> {
  return http.post(`${baseUrl}${path}`, JSON.stringify(body), {
    headers: {
      'Content-Type': 'application/json',
      Accept: 'application/json'
    },
    tags: { endpoint: path }
  });
}

function randomGuid(): string {
  const hex = crypto.sha256(`${__VU}-${__ITER}-${Math.random()}`, 'hex').slice(0, 32);
  return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20, 32)}`;
}

export default function runApiReadScenario(): void {
  const brewerId = randomGuid();
  const reviewId = randomGuid();
  const beerId = randomGuid();

  const responses = [
    get('/swagger/v1/swagger.json'),
    get('/api/v1/beerTypes'),
    get('/api/v1/beerStyles'),
    get('/api/v1/beerCategories'),
    get('/api/v1/breweryTypes'),
    get('/api/v1/beers'),
    get(`/api/v1/beers/${beerId}`),
    get('/api/v1/brewers'),
    get(`/api/v1/brewers/${brewerId}`),
    get(`/api/v1/brewers/${brewerId}/reviews`),
    get(`/api/v1/brewers/${brewerId}/reviews/${reviewId}`),
    post('/api/v1/beers/search?PageNumber=1&PageSize=10&SortOrder=Ascending&OrderBy=beerId', {
      id: null,
      name: '',
      brewerId: null,
      brewerName: ''
    }),
    post(`/api/v1/brewers/${brewerId}/reviews/search?PageNumber=1&PageSize=10&SortOrder=Ascending&OrderBy=reviewId`, {
      id: null,
      name: '',
      reviewerName: '',
      minimumRating: 1,
      maximumRating: 5
    })
  ];

  for (const response of responses) {
    check(response, {
      'status is read-safe': (result) => [200, 400, 404].includes(result.status)
    });
  }

  sleep(Number(__ENV.SLEEP_SECONDS ?? 1));
}

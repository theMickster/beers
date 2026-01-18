import type { BreweryType } from '@beers/metadata/data-access';
import type { BrewerPatch, CreateBrewer, UpdateBrewer } from './brewer.models';
import {
  validateBrewerId,
  validateBrewerPatch,
  validateCreateBrewer,
  validateUpdateBrewer,
} from './brewer.validators';

const GUID = '3f2504e0-4f89-11d3-9a0c-0305e82c3301';
const BREWERY_TYPE_ID = 'b7e0a3c1-5d2f-4a8b-9c6e-1f3d5a7b9c0d';
const CURRENT_YEAR = 2026;
const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';
const BODY_REQUIRED = ['Request body is required.'];

// Pin the clock so the year boundary cannot move under the tests at New Year.
beforeEach(() => {
  vi.useFakeTimers();
  vi.setSystemTime(new Date(`${CURRENT_YEAR}-06-15T12:00:00Z`));
});

afterEach(() => {
  vi.useRealTimers();
});

function makeCreateBrewer(overrides: Partial<CreateBrewer> = {}): CreateBrewer {
  return {
    name: 'Acme Brewing',
    foundedIn: 1990,
    headquarters: 'Portland, OR',
    website: 'https://acme.test',
    breweryType: { id: BREWERY_TYPE_ID, name: 'Micro' },
    ...overrides,
  };
}

describe('validateBrewerId', () => {
  it('accepts a GUID in any letter case', () => {
    expect(validateBrewerId(GUID)).toEqual([]);
    expect(validateBrewerId(GUID.toUpperCase())).toEqual([]);
  });

  it.each(['', '   ', 'brewer-1', 'a/b c', `${GUID}x`, GUID.replace(/-/g, '')])(
    'rejects the non-GUID %j',
    (id) => {
      expect(validateBrewerId(id)).toHaveLength(1);
    },
  );
});

describe('validateCreateBrewer', () => {
  it('accepts a valid brewer', () => {
    expect(validateCreateBrewer(makeCreateBrewer())).toEqual([]);
  });

  describe('name', () => {
    it.each(['', '   '])('rejects blank %j', (name) => {
      expect(validateCreateBrewer(makeCreateBrewer({ name }))).toEqual([
        'Name is required.',
      ]);
    });

    it('allows 256 characters and rejects 257', () => {
      expect(
        validateCreateBrewer(makeCreateBrewer({ name: 'a'.repeat(256) })),
      ).toEqual([]);
      expect(
        validateCreateBrewer(makeCreateBrewer({ name: 'a'.repeat(257) })),
      ).toEqual(['Name must be 256 characters or fewer.']);
    });
  });

  describe('headquarters', () => {
    it.each(['', '   '])('rejects blank %j', (headquarters) => {
      expect(validateCreateBrewer(makeCreateBrewer({ headquarters }))).toEqual([
        'Headquarters is required.',
      ]);
    });

    it('allows 256 characters and rejects 257', () => {
      expect(
        validateCreateBrewer(
          makeCreateBrewer({ headquarters: 'a'.repeat(256) }),
        ),
      ).toEqual([]);
      expect(
        validateCreateBrewer(
          makeCreateBrewer({ headquarters: 'a'.repeat(257) }),
        ),
      ).toEqual(['Headquarters must be 256 characters or fewer.']);
    });
  });

  describe('website', () => {
    it.each(['', '   '])('rejects blank %j', (website) => {
      expect(validateCreateBrewer(makeCreateBrewer({ website }))).toEqual([
        'Website is required.',
      ]);
    });

    it.each(['https://acme.test', 'HTTPS://ACME.TEST', 'HttPs://acme.test'])(
      'accepts %j in any letter case',
      (website) => {
        expect(validateCreateBrewer(makeCreateBrewer({ website }))).toEqual([]);
      },
    );

    it('rejects http://', () => {
      expect(
        validateCreateBrewer(makeCreateBrewer({ website: 'http://acme.test' })),
      ).toEqual(['Website must start with https://.']);
    });

    it('rejects a website without a scheme', () => {
      expect(
        validateCreateBrewer(makeCreateBrewer({ website: 'acme.test' })),
      ).toEqual(['Website must start with https://.']);
    });

    it('allows 500 characters and rejects 501', () => {
      const site = (length: number) =>
        `https://${'a'.repeat(length - 'https://'.length)}`;
      expect(
        validateCreateBrewer(makeCreateBrewer({ website: site(500) })),
      ).toEqual([]);
      expect(
        validateCreateBrewer(makeCreateBrewer({ website: site(501) })),
      ).toEqual(['Website must be 500 characters or fewer.']);
    });

    it('reports both length and scheme violations', () => {
      const errors = validateCreateBrewer(
        makeCreateBrewer({ website: `http://${'a'.repeat(500)}` }),
      );
      expect(errors).toHaveLength(2);
    });
  });

  describe('foundedIn', () => {
    it.each([1500, 1990, CURRENT_YEAR])('accepts %d', (foundedIn) => {
      expect(validateCreateBrewer(makeCreateBrewer({ foundedIn }))).toEqual([]);
    });

    it.each([1499, CURRENT_YEAR + 1, 0, -1990, 1990.5, Number.NaN])(
      'rejects %d',
      (foundedIn) => {
        expect(validateCreateBrewer(makeCreateBrewer({ foundedIn }))).toEqual([
          `Founded in must be a year between 1500 and ${CURRENT_YEAR}.`,
        ]);
      },
    );
  });

  describe('breweryType', () => {
    it('requires id and name separately', () => {
      expect(
        validateCreateBrewer(
          makeCreateBrewer({ breweryType: { id: '', name: 'Micro' } }),
        ),
      ).toEqual(['Brewery type id is required.']);
      expect(
        validateCreateBrewer(
          makeCreateBrewer({ breweryType: { id: BREWERY_TYPE_ID, name: ' ' } }),
        ),
      ).toEqual(['Brewery type name is required.']);
    });

    it.each(['bt-1', 'not a guid', `${BREWERY_TYPE_ID}0`])(
      'rejects the non-GUID id %j',
      (id) => {
        const breweryType = { id, name: 'Micro' } as unknown as BreweryType;
        expect(validateCreateBrewer(makeCreateBrewer({ breweryType }))).toEqual(
          ['Brewery type id must be a valid GUID.'],
        );
      },
    );

    it('rejects the all-zero GUID, which the server treats as empty', () => {
      expect(
        validateCreateBrewer(
          makeCreateBrewer({ breweryType: { id: EMPTY_GUID, name: 'Micro' } }),
        ),
      ).toEqual(['Brewery type id must not be the empty GUID.']);
    });

    it('accepts a valid GUID in any letter case', () => {
      for (const id of [BREWERY_TYPE_ID, BREWERY_TYPE_ID.toUpperCase()]) {
        expect(
          validateCreateBrewer(
            makeCreateBrewer({ breweryType: { id, name: 'Micro' } }),
          ),
        ).toEqual([]);
      }
    });

    it('rejects a brewery type that is not an object', () => {
      const text = 'Micro' as unknown as BreweryType;
      expect(
        validateCreateBrewer(makeCreateBrewer({ breweryType: text })),
      ).toEqual(['Brewery type is required.']);
    });

    it('rejects a missing brewery type', () => {
      const missing = null as unknown as BreweryType;
      expect(
        validateCreateBrewer(makeCreateBrewer({ breweryType: missing })),
      ).toEqual(['Brewery type is required.']);
    });
  });

  it.each([null, undefined, 'brewer', 42])(
    'rejects the non-object body %j without throwing',
    (body) => {
      expect(validateCreateBrewer(body as unknown as CreateBrewer)).toEqual(
        BODY_REQUIRED,
      );
    },
  );

  it('rejects non-string and non-numeric field values without throwing', () => {
    const errors = validateCreateBrewer({
      name: 1,
      foundedIn: '1990',
      headquarters: null,
      website: {},
    } as unknown as CreateBrewer);
    expect(errors).toHaveLength(5);
  });

  it('reports every violated rule with a distinct message', () => {
    const errors = validateCreateBrewer(
      makeCreateBrewer({
        name: '',
        foundedIn: 1000,
        headquarters: '',
        website: '',
        breweryType: { id: '', name: '' },
      }),
    );
    expect(errors).toHaveLength(6);
    expect(new Set(errors).size).toBe(errors.length);
    expect(errors.every((message) => message.length > 0)).toBe(true);
  });
});

describe('validateUpdateBrewer', () => {
  const update: UpdateBrewer = { ...makeCreateBrewer(), brewerId: GUID };

  it('accepts matching ids and valid fields', () => {
    expect(validateUpdateBrewer(GUID, update)).toEqual([]);
  });

  it('compares the ids case-insensitively', () => {
    expect(validateUpdateBrewer(GUID.toUpperCase(), update)).toEqual([]);
  });

  it('rejects a body id that differs from the route id', () => {
    expect(
      validateUpdateBrewer(GUID, {
        ...update,
        brewerId: '00000000-0000-0000-0000-000000000001',
      }),
    ).toEqual(['Brewer id in the body must match the route id.']);
  });

  it('rejects a route id that is not a GUID', () => {
    expect(
      validateUpdateBrewer('brewer-1', { ...update, brewerId: 'brewer-1' }),
    ).toEqual(['Brewer id must be a valid GUID.']);
  });

  it('includes field violations', () => {
    expect(validateUpdateBrewer(GUID, { ...update, name: '' })).toEqual([
      'Name is required.',
    ]);
  });

  it.each([null, undefined, 'brewer'])(
    'rejects the non-object body %j without throwing',
    (body) => {
      expect(
        validateUpdateBrewer(GUID, body as unknown as UpdateBrewer),
      ).toEqual(BODY_REQUIRED);
    },
  );

  it.each([null, undefined, 42, {}])(
    'rejects the non-string route id %j without throwing',
    (routeId) => {
      expect(
        validateUpdateBrewer(routeId as unknown as string, update),
      ).toEqual([
        'Brewer id must be a valid GUID.',
        'Brewer id in the body must match the route id.',
      ]);
    },
  );

  it('rejects a body id that is not a string', () => {
    const body = { ...update, brewerId: 7 } as unknown as UpdateBrewer;
    expect(validateUpdateBrewer(GUID, body)).toEqual([
      'Brewer id in the body must match the route id.',
    ]);
  });
});

describe('validateBrewerPatch', () => {
  it('accepts a single valid field', () => {
    expect(validateBrewerPatch({ name: 'Renamed' })).toEqual([]);
  });

  it('accepts every allow-listed field together', () => {
    const { name, foundedIn, headquarters, website, breweryType } =
      makeCreateBrewer();
    expect(
      validateBrewerPatch({
        name,
        foundedIn,
        headquarters,
        website,
        breweryType,
      }),
    ).toEqual([]);
  });

  it('rejects an empty object', () => {
    expect(validateBrewerPatch({})).toEqual([
      'At least one field to change is required.',
    ]);
  });

  it('rejects an object whose values are all undefined', () => {
    expect(
      validateBrewerPatch({ name: undefined, website: undefined }),
    ).toEqual(['At least one field to change is required.']);
  });

  it.each(['brewerId', 'isDeletable', 'rating', 'unknown'])(
    'rejects the key %s even alongside a valid field',
    (key) => {
      const changes = { name: 'Renamed', [key]: 'x' } as BrewerPatch;
      expect(validateBrewerPatch(changes)).toEqual([
        `Field '${key}' cannot be patched.`,
      ]);
    },
  );

  it.each([
    ['__proto__', '{"__proto__":{"x":1}}'],
    ['constructor', '{"constructor":1}'],
  ])('rejects the own key %s from parsed JSON as unknown', (key, json) => {
    const changes = JSON.parse(json) as BrewerPatch;
    expect(validateBrewerPatch(changes)).toEqual([
      'At least one field to change is required.',
      `Field '${key}' cannot be patched.`,
    ]);
  });

  it.each([null, undefined, 'name', 42])(
    'rejects the non-object %j without throwing',
    (changes) => {
      expect(validateBrewerPatch(changes as unknown as BrewerPatch)).toEqual(
        BODY_REQUIRED,
      );
    },
  );

  it('rejects an unknown key whose value is undefined', () => {
    const changes = { name: 'Renamed', brewerId: undefined } as BrewerPatch;
    expect(validateBrewerPatch(changes)).toHaveLength(1);
  });

  it('applies the create rules to each present value', () => {
    expect(validateBrewerPatch({ name: '' })).toEqual(['Name is required.']);
    expect(validateBrewerPatch({ name: 'a'.repeat(257) })).toHaveLength(1);
    expect(validateBrewerPatch({ headquarters: ' ' })).toEqual([
      'Headquarters is required.',
    ]);
    expect(validateBrewerPatch({ website: 'http://acme.test' })).toEqual([
      'Website must start with https://.',
    ]);
    expect(validateBrewerPatch({ foundedIn: 1499 })).toHaveLength(1);
    expect(validateBrewerPatch({ foundedIn: 1500 })).toEqual([]);
    expect(validateBrewerPatch({ foundedIn: CURRENT_YEAR + 1 })).toHaveLength(
      1,
    );
    expect(
      validateBrewerPatch({ breweryType: { id: '', name: '' } }),
    ).toHaveLength(2);
    expect(
      validateBrewerPatch({ breweryType: { id: 'bt-1', name: 'Micro' } }),
    ).toEqual(['Brewery type id must be a valid GUID.']);
  });
});

import type {
  BrewerReviewSearchFilters,
  CreateBrewerReview,
} from './brewer-review.models';
import {
  validateCreateBrewerReview,
  validateReviewId,
  validateReviewSearch,
} from './brewer-review.validators';

const GUID = '3f2504e0-4f89-11d3-9a0c-0305e82c3301';
const BODY_REQUIRED = ['Request body is required.'];

function makeReview(
  overrides: Partial<CreateBrewerReview> = {},
): CreateBrewerReview {
  return {
    reviewerName: 'Sam',
    title: 'Great',
    comments: 'Would visit again',
    rating: 4,
    ...overrides,
  };
}

describe('validateReviewId', () => {
  it('accepts a GUID in any letter case', () => {
    expect(validateReviewId(GUID)).toEqual([]);
    expect(validateReviewId(GUID.toUpperCase())).toEqual([]);
  });

  it.each(['', ' ', 'review-1', 'a/b c', `${GUID}0`])(
    'rejects the non-GUID %j',
    (id) => {
      expect(validateReviewId(id)).toEqual(['Review id must be a valid GUID.']);
    },
  );
});

describe('validateCreateBrewerReview', () => {
  it('accepts a valid review', () => {
    expect(validateCreateBrewerReview(makeReview())).toEqual([]);
  });

  describe('rating', () => {
    it.each([1, 2, 5])('accepts %d', (rating) => {
      expect(validateCreateBrewerReview(makeReview({ rating }))).toEqual([]);
    });

    it.each([0, 6, 4.5, -1, Number.NaN])('rejects %d', (rating) => {
      expect(validateCreateBrewerReview(makeReview({ rating }))).toEqual([
        'Rating must be a whole number from 1 to 5.',
      ]);
    });
  });

  describe.each([
    ['reviewerName', 'Reviewer name', 128],
    ['title', 'Title', 256],
    ['comments', 'Comments', 4000],
  ] as const)('%s', (field, label, limit) => {
    it.each(['', '   '])('rejects blank %j', (value) => {
      expect(
        validateCreateBrewerReview(makeReview({ [field]: value })),
      ).toEqual([`${label} is required.`]);
    });

    it('allows the limit and rejects one more', () => {
      expect(
        validateCreateBrewerReview(makeReview({ [field]: 'a'.repeat(limit) })),
      ).toEqual([]);
      expect(
        validateCreateBrewerReview(
          makeReview({ [field]: 'a'.repeat(limit + 1) }),
        ),
      ).toEqual([`${label} must be ${limit} characters or fewer.`]);
    });
  });

  it.each([null, undefined, 'review', 42])(
    'rejects the non-object body %j without throwing',
    (body) => {
      expect(
        validateCreateBrewerReview(body as unknown as CreateBrewerReview),
      ).toEqual(BODY_REQUIRED);
    },
  );

  it('rejects non-string text and non-numeric ratings without throwing', () => {
    const errors = validateCreateBrewerReview({
      reviewerName: 1,
      title: null,
      comments: {},
      rating: '4',
    } as unknown as CreateBrewerReview);
    expect(errors).toHaveLength(4);
  });

  it('reports every violated rule with a distinct message', () => {
    const errors = validateCreateBrewerReview(
      makeReview({ reviewerName: '', title: '', comments: '', rating: 0 }),
    );
    expect(errors).toHaveLength(4);
    expect(new Set(errors).size).toBe(4);
  });
});

describe('validateReviewSearch', () => {
  it('accepts no filters', () => {
    expect(validateReviewSearch({})).toEqual([]);
  });

  it('accepts blank text filters and free-text name filters', () => {
    expect(
      validateReviewSearch({ id: '', name: '  ', reviewerName: 'x' }),
    ).toEqual([]);
    expect(validateReviewSearch({ id: '   ' })).toEqual([]);
  });

  it('accepts an id filter that is a GUID in any letter case', () => {
    expect(validateReviewSearch({ id: GUID })).toEqual([]);
    expect(validateReviewSearch({ id: GUID.toUpperCase() })).toEqual([]);
  });

  it.each(['x', 'review-1', `${GUID}0`, ` ${GUID}`])(
    'rejects the non-GUID id filter %j',
    (id) => {
      expect(validateReviewSearch({ id })).toEqual([
        'Id filter must be a valid GUID.',
      ]);
    },
  );

  it.each([null, undefined, 'filters', 42])(
    'rejects the non-object filters %j without throwing',
    (filters) => {
      expect(
        validateReviewSearch(filters as unknown as BrewerReviewSearchFilters),
      ).toEqual(BODY_REQUIRED);
    },
  );

  it('rejects ratings that are not numbers', () => {
    expect(
      validateReviewSearch({
        minimumRating: '3',
        maximumRating: null,
      } as unknown as BrewerReviewSearchFilters),
    ).toHaveLength(2);
  });

  it('accepts ratings on the bounds, including min equal to max', () => {
    expect(
      validateReviewSearch({ minimumRating: 1, maximumRating: 5 }),
    ).toEqual([]);
    expect(
      validateReviewSearch({ minimumRating: 3, maximumRating: 3 }),
    ).toEqual([]);
  });

  it.each([0, 6, 2.5, Number.NaN])(
    'rejects a minimum of %d',
    (minimumRating) => {
      expect(validateReviewSearch({ minimumRating })).toEqual([
        'Minimum rating must be a whole number from 1 to 5.',
      ]);
    },
  );

  it.each([0, 6, 2.5, Number.NaN])(
    'rejects a maximum of %d',
    (maximumRating) => {
      expect(validateReviewSearch({ maximumRating })).toEqual([
        'Maximum rating must be a whole number from 1 to 5.',
      ]);
    },
  );

  it('rejects a minimum greater than the maximum', () => {
    const filters: BrewerReviewSearchFilters = {
      minimumRating: 4,
      maximumRating: 2,
    };
    expect(validateReviewSearch(filters)).toEqual([
      'Minimum rating must not be greater than maximum rating.',
    ]);
  });

  it('does not add a range message when a bound is already invalid', () => {
    expect(
      validateReviewSearch({ minimumRating: 9, maximumRating: 2 }),
    ).toEqual(['Minimum rating must be a whole number from 1 to 5.']);
  });
});

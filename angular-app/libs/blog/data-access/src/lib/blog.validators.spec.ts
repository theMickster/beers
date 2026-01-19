import type { BlogPostSearchFilters, CreateNewsBlogPost } from './blog.models';
import {
  validateCreatePost,
  validatePostId,
  validatePostSearch,
  validateUpdatePost,
} from './blog.validators';

const GUID = '3f2504e0-4f89-11d3-9a0c-0305e82c3301';
const BODY_REQUIRED = ['Request body is required.'];
const POST_TYPE_INVALID =
  'Post type must be a valid value (TextPost, ImageGallery, EventAnnouncement)';

function makePost(
  overrides: Partial<CreateNewsBlogPost> = {},
): CreateNewsBlogPost {
  return {
    title: 'Grand opening',
    body: 'Doors open at noon.',
    postType: 'TextPost',
    tags: [],
    imageUrls: [],
    eventDate: null,
    eventLocation: null,
    publishedDate: null,
    ...overrides,
  };
}

const makeEvent = (overrides: Partial<CreateNewsBlogPost> = {}) =>
  makePost({
    postType: 'EventAnnouncement',
    body: '',
    eventDate: '2026-05-01T18:30:00Z',
    eventLocation: 'Taproom',
    ...overrides,
  });

const makeGallery = (overrides: Partial<CreateNewsBlogPost> = {}) =>
  makePost({
    postType: 'ImageGallery',
    body: '',
    imageUrls: ['https://img.test/1.png'],
    ...overrides,
  });

describe('validatePostId', () => {
  it('accepts a GUID in any letter case', () => {
    expect(validatePostId(GUID)).toEqual([]);
    expect(validatePostId(GUID.toUpperCase())).toEqual([]);
  });

  it.each(['', ' ', 'post-1', 'a/b c', `${GUID}0`])(
    'rejects the non-GUID %j',
    (id) => {
      expect(validatePostId(id)).toEqual(['Post id must be a valid GUID.']);
    },
  );
});

describe('validateCreatePost', () => {
  it('accepts a valid text post', () => {
    expect(validateCreatePost(makePost())).toEqual([]);
  });

  it('accepts a valid event announcement', () => {
    expect(validateCreatePost(makeEvent())).toEqual([]);
  });

  it('accepts a valid image gallery', () => {
    expect(validateCreatePost(makeGallery())).toEqual([]);
  });

  it('accepts a draft and a post type in another letter case', () => {
    expect(validateCreatePost(makePost({ publishedDate: null }))).toEqual([]);
    expect(
      validateCreatePost(
        makePost({ postType: 'textpost' as unknown as 'TextPost' }),
      ),
    ).toEqual([]);
  });

  describe('title', () => {
    it.each(['', '   '])('rejects blank %j', (title) => {
      expect(validateCreatePost(makePost({ title }))).toEqual([
        'Title cannot be null, empty, or whitespace',
      ]);
    });

    it('allows 256 characters and rejects one more', () => {
      expect(validateCreatePost(makePost({ title: 'a'.repeat(256) }))).toEqual(
        [],
      );
      expect(validateCreatePost(makePost({ title: 'a'.repeat(257) }))).toEqual([
        'Title cannot be longer than 256 characters',
      ]);
    });
  });

  describe('post type', () => {
    it.each(['', '   '])('rejects blank %j', (postType) => {
      expect(
        validateCreatePost(
          makePost({ postType: postType as unknown as 'TextPost' }),
        ),
      ).toEqual(['Post type cannot be null, empty, or whitespace']);
    });

    it('rejects an unknown value without applying a conditional rule', () => {
      expect(
        validateCreatePost(
          makePost({ postType: 'Podcast' as unknown as 'TextPost', body: '' }),
        ),
      ).toEqual([POST_TYPE_INVALID]);
    });
  });

  describe('TextPost', () => {
    it.each(['', '   '])('rejects a blank body %j', (body) => {
      expect(validateCreatePost(makePost({ body }))).toEqual([
        'Body cannot be null, empty, or whitespace',
      ]);
    });

    it('allows 4000 characters and rejects one more', () => {
      expect(validateCreatePost(makePost({ body: 'a'.repeat(4000) }))).toEqual(
        [],
      );
      expect(validateCreatePost(makePost({ body: 'a'.repeat(4001) }))).toEqual([
        'Body cannot be longer than 4000 characters',
      ]);
    });
  });

  describe('EventAnnouncement', () => {
    it('does not require a body', () => {
      expect(validateCreatePost(makeEvent({ body: '' }))).toEqual([]);
    });

    it('requires an event date', () => {
      expect(validateCreatePost(makeEvent({ eventDate: null }))).toEqual([
        'Event date is required for EventAnnouncement posts',
      ]);
    });

    it.each([null, '', '   '])('requires a location, not %j', (location) => {
      expect(
        validateCreatePost(makeEvent({ eventLocation: location })),
      ).toEqual(['Event location is required for EventAnnouncement posts']);
    });
  });

  describe('ImageGallery', () => {
    it('does not require a body', () => {
      expect(validateCreatePost(makeGallery({ body: '' }))).toEqual([]);
    });

    it('requires at least one image url', () => {
      expect(validateCreatePost(makeGallery({ imageUrls: [] }))).toEqual([
        'ImageUrls must contain at least one url for ImageGallery posts',
      ]);
    });

    it('rejects image urls that are not a list without throwing', () => {
      expect(
        validateCreatePost(
          makeGallery({ imageUrls: null as unknown as string[] }),
        ),
      ).toEqual([
        'ImageUrls must contain at least one url for ImageGallery posts',
      ]);
    });
  });

  describe('dates', () => {
    it.each([
      '2026-05-01',
      '2026-05-01T18:30',
      '2026-05-01T18:30:00',
      '2026-05-01T18:30:00Z',
      '2026-05-01T18:30:00.1234567Z',
      '2026-05-01T18:30:00-05:00',
      '2026-05-01T18:30+02:00',
      '2026-05-01T18:30:00+14:00',
      '2026-05-01T18:30:00-14:00',
      '2026-05-01T18:30:00+13:59',
      '2024-02-29',
    ])('accepts %s', (date) => {
      expect(
        validateCreatePost(makePost({ eventDate: date, publishedDate: date })),
      ).toEqual([]);
    });

    it.each([
      '',
      'May 1 2026',
      '2026/05/01',
      '2026-05-01 18:30:00',
      '2026-05-01t18:30:00z',
      '2026-05-01T18:30:00.',
      '2026-05-01T18',
      '2026-13-01',
      '2026-02-30',
      '2025-02-29',
      '0000-01-01',
      '2026-05-01T24:00:00',
      '2026-05-01T18:60:00',
      '2026-05-01T18:30:60',
      '2026-05-01T18:30:00+24:00',
      '2026-05-01T18:30:00+14:01',
      '2026-05-01T18:30:00-14:30',
      '2026-05-01T18:30:00+00:60',
      '2026-05-01T18:30:00.12345678901234567',
    ])('rejects %j', (date) => {
      expect(validateCreatePost(makePost({ eventDate: date }))).toEqual([
        'Event date must be an ISO 8601 date, such as 2026-05-01 or 2026-05-01T18:30:00Z.',
      ]);
      expect(validateCreatePost(makePost({ publishedDate: date }))).toEqual([
        'Published date must be an ISO 8601 date, such as 2026-05-01 or 2026-05-01T18:30:00Z.',
      ]);
    });

    it('reports an invalid event date once for an event announcement', () => {
      expect(validateCreatePost(makeEvent({ eventDate: 'tomorrow' }))).toEqual([
        'Event date must be an ISO 8601 date, such as 2026-05-01 or 2026-05-01T18:30:00Z.',
      ]);
    });
  });

  it.each([null, undefined, 'post', 42])(
    'rejects the non-object body %j without throwing',
    (body) => {
      expect(validateCreatePost(body as unknown as CreateNewsBlogPost)).toEqual(
        BODY_REQUIRED,
      );
    },
  );

  it('rejects non-string text without throwing', () => {
    const errors = validateCreatePost({
      title: 1,
      body: null,
      postType: 'TextPost',
      tags: [],
      imageUrls: [],
      eventDate: null,
      eventLocation: null,
      publishedDate: null,
    } as unknown as CreateNewsBlogPost);
    expect(errors).toEqual([
      'Title cannot be null, empty, or whitespace',
      'Body cannot be null, empty, or whitespace',
    ]);
  });

  it('reports every violated rule', () => {
    const errors = validateCreatePost(
      makeEvent({ title: '', eventDate: null, eventLocation: '' }),
    );
    expect(errors).toHaveLength(3);
    expect(new Set(errors).size).toBe(3);
  });
});

describe('validateUpdatePost', () => {
  it('accepts a valid post with a GUID id', () => {
    expect(validateUpdatePost(GUID, makePost())).toEqual([]);
  });

  it('requires the post id to be a GUID', () => {
    expect(validateUpdatePost('post-1', makePost())).toEqual([
      'Post id must be a valid GUID.',
    ]);
  });

  it('applies the create rules to the post', () => {
    expect(validateUpdatePost(GUID, makePost({ title: '' }))).toEqual([
      'Title cannot be null, empty, or whitespace',
    ]);
  });

  it('rejects a non-object body', () => {
    expect(
      validateUpdatePost(GUID, null as unknown as CreateNewsBlogPost),
    ).toEqual(BODY_REQUIRED);
  });
});

describe('validatePostSearch', () => {
  it('accepts no filters', () => {
    expect(validatePostSearch({})).toEqual([]);
  });

  it('accepts every filter set to a valid value', () => {
    expect(
      validatePostSearch({
        brewerId: GUID,
        postType: 'ImageGallery',
        tag: 'ipa',
        dateRangeStart: '2026-01-01',
        dateRangeEnd: '2026-01-31T23:59:59Z',
        includeDrafts: true,
      }),
    ).toEqual([]);
  });

  it('accepts blank text filters', () => {
    expect(
      validatePostSearch({
        brewerId: '',
        postType: '' as unknown as 'TextPost',
        tag: '  ',
        dateRangeStart: '',
        dateRangeEnd: '   ',
      }),
    ).toEqual([]);
  });

  it('accepts a brewer id filter in any letter case', () => {
    expect(validatePostSearch({ brewerId: GUID.toUpperCase() })).toEqual([]);
  });

  it.each(['x', `${GUID}0`, ` ${GUID}`])(
    'rejects the non-GUID brewer id filter %j',
    (brewerId) => {
      expect(validatePostSearch({ brewerId })).toEqual([
        'Brewer id filter must be a valid GUID.',
      ]);
    },
  );

  it.each(['Podcast', 'Text', 'TextPosts'])(
    'rejects the unknown post type %j',
    (postType) => {
      expect(
        validatePostSearch({ postType: postType as unknown as 'TextPost' }),
      ).toEqual([POST_TYPE_INVALID]);
    },
  );

  it.each([
    'TextPost',
    'ImageGallery',
    'EventAnnouncement',
    'eventannouncement',
  ])('accepts the post type %j', (postType) => {
    expect(
      validatePostSearch({ postType: postType as unknown as 'TextPost' }),
    ).toEqual([]);
  });

  it('rejects dates that are not ISO 8601', () => {
    expect(
      validatePostSearch({
        dateRangeStart: '01/02/2026',
        dateRangeEnd: '2026-02-30',
      }),
    ).toEqual([
      'Date range start must be an ISO 8601 date, such as 2026-05-01 or 2026-05-01T18:30:00Z.',
      'Date range end must be an ISO 8601 date, such as 2026-05-01 or 2026-05-01T18:30:00Z.',
    ]);
  });

  it('accepts an end equal to the start', () => {
    expect(
      validatePostSearch({
        dateRangeStart: '2026-01-01',
        dateRangeEnd: '2026-01-01',
      }),
    ).toEqual([]);
  });

  it('rejects an end before the start', () => {
    expect(
      validatePostSearch({
        dateRangeStart: '2026-02-01',
        dateRangeEnd: '2026-01-31T23:59:59Z',
      }),
    ).toEqual([
      'DateRangeEnd must be greater than or equal to DateRangeStart.',
    ]);
  });

  describe('in a UTC+ time zone', () => {
    const originalTimeZone = process.env['TZ'];

    beforeEach(() => {
      process.env['TZ'] = 'Asia/Tokyo';
    });

    afterEach(() => {
      if (originalTimeZone === undefined) {
        delete process.env['TZ'];
      } else {
        process.env['TZ'] = originalTimeZone;
      }
    });

    it('compares a date-time with no offset as UTC, like a date-only value', () => {
      expect(
        validatePostSearch({
          dateRangeStart: '2026-05-01',
          dateRangeEnd: '2026-05-01T01:00',
        }),
      ).toEqual([]);
    });

    it('still rejects a reversed range', () => {
      expect(
        validatePostSearch({
          dateRangeStart: '2026-05-02',
          dateRangeEnd: '2026-05-01T23:59',
        }),
      ).toEqual([
        'DateRangeEnd must be greater than or equal to DateRangeStart.',
      ]);
    });
  });

  it('does not add a range message when a date is already invalid', () => {
    expect(
      validatePostSearch({
        dateRangeStart: 'soon',
        dateRangeEnd: '2026-01-01',
      }),
    ).toHaveLength(1);
  });

  it.each([null, undefined, 'filters', 42])(
    'rejects the non-object filters %j without throwing',
    (filters) => {
      expect(
        validatePostSearch(filters as unknown as BlogPostSearchFilters),
      ).toEqual(BODY_REQUIRED);
    },
  );
});

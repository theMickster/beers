import type { BrewerSlim } from '@beers/beer/data-access';

/** The members of the API's `NewsBlogPostType` enum (`NewsBlogPostType.cs`). They travel as strings. */
export const POST_TYPES = [
  'TextPost',
  'ImageGallery',
  'EventAnnouncement',
] as const;

export type PostType = (typeof POST_TYPES)[number];

/** Mirrors the API's blog post response (camelCase JSON; dates are ISO strings). */
export interface NewsBlogPost {
  readonly newsBlogPostId: string;
  readonly brewerId: string;
  readonly title: string;
  readonly body: string;
  readonly postType: PostType;
  readonly tags: readonly string[];
  readonly imageUrls: readonly string[];
  readonly eventDate: string | null;
  readonly eventLocation: string | null;
  /** `null` marks a draft. */
  readonly publishedDate: string | null;
  readonly author: BrewerSlim;
  readonly isDeletable: boolean;
}

/**
 * The brewer comes from the route. `eventDate` and `eventLocation` are required for
 * `EventAnnouncement`, `body` for `TextPost` and at least one `imageUrls` entry for `ImageGallery`.
 */
export interface CreateNewsBlogPost {
  readonly title: string;
  readonly body: string;
  readonly postType: PostType;
  readonly tags: readonly string[];
  readonly imageUrls: readonly string[];
  readonly eventDate: string | null;
  readonly eventLocation: string | null;
  readonly publishedDate: string | null;
}

/** The API replaces the whole post (PUT); it has no PATCH. Both ids come from the route. */
export type UpdateNewsBlogPost = CreateNewsBlogPost;

export type BlogPostOrderBy = 'postId' | 'brewerId' | 'title' | 'publishedDate';

/** The server clamps `pageSize` to 50 (default 10). */
export interface BlogPostPageQuery {
  readonly pageNumber?: number;
  readonly pageSize?: number;
  readonly sortOrder?: 'asc' | 'desc';
  readonly orderBy?: BlogPostOrderBy;
}

/**
 * `tag` is a single value, not a list. Dates bound `publishedDate` inclusively. Drafts are
 * excluded unless `includeDrafts` is true.
 */
export interface BlogPostSearchFilters {
  readonly brewerId?: string;
  readonly postType?: PostType;
  readonly tag?: string;
  readonly dateRangeStart?: string;
  readonly dateRangeEnd?: string;
  readonly includeDrafts?: boolean;
}

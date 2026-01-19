import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { validateBrewerId } from '@beers/brewer/data-access';
import {
  API_BASE_URL,
  toPageParams,
  whenValid,
  type PagedResult,
} from '@beers/shared/data-access';
import type { Observable } from 'rxjs';
import type {
  BlogPostPageQuery,
  BlogPostSearchFilters,
  CreateNewsBlogPost,
  NewsBlogPost,
  UpdateNewsBlogPost,
} from './blog.models';
import {
  validateCreatePost,
  validatePostId,
  validatePostSearch,
  validateUpdatePost,
} from './blog.validators';

const TEXT_FILTER_KEYS = [
  'brewerId',
  'postType',
  'tag',
  'dateRangeStart',
  'dateRangeEnd',
] as const;

/**
 * Thin HTTP wrapper over the blog post endpoints. No state, no error mapping.
 *
 * Every method validates first and fails with a client `ApiError` without sending a request
 * when the input breaks a server rule. The API has no PATCH for posts, so `update` replaces the
 * whole post.
 */
@Injectable({ providedIn: 'root' })
export class BlogApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  /** Unpaged list, drafts included. An unknown brewer answers 200 with `[]`, never 404. */
  listByBrewer(brewerId: string): Observable<readonly NewsBlogPost[]> {
    return whenValid(validateBrewerId(brewerId), () =>
      this.http.get<NewsBlogPost[]>(this.postsUrl(brewerId)),
    );
  }

  getById(brewerId: string, postId: string): Observable<NewsBlogPost> {
    const violations = [
      ...validateBrewerId(brewerId),
      ...validatePostId(postId),
    ];
    return whenValid(violations, () =>
      this.http.get<NewsBlogPost>(this.postUrl(brewerId, postId)),
    );
  }

  /** Searches across all brewers. Paging goes in the query string and filters in the body. */
  search(
    page: BlogPostPageQuery,
    filters: BlogPostSearchFilters,
  ): Observable<PagedResult<NewsBlogPost>> {
    return whenValid(validatePostSearch(filters), () =>
      this.http.post<PagedResult<NewsBlogPost>>(
        `${this.baseUrl}/posts/search`,
        toSearchBody(filters),
        { params: toPageParams(page) },
      ),
    );
  }

  create(brewerId: string, post: CreateNewsBlogPost): Observable<NewsBlogPost> {
    const violations = [
      ...validateBrewerId(brewerId),
      ...validateCreatePost(post),
    ];
    return whenValid(violations, () =>
      this.http.post<NewsBlogPost>(this.postsUrl(brewerId), toWriteBody(post)),
    );
  }

  /** The API requires the body ids to equal the route ids, so both are taken from the arguments. */
  update(
    brewerId: string,
    postId: string,
    post: UpdateNewsBlogPost,
  ): Observable<NewsBlogPost> {
    const violations = [
      ...validateBrewerId(brewerId),
      ...validateUpdatePost(postId, post),
    ];
    return whenValid(violations, () =>
      this.http.put<NewsBlogPost>(this.postUrl(brewerId, postId), {
        brewerId,
        newsBlogPostId: postId,
        ...toWriteBody(post),
      }),
    );
  }

  delete(brewerId: string, postId: string): Observable<void> {
    const violations = [
      ...validateBrewerId(brewerId),
      ...validatePostId(postId),
    ];
    return whenValid(violations, () =>
      this.http.delete<void>(this.postUrl(brewerId, postId)),
    );
  }

  private postsUrl(brewerId: string): string {
    return `${this.baseUrl}/brewers/${encodeURIComponent(brewerId)}/posts`;
  }

  private postUrl(brewerId: string, postId: string): string {
    return `${this.postsUrl(brewerId)}/${encodeURIComponent(postId)}`;
  }
}

/** Field by field, so extra properties on the input never reach the API. */
function toWriteBody(post: CreateNewsBlogPost): CreateNewsBlogPost {
  return {
    title: post.title,
    body: post.body,
    postType: post.postType,
    tags: post.tags,
    imageUrls: post.imageUrls,
    eventDate: post.eventDate,
    eventLocation: post.eventLocation,
    publishedDate: post.publishedDate,
  };
}

/** The API requires a body, so an empty filter set still sends `{}`. */
function toSearchBody(
  filters: BlogPostSearchFilters,
): Record<string, string | boolean> {
  const body: Record<string, string | boolean> = {};
  for (const key of TEXT_FILTER_KEYS) {
    const value = filters[key];
    if (typeof value === 'string' && value.trim() !== '') {
      body[key] = value;
    }
  }
  if (typeof filters.includeDrafts === 'boolean') {
    body['includeDrafts'] = filters.includeDrafts;
  }
  return body;
}

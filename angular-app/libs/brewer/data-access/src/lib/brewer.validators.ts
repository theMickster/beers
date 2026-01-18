import type { BreweryType } from '@beers/metadata/data-access';
import {
  BREWER_PATCH_FIELDS,
  type BrewerPatch,
  type CreateBrewer,
  type UpdateBrewer,
} from './brewer.models';
import {
  isBlank,
  isEmptyGuid,
  isGuid,
  isObject,
  MAX_HEADQUARTERS_LENGTH,
  MAX_NAME_LENGTH,
  MAX_WEBSITE_LENGTH,
  MIN_FOUNDED_IN,
  REQUEST_BODY_REQUIRED,
} from './validation';

/*
 * Client-side mirror of the server's FluentValidation rules. Each function returns the
 * violated rules as messages; an empty array means the input is valid.
 */

export function validateBrewerId(brewerId: string): readonly string[] {
  return isGuid(brewerId) ? [] : ['Brewer id must be a valid GUID.'];
}

export function validateCreateBrewer(brewer: CreateBrewer): readonly string[] {
  if (!isObject(brewer)) return [REQUEST_BODY_REQUIRED];
  return [
    ...validateName(brewer.name),
    ...validateFoundedIn(brewer.foundedIn),
    ...validateHeadquarters(brewer.headquarters),
    ...validateWebsite(brewer.website),
    ...validateBreweryType(brewer.breweryType),
  ];
}

/** The route id and the body id must name the same brewer (GUIDs compare case-insensitively). */
export function validateUpdateBrewer(
  brewerId: string,
  brewer: UpdateBrewer,
): readonly string[] {
  if (!isObject(brewer)) {
    return [...validateBrewerId(brewerId), REQUEST_BODY_REQUIRED];
  }
  const idsMatch =
    typeof brewerId === 'string' &&
    typeof brewer.brewerId === 'string' &&
    brewer.brewerId.toLowerCase() === brewerId.toLowerCase();
  return [
    ...validateBrewerId(brewerId),
    ...(idsMatch ? [] : ['Brewer id in the body must match the route id.']),
    ...validateCreateBrewer(brewer),
  ];
}

/**
 * A patch needs at least one defined field, may only contain allow-listed keys (so `brewerId`,
 * `isDeletable` and `rating` are rejected), and each present value must pass the create rules.
 */
export function validateBrewerPatch(changes: BrewerPatch): readonly string[] {
  if (!isObject(changes)) return [REQUEST_BODY_REQUIRED];
  const keys = Object.keys(changes);
  const unknownKeys = keys.filter(
    (key) => !(BREWER_PATCH_FIELDS as readonly string[]).includes(key),
  );
  const errors = unknownKeys.map((key) => `Field '${key}' cannot be patched.`);

  if (changes.name !== undefined) errors.push(...validateName(changes.name));
  if (changes.foundedIn !== undefined) {
    errors.push(...validateFoundedIn(changes.foundedIn));
  }
  if (changes.headquarters !== undefined) {
    errors.push(...validateHeadquarters(changes.headquarters));
  }
  if (changes.website !== undefined) {
    errors.push(...validateWebsite(changes.website));
  }
  if (changes.breweryType !== undefined) {
    errors.push(...validateBreweryType(changes.breweryType));
  }

  const hasChange = BREWER_PATCH_FIELDS.some(
    (field) => changes[field] !== undefined,
  );
  return hasChange
    ? errors
    : ['At least one field to change is required.', ...errors];
}

function validateName(name: string): readonly string[] {
  if (isBlank(name)) return ['Name is required.'];
  return name.length > MAX_NAME_LENGTH
    ? [`Name must be ${MAX_NAME_LENGTH} characters or fewer.`]
    : [];
}

function validateHeadquarters(headquarters: string): readonly string[] {
  if (isBlank(headquarters)) return ['Headquarters is required.'];
  return headquarters.length > MAX_HEADQUARTERS_LENGTH
    ? [`Headquarters must be ${MAX_HEADQUARTERS_LENGTH} characters or fewer.`]
    : [];
}

function validateWebsite(website: string): readonly string[] {
  if (isBlank(website)) return ['Website is required.'];
  const errors: string[] = [];
  if (website.length > MAX_WEBSITE_LENGTH) {
    errors.push(`Website must be ${MAX_WEBSITE_LENGTH} characters or fewer.`);
  }
  if (!website.toLowerCase().startsWith('https://')) {
    errors.push('Website must start with https://.');
  }
  return errors;
}

function validateFoundedIn(foundedIn: number): readonly string[] {
  const currentYear = new Date().getUTCFullYear();
  const valid =
    Number.isInteger(foundedIn) &&
    foundedIn >= MIN_FOUNDED_IN &&
    foundedIn <= currentYear;
  return valid
    ? []
    : [
        `Founded in must be a year between ${MIN_FOUNDED_IN} and ${currentYear}.`,
      ];
}

function validateBreweryType(
  breweryType: BreweryType | null | undefined,
): readonly string[] {
  if (!isObject(breweryType)) return ['Brewery type is required.'];
  return [
    ...validateBreweryTypeId(breweryType.id),
    // The server does not check the name; the client is intentionally stricter here.
    ...(isBlank(breweryType.name) ? ['Brewery type name is required.'] : []),
  ];
}

/** The server model's id is a non-empty `Guid`. */
function validateBreweryTypeId(id: string): readonly string[] {
  if (isBlank(id)) return ['Brewery type id is required.'];
  if (!isGuid(id)) return ['Brewery type id must be a valid GUID.'];
  return isEmptyGuid(id) ? ['Brewery type id must not be the empty GUID.'] : [];
}

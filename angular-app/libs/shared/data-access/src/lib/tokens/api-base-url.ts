import { InjectionToken } from '@angular/core';

/** Base URL of the Beers API, including the version segment. Must be provided by the app. */
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');

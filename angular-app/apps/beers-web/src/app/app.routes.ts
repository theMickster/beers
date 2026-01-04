import { Route } from '@angular/router';

export const appRoutes: Route[] = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () =>
      import('./landing/landing').then((m) => m.LandingComponent),
  },
  {
    path: 'dev/theme-harness',
    loadComponent: () =>
      import('./theme-harness/theme-harness').then(
        (m) => m.ThemeHarnessComponent,
      ),
  },
];

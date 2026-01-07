import { Route } from '@angular/router';

const samplesRoutes: Route[] =
  typeof ENABLE_SAMPLES !== 'undefined' && ENABLE_SAMPLES
    ? [
        {
          path: 'samples',
          loadComponent: () =>
            import('@beers/shared/ui-daisy-demo').then(
              (m) => m.SamplesComponent,
            ),
        },
      ]
    : [];

const comingSoon = (path: string, title: string): Route => ({
  path,
  title,
  data: { title },
  loadComponent: () =>
    import('./coming-soon/coming-soon').then((m) => m.ComingSoonComponent),
});

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
  comingSoon('explorer', 'Explorer'),
  comingSoon('brewers', 'Brewers'),
  comingSoon('blog', 'Blog'),
  comingSoon('dashboard', 'Dashboard'),
  comingSoon('compare', 'Compare'),
  ...samplesRoutes,
];

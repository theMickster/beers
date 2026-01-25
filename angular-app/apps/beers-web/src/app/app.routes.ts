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
  {
    path: 'explorer',
    title: 'Explorer',
    data: { title: 'Explorer' },
    loadComponent: () =>
      import('@beers/beer/feature-explorer').then((m) => m.ExplorerComponent),
  },
  {
    path: 'beers/:beerId',
    title: 'Beer',
    data: { title: 'Beer' },
    loadComponent: () =>
      import('@beers/beer/feature-detail').then((m) => m.BeerDetailComponent),
  },
  comingSoon('brewers', 'Brewers'),
  comingSoon('blog', 'Blog'),
  comingSoon('dashboard', 'Dashboard'),
  comingSoon('compare', 'Compare'),
  ...samplesRoutes,
];

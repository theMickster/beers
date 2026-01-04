import { ChangeDetectionStrategy, Component } from '@angular/core';

interface HarnessTheme {
  readonly id: string;
  readonly label: string;
  readonly dataTheme: 'fourteener-pale' | 'fourteener-stout';
}

interface HarnessIcon {
  readonly name: string;
  readonly iconClass: string;
}

interface HarnessSwatch {
  readonly name: string;
  readonly swatchClass: string;
}

@Component({
  selector: 'bw-theme-harness',
  templateUrl: './theme-harness.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ThemeHarnessComponent {
  protected readonly themes: readonly HarnessTheme[] = [
    { id: 'pale', label: 'Pale theme', dataTheme: 'fourteener-pale' },
    { id: 'stout', label: 'Stout theme', dataTheme: 'fourteener-stout' },
  ];

  protected readonly icons: readonly HarnessIcon[] = [
    { name: 'Beer', iconClass: 'fa-beer-mug-empty' },
    { name: 'Brewery', iconClass: 'fa-industry' },
    { name: 'Flight', iconClass: 'fa-layer-group' },
    { name: 'Review', iconClass: 'fa-star' },
    { name: 'Search', iconClass: 'fa-magnifying-glass' },
    { name: 'Seasonal', iconClass: 'fa-calendar-days' },
  ];

  protected readonly swatches: readonly HarnessSwatch[] = [
    { name: 'primary', swatchClass: 'bg-primary text-primary-content' },
    { name: 'secondary', swatchClass: 'bg-secondary text-secondary-content' },
    { name: 'accent', swatchClass: 'bg-accent text-accent-content' },
    { name: 'neutral', swatchClass: 'bg-neutral text-neutral-content' },
    { name: 'info', swatchClass: 'bg-info text-info-content' },
    { name: 'success', swatchClass: 'bg-success text-success-content' },
    { name: 'warning', swatchClass: 'bg-warning text-warning-content' },
    { name: 'error', swatchClass: 'bg-error text-error-content' },
  ];
}

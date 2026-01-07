import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
} from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ThemeService } from '@beers/shared/util';

interface NavItem {
  readonly label: string;
  readonly path: string;
}

const NAV_ITEMS: readonly NavItem[] = [
  { label: 'Explorer', path: '/explorer' },
  { label: 'Brewers', path: '/brewers' },
  { label: 'Blog', path: '/blog' },
  { label: 'Dashboard', path: '/dashboard' },
  { label: 'Compare', path: '/compare' },
];

@Component({
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  selector: 'bw-app-layout',
  templateUrl: './app-layout.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '(document:keydown.escape)': 'setMenuOpen(false)' },
})
export class AppLayoutComponent {
  protected readonly theme = inject(ThemeService);
  protected readonly navItems = NAV_ITEMS;
  protected readonly menuOpen = signal(false);

  protected toggleTheme(): void {
    this.theme.toggle();
  }

  protected setMenuOpen(open: boolean): void {
    this.menuOpen.set(open);
  }
}

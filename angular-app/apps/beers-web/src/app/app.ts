import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ThemeService } from '@beers/shared/util';

@Component({
  imports: [],
  selector: 'bw-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  protected readonly theme = inject(ThemeService);

  protected toggleTheme(): void {
    this.theme.toggle();
  }
}

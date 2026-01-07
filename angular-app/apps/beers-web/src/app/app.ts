import { ChangeDetectionStrategy, Component } from '@angular/core';
import { AppLayoutComponent } from '@beers/shared/app-layout';

@Component({
  imports: [AppLayoutComponent],
  selector: 'bw-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {}

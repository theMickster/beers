import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'bw-landing',
  templateUrl: './landing.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'block' },
})
export class LandingComponent {}

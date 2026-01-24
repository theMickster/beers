import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/** A centered "nothing here" message. Project an action (e.g. a button) as content. */
@Component({
  selector: 'bw-empty-state',
  template: `
    <div
      class="flex flex-col items-center gap-3 rounded-box border border-dashed border-base-300 bg-base-100 px-6 py-12 text-center"
    >
      <i
        class="fa-solid text-4xl opacity-50"
        [class]="icon()"
        aria-hidden="true"
      ></i>
      <h2 class="text-xl font-semibold">{{ heading() }}</h2>
      @if (message()) {
        <p class="max-w-md opacity-75">{{ message() }}</p>
      }
      <ng-content />
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmptyStateComponent {
  readonly heading = input.required<string>();
  readonly message = input('');
  /** Font Awesome icon class, e.g. `fa-beer-mug-empty`. */
  readonly icon = input('fa-beer-mug-empty');
}

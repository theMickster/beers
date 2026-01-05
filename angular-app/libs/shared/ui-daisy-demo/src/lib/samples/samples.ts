import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  signal,
  viewChild,
} from '@angular/core';

interface SampleTab {
  readonly id: string;
  readonly label: string;
  readonly body: string;
}

@Component({
  selector: 'bw-samples',
  templateUrl: './samples.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SamplesComponent {
  protected readonly tabs: readonly SampleTab[] = [
    {
      id: 'tasting',
      label: 'Tasting notes',
      body: 'Citrus peel, pine resin, and a dry finish.',
    },
    {
      id: 'brewery',
      label: 'Brewery',
      body: 'Brewed in small batches at 5,280 feet.',
    },
    {
      id: 'reviews',
      label: 'Reviews',
      body: 'Rated 4.5 stars by 128 beer people.',
    },
  ];

  protected readonly rating = signal(3);
  protected readonly selectedTabId = signal(this.tabs[0].id);

  private readonly dialog =
    viewChild.required<ElementRef<HTMLDialogElement>>('sampleDialog');

  protected openModal(): void {
    this.dialog().nativeElement.showModal();
  }

  protected selectTab(id: string): void {
    this.selectedTabId.set(id);
  }

  protected onTabKeydown(event: KeyboardEvent, index: number): void {
    const last = this.tabs.length - 1;
    const targets: Partial<Record<string, number>> = {
      ArrowRight: index === last ? 0 : index + 1,
      ArrowLeft: index === 0 ? last : index - 1,
      Home: 0,
      End: last,
    };
    const target = targets[event.key];
    if (target === undefined) return;
    event.preventDefault();
    this.selectTab(this.tabs[target].id);
    (event.currentTarget as HTMLElement).parentElement
      ?.querySelectorAll<HTMLElement>('[role="tab"]')
      [target]?.focus();
  }
}

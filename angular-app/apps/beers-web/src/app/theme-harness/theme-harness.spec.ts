import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ThemeHarnessComponent } from './theme-harness';

const ICON_CLASSES = [
  'fa-beer-mug-empty',
  'fa-industry',
  'fa-layer-group',
  'fa-star',
  'fa-magnifying-glass',
  'fa-calendar-days',
];

describe('ThemeHarnessComponent', () => {
  let root: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ThemeHarnessComponent],
    }).compileComponents();
    const fixture: ComponentFixture<ThemeHarnessComponent> =
      TestBed.createComponent(ThemeHarnessComponent);
    fixture.detectChanges();
    root = fixture.nativeElement;
  });

  it.each([
    ['Pale theme', 'fourteener-pale'],
    ['Stout theme', 'fourteener-stout'],
  ])('renders the %s scope', (label, dataTheme) => {
    const section = root.querySelector(`section[aria-label="${label}"]`);
    expect(section?.getAttribute('data-theme')).toBe(dataTheme);
  });

  it.each(['Pale theme', 'Stout theme'])(
    'renders all six icons in the %s scope',
    (label) => {
      const section = root.querySelector(
        `section[aria-label="${label}"]`,
      ) as HTMLElement;
      for (const iconClass of ICON_CLASSES) {
        expect(
          section.querySelector(`i.fa-solid.${iconClass}[aria-hidden="true"]`),
        ).toBeTruthy();
      }
    },
  );
});

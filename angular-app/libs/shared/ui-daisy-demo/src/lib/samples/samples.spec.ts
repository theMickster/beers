import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SamplesComponent } from './samples';

const STYLES_PATH = resolve(process.cwd(), 'apps/beers-web/src/styles.css');

function readThemeColor(css: string, theme: string, token: string): string {
  const themeBlock = css.match(
    new RegExp(`name:\\s*"${theme}";([\\s\\S]*?)\\n}`),
  )?.[1];
  const value = themeBlock?.match(
    new RegExp(`--color-${token}:\\s*(#[0-9A-Fa-f]{6})\\s*;`),
  )?.[1];
  if (!value) {
    throw new Error(
      `--color-${token} not found for ${theme} in ${STYLES_PATH}`,
    );
  }
  return value;
}

function relativeLuminance(hex: string): number {
  const [r, g, b] = [1, 3, 5].map((start) => {
    const channel = parseInt(hex.slice(start, start + 2), 16) / 255;
    return channel <= 0.03928
      ? channel / 12.92
      : ((channel + 0.055) / 1.055) ** 2.4;
  });
  return 0.2126 * r + 0.7152 * g + 0.0722 * b;
}

function contrastRatio(foreground: string, background: string): number {
  const [lighter, darker] = [
    relativeLuminance(foreground),
    relativeLuminance(background),
  ].sort((a, b) => b - a);
  return (lighter + 0.05) / (darker + 0.05);
}

describe('SamplesComponent', () => {
  let fixture: ComponentFixture<SamplesComponent>;
  let root: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SamplesComponent],
    }).compileComponents();
    fixture = TestBed.createComponent(SamplesComponent);
    fixture.detectChanges();
    root = fixture.nativeElement;
  });

  it.each([
    ['buttons', 'button.btn.btn-primary'],
    ['cards', 'article.card'],
    ['badges', 'span.badge'],
    ['star rating', '.rating input[type="radio"]'],
    ['modal', 'dialog.modal'],
    ['tabs', '[role="tablist"] [role="tab"]'],
    ['input', 'input.input'],
    ['select', 'select.select'],
    ['textarea', 'textarea.textarea'],
    ['toggle', 'input.toggle'],
    ['alerts', '.alert'],
  ])('renders %s', (_group, selector) => {
    expect(root.querySelector(selector)).toBeTruthy();
  });

  it('gives every form control a visible label', () => {
    for (const control of root.querySelectorAll(
      'input.input, select, textarea, input.toggle',
    )) {
      const labelled = control.id
        ? root.querySelector(`label[for="${control.id}"]`)
        : control.closest('label');
      expect(labelled?.textContent?.trim()).toBeTruthy();
    }
  });

  it('selects a tab on click and shows only its panel', () => {
    const tabs = root.querySelectorAll<HTMLElement>('[role="tab"]');
    expect(tabs[0].getAttribute('aria-selected')).toBe('true');

    tabs[1].click();
    fixture.detectChanges();

    expect(tabs[0].getAttribute('aria-selected')).toBe('false');
    expect(tabs[1].getAttribute('aria-selected')).toBe('true');
    const panels = root.querySelectorAll<HTMLElement>('[role="tabpanel"]');
    expect(panels[0].hidden).toBe(true);
    expect(panels[1].hidden).toBe(false);
  });

  it('moves tab selection with the arrow keys', () => {
    const tabs = root.querySelectorAll<HTMLElement>('[role="tab"]');
    tabs[0].dispatchEvent(
      new KeyboardEvent('keydown', { key: 'ArrowRight', bubbles: true }),
    );
    fixture.detectChanges();
    expect(tabs[1].getAttribute('aria-selected')).toBe('true');
  });

  it('opens the dialog with showModal when the trigger is clicked', () => {
    const dialog = root.querySelector('dialog') as HTMLDialogElement;
    const showModal = vi.fn();
    dialog.showModal = showModal;

    const trigger = [...root.querySelectorAll('button')].find(
      (button) => button.textContent?.trim() === 'Open modal',
    );
    trigger?.click();

    expect(showModal).toHaveBeenCalledOnce();
  });

  it('updates the rating when a star is chosen', () => {
    const stars = root.querySelectorAll<HTMLInputElement>('.rating input');
    stars[3].click();
    fixture.detectChanges();
    expect(root.textContent).toContain('Your rating: 4 of 5');
  });

  describe('info colors', () => {
    // The info/info-content pairing is for UI components (the info alert and
    // info badge) and large elements only. It is NOT used for body copy: in the
    // pale theme it is below 4.5:1 but clears the 3:1 UI-component threshold
    // (docs/spikes/834-color-scheme-app-icons.md).
    const css = readFileSync(STYLES_PATH, 'utf8');

    it('renders the info alert and info badge', () => {
      expect(root.querySelector('.alert.alert-info')).toBeTruthy();
      expect(root.querySelector('.badge.badge-info')).toBeTruthy();
    });

    it.each(['fourteener-pale', 'fourteener-stout'])(
      'meets 3:1 for info/info-content in %s',
      (theme) => {
        const ratio = contrastRatio(
          readThemeColor(css, theme, 'info-content'),
          readThemeColor(css, theme, 'info'),
        );
        expect(ratio).toBeGreaterThanOrEqual(3);
      },
    );
  });
});

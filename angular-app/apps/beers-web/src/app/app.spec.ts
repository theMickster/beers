import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AppComponent } from './app';

describe('App', () => {
  let component: AppComponent;
  let fixture: ComponentFixture<AppComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the app component', () => {
    expect(component).toBeTruthy();
  });

  it('renders the app layout', () => {
    expect(
      (fixture.nativeElement as HTMLElement).querySelector('bw-app-layout'),
    ).toBeTruthy();
  });

  it('should apply change detection strategy OnPush', () => {
    const metadata = (AppComponent as unknown as { ɵcmp: { onPush: boolean } })[
      'ɵcmp'
    ];
    expect(metadata.onPush).toBeTruthy();
  });
});

import { Component, signal, inject, computed } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterOutlet } from '@angular/router';
import { AstronautSearchComponent } from './components/astronaut-search/astronaut-search.component';
import { AstronautDutyListComponent } from './components/astronaut-duty-list/astronaut-duty-list.component';
import { AstronautApiService } from './services/astronaut-api.service';
import { AstronautDutiesResponse, PersonAstronaut, AstronautDuty } from './models/astronaut.model';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AstronautSearchComponent, AstronautDutyListComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly astronautApiService = inject(AstronautApiService);
  private readonly destroyRef = takeUntilDestroyed();
  
  protected readonly title = signal('Stargate - Astronaut Career Tracking System');
  protected readonly searchResults = signal<AstronautDutiesResponse | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  // Computed signals for cleaner template
  protected readonly person = computed<PersonAstronaut | null>(() => 
    this.searchResults()?.person ?? null
  );

  protected readonly duties = computed<AstronautDuty[]>(() => 
    this.searchResults()?.astronautDuties ?? []
  );

  protected handleSearch(name: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.searchResults.set(null);

    this.astronautApiService
      .getAstronautDutiesByName(name)
      .pipe(this.destroyRef)
      .subscribe({
        next: (response) => {
          this.searchResults.set(response);
          this.isLoading.set(false);
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isLoading.set(false);
        }
      });
  }
}

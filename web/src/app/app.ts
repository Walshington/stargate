import { Component, signal, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AstronautSearchComponent } from './components/astronaut-search/astronaut-search.component';
import { AstronautDutyListComponent } from './components/astronaut-duty-list/astronaut-duty-list.component';
import { AstronautApiService } from './services/astronaut-api.service';
import { AstronautDutiesResponse } from './models/astronaut.model';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AstronautSearchComponent, AstronautDutyListComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly astronautApiService = inject(AstronautApiService);
  
  protected readonly title = signal('Stargate - Astronaut Career Tracking System');
  searchResults = signal<AstronautDutiesResponse | null>(null);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  handleSearch(name: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.searchResults.set(null);

    this.astronautApiService.getAstronautDutiesByName(name).subscribe({
      next: (response) => {
        this.searchResults.set(response);
        this.isLoading.set(false);
      },
      error: (error) => {
        this.errorMessage.set(error.message);
        this.isLoading.set(false);
      }
    });
  }
}

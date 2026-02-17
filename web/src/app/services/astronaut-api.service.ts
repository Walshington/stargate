import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AstronautDutiesResponse } from '../models/astronaut.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AstronautApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/AstronautDuty`;

  getAstronautDutiesByName(name: string): Observable<AstronautDutiesResponse> {
    return this.http.get<AstronautDutiesResponse>(`${this.baseUrl}/${encodeURIComponent(name)}`).pipe(
      catchError(this.handleError)
    );
  }

  private handleError = (error: HttpErrorResponse): Observable<never> => {
    let errorMessage = 'An error occurred while fetching astronaut data.';
    
    if (error.status === 404) {
      errorMessage = 'Astronaut not found. Please check the name and try again.';
    } else if (error.status === 0) {
      errorMessage = 'Unable to connect to the server. Please ensure the API is running.';
    } else if (error.error?.message) {
      errorMessage = error.error.message;
    }
    
    return throwError(() => new Error(errorMessage));
  };
}

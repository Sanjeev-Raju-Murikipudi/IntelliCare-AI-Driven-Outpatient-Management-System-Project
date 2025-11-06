import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { CreateDoctorDto } from '../models/create-doctor.dto';
import { Doctor } from '../models/doctor.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class DoctorService {
  private http = inject(HttpClient);

  private apiUrl = 'https://localhost:7215/api';
  private createDoctorEndpoint = `${this.apiUrl}/User/create-doctor`;
  private getAllDoctorsEndpoint = `${this.apiUrl}/Doctor/all-doctors`;

  /**
   * Admin creates a doctor account.
   */
  async createDoctor(dto: CreateDoctorDto): Promise<void> {
    const maxRetries = 3;

    for (let attempt = 0; attempt < maxRetries; attempt++) {
      try {
        await this.http.post<void>(this.createDoctorEndpoint, dto).toPromise();
        return;
      } catch (error) {
        if (
          error instanceof HttpErrorResponse &&
          [400, 401, 403, 500].includes(error.status)
        ) {
          console.error(`Non-transient error (${error.status}) on attempt ${attempt + 1}:`, error);
          throw error;
        }

        if (attempt === maxRetries - 1) {
          console.error(`All ${maxRetries} attempts failed.`, error);
          throw error;
        }

        const delay = Math.pow(2, attempt) * 1000;
        console.warn(`Attempt ${attempt + 1} failed. Retrying in ${delay / 1000}s...`, error);
        await new Promise(resolve => setTimeout(resolve, delay));
      }
    }
  }

  /**
   * Admin fetches all doctors.
   */
  getAllDoctors(): Observable<Doctor[]> {
    return this.http.get<Doctor[]>(this.getAllDoctorsEndpoint);
  }
}







// import { Injectable, inject } from '@angular/core';
// import { HttpClient, HttpErrorResponse } from '@angular/common/http';
// import { CreateDoctorDto } from '../models/create-doctor.dto';

// @Injectable({
//   providedIn: 'root',
// })
// export class DoctorService {
//   private http = inject(HttpClient);
  
//   private apiUrl = 'https://localhost:7215/api'; 
//   private createDoctorEndpoint = `${this.apiUrl}/User/create-doctor`; 

//   /**
//    * Performs the API call to create a doctor with exponential backoff.
//    * Authentication (JWT) is now handled automatically by the AuthInterceptor.
//    */
//   async createDoctor(dto: CreateDoctorDto): Promise<void> {
//     const maxRetries = 3;

//     // NOTE: We rely entirely on the AuthInterceptor to add the Authorization header (JWT).
    
//     // We send the entire DTO, including the DoctorCreationKey, in the body.
//     // The API server should validate the DoctorCreationKey on its end.
//     const requestBody = dto;

//     for (let attempt = 0; attempt < maxRetries; attempt++) {
//       try {
//         // We no longer pass explicit HTTP headers, relying on the interceptor.
//         await this.http.post<void>(this.createDoctorEndpoint, requestBody).toPromise();
//         return; // Success, exit the loop

//       } catch (error) {
//         if (error instanceof HttpErrorResponse && (error.status === 400 || error.status === 401 || error.status === 403 || error.status === 500)) {
//           console.error(`Non-transient error (${error.status}) on attempt ${attempt + 1}:`, error);
//           throw error;
//         }

//         if (attempt === maxRetries - 1) {
//           console.error(`All ${maxRetries} attempts failed.`, error);
//           throw error;
//         }

//         const delay = Math.pow(2, attempt) * 1000;
//         console.warn(`Attempt ${attempt + 1} failed. Retrying in ${delay / 1000}s...`, error);
//         await new Promise(resolve => setTimeout(resolve, delay));
//       }
//     }
//   }
// }

import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';

import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { CreateAdminDto } from '../models/create-admin.dto';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private apiUrl = 'https://localhost:7215/api';
  private createAdminEndpoint = `${this.apiUrl}/User/create-admin`;

  constructor(private http: HttpClient) {}

  createAdmin(dto: CreateAdminDto): Observable<any> {
    return this.http.post(this.createAdminEndpoint, dto).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: HttpErrorResponse) {
    console.error('Admin creation error:', error);
    return throwError(() => error);
  }
}
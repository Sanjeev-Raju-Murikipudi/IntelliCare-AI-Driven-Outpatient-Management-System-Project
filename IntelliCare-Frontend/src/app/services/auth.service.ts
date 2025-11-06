import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

export interface RegisterUserDto {
  username: string;
  password: string;
  mobileNumber: string;
  contactEmail: string;
}

export interface LoginRequestDto {
  username: string;
  password: string;
}

export interface OtpVerifyDto {
  username: string;
  otpCode: string;
}

export interface UserData {
  username: string;
  role: string;
  token: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private baseUrl = 'https://localhost:7215/api/User';

  constructor(private http: HttpClient) {}

  // Register
  registerPatient(dto: RegisterUserDto): Observable<any> {
    return this.http.post(`${this.baseUrl}/register-patient`, dto).pipe(
      catchError(this.handleError)
    );
}

  // Login
  login(dto: LoginRequestDto): Observable<any> {
    return this.http.post(`${this.baseUrl}/login`, dto).pipe(
      catchError(this.handleError)
    );
  }

  // Verify OTP
  verifyOtp(dto: OtpVerifyDto): Observable<any> {
    return this.http.post(`${this.baseUrl}/verify-otp`, dto).pipe(
      catchError(this.handleError)
    );
  }

  // Store token and user
  storeAuthData(token: string, user: any): void {
    localStorage.setItem('token', token);
    localStorage.setItem('user', JSON.stringify(user));
  }

  // Get token
  getToken(): string | null {
    return localStorage.getItem('token');
  }

  // Get full user object
  getUser(): any {
    return JSON.parse(localStorage.getItem('user') || '{}');
  }

  // Get normalized user data
  getUserData(): { token: string; role: string } | null {
    const token = localStorage.getItem('token');
    const user = localStorage.getItem('user');

    if (token && user) {
      const parsed = JSON.parse(user);
      return {
        token,
        role: parsed.roleName // Ensure this matches backend response
      };
    }

    return null;
  }


   // Get user role from token payload
  getUserRole(): string | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.role || null;
    } catch {
      return null;
    }
  }

  // Auth checks
  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  isLoggedIn(): boolean {
    return this.isAuthenticated();
  }

  isDoctorOrAdmin(): boolean {
    const role = this.getRole();
    return role === 'doctor' || role === 'admin';
  }

  isPatient(): boolean {
    return this.getRole() === 'patient';
  }

  isAdmin(): boolean {
    const user = this.getUserData();
    return user?.role?.toLowerCase() === 'admin';
  }
  //  isDoctor(): boolean {
  //   const user = this.getUserData();
  //   return user?.role?.toLowerCase() === 'doctor';
  // }
  //   isPatient(): boolean {
  //   const user = this.getUserData();
  //   return user?.role?.toLowerCase() === 'patient';
  // }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  }

  // Error handler
  private handleError(error: HttpErrorResponse) {
    console.error('AuthService error:', error);
    const status = error.status;
    const message =
      error.error?.error ||
      error.error?.message ||
      error.message ||
      'An unexpected error occurred.';
    return throwError(() => ({ status, message }));
  }
  getRole(): string {
  const token = localStorage.getItem('token');
  if (!token) return '';
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    // return payload.role || '';
     return (payload.role || '').toLowerCase(); // normalize
  } catch {
    return '';
  }
}
}

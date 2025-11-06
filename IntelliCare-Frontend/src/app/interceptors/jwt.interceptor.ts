import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service'; // Ensure you import the fixed AuthService

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    // 1. Get the user data (which includes the token and role)
    const user = this.authService.getUserData();
    const token = user?.token;

    // 2. Check if a token exists and attach it as a Bearer token
    if (token) {
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`, // The crucial step
        },
      });
    }

    // 3. Continue the request
    return next.handle(request);
  }
}
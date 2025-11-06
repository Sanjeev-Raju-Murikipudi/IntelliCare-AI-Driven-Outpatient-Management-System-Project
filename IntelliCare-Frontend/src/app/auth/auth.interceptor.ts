import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // 1. Retrieve user data from local storage
    const userData = localStorage.getItem('user');

    if (userData) {
      try {
        // 2. Parse the data and extract the token
        const user = JSON.parse(userData);
        // CRITICAL ASSUMPTION: The JWT is stored under the property 'token' in the user object.
        const authToken = user.token; 
        
        if (authToken) {
          // 3. Clone the request and add the Authorization header
          const authReq = request.clone({
            setHeaders: {
              // Standard format for JWT: 'Bearer ' followed by the token
              Authorization: `Bearer ${authToken}`
            }
          });
          return next.handle(authReq);
        }
      } catch (e) {
        // If localStorage data is corrupted, log error but proceed without token
        console.error('Interceptor Error: Could not parse user data from localStorage.', e);
      }
    }

    // 4. Pass the original request if no token is found
    return next.handle(request);
  }
}

import { Injectable } from '@angular/core';
import {
  CanActivate,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
  Router,
  UrlTree,
} from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root',
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    const user = this.authService.getUserData();
    const requiredRole = route.data['role'] as string;

    if (user && user.token) {
      if (requiredRole && user.role !== requiredRole) {
        this.toastr.error(`Access Denied. Only ${requiredRole} users can access this page.`);
        return this.router.createUrlTree(['/home']);
      }

      return true;
    } else {
      this.toastr.warning('Please log in to access this page.');
      localStorage.setItem('redirect_to', state.url);
      return this.router.createUrlTree(['/login']);
    }
  }
}

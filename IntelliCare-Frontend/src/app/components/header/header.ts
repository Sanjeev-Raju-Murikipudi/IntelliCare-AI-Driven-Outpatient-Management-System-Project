import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-header',
  standalone: false,
  templateUrl: './header.html',
  styleUrls: ['./header.css']
})
export class Header {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  
getRole(): string {
    return this.authService.getRole(); // returns 'admin', 'doctor', or 'patient'
  }

  isLoggedIn(): boolean {
    return this.authService.isLoggedIn();
  }

  isPatient(): boolean {
    return this.authService.getRole() === 'Patient';
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/home']);
  }
}
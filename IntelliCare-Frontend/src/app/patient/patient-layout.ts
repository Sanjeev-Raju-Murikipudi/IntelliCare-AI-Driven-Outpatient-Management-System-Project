import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-patient-layout',
  standalone: false,
  templateUrl: './patient-layout.html',
  styleUrls: ['./patient-layout.css']
})
export class PatientLayout {
  navItems = [
    { label: 'Update Profile', route: 'profile', icon: 'bi bi-person-circle' },
    { label: 'My Appointments', route: 'appointments', icon: 'bi bi-calendar-check-fill' }
  ];

  constructor(private router: Router, private authService: AuthService) {}

  navigateTo(view: string): void {
    this.router.navigate([`/patient/${view}`]);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  isActive(route: string): boolean {
    return this.router.url.includes(route);
  }
}

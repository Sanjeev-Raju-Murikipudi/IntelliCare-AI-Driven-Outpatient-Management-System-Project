import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-admin-layout',
  standalone: false,
  templateUrl: './admin-layout.html',
  styleUrls: ['./admin-layout.css']
})
export class AdminLayout {
  isSidebarCollapsed = false;
  isDarkMode = false;

  navItems = [
    { label: 'Dashboard', route: 'dashboard', icon: 'bi bi-grid-fill' },
    { label: 'Create Slots', route: 'CreateSlots', icon: 'bi bi-bar-chart-fill' },
    { label: 'Appointments', route: 'appointments', icon: 'bi bi-calendar-check-fill' },
    { label: 'Prescription', route: 'prescription', icon: 'bi bi-calendar-check-fill' },
    { label: 'Add New Admin', route: 'add-new-admin', icon: 'bi bi-person-plus-fill' },
    { label: 'Add Doctor', route: 'add-doctor', icon: 'bi bi-person-plus-fill' },
    { label: 'Doctors List', route: 'doctor-list', icon: 'bi bi-people-fill' },
    { label: 'Patients', route: 'patients', icon: 'bi bi-file-earmark-person-fill' },
    { label: 'Billing', route: 'billing', icon: 'bi bi-credit-card-fill' },
    { label: 'Analytics', route: 'analytics', icon: 'bi bi-bar-chart-fill' }
  ];

  constructor(private router: Router, private authService: AuthService) {}

  navigateTo(view: string): void {
    this.router.navigate([`/admin/${view}`]);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  isActive(route: string): boolean {
    return this.router.url.includes(route);
  }

}
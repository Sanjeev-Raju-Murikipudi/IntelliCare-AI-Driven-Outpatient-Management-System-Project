import { Component, OnInit } from '@angular/core';
import { DashboardService } from '../../services/dashboard.service';

@Component({
  selector: 'app-appointments',
  standalone: false,
  templateUrl: './appointments.html',
  styleUrl: './appointments.css',
})
export class Appointments implements OnInit {
  appointments: any[] = [];
  filteredAppointments: any[] = [];
  pagedAppointments: any[] = [];

  // Filters
  searchPatient = '';
  searchDoctor = '';
  searchStatus = '';

  // Pagination
  currentPage = 1;
  pageSize = 7;

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.dashboardService.getAllAppointmentsHistory().subscribe(data => {
      this.appointments = data.filter(appt =>
        ['Booked', 'Cancelled', 'InProgress', 'Completed'].includes(appt.status)
      );
      this.applyFilters();
    });
  }

  applyFilters(): void {
    this.filteredAppointments = this.appointments.filter(appt =>
      (this.searchPatient === '' || appt.patientName.toLowerCase().includes(this.searchPatient.toLowerCase())) &&
      (this.searchDoctor === '' || appt.doctorName.toLowerCase().includes(this.searchDoctor.toLowerCase())) &&
      (this.searchStatus === '' || appt.status === this.searchStatus)
    );
    this.setPage(1);
  }

  setPage(page: number): void {
    this.currentPage = page;
    const start = (page - 1) * this.pageSize;
    const end = start + this.pageSize;
    this.pagedAppointments = this.filteredAppointments.slice(start, end);
  }

  get totalPages(): number[] {
    return Array(Math.ceil(this.filteredAppointments.length / this.pageSize)).fill(0).map((_, i) => i + 1);
  }
}
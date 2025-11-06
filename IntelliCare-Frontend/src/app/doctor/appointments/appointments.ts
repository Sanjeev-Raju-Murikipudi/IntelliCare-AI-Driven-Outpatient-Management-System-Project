import { Component, OnInit } from '@angular/core';
import { AppointmentService, Appointment } from '../../services/appointment';
 
@Component({
  standalone: false,
  selector: 'app-appointments',
  templateUrl: './appointments.html'
})
export class AppointmentsComponent implements OnInit {
  appointments: Appointment[] = [];
  loading = true;
  error = '';
  success = '';
 
  constructor(private appointmentService: AppointmentService) {}
 
  ngOnInit(): void {
    console.log('AppointmentsComponent initialized');
    this.fetchAppointments();
  }
 
  fetchAppointments(): void {
    this.loading = true;
    this.error = '';
    this.success = '';
    this.appointmentService.getTodayAppointments().subscribe({
      next: (data) => {
        console.log('Appointments loaded:', data);
        this.appointments = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('API error:', err);
        this.error = 'Failed to load appointments.';
        this.loading = false;
      }
    });
  }
 
  loadAppointments(): void {
    console.log('Manual trigger');
    this.fetchAppointments();
  }
 
  markComplete(appointmentId: number): void {
    this.appointmentService.markAsComplete(appointmentId).subscribe({
      next: (res) => {
        this.success = res.message;
        this.error = '';
        this.fetchAppointments(); // refresh list
      },
      error: (err) => {
        this.success = '';
        this.error = err.error?.error || 'Failed to mark appointment as completed.';
      }
    });
  }
}
 
 
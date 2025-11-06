import { Component } from '@angular/core';
import { AppointmentService, Appointment } from '../../services/appointment';
 
@Component({
  selector: 'app-appointments-by-date',
  standalone: false,
  templateUrl: './appointments-by-date.html',
  styleUrls: ['./appointments-by-date.css'] // ✅ corrected from styleUrl to styleUrls
})
export class AppointmentsByDate {
  selectedDate: string = this.formatDate(new Date()); // ✅ use string for input binding
  appointments: Appointment[] = [];
  loading = false;
  error = '';
  success = '';
 
  constructor(private appointmentService: AppointmentService) {}
 
  fetchAppointments(): void {
    this.loading = true;
    this.error = '';
    this.success = '';
 
    this.appointmentService.getAppointmentsByDate(this.selectedDate).subscribe({
      next: (data) => {
        this.appointments = data;
        this.loading = false;
        this.success = `Loaded ${data.length} appointments for ${this.selectedDate}`;
      },
      error: (err) => {
        this.error = err.error?.error || 'Failed to load appointments.';
        this.loading = false;
      }
    });
  }
 
  private formatDate(date: Date): string {
    return date.toISOString().split('T')[0]; // ✅ format as yyyy-MM-dd
  }
}
 
 
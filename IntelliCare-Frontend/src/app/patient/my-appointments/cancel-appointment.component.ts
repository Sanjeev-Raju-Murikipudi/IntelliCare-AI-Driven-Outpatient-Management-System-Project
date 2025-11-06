// cancel-appointment.component.ts
import { Component, Input } from '@angular/core';
import { AppointmentService } from '../../services/appointment.service2';
 
@Component({
  selector: 'app-cancel-appointment',
  template: `
    <button class="btn btn-danger" (click)="cancel()">Cancel</button>
  `
})
export class CancelAppointmentComponent {
  @Input() appointmentID!: number;
 
  constructor(private appointmentService: AppointmentService) {}
 
  cancel() {
    this.appointmentService.cancelAppointment(this.appointmentID).subscribe({
      next: (response) => {
        alert('Your appointment is cancelled.');
         //this.cancel.emit(this.appointmentID);
        // Remove from UI (emit event or update list)
      },
      error: (err) => {
        console.error('Error cancelling appointment:', err);
        alert('Failed to cancel appointment. Please try again.');
      }
    });
  }
}
 
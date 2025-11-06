// import { Component } from '@angular/core';

// @Component({
//   selector: 'app-my-appointments',
//   standalone: false,
//   templateUrl: './my-appointments.html',
//   styleUrl: './my-appointments.css',
// })
// export class MyAppointments {

// }


import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppointmentService } from '../../services/appointment.service2';

 
declare var bootstrap: any;
 
@Component({
  selector: 'app-my-appointments',
  standalone: false,
  // imports: [CommonModule],
  templateUrl: './my-appointments.html',
  styleUrls: ['./my-appointments.css']
})
export class MyAppointments implements OnInit {
  appointments: any[] = [];
  isLoading = false;
  error: string | null = null;
 
  selectedAppointment: any = null;
  availableSlots: any[] = [];
  isSlotsLoading = false;
  slotsError: string | null = null;
 
  appointmentToCancel: any = null;
  appointmentToReschedule: any = null;
  selectedNewSlot: string | null = null;
 
  constructor(private appointmentService: AppointmentService) {}
 
  ngOnInit(): void {
    this.loadMyAppointments();
  }
 
  loadMyAppointments(): void {
    this.isLoading = true;
    this.error = null;
    this.appointmentService.getMyAppointments().subscribe({
      next: (data) => {
        this.appointments = data || [];
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Oops You have no appointments', err);
        this.error = 'Oops You have no appointments';
        this.isLoading = false;
      }
    });
  }
 
  openCancelModal(appointment: any) {
    this.appointmentToCancel = appointment;
    const modalEl = document.getElementById('cancelModal');
    const modal = new bootstrap.Modal(modalEl!);
    modal.show();
  }
 
  confirmCancel() {
    if (!this.appointmentToCancel) return;
    this.appointmentService.cancelAppointment(this.appointmentToCancel.appointmentID).subscribe({
      next: () => {
        this.loadMyAppointments();
        this.showToast('Appointment cancelled successfully', 'success');
        this.closeModal('cancelModal');
      },
      error: (err) => {
        console.error(err);
        this.showToast('Failed to cancel appointment', 'danger');
        this.closeModal('cancelModal');
      }
    });
  }
 
  showReschedule(appointment: any) {
    this.selectedAppointment = appointment;
    this.availableSlots = [];
    this.slotsError = null;
    this.isSlotsLoading = true;
 
    this.appointmentService.getDoctorAvailability(appointment.doctorID).subscribe({
      next: (slots) => {
        this.availableSlots = slots;
        this.isSlotsLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.slotsError = 'Failed to load slots';
        this.isSlotsLoading = false;
      }
    });
  }
 
  openRescheduleModal(appointment: any, slotDateTime: string) {
    this.appointmentToReschedule = appointment;
    this.selectedNewSlot = slotDateTime;
    const modalEl = document.getElementById('rescheduleModal');
    const modal = new bootstrap.Modal(modalEl!);
    modal.show();
  }
 
  confirmRescheduleAction() {
    if (!this.appointmentToReschedule || !this.selectedNewSlot) {
      this.showToast('Please select a slot before confirming.', 'danger');
      return;
    }
 
    this.appointmentService.rescheduleAppointment(this.appointmentToReschedule.appointmentID, this.selectedNewSlot)
      .subscribe({
        next: () => {
          this.showToast('Appointment rescheduled successfully!', 'success');
          this.closeModal('rescheduleModal');
          this.selectedAppointment = null;
          this.selectedNewSlot = null;
          this.loadMyAppointments();
        },
        error: () => {
          this.showToast('Reschedule failed', 'danger');
          this.closeModal('rescheduleModal');
        }
      });
  }
 
  viewQueue(appointment: any) {
    const dateParam = appointment.date_Time;
    this.appointmentService.getQueue(appointment.doctorID, dateParam).subscribe({
      next: (q) => {
        this.showToast('Queue position: ' + JSON.stringify(q), 'info');
      },
      error: (err) => {
        console.error(err);
        this.showToast('Failed to get queue info', 'danger');
      }
    });
  }
 
  closeModal(id: string) {
    const modalEl = document.getElementById(id);
    const modal = bootstrap.Modal.getInstance(modalEl!);
    modal?.hide();
  }
 
  showToast(message: string, type: 'success' | 'danger' | 'info') {
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-bg-${type} border-0 position-fixed top-0 start-50 translate-middle-x mt-3`;
    toast.role = 'alert';
    toast.style.zIndex = '9999';
    toast.innerHTML = `
      <div class="d-flex">
        <div class="toast-body">${message}</div>
        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
      </div>`;
    document.body.appendChild(toast);
    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();
  }
}
 
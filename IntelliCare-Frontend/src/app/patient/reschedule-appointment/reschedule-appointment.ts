// import { Component } from '@angular/core';

// @Component({
//   selector: 'app-reschedule-appointment',
//   standalone: false,
//   templateUrl: './reschedule-appointment.html',
//   styleUrl: './reschedule-appointment.css',
// })
// export class RescheduleAppointment {

// }


import { Component, Input, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { AppointmentService } from '../../services/appointment.service2';
 
declare var bootstrap: any;
 
@Component({
  selector: 'app-reschedule-appointment',
  standalone: false,
  templateUrl: './reschedule-appointment.html'
})
export class RescheduleAppointment implements OnInit {
  @Input() doctorId!: number;
  @Input() appointmentId!: number;
  @Input() doctorName!: string;
 
  slots: any[] = [];
  selectedNewSlot: string | null = null;
 
  constructor(private appointmentService: AppointmentService) {}
 
  ngOnInit() {
    this.appointmentService.getDoctorAvailability(this.doctorId).subscribe({
      next: (data: any[]) => this.slots = data,
      error: () => this.showToast('Failed to load slots', 'danger')
    });
  }
 
  openRescheduleModal(slotDateTime: string) {
    this.selectedNewSlot = slotDateTime;
    const modalEl = document.getElementById('rescheduleModal');
    const modal = new bootstrap.Modal(modalEl!);
    modal.show();
  }
 
  confirmRescheduleAction() {
    if (!this.selectedNewSlot) {
      this.showToast('Please select a slot before confirming.', 'danger');
      return;
    }
 
    this.appointmentService.rescheduleAppointment(this.appointmentId, this.selectedNewSlot)
      .subscribe({
        next: () => {
          this.showToast('Appointment rescheduled successfully!', 'success');
          this.closeModal('rescheduleModal');
          this.selectedNewSlot = null;
        },
        error: () => {
          this.showToast('Failed to reschedule appointment', 'danger');
          this.closeModal('rescheduleModal');
        }
      });
  }
 
  closeModal(id: string) {
    const modalEl = document.getElementById(id);
    const modal = bootstrap.Modal.getInstance(modalEl!);
    modal?.hide();
  }
 
  showToast(message: string, type: 'success' | 'danger') {
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
 
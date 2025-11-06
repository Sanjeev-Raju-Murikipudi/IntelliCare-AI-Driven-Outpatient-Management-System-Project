// import { Component } from '@angular/core';

// @Component({
//   selector: 'app-doctor-details',
//   standalone: false,
//   templateUrl: './doctor-details.html',
//   styleUrl: './doctor-details.css',
// })
// export class DoctorDetails {

// }




import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AppointmentService } from '../../services/appointment.service2';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; // Needed for ngModel binding
 
declare var bootstrap: any; // For Bootstrap toast
 
@Component({
  selector: 'app-doctor-details',
  standalone: false,
  //imports: [CommonModule, FormsModule],
  templateUrl: './doctor-details.html',
  styleUrls: ['./doctor-details.css']
})
export class DoctorDetailsComponent implements OnInit {
  doctorId!: number;
  doctorDetails: any;
  slots: { date: string; times: Array<{ time: string; slotID: number; appointmentFee?: number; date_Time?: string }> }[] = [];
  selectedDaySlots: Array<{ time: string; slotID: number; appointmentFee?: number; date_Time?: string }> = [];
  selectedDay: string | null = null;
  selectedSlot: number | null = null;
  selectedSlotDateTime: string | null = null;
  selectedSlotFee: number | null = null;
  appointmentReason: string = ''; // ✅ Reason field
  isLoadingAvailability = false;
  availabilityError: string | null = null;
  bookingInProgress = false;
  bookingError: string | null = null;
 
  constructor(private route: ActivatedRoute, private appointmentService: AppointmentService) {}
 
  ngOnInit() {
    this.doctorId = +this.route.snapshot.paramMap.get('id')!;
    this.loadDoctorDetails();
    this.loadDoctorAvailability();
    this.isLoggedIn = !!localStorage.getItem('authToken'); // or use AuthService

  }
 
  loadDoctorDetails() {
    this.appointmentService.getDoctorDetails(this.doctorId)
      .subscribe((data: any) => this.doctorDetails = data);
  }
  
 isLoggedIn: boolean = false;


  loadDoctorAvailability() {
    this.isLoadingAvailability = true;
    this.availabilityError = null;
    this.appointmentService.getDoctorAvailability(this.doctorId)
      .subscribe({
        next: (data: any) => {
          try {
            if (!data) {
              this.slots = [];
            } else if (Array.isArray(data) && data.length > 0 && data[0].date_Time) {
              const grouped: any[] = [];
              (data as any[]).forEach(slot => {
                const date = new Date(slot.date_Time).toLocaleDateString('en-US', { weekday: 'short', day: 'numeric' });
                const time = new Date(slot.date_Time).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
                let dayGroup = grouped.find((g: any) => g.date === date);
                if (!dayGroup) { dayGroup = { date, times: [] }; grouped.push(dayGroup); }
                dayGroup.times.push({ time, slotID: slot.slotID, appointmentFee: slot.appointmentFee, date_Time: slot.date_Time });
              });
              this.slots = grouped;
            } else if (Array.isArray(data) && data.length > 0 && data[0].date && data[0].times) {
              this.slots = data;
            } else if (Array.isArray(data) && typeof data[0] === 'string') {
              this.slots = [{ date: new Date().toLocaleDateString('en-US', { weekday: 'short', day: 'numeric' }), times: data }];
            } else {
              console.warn('Unknown availability shape:', data);
              this.slots = [];
            }
          } catch (e) {
            console.error('Error processing availability', e);
            this.slots = [];
          }
 
          try {
            if ((!this.doctorDetails || !this.doctorDetails.appointmentFee) && this.slots && this.slots.length) {
              const fee = this.slots
                .flatMap((d: any) => d.times)
                .map((t: any) => t.appointmentFee)
                .find((f: any) => f != null);
              if (fee != null) {
                this.doctorDetails = this.doctorDetails || {};
                this.doctorDetails.appointmentFee = fee;
              }
            }
          } catch (e) {
            /* ignore */
          }
 
          this.isLoadingAvailability = false;
        },
        error: (err) => {
          console.error('Failed to load availability', err);
          this.availabilityError = 'Please LOGIN with your patient credentials....';
          this.isLoadingAvailability = false;
        }
      });
  }
 
  selectDay(day: any) {
    this.selectedDay = day.date;
    this.selectedDaySlots = day.times || [];
    this.selectedSlot = null;
    this.selectedSlotDateTime = null;
    this.selectedSlotFee = null;
  }
 
  selectSlot(slot: { time: string; slotID: number; appointmentFee?: number; date_Time?: string }) {
    this.selectedSlot = slot.slotID;
    this.selectedSlotDateTime = slot.date_Time ?? null;
    this.selectedSlotFee = slot.appointmentFee ?? null;
    console.log('Selected slot ->', { slotID: this.selectedSlot, date_Time: this.selectedSlotDateTime, fee: this.selectedSlotFee });
  }
 
  bookAppointment() {
    if (!this.selectedSlot || !this.appointmentReason.trim()) return;
    this.bookingError = null;
    this.bookingInProgress = true;
 
    const dto = {
      doctorID: this.doctorId,
      date_Time: this.selectedSlotDateTime,
      reason: this.appointmentReason.trim(),
      fee: this.selectedSlotFee ?? this.doctorDetails?.appointmentFee ?? 0
    };
 
    this.appointmentService.bookAppointment(dto)
      .subscribe({
        next: () => {
          this.bookingInProgress = false;
          this.showToast('Your appointment is booked successfully!', 'success');
          this.appointmentReason = '';
        },
        error: (err) => {
          this.bookingInProgress = false;
 
          // ✅ Use API message instead of hardcoded text
          const apiMessage = err.error?.message || 'Failed to book appointment';
          this.showToast(apiMessage, 'danger');
 
          this.bookingError = null;
        }
      });
  }
 
  // ✅ Helper to calculate end time based on next slot or fallback
  getEndTime(index: number, slots: Array<{ time: string; date_Time?: string }>): string {
    const currentSlotDate = new Date(slots[index].date_Time!);
 
    if (index < slots.length - 1) {
      const nextSlotDate = new Date(slots[index + 1].date_Time!);
      return nextSlotDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    } else {
      let duration = 10; // default
      if (index > 0) {
        const prevSlotDate = new Date(slots[index].date_Time!);
        const prevPrevSlotDate = new Date(slots[index - 1].date_Time!);
        duration = (prevSlotDate.getTime() - prevPrevSlotDate.getTime()) / 60000; // in minutes
      }
      const end = new Date(currentSlotDate.getTime() + duration * 60000);
      return end.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }
  }
 
  // ✅ Toast helper (Top Center)
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
 
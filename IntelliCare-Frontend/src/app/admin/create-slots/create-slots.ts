import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AppointmentService } from '../../services/appointment.service';

@Component({
  selector: 'app-create-slots',
  standalone: false,
  templateUrl: './create-slots.html',
  styleUrls: ['./create-slots.css']
})
export class CreateSlots implements OnInit {
  // Form for creating slots
  slotForm!: FormGroup;

  // Data arrays
  doctors: any[] = [];
  availableSlots: any[] = [];

  // UI states
  isModalOpen = false;
  loading = false;
  errorMessage: string | null = null;
  customAlertMessage: string | null = null;

  // Selected doctor for availability
  selectedDoctorId: number | null = null;

  constructor(private fb: FormBuilder, private appointmentService: AppointmentService) {}

  ngOnInit(): void {
    this.slotForm = this.fb.group({
      doctorID: ['', Validators.required],
      date: ['', Validators.required],
      startTime: ['', Validators.required],
      endTime: ['', Validators.required],
      intervalMinutes: ['', [Validators.required, Validators.min(1)]],
      fee: ['', [Validators.required, Validators.min(1)]]
    });

    this.loadDoctors();
  }

  /** Load doctors for dropdown */
  loadDoctors() {
    this.appointmentService.getDoctors().subscribe({
      next: (res) => this.doctors = res,
      error: (err) => {
        console.error('Error loading doctors:', err);
        this.errorMessage = 'Failed to load doctors. Please check API.';
      }
    });
  }

  /** Fetch availability when doctor is selected */
  onDoctorSelect() {
    if (!this.selectedDoctorId) {
      this.availableSlots = [];
      return;
    }
    this.loading = true;
    this.appointmentService.getDoctorAvailability(this.selectedDoctorId).subscribe({
      next: (res) => {
        this.availableSlots = res;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error fetching availability:', err);
        this.errorMessage = 'Unable to fetch availability. Please try again later.';
        this.loading = false;
      }
    });
  }

  /** Open modal */
  openCreateModal() {
    this.isModalOpen = true;
    this.errorMessage = null;
    this.customAlertMessage = null;
  }

  /** Close modal */
  closeCreateModal() {
    this.isModalOpen = false;
    this.slotForm.reset();
  }

  /** Save new slot */
  saveSlot() {
    if (this.slotForm.invalid) {
      this.errorMessage = 'Please fill all required fields correctly.';
      return;
    }

    this.loading = true;
    const payload = {
      doctorId: this.slotForm.value.doctorID,
      date: this.slotForm.value.date,
      startTime: this.slotForm.value.startTime,
      endTime: this.slotForm.value.endTime,
      intervalMinutes: this.slotForm.value.intervalMinutes,
      fee: this.slotForm.value.fee
    };

    this.appointmentService.createDoctorSlot(payload).subscribe({
      next: () => {
        this.loading = false;
        this.isModalOpen = false;
        this.customAlertMessage = 'Slot created successfully!';
        if (this.selectedDoctorId) {
          this.onDoctorSelect(); // Refresh availability for selected doctor
        }
      },
      error: (err) => {
        console.error('Error creating slot:', err);
        this.errorMessage = 'Failed to create slot. Please check API.';
        this.loading = false;
      }
    });
  }
}
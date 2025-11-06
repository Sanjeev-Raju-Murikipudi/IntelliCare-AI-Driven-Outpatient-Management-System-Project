import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { DoctorService } from '../../services/doctor.service';
import { CreateDoctorDto } from '../../models/create-doctor.dto';

// Note: Ensure your component is configured as standalone or imported into an NgModule.
// I have used the class name "AddDoctor" as per your initial file, but updated to use
// modern Angular dependency injection and structure.

@Component({
  selector: 'app-add-doctor',
  standalone: false,
  templateUrl: './add-doctor.html',
  styleUrls: ['./add-doctor.css'], // Use the dedicated CSS file
  // standalone: false (as per your initial file)
})
export class AddDoctor implements OnInit {
  private fb = inject(FormBuilder);
  private doctorService = inject(DoctorService);

  // State signals
  isLoading = signal(false);
  statusMessage = signal('');
  isSuccess = signal(false);

  // Reactive Form Group
  doctorForm!: FormGroup;

  ngOnInit(): void {
    // Define the form controls and validators to match your C# DTO
    this.doctorForm = this.fb.group({
      Username: ['', [Validators.required, Validators.maxLength(50)]],
      Password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(100)]],
      MobileNumber: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
      ContactEmail: ['', [Validators.required, Validators.email, Validators.maxLength(255)]],
      DoctorCreationKey: ['', [Validators.required]],
    });
  }

  // Helper function for template validation checks
  isInvalid(controlName: string): boolean {
    const control = this.doctorForm.get(controlName);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }

  async onSubmit(): Promise<void> {
    this.statusMessage.set(''); // Clear previous messages

    if (this.doctorForm.invalid) {
      this.doctorForm.markAllAsTouched(); // Show all validation errors
      this.statusMessage.set('Please correct the validation errors in the form.');
      this.isSuccess.set(false);
      return;
    }

    this.isLoading.set(true);
    const dto: CreateDoctorDto = this.doctorForm.value;

    try {
      await this.doctorService.createDoctor(dto);

      this.statusMessage.set(`Doctor account for "${dto.Username}" created successfully!`);
      this.isSuccess.set(true);
      this.doctorForm.reset(); // Clear form on success
      this.doctorForm.patchValue({ DoctorCreationKey: '' });
    } catch (error) {
      let errorMessage = 'An unexpected error occurred during creation.';
      this.isSuccess.set(false);

      if (error instanceof HttpErrorResponse) {
        if (error.status === 403) {
          errorMessage = 'Forbidden: Only Admins are allowed to create doctor accounts.';
        } else if (error.status === 400 && error.error?.error) {
          errorMessage = error.error.error;
        } else if (error.status === 400 && error.error?.errors) {
          const validationErrors = Object.values(error.error.errors).flat().join('; ');
          errorMessage = `Validation failed on server: ${validationErrors}`;
        } else if (error.status === 500) {
          errorMessage = 'Server Error: Could not complete the request.';
        } else {
          errorMessage = `API Error (${error.status}): ${error.message}`;
        }
      }

      this.statusMessage.set(errorMessage);
      console.error('Doctor Creation Error:', error);

    } finally {
      this.isLoading.set(false);
    }
  }
}

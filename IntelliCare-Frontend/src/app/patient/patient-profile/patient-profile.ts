import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PatientService } from '../../services/patient-dashboard.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-patient-profile',
  standalone: false,
  templateUrl: './patient-profile.html',
  styleUrls: ['./patient-profile.css']
})
export class PatientProfile implements OnInit {
  form!: FormGroup;
  profileData: any;
  editMode = false;
  loading = false;
  bloodGroups = ['A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'O+', 'O-'];

  constructor(
    private fb: FormBuilder,
    private patientService: PatientService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadProfile();
  }

  initForm(): void {
    this.form = this.fb.group({
      fullName: ['', Validators.required],
      dob: ['', Validators.required],
      gender: ['', Validators.required],
      bloodGroup: ['', Validators.required],
      phoneNumber: ['', [Validators.pattern(/^\d{10}$/)]],
      contactEmail: ['', [Validators.email]],
      insuranceDetails: [''],
      medicalHistory: ['', Validators.required],
      address: ['', Validators.required]
    });
  }

  loadProfile(): void {
    const token = localStorage.getItem('token');
    let username = '';

    if (token) {
      try {
        const decoded = JSON.parse(atob(token.split('.')[1]));
        username = decoded.username || '';
      } catch {
        this.toastr.error('Invalid token');
        return;
      }
    }

    if (!username) {
      this.toastr.error('Username missing in token');
      return;
    }

    this.loading = true;

    this.patientService.getPatientByUsername(username).subscribe({
      next: (res: any) => {
        this.profileData = res;
        this.loading = false;
      },
      error: () => {
        this.profileData = null;
        this.loading = false;
      }
    });
  }

  enableEdit(): void {
    this.editMode = true;
    if (!this.profileData) {
      this.form.reset();
    } else {
      this.form.patchValue({
        fullName: this.profileData.name, // ✅ map backend 'Name'
        dob: this.profileData.dob.split('T')[0], // ✅ remove time for date input
        gender: this.profileData.gender,
        bloodGroup: this.profileData.bloodGroup,
        phoneNumber: this.profileData.phoneNumber,
        contactEmail: this.profileData.contactEmail,
        insuranceDetails: this.profileData.insuranceDetails,
        medicalHistory: this.profileData.medicalHistory,
        address: this.profileData.address
      });
    }
  }

  cancelEdit(): void {
    this.editMode = false;
    if (this.profileData) {
      this.enableEdit(); // ✅ reload form with existing data
    }
  }

  submit(): void {
  if (this.form.invalid) {
    this.toastr.error('Please fill all required fields');
    return;
  }

  const rawDob = this.form.get('dob')?.value;
  const dobWithTime = `${rawDob}T00:00:00`;

  const token = localStorage.getItem('token');
  const decoded = token ? JSON.parse(atob(token.split('.')[1])) : null;
  const username = decoded?.username || '';

  this.loading = true;

  if (this.profileData && this.profileData.patientId) {
    // ✅ Update existing profile
    const updatePayload = {
      patientId: this.profileData.patientId,
      name: this.form.get('fullName')?.value, // backend expects 'name'
      dob: dobWithTime,
      gender: this.form.get('gender')?.value,
      bloodGroup: this.form.get('bloodGroup')?.value,
      phoneNumber: this.form.get('phoneNumber')?.value,
      contactEmail: this.form.get('contactEmail')?.value,
      insuranceDetails: this.form.get('insuranceDetails')?.value,
      medicalHistory: this.form.get('medicalHistory')?.value,
      address: this.form.get('address')?.value
    };

    this.patientService.updatePatient(this.profileData.patientId, updatePayload).subscribe({
      next: () => {
        this.toastr.success('Profile updated successfully');
        this.loading = false;
        this.editMode = false;
        this.loadProfile();
      },
      error: () => {
        this.toastr.error('Update failed');
        this.loading = false;
      }
    });
  } else {
    // ✅ Complete new profile
    const createPayload = {
      username: username, // required by CreatePatientDto
      fullName: this.form.get('fullName')?.value, // required by CreatePatientDto
      dob: dobWithTime,
      gender: this.form.get('gender')?.value,
      bloodGroup: this.form.get('bloodGroup')?.value,
      phoneNumber: this.form.get('phoneNumber')?.value,
      contactEmail: this.form.get('contactEmail')?.value,
      insuranceDetails: this.form.get('insuranceDetails')?.value,
      medicalHistory: this.form.get('medicalHistory')?.value,
      address: this.form.get('address')?.value
    };

    this.patientService.completeProfile(createPayload).subscribe({
      next: (res) => {
        this.toastr.success(res.message || 'Profile completed successfully');
        if (res.token) {
          localStorage.setItem('token', res.token);
        }
        this.loading = false;
        this.editMode = false;
        this.loadProfile();
      },
      error: () => {
        this.toastr.error('Profile completion failed');
        this.loading = false;
      }
    });
  }
}
}
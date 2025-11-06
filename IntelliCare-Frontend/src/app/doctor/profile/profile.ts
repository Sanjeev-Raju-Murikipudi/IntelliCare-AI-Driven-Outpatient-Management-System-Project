import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DoctorService } from '../../services/doctor';
import { ToastrService } from 'ngx-toastr';
 
@Component({
  standalone: false,
  selector: 'app-profile',
  templateUrl: './profile.html',
  styleUrls: ['./profile.css']
})
export class ProfileComponent implements OnInit {
  profileForm!: FormGroup;
  previewUrl: string = 'https://cdn-icons-png.flaticon.com/512/847/847969.png';
  loading = false;
  editMode = false;
  profileData: any;
 
  constructor(
    private fb: FormBuilder,
    private doctorService: DoctorService,
    private toastr: ToastrService
  ) {}
 
  ngOnInit(): void {
    this.initForm();
    this.loadProfile();
  }
 
  initForm(): void {
    this.profileForm = this.fb.group({
      name: ['', Validators.required],
      specialization: ['', Validators.required],
      education: ['', Validators.required],
      address: ['', Validators.required],
      experienceYears: [1, [Validators.required, Validators.min(1)]],
      photoUrl: ['', Validators.required]
    });
  }
 
  loadProfile(): void {
    const token = localStorage.getItem('token');
    let username = '';
 
    try {
      if (token) {
        const decoded = JSON.parse(atob(token.split('.')[1]));
        username = decoded.username || '';
      }
    } catch {
      this.toastr.error('Invalid token');
      return;
    }
 
    if (!username) {
      this.toastr.error('Username missing');
      return;
    }
 
    this.loading = true;
    this.doctorService.getDoctorProfile(username).subscribe({
      next: (res) => {
        this.profileData = res;
        this.previewUrl = res.photoUrl
          ? (res.photoUrl.startsWith('http') ? res.photoUrl : `https://localhost:7215${res.photoUrl}`)
          : this.previewUrl;
 
        this.profileForm.patchValue(res);
        this.editMode = false;
        this.loading = false;
      },
      error: () => {
        this.toastr.error('Failed to load profile');
        this.loading = false;
      }
    });
  }
 
  enableEdit(): void {
    this.editMode = true;
  }
 
  cancelEdit(): void {
    this.editMode = false;
    this.profileForm.patchValue(this.profileData);
    this.previewUrl = this.profileData?.photoUrl
      ? (this.profileData.photoUrl.startsWith('http') ? this.profileData.photoUrl : `https://localhost:7215${this.profileData.photoUrl}`)
      : this.previewUrl;
  }
 
  onFileSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
 
    const reader = new FileReader();
    reader.onload = () => {
      this.previewUrl = reader.result as string;
    };
    reader.readAsDataURL(file);
 
    const formData = new FormData();
    formData.append('photo', file);
 
    this.doctorService.uploadPhoto(formData).subscribe({
      next: (res) => {
        const fullUrl = res.url.startsWith('http') ? res.url : `https://localhost:7215${res.url}`;
        this.previewUrl = fullUrl;
        this.profileForm.patchValue({ photoUrl: res.url });
        this.toastr.success('Photo uploaded successfully');
      },
      error: (err) => {
        this.toastr.error('Upload failed');
        console.error(err);
      }
    });
  }
 
  submit(): void {
    if (this.profileForm.invalid) {
      this.toastr.error('Please fill all required fields');
      return;
    }
 
    const token = localStorage.getItem('token');
    let username = '';
    try {
      if (token) {
        const decoded = JSON.parse(atob(token.split('.')[1]));
        username = decoded.username || '';
      }
    } catch {
      this.toastr.error('Invalid token');
      return;
    }
 
    if (!username) {
      this.toastr.error('Username missing');
      return;
    }
 
    const payload = {
      ...this.profileForm.value,
      username
    };
 
    this.loading = true;
    this.doctorService.completeProfile(payload).subscribe({
      next: (res) => {
        this.toastr.success(res.message || 'Profile updated successfully');
        this.loading = false;
        this.editMode = false;
        this.loadProfile();
      },
      error: (err) => {
        this.toastr.error(err.error?.error || 'Update failed');
        this.loading = false;
      }
    });
  }
}
 
 
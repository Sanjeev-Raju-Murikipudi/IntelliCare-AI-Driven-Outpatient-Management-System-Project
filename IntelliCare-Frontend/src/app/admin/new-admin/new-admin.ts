import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AdminService } from '../../services/admin.service';
import { CreateAdminDto } from '../../models/create-admin.dto';
import { ToastrService } from 'ngx-toastr';
import { ChartConfiguration } from 'chart.js';
@Component({
  selector: 'app-new-admin',
  standalone: false,
  templateUrl: './new-admin.html',
  styleUrls: ['./new-admin.css']
})
export class NewAdmin {
  adminForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private adminService: AdminService,
    private toastr: ToastrService
  ) {
    this.adminForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6)]],
      mobileNumber: ['', Validators.required],
      name: ['', Validators.required],
      adminCreationKey: ['', Validators.required],
      contactEmail: ['', [Validators.required, Validators.email]]
    });
  }

  onSubmit(): void {
    if (this.adminForm.invalid) return;

    const dto: CreateAdminDto = this.adminForm.value;

    this.adminService.createAdmin(dto).subscribe({
      next: (response) => {
        this.toastr.success(response.message, 'Success');
        this.adminForm.reset();
      },
      error: (err) => {
        const errorMsg = err.error?.error || 'Unexpected error occurred.';
        this.toastr.error(errorMsg, 'Error');
      }
    });
  }
}
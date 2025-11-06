import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { AuthService, RegisterUserDto } from '../services/auth.service';
import { ToastrService } from 'ngx-toastr';
 
 
@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
     constructor(private authService: AuthService, private toastr: ToastrService) {}
 
  onSubmit(form: NgForm): void {
    if (!form.valid) {
      this.toastr.info('Please fill all fields correctly.'); // 🔵 Info toast
      return;
    }
 
    const dto: RegisterUserDto = {
      username: form.value.username,
      password: form.value.password,
      mobileNumber: form.value.mobile,
      contactEmail: form.value.email
    };
 
    this.authService.registerPatient(dto).subscribe({
      next: (response) => {
        this.toastr.success(response.message || 'Patient account registered successfully!'); // 🟩 Success toast
        form.resetForm();
      },
      error: (error) => {
        if (error.status === 400) {
          this.toastr.warning('Invalid details entered. Please check and try again.'); // 🟨 Warning toast
        } else {
          this.toastr.error('Registration failed. Please try again later.'); // 🟥 Error toast
        }
      }
    });
  }
}
 
 
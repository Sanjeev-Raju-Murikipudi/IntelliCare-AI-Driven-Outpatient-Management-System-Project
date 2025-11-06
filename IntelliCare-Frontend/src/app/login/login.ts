import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class Login {
  loading = false; // ✅ added for spinner and button disable

  constructor(
    private auth: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  onLogin(form: NgForm): void {
    if (!form.valid) {
      this.toastr.info('Please fill in all fields correctly.');
      return;
    }

    this.loading = true; // ✅ start loading

    const dto = {
      username: form.value.username,
      password: form.value.password
    };

    this.auth.login(dto).subscribe({
      next: (res: any) => {
        console.log('Login successful:', res);
        this.toastr.success(res.message || 'OTP sent to your email.');
        localStorage.setItem('user', JSON.stringify(res.user));
        form.resetForm();
        this.router.navigate(['/verify-otp']);
        this.loading = false; // ✅ stop loading
      },
      error: (err: { status: number; message: string }) => {
        if (err.status === 401) {
          this.toastr.error('Invalid username or password.');
        } else if (err.status === 403) {
          this.toastr.warning('Account is locked. Try again in a few minutes.');
        } else {
          this.toastr.error(err.message || 'Login failed.');
        }

        console.warn('Login error:', err);
        this.loading = false; // ✅ stop loading on error
      }
    });
  }
}


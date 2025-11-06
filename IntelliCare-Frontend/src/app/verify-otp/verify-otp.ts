import { Component } from '@angular/core';
import { NgForm } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
 
@Component({
  standalone: false,
  selector: 'app-verify-otp',
  templateUrl: './verify-otp.html',
  styleUrls: ['./verify-otp.css']
})
export class VerifyOtp {
  loading = false; // ✅ added for spinner and button disable
 
  constructor(
    private auth: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {}
 
  onVerify(form: NgForm): void {
  if (!form.valid) {
    this.toastr.info('Please enter the OTP.');
    return;
  }

  this.loading = true;

  const username = JSON.parse(localStorage.getItem('user') || '{}')?.username;
  const dto = {
    username,
    otpCode: form.value.otp
  };

  this.auth.verifyOtp(dto).subscribe({
    next: (res: any) => {
      this.toastr.success(res.message || 'OTP verified!');

      // ✅ Store token and normalized user object
      localStorage.setItem('token', res.token);
      localStorage.setItem('user', JSON.stringify({
        username: res.user.username,
        roleName: res.user.roleName // keep original
      }));

      const role = res.user.roleName?.toLowerCase();
      switch (role) {
        case 'doctor':
          this.router.navigate(['/doctor/profile']);
          break;
        case 'admin':
          this.router.navigate(['/admin/dashboard']);
          break;
        case 'patient':
          this.router.navigate(['/home']);
          break;
        default:
          this.router.navigate(['/login']);
          break;
      }

      this.loading = false;
    },
    error: (err) => {
      const msg = err.error?.error || 'OTP verification failed.';
      this.toastr.error(msg);
      this.loading = false;
    }
  });
  }
}



// import { Component } from '@angular/core';
// import { NgForm } from '@angular/forms';
// import { AuthService } from '../services/auth.service';
// import { Router, ActivatedRoute } from '@angular/router'; 
// import { ToastrService } from 'ngx-toastr';

// @Component({
//   selector: 'app-verify-otp',
//   standalone: false,
//   templateUrl: './verify-otp.html',
//   styleUrl: './verify-otp.css',
// })
// export class VerifyOtp {
//   loading = false;
//   // Inject ActivatedRoute to read query parameters
//   constructor(
//     private auth: AuthService,
//     private router: Router,
//     private activatedRoute: ActivatedRoute, // Must be injected
//     private toastr: ToastrService
//   ) {}

//   onVerify(form: NgForm): void {
//     if (!form.valid) {
//       this.toastr.info('Please enter the OTP.');
//       return;
//     }
//     this.loading = true;
//     const userDataString = localStorage.getItem('user') || '{}';
//     const existingUser = JSON.parse(userDataString);
//     const username = existingUser.username;

//     if (!username) {
//       this.toastr.error('User context lost. Please log in again.');
//       this.router.navigate(['/login']);
//       return;
//     }

//     const dto = {
//       username: username,
//       otpCode: form.value.otp,
//     };

//     this.auth.verifyOtp(dto).subscribe({
//       next: (res: any) => {
//         this.toastr.success(
//           res.message || 'OTP verified! You are now logged in.'
//         );

//         // Store token and role
//         const completeUserObject = {
//           ...existingUser,
//           token: res.token,
//           role: res.role,
//         };

//         localStorage.setItem('user', JSON.stringify(completeUserObject));

//         // 🔑 CRITICAL FIX: Use the synchronous snapshot to read the parameter
//         const redirectUrl = this.activatedRoute.snapshot.queryParams['redirectTo'];
        
//         if (redirectUrl) {
//             // 1. If a redirect URL exists (e.g., /admin), navigate directly there.
//             this.router.navigateByUrl(redirectUrl);
            
//             // 2. IMPORTANT: Return immediately to stop further navigation logic below.
//             return;
//         } 
        
//         // --- START Default Role-Based Navigation (Only executed if NO redirectUrl was found) ---

//         if (completeUserObject.role === 'Admin') {
//             this.router.navigate(['/admin']);
//         } else if (completeUserObject.role === 'Patient') {
//             this.router.navigate(['/dashboard']);
//         } else if (completeUserObject.role === 'Doctor') {
//             this.router.navigate(['/doctor-dashboard']);
//         } else {
//             // Fallback if role is undefined or unrecognized
//             this.router.navigate(['/home']);
//         }
        
//         // --- END Default Role-Based Navigation ---

//       },
//       error: (err) => {
//         const msg = err.error?.error || 'OTP verification failed.';
//         this.toastr.error(msg);
//       },
//     });
//   }
// }

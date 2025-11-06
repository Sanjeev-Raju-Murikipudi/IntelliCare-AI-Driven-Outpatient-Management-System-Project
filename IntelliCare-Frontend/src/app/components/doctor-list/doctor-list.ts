// import { Component } from '@angular/core';

// @Component({
//   selector: 'app-doctor-list',
//   standalone: false,
//   templateUrl: './doctor-list.html',
//   styleUrl: './doctor-list.css',
// })
// export class DoctorList {

// }


import { AuthService } from '../../services/auth.service';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { DoctorService } from '../../services/doctor.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-doctor-list',
  standalone: false,
  // imports: [ RouterModule],
  templateUrl: './doctor-list.html',
  styleUrls: ['./doctor-list.css']
})
export class DoctorListComponent implements OnInit {
  doctors: any[] = [];
  isLoading = false;
  error: string | null = null;
  isPatientUser = false;

  constructor(private doctorService: DoctorService, private router: Router,private authService: AuthService) {}

  ngOnInit(): void {
    this.isPatientUser = this.authService.isPatient();
    this.loadDoctors();
  }

  // ✅ Fetch all doctors from the database
  loadDoctors(): void {
    this.isLoading = true;
    this.error = null;
    this.doctorService.getAllDoctors().subscribe({
      next: (data) => {
        console.log('GET /Doctor/all ->', data);
        this.doctors = data || [];
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error fetching doctors', err);
        this.error = 'Failed to load doctors from the API. Check console/network.';
        this.isLoading = false;
      }
    });
  }

  // ✅ Navigate to doctor details page when View Details is clicked
  viewDetails(doctorId: number): void {
    this.router.navigate(['/doctor', doctorId]);
  }
}

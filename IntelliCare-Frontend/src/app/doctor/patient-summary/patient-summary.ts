import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DoctorService } from '../../services/doctor';
import { ToastrService } from 'ngx-toastr';
 
@Component({
  standalone: false,
  selector: 'app-patient-summary',
  templateUrl: './patient-summary.html',
  styleUrls: ['./patient-summary.css']
})
export class PatientSummaryComponent implements OnInit {
  appointmentId!: number;
  summary: any;
  loading = true;
 
  constructor(
    private route: ActivatedRoute,
    private doctorService: DoctorService,
    private toastr: ToastrService
  ) {}
 
  ngOnInit(): void {
    this.appointmentId = Number(this.route.snapshot.paramMap.get('id'));
 
    this.doctorService.getPatientSummary(this.appointmentId).subscribe({
      next: (res) => {
        this.summary = res;
        this.loading = false;
      },
      error: (err) => {
        this.toastr.error(err.error?.message || 'Failed to load patient summary');
        this.loading = false;
      }
    });
  }
}
 
 
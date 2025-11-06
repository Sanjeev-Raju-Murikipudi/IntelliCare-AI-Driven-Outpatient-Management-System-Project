import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ConsultationService, RecordConsultationDto, PrescriptionDetailDto } from '../../services/consultation.service';
 
@Component({
  standalone: false,
  selector: 'app-consultation',
  templateUrl: './consultation.html',
  styleUrls: ['./consultation.css']
})
export class Consultation implements OnInit {
  dto: RecordConsultationDto = {
    appointmentId: 0,
    notes: '',
    diagnosis: '',
    medication: ''
  };
 
  prescription: PrescriptionDetailDto | null = null;
  loading = true;
  error = '';
 
  constructor(
    private route: ActivatedRoute,
    private consultationService: ConsultationService
  ) {}
 
  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id || isNaN(id)) {
      this.error = 'Invalid appointment ID';
      this.loading = false;
      return;
    }
 
    this.dto.appointmentId = id;
 
    this.consultationService.getPrescription(id).subscribe({
      next: (res) => {
        this.prescription = res;
        this.loading = false;
      },
      error: () => {
        this.prescription = null;
        this.loading = false;
      }
    });
  }
 
  submitConsultation() {
    this.loading = true;
    this.error = '';
    this.consultationService.recordConsultation(this.dto).subscribe({
      next: (res) => {
        this.prescription = res;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.error || 'Failed to record consultation';
        this.loading = false;
      }
    });
  }
}
 
 
import { Component, OnInit } from '@angular/core';
import { PrescriptionService } from '../../services/prescription.service';
import { PrescriptionDetailDto, PrescriptionStatusUpdateDto } from '../../models/prescription.model';

@Component({
  selector: 'app-prescription',
  standalone: false,
  templateUrl: './prescription.html',
  styleUrls: ['./prescription.css']
})
export class Prescription implements OnInit {
  prescriptions: PrescriptionDetailDto[] = [];
  filteredPrescriptions: PrescriptionDetailDto[] = [];

  searchPatient = '';
  filterStatus = '';
  loading = false;
  success = '';
  error = '';

  // Pagination
  currentPage = 1;
  pageSize = 5;

  constructor(private prescriptionService: PrescriptionService) {}

  ngOnInit(): void {
    this.loadPrescriptions();
  }

  loadPrescriptions() {
    this.loading = true;
    this.prescriptionService.getAllPrescriptions().subscribe({
      next: (data) => {
        this.prescriptions = data;
        this.applyFilters();
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load prescriptions.';
        this.loading = false;
      }
    });
  }

  applyFilters() {
    this.currentPage = 1;
    this.filteredPrescriptions = this.prescriptions.filter(p =>
      (!this.searchPatient || p.patientName.toLowerCase().includes(this.searchPatient.toLowerCase())) &&
      (!this.filterStatus || p.pharmacyStatus === this.filterStatus)
    );
  }

  paginatedPrescriptions(): PrescriptionDetailDto[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredPrescriptions.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredPrescriptions.length / this.pageSize);
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }

  updatePrescription(prescription: PrescriptionDetailDto) {
    const dto: PrescriptionStatusUpdateDto = {
      newStatus: prescription.pharmacyStatus as any,
      deliveryETA: prescription.deliveryETA
    };

    this.prescriptionService.updatePrescriptionStatus(prescription.clinicalRecordId, dto).subscribe({
      next: () => {
        this.success = `Updated ${prescription.patientName}'s prescription.`;
        this.error = '';
        this.loadPrescriptions(); // Refresh data
      },
      error: (err) => {
        this.error = err.error?.error || 'Failed to update prescription.';
        this.success = '';
      }
    });
  }
}
import { Component, OnInit } from '@angular/core';
import { PatientService } from '../../services/patient.service';
import { Patient } from '../../models/patient.model';

@Component({
  selector: 'app-patients',
  standalone: false,
  templateUrl: './patients.html',
  styleUrls: ['./patients.css']
})
export class Patients implements OnInit {
  patients: Patient[] = [];
  loading = true;
  error = '';

  // --- Filter Properties ---
  filterPatientName: string = '';
  filterGender: string = '';
  filterBloodGroup: string = '';

  // --- Pagination Properties ---
  currentPage: number = 1;
  itemsPerPage: number = 2;

  constructor(private patientService: PatientService) {}

  ngOnInit(): void {
    this.patientService.getAllPatients().subscribe({
      next: (data) => {
        this.patients = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load patient data.';
        this.loading = false;
      }
    });
  }

  // --- Filter Logic ---
  get filteredPatients(): Patient[] {
    const nameFilter = this.filterPatientName.toLowerCase().trim();
    const genderFilter = this.filterGender;
    const bloodGroupFilter = this.filterBloodGroup;

    return this.patients.filter(patient => {
      const matchesName = !nameFilter || patient.name.toLowerCase().includes(nameFilter);
      const matchesGender = !genderFilter || patient.gender === genderFilter;
      const matchesBloodGroup = !bloodGroupFilter || patient.bloodGroup === bloodGroupFilter;
      return matchesName && matchesGender && matchesBloodGroup;
    });
  }

  // --- Pagination Logic ---
  get paginatedPatients(): Patient[] {
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.filteredPatients.slice(startIndex, endIndex);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredPatients.length / this.itemsPerPage);
  }

  changePage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }

  clearFilters(): void {
    this.filterPatientName = '';
    this.filterGender = '';
    this.filterBloodGroup = '';
    this.currentPage = 1;
  }
}
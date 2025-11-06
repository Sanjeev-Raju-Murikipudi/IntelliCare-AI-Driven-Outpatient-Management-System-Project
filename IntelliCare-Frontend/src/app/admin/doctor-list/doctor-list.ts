import { Component, OnInit } from '@angular/core';
import { DoctorService } from '../../services/doctor.service';
import { Doctor } from '../../models/doctor.model';

@Component({
  selector: 'app-doctor-list',
  standalone: false,
  templateUrl: './doctor-list.html',
  styleUrls: ['./doctor-list.css']
})
export class DoctorList implements OnInit {
  doctors: Doctor[] = [];
  loading = true;
  error = '';

  // --- Filter Properties ---
  filterDoctorName: string = '';
  filterSpecialization: string = '';

  // --- Pagination Properties ---
  currentPage: number = 1;
  itemsPerPage: number = 3;

  constructor(private doctorService: DoctorService) {}

  ngOnInit(): void {
    this.doctorService.getAllDoctors().subscribe({
      next: (data) => {
        this.doctors = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load doctor data.';
        this.loading = false;
      }
    });
  }

  // --- Filter Logic ---
  get filteredDoctors(): Doctor[] {
    const nameFilter = this.filterDoctorName.toLowerCase().trim();
    const specializationFilter = this.filterSpecialization.toLowerCase().trim();

    return this.doctors.filter(doctor => {
      const matchesName = !nameFilter || doctor.name.toLowerCase().includes(nameFilter);
      const matchesSpecialization = !specializationFilter || doctor.specialization.toLowerCase().includes(specializationFilter);
      return matchesName && matchesSpecialization;
    });
  }

  // --- Pagination Logic ---
  get paginatedDoctors(): Doctor[] {
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.filteredDoctors.slice(startIndex, endIndex);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredDoctors.length / this.itemsPerPage);
  }

  changePage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }

  clearFilters(): void {
    this.filterDoctorName = '';
    this.filterSpecialization = '';
    this.currentPage = 1;
  }
}
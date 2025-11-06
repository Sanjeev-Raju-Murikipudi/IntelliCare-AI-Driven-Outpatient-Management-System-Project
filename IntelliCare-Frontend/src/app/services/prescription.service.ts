import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PrescriptionDetailDto, PrescriptionStatusUpdateDto } from '../models/prescription.model';

@Injectable({
  providedIn: 'root'
})
export class PrescriptionService {
  private apiUrl = 'https://localhost:7215/api/Consultation';

  constructor(private http: HttpClient) {}

  getPrescriptionByAppointmentId(appointmentId: number): Observable<PrescriptionDetailDto> {
    return this.http.get<PrescriptionDetailDto>(`${this.apiUrl}/${appointmentId}`);
  }

  updatePrescriptionStatus(clinicalRecordId: number, dto: PrescriptionStatusUpdateDto): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/PrescriptionStatus/${clinicalRecordId}`, dto);
  }
  
getAllPrescriptions(): Observable<PrescriptionDetailDto[]> {
  return this.http.get<PrescriptionDetailDto[]>(`${this.apiUrl}/AllPrescriptions`);
}
}

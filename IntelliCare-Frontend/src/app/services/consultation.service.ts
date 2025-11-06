import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
 
// Inline DTOs
export interface RecordConsultationDto {
  appointmentId: number;
  notes: string;
  diagnosis: string;
  medication: string;
}
 
export interface PrescriptionDetailDto {
  hospitalName: string;
  clinicalRecordId: number;
  prescriptionDate: string;
  patientName: string;
  doctorName: string;
  doctorSpecialty: string;
  medicationDetails: string;
  pharmacyStatus: string;
  deliveryETA: string | null;
}
 
@Injectable({
  providedIn: 'root'
})
export class ConsultationService {
  private apiUrl = 'https://localhost:7215/api/Consultation';
 
  constructor(private http: HttpClient) {}
 
  // POST: Record a new consultation
  recordConsultation(dto: RecordConsultationDto): Observable<PrescriptionDetailDto> {
    return this.http.post<PrescriptionDetailDto>(this.apiUrl, dto);
  }
 
  // GET: Fetch existing prescription for an appointment
  getPrescription(appointmentId: number): Observable<PrescriptionDetailDto> {
    return this.http.get<PrescriptionDetailDto>(`${this.apiUrl}/${appointmentId}`);
  }
 
}
 
 
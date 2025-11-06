import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UpdatePatientDto } from '../models/CreatePatientDto';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private baseUrl = 'https://localhost:7215/api';

  constructor(private http: HttpClient) {}

  getAllPatients(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Patient`);
  }

  getAllDoctors(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Doctor/all-doctors`);
    // https://localhost:7215/api/Doctor/all-doctors
  }

  getAllAppointments(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Consultation/AllAppointments`);
  }

  getAllDoctorsAppointmentsToday(): Observable<any[]> {
  return this.http.get<any[]>('https://localhost:7215/api/appointments/appointments/today/all');
    }


    getAllAppointmentsHistory(): Observable<any[]> {
  return this.http.get<any[]>(`${this.baseUrl}/appointments/appointments/all`);
}
updatePatient(id: number, payload: UpdatePatientDto): Observable<any> {
  return this.http.put(`/api/Patient/${id}`, payload);
}


}
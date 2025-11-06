import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
 
@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private baseUrl = 'https://localhost:7215/api';
 
  constructor(private http: HttpClient) {}
 
  createDoctorSlot(payload: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/appointments/createDoctorSlot`, payload);
  }
 
  getDoctors(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/Doctor/all-doctors`);
   // https://localhost:7215/api/Doctor/all-doctors
  }
  getSlots(): Observable<any[]> {
  return this.http.get<any[]>(`${this.baseUrl}/appointments/getSlots`);
}


  getDoctorAvailability(doctorId: number): Observable<any[]> {
  return this.http.get<any[]>(`${this.baseUrl}/appointments/DoctorAvailability?doctorId=${doctorId}`);
}
}
 
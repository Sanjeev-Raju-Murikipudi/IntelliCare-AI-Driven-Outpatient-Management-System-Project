import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
 
export interface Appointment {
  appointmentId: number;
  patientName: string;
  scheduledTime: string;
  status: string;
  reason: string;
}
 
@Injectable({
  providedIn: 'root'
})
export class AppointmentService {
  private apiUrl = 'https://localhost:7215/api/Doctor/appointments/today';
// Adjust if your base path differs
 
  constructor(private http: HttpClient) {}
 
  getTodayAppointments(): Observable<Appointment[]> {
    return this.http.get<Appointment[]>(this.apiUrl);
  }
 
  markAsComplete(appointmentId: number): Observable<{ message: string }> {
  return this.http.post<{ message: string }>(
    `https://localhost:7215/api/Doctor/appointments/markAsComplete?appointmentId=${appointmentId}`,
    {}
  );
}
 
 
getAppointmentsByDate(dateString: string): Observable<Appointment[]> {
  return this.http.get<Appointment[]>(
    `https://localhost:7215/api/Doctor/appointments/by-date?date=${dateString}`
  );
}
 
}
 
 
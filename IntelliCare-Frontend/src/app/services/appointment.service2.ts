import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
 
@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private baseUrl = 'https://localhost:7215/api'; // ✅ Base API URL
 
  constructor(private http: HttpClient) {}
 
  // ✅ Helper to include Authorization header
  private getAuthOptions() {
    const token = localStorage.getItem('token'); // Ensure token is stored after login
    return token
      ? {
          headers: new HttpHeaders({
            Authorization: `Bearer ${token}`
          })
        }
      : {};
  }
 
  // ✅ Get doctor details by ID
  getDoctorDetails(doctorId: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/Doctor/${doctorId}`, this.getAuthOptions());
  }
 
  // ✅ Get doctor availability slots
  getDoctorAvailability(doctorId: number): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.baseUrl}/appointments/DoctorAvailability?doctorId=${doctorId}`,
      this.getAuthOptions()
    );
  }
 
  // ✅ Get authenticated user's appointments
  getMyAppointments(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/appointments/myAppointments`, this.getAuthOptions());
  }
 
  // ✅ Cancel appointment
  cancelAppointment(appointmentId: number): Observable<any> {
    return this.http.post(
      `${this.baseUrl}/appointments/cancelAppointment`,
      { appointmentId }, // ✅ camelCase
      {
        ...this.getAuthOptions(),
        responseType: 'text' as 'json' // ✅ Handle plain text response
      }
    );
  }
 
  // ✅ Reschedule appointment
  rescheduleAppointment(appointmentId: number, newDate_Time: string): Observable<any> {
    return this.http.post(
      `${this.baseUrl}/appointments/rescheduleAppointment`,
      {
        appointmentId,      // ✅ camelCase
        newDate_Time        // ✅ camelCase
      },
      {
        ...this.getAuthOptions(),
        responseType: 'text' as 'json'
      }
    );
  }
 
  // ✅ Get queue info for a doctor on a specific date
  getQueue(doctorId: number, date: string): Observable<any> {
    const url = `${this.baseUrl}/appointments/queue/${doctorId}?date=${encodeURIComponent(date)}`;
    return this.http.get<any>(url, this.getAuthOptions());
  }
 
  // ✅ Book appointment
  bookAppointment(dto: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/appointments/bookAppointment`, dto, this.getAuthOptions());
  }
}
 
 
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
 
// ✅ DTOs for request payloads
export interface CompleteDoctorProfileDto {
  username: string;
  name: string;
  specialization: string;
  education: string;
  address: string;
  experienceYears: number;
  photoUrl: string;
}
 
export interface ConsultationDto {
  appointmentId: number;
  diagnosis: string;
  prescription: string;
  notes?: string;
}
 
@Injectable({
  providedIn: 'root'
})
export class DoctorService {
  private baseUrl = 'https://localhost:7215/api/Doctor';
 
  constructor(private http: HttpClient) {}
 
  // ✅ Secure headers with JWT
  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
  }
 
  /**
   * ✅ Complete doctor profile
   */
  completeProfile(dto: CompleteDoctorProfileDto): Observable<{ message: string; profile: CompleteDoctorProfileDto }> {
    return this.http.post<{ message: string; profile: CompleteDoctorProfileDto }>(
      `${this.baseUrl}/complete-profile`,
      dto,
      { headers: this.getAuthHeaders() }
    );
  }
 
  /**
   * ✅ Get doctor profile by username
   */
  getDoctorProfile(username: string): Observable<CompleteDoctorProfileDto> {
    return this.http.get<CompleteDoctorProfileDto>(
      `${this.baseUrl}/profile`,
      {
        params: { username },
        headers: this.getAuthHeaders()
      }
    );
  }
 
  /**
   * ✅ Upload profile photo
   */
  uploadPhoto(formData: FormData): Observable<{ url: string }> {
    return this.http.post<{ url: string }>(
      `${this.baseUrl}/upload-photo`,
      formData,
      { headers: this.getAuthHeaders() }
    );
  }
 
  /**
   * 📅 Get today's appointments
   */
  getTodayAppointments(): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.baseUrl}/appointments/today`,
      { headers: this.getAuthHeaders() }
    );
  }
 
  /**
   * 🧑‍⚕️ Get patient summary by appointment ID
   */
  getPatientSummary(appointmentId: number): Observable<any> {
    return this.http.get<any>(
      `${this.baseUrl}/appointments/patientSummary`,
      {
        params: { appointmentId: appointmentId.toString() },
        headers: this.getAuthHeaders()
      }
    );
  }
 
  /**
   * 📝 Record consultation details
   */
  recordConsultation(dto: ConsultationDto): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(
      `${this.baseUrl}/recordConsultation`,
      dto,
      { headers: this.getAuthHeaders() }
    );
  }
 
  /**
   * ✅ Mark appointment as complete
   */
  markAppointmentComplete(appointmentId: number): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(
      `${this.baseUrl}/appointments/markAsComplete`,
      {},
      {
        params: { appointmentId: appointmentId.toString() },
        headers: this.getAuthHeaders()
      }
    );
  }
 
  /**
   * 📆 Get appointments by specific date (YYYY-MM-DD)
   */
  getAppointmentsByDate(date: string): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.baseUrl}/appointments/by-date`,
      {
        params: { date },
        headers: this.getAuthHeaders()
      }
    );
  }
}
 
 
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Patient } from '../models/patient.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PatientService {
  private apiUrl = 'https://localhost:7215/api/Patient'; // Adjust if needed

  constructor(private http: HttpClient) {}

  getAllPatients(): Observable<Patient[]> {
    return this.http.get<Patient[]>(this.apiUrl);
  }
  
completeProfile(payload: any): Observable<any> {
  return this.http.post(`${this.apiUrl}/complete-profile`, payload);
}

updatePatient(patientId: number, payload: any): Observable<any> {
  return this.http.put(`${this.apiUrl}/${patientId}`, payload);
}

getPatientById(id: number): Observable<any> {
  return this.http.get(`${this.apiUrl}/${id}`);
}
getPatientByUsername(username: string): Observable<any> {
  return this.http.get(`${this.apiUrl}/by-username/${username}`);
}
}

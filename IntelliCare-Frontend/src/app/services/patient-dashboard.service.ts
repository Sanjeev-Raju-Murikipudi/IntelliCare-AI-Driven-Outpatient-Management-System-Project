import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PatientPublicDto, PatientUpdateDto, CreatePatientDto } from '../models/patient.dashboard.model';
 
@Injectable({
  providedIn: 'root'
})
export class PatientService {
  private baseUrl = 'https://localhost:7215/api/Patient';
 
  constructor(private http: HttpClient) {}
 
  getAllPatients(): Observable<PatientPublicDto[]> {
    return this.http.get<PatientPublicDto[]>(this.baseUrl);
  }
 
  getPatientById(id: number): Observable<PatientPublicDto> {
    return this.http.get<PatientPublicDto>(`${this.baseUrl}/${id}`);
  }
 
  updatePatient(id: number, payload: PatientUpdateDto): Observable<string> {
    return this.http.put(`${this.baseUrl}/${id}`, payload, { responseType: 'text' });
  }
//  getPatientByUsername(username: string): Observable<any> {
//   return this.http.get(`${this.baseUrl}/by-username/${username}`);
// }
getPatientByUsername(username: string): Observable<any> {
  const token = localStorage.getItem('token');
  const headers = { Authorization: `Bearer ${token}` };
  return this.http.get(`${this.baseUrl}/by-username/${username}`, { headers });
}
  deleteSelf(username: string): Observable<string> {
    return this.http.request('DELETE', `${this.baseUrl}/delete-self`, {
      body: JSON.stringify(username),
      headers: { 'Content-Type': 'application/json' },
      responseType: 'text' as const
    });
  }
 
  completeProfile(dto: CreatePatientDto): Observable<any> {
    return this.http.post(`${this.baseUrl}/complete-profile`, dto);
  }
}
 
 
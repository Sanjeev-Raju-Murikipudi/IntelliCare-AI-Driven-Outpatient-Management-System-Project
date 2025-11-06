import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ReportSummaryDto } from '../models/report-summary.dto';
import { ReportDetailDto } from '../models/report-detail.dto';
import { Doctor } from '../models/doctor.model';
 
@Injectable({ providedIn: 'root' })
export class ReportService {
  private baseUrl = 'https://localhost:7215/api';
   private getAllDoctorsEndpoint = `${this.baseUrl}/Doctor/all-doctors`;
 
  constructor(private http: HttpClient) {}
 
  generateReport(payload: any) {
    return this.http.post(`${this.baseUrl}/Report/Generate`, payload);
  }
 
  getAllReports(): Observable<ReportSummaryDto[]> {
    return this.http.get<ReportSummaryDto[]>(`${this.baseUrl}/Report/All`);
  }
 
  deleteReport(reportID: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/Report/${reportID}`);
  }
 
 getReportDetail(reportID: number): Observable<ReportDetailDto> {
  return this.http.get<ReportDetailDto>(`${this.baseUrl}/Report/Detail/${reportID}`);
}
 getAllDoctors(): Observable<Doctor[]> {
    return this.http.get<Doctor[]>(this.getAllDoctorsEndpoint);
  }
 
}
 
 
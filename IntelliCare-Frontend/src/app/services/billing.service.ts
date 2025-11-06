
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Invoice, CreateInvoiceDTO } from '../models/invoice.model';
import { Patient } from '../models/patient.model';

@Injectable({
  providedIn: 'root'
})
export class BillingService {
  private invoiceApiUrl = 'https://localhost:7215/api/Invoice';
  private prescriptionsApiUrl = 'https://localhost:7215/api/Consultation/AllPrescriptions';
  private patientsApiUrl = 'https://localhost:7215/api/Patient';

  constructor(private http: HttpClient) {}

  /** Fetch all invoices */
  getAllInvoices(): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(this.invoiceApiUrl);
  }

  /** Fetch invoice by ID */
  getInvoiceById(id: number): Observable<Invoice> {
    return this.http.get<Invoice>(`${this.invoiceApiUrl}/${id}`);
  }

  /** Download invoice PDF */
  getInvoicePdf(invoiceId: number): Observable<Blob> {
    return this.http.get(`${this.invoiceApiUrl}/${invoiceId}/pdf`, { responseType: 'blob' });
  }

  /** Fetch invoices by payment status */
  getByStatus(status: string): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(`${this.invoiceApiUrl}/amount/${status}`);
  }

  /** Fetch invoices by claim status */
  getByClaimStatus(claimStatus: string): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(`${this.invoiceApiUrl}/claim/${claimStatus}`);
  }

  /** Fetch invoices by patient ID */
  getByPatientId(patientId: number): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(`${this.invoiceApiUrl}/patient/${patientId}`);
  }

  /** Create a new invoice */
  createInvoice(dto: CreateInvoiceDTO): Observable<Invoice> {
    return this.http.post<Invoice>(this.invoiceApiUrl, dto);
  }

  /** Fetch all prescriptions */
  getAllPrescriptions(): Observable<any[]> {
    return this.http.get<any[]>(this.prescriptionsApiUrl);
  }

  /** Fetch all patients */
  getAllPatients(): Observable<Patient[]> {
    return this.http.get<Patient[]>(this.patientsApiUrl);
  }
}

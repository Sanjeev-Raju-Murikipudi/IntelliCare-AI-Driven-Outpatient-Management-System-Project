import { Component, OnInit } from '@angular/core';
import { BillingService } from '../../services/billing.service';
import { Invoice, CreateInvoiceDTO } from '../../models/invoice.model';
import { HttpErrorResponse } from '@angular/common/http';
import { Patient } from '../../models/patient.model';

@Component({
  selector: 'app-billing',
  standalone: false,
  templateUrl: './billing.html',
  styleUrls: ['./billing.css'],
})
export class Billing implements OnInit {
  invoices: Invoice[] = [];
  isLoading = true;
  errorMessage: string | null = null;
  customAlertMessage: string | null = null;

  isModalOpen = false;
  isSaving = false;

  newInvoice: CreateInvoiceDTO = {
    patientID: 0,
    clinicalRecordID: null,
    amount: 0,
    insuranceProvider: 'None',
    status: 'Paid',
    claimStatus: 'No claim',
  };

  filterInvoiceId: string = '';
filterStatus: string = '';
filterClaimStatus: string = '';

get filteredInvoices(): Invoice[] {
  const idFilter = this.filterInvoiceId.toLowerCase().trim();
  const statusFilter = this.filterStatus;
  const claimFilter = this.filterClaimStatus;

  return this.invoices.filter(invoice => {
    const matchesId = !idFilter || invoice.invoiceID.toString().includes(idFilter);
    const matchesStatus = !statusFilter || invoice.status === statusFilter;
    const matchesClaimStatus = !claimFilter || invoice.claimStatus === claimFilter;
    return matchesId && matchesStatus && matchesClaimStatus;
  });
}

clearFilters(): void {
  this.filterInvoiceId = '';
  this.filterStatus = '';
  this.filterClaimStatus = '';
  this.customAlertMessage = 'All filters cleared. Showing all invoices.';
}

  eligiblePatients: any[] = [];
  allPatients: Patient[] = [];
  selectedPatient: any = null;

  constructor(private billingService: BillingService) {}

  ngOnInit(): void {
    this.fetchInvoices();
    this.loadPatientNames();
  }

  fetchInvoices(): void {
    this.isLoading = true;
    this.billingService.getAllInvoices().subscribe({
      next: (data) => {
        this.invoices = data;
        this.isLoading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.isLoading = false;
        this.errorMessage = `Failed to load invoices. Status: ${error.status}. Details: ${error.message}`;
      }
    });
  }

  openCreateModal(): void {
    this.newInvoice = {
      patientID: 0,
      clinicalRecordID: null,
      amount: 0,
      insuranceProvider: 'None',
      status: 'Paid',
      claimStatus: 'No claim',
    };
    this.selectedPatient = null;
    this.isModalOpen = true;
    this.loadEligiblePatients();
  }

  closeCreateModal(): void {
    this.isModalOpen = false;
  }

  /** Load prescriptions and patients, then filter eligible patients */
  loadEligiblePatients(): void {
    this.billingService.getAllPrescriptions().subscribe({
      next: (prescriptions) => {
        this.billingService.getAllPatients().subscribe({
          next: (patients) => {
            this.allPatients = patients;

            // Combine both lists: only patients who exist in both
            this.eligiblePatients = prescriptions
              .map(p => {
                const matchedPatient = patients.find(pt => pt.name.trim().toLowerCase() === p.patientName.trim().toLowerCase());
                if (matchedPatient) {
                  return {
                    patientId: matchedPatient.patientId,
                    patientName: p.patientName,
                    clinicalRecordId: p.clinicalRecordId,
                    doctorName: p.doctorName
                  };
                }
                return null;
              })
              .filter(item => item !== null);
          },
          error: (err) => console.error('Failed to load patients:', err)
        });
      },
      error: (err) => {
        console.error('Error loading prescriptions:', err);
        this.errorMessage = 'Failed to load eligible patients.';
      }
    });
  }

  /** Auto-fill IDs internally */
  onPatientSelect(): void {
    if (this.selectedPatient) {
      this.newInvoice.patientID = this.selectedPatient.patientId;
      this.newInvoice.clinicalRecordID = this.selectedPatient.clinicalRecordId;
    }
  }

  saveInvoice(): void {
    if (this.newInvoice.amount <= 0) {
      this.customAlertMessage = 'Amount must be greater than zero.';
      return;
    }

    this.isSaving = true;
    this.billingService.createInvoice(this.newInvoice).subscribe({
      next: (createdInvoice) => {
        this.invoices.push(createdInvoice);
        this.isSaving = false;
        this.closeCreateModal();
        this.customAlertMessage = `Invoice #${createdInvoice.invoiceID} created successfully.`;
      },
      error: (error: HttpErrorResponse) => {
        this.isSaving = false;
        this.errorMessage = `Failed to create invoice. Status: ${error.status}. Details: ${error.message}`;
      }
    });
  }

  downloadPdf(invoiceId: number): void {
    this.billingService.getInvoicePdf(invoiceId).subscribe({
      next: (data) => {
        const fileURL = URL.createObjectURL(data);
        const a = document.createElement('a');
        a.href = fileURL;
        a.download = `invoice_${invoiceId}.pdf`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(fileURL);
      },
      error: (error: HttpErrorResponse) => {
        this.customAlertMessage = `Error downloading PDF for #${invoiceId}.`;
      }
    });
  }

patientNameMap: { [key: number]: string } = {};

loadPatientNames(): void {
  this.billingService.getAllPatients().subscribe({
    next: (patients) => {
      patients.forEach(pt => {
        this.patientNameMap[pt.patientId] = pt.name; // ✅ Correct mapping
      });
    },
    error: (err) => console.error('Failed to load patients:', err)
  });
}


}
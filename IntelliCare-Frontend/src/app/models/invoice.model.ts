// Interface matching the InvoiceDTO returned by the C# backend
export interface Invoice {
    // 🛑 FIX: Changed properties to camelCase (standard Angular/JSON convention)
    invoiceID: number; 
    patientID: number; 
    clinicalRecordID: number; 
    amount: number; 
    insuranceProvider: string; 
    status: 'Paid' | 'Pending' | 'Due'; 
    claimStatus: 'Approved' | 'Pending' | 'Denied' | 'No claim'; 
}

// DTO for creating or updating an Invoice (also camelCase for consistency)
export interface CreateInvoiceDTO {
    patientID: number;
    clinicalRecordID: number|null;
    amount: number;
    insuranceProvider: string;
    status: 'Paid' | 'Pending' | 'Due';
    claimStatus: 'Approved' | 'Pending' | 'Denied' | 'No claim';
}
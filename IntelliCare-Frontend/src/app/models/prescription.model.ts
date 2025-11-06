export interface PrescriptionStatusUpdateDto {
  newStatus: 'Ready to Deliver' | 'Not yet Delivered' | 'Delivered';
  deliveryETA?: string;
}

export interface PrescriptionDetailDto {
  hospitalName: string;
  clinicalRecordId: number;
  prescriptionDate: string;
  patientName: string;
  doctorName: string;
  doctorSpecialty: string;
  medicationDetails: string;
  pharmacyStatus: string;
  deliveryETA?: string;
}
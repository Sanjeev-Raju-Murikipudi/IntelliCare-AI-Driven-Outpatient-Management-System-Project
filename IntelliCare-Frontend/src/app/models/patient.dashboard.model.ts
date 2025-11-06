export interface PatientPublicDto {
  patientId: number;
  name: string;
  phoneNumber: string;
  contactEmail: string;
  dob: string;
  gender: string;
  bloodGroup: string;
}
 
export interface PatientUpdateDto {
  patientId: number;
  name: string;
  phoneNumber: string;
  contactEmail: string;
  dob: string;
  insuranceDetails: string;
  medicalHistory: string;
  address: string;
}
 
export interface CreatePatientDto {
  username: string;
  fullName: string;
  dob: string;
  gender: 'Male' | 'Female' | 'Other';
  bloodGroup: string;
  insuranceDetails?: string;
  medicalHistory: string;
  address: string;
}
 
 
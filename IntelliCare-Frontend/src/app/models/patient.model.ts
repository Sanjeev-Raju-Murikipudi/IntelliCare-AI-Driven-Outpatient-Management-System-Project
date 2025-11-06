export interface Patient {
  patientId: number;
  name: string;
  phoneNumber: string;
  contactEmail?: string;
  dob: string;
  age: number;
  gender: string;
  bloodGroup: string;
  insuranceDetails: string;
  medicalHistory: string;
  address: string;
}

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AdminRoutingModule } from './admin-routing-module'; 

// Components
import { AdminLayout } from './admin-layout';
import { Dashboard } from './dashboard/dashboard'; 
import { AddDoctor } from './add-doctor/add-doctor';
import { DoctorList } from './doctor-list/doctor-list';
import { Appointments } from './appointments/appointments';
import { Billing } from './billing/billing';
import { Analytics } from './analytics/analytics';
import { Patients } from './patients/patients';
import { NewAdmin } from './new-admin/new-admin';
import { CreateSlots } from './create-slots/create-slots';
import { Prescription } from './prescription/prescription';
import { NgChartsModule } from 'ng2-charts';

@NgModule({
  declarations: [
    AdminLayout,
    Dashboard,
    AddDoctor,
    DoctorList,
    Appointments,
    Billing,
    Analytics,
    Patients,
    NewAdmin,
    CreateSlots,
   Prescription
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    AdminRoutingModule,
    NgChartsModule
  ],
  exports: [
    // AdminLayout,
    // Billing,
    // AddDoctor,
    // NewAdmin,
    // CreateSlots,
    // Analytics,
    // DoctorList,
    // Patients, 
    // Prescription
  ]
})
export class AdminModule { }
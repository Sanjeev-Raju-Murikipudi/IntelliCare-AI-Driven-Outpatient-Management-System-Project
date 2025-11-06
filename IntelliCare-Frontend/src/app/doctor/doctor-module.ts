import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
 
import { DoctorRoutingModule } from './doctor-routing-module';
import { ProfileComponent } from './profile/profile';
import { AppointmentsComponent } from './appointments/appointments';
import { Dashboard } from './dashboard/dashboard';
import { PatientSummaryComponent } from './patient-summary/patient-summary';
import { Consultation } from './consultation/consultation';
import { AppointmentsByDate } from './appointments-by-date/appointments-by-date';
 
 
@NgModule({
  declarations: [
    ProfileComponent,
    AppointmentsComponent,
    Dashboard,
    PatientSummaryComponent,
    Consultation,
    AppointmentsByDate
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    DoctorRoutingModule
  ]
})
export class DoctorModule {}
 
 
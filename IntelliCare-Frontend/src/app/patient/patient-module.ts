import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { PatientRoutingModule } from './patient-routing-module';
import { PatientProfile } from './patient-profile/patient-profile';
import { MyAppointments } from './my-appointments/my-appointments';
import { PatientLayout } from './patient-layout';
import { RescheduleAppointment } from './reschedule-appointment/reschedule-appointment';

@NgModule({
  declarations: [
    PatientProfile,    // ✅ Use your class names
    MyAppointments,
    PatientLayout,
    RescheduleAppointment
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule, // ✅ Needed for [formGroup]
    RouterModule,         // ✅ Needed for router-outlet
    PatientRoutingModule
  ]
})
export class PatientModule {}
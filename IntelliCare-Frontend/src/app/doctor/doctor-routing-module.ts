import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Dashboard } from './dashboard/dashboard';
import { ProfileComponent } from './profile/profile';
import { AppointmentsComponent } from './appointments/appointments';
import { PatientSummaryComponent } from './patient-summary/patient-summary';
import { Consultation } from './consultation/consultation';
import { AppointmentsByDate } from './appointments-by-date/appointments-by-date';
 
const routes: Routes = [
  {
    path: '',
    component: Dashboard,
    children: [
      { path: 'profile', component: ProfileComponent },
      { path: 'appointments', component: AppointmentsComponent },
      { path: 'appointments/by-date', component: AppointmentsByDate },
      { path: 'patient-summary/:id', component: PatientSummaryComponent },
      { path: 'consultation/:id', component: Consultation }
    ]
  }
];
 
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DoctorRoutingModule {}
 
 
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PatientProfile } from './patient-profile/patient-profile';
import { MyAppointments } from './my-appointments/my-appointments';
import { PatientLayout } from './patient-layout';

const routes: Routes = [
  {
    path: '',
    component: PatientLayout,
    children: [
      { path: 'profile', component: PatientProfile },
      { path: 'appointments', component: MyAppointments },
      { path: '', redirectTo: 'profile', pathMatch: 'full' }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PatientRoutingModule {}
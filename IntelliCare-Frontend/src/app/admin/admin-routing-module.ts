import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminLayout } from './admin-layout';
import { Dashboard } from './dashboard/dashboard';
import { AddDoctor } from './add-doctor/add-doctor';
import { Analytics } from './analytics/analytics';
import { Billing } from './billing/billing';
import { CreateSlots } from './create-slots/create-slots';
import { DoctorList } from './doctor-list/doctor-list';
import { NewAdmin } from './new-admin/new-admin';
import { Patients } from './patients/patients';
import { Prescription } from './prescription/prescription';
import { Appointments } from './appointments/appointments';


  const routes: Routes = [
  {
    path: '',
    component: AdminLayout,
    children: [
      { path: 'dashboard', component: Dashboard },
      { path: 'add-doctor', component: AddDoctor },
        { path: 'appointments', component: Appointments },
      { path: 'analytics', component: Analytics },
      { path: 'billing', component: Billing },
      { path: 'CreateSlots', component: CreateSlots },
      { path: 'doctor-list', component: DoctorList },
      { path: 'add-new-admin', component: NewAdmin },
      { path: 'patients', component: Patients },
      { path: 'prescription', component: Prescription }
    ]
  }
];
 


@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }


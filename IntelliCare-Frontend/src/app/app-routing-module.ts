import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Login } from './login/login';
import { HomePage } from './pages/home-page/home-page';
import { VerifyOtp } from './verify-otp/verify-otp';
import { Register } from './register/register';
import { AboutUs } from './about-us/about-us';
import { ContactUs } from './contact-us/contact-us';
import { DoctorListComponent } from './components/doctor-list/doctor-list';
import { DoctorDetailsComponent } from './components/doctor-details/doctor-details';
import { AuthGuard } from './guards/auth-guard';

const routes: Routes = [
  // ✅ Default route
  { path: '', redirectTo: '/home', pathMatch: 'full' },

  // ✅ Public routes
  { path: 'home', component: HomePage },
  { path: 'about', component: AboutUs },
  { path: 'contact', component: ContactUs },

  // ✅ Authentication routes
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'verify-otp', component: VerifyOtp },

  // ✅ Public doctor list & details (renamed to avoid conflict)
  { path: 'doctors', component: DoctorListComponent },
  { path: 'doctors/:id', component: DoctorDetailsComponent },

  // ✅ Admin module (lazy-loaded)
  {
    path: 'admin',
    loadChildren: () => import('./admin/admin-module').then(m => m.AdminModule),
    canActivate: [AuthGuard],
    data: { expectedRole: 'Admin' }
  },

  // ✅ Doctor module (lazy-loaded)
  {
    path: 'doctor',
    loadChildren: () => import('./doctor/doctor-module').then(m => m.DoctorModule),
    canActivate: [AuthGuard],
    data: { expectedRole: 'Doctor' }
  },

  // ✅ Patient module (lazy-loaded)
  {
    path: 'patient',
    loadChildren: () => import('./patient/patient-module').then(m => m.PatientModule),
    canActivate: [AuthGuard],
    data: { expectedRole: 'Patient' }
  },

  // ✅ Wildcard route
  { path: '**', redirectTo: '/home' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}







import { NgModule, ErrorHandler } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ToastrModule } from 'ngx-toastr';
import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { Header } from './components/header/header';
import { HomePage } from './pages/home-page/home-page';
import { DoctorCard } from './components/doctor-card/doctor-card';
import { Footer } from './components/footer/footer';
import { AdminModule } from './admin/admin-module';
import { Login } from './login/login';
import { Register } from './register/register';
import { VerifyOtp } from './verify-otp/verify-otp';
import { AboutHospitalModule } from './about-us/about-us.module';
import { ContactUsModule } from './contact-us/contact-us.module';
import { RouterModule } from '@angular/router';
import { provideHttpClient, HTTP_INTERCEPTORS, withInterceptorsFromDi } from '@angular/common/http';
import { JwtInterceptor } from './interceptors/jwt.interceptor';
import { GlobalErrorHandler } from './core/global-error-handler';
import { provideAnimations } from '@angular/platform-browser/animations';
import { DoctorListComponent } from './components/doctor-list/doctor-list';
import { DoctorDetailsComponent } from './components/doctor-details/doctor-details'; 

@NgModule({
  declarations: [
    App,
    Header,
    HomePage,
    DoctorCard,
    Footer,
    Login,
    Register,
    VerifyOtp,
    DoctorListComponent,
    DoctorDetailsComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    AdminModule,
    FormsModule,
    ReactiveFormsModule,
    AboutHospitalModule,
    ContactUsModule,
    RouterModule,
    ToastrModule.forRoot({
      positionClass: 'toast-top-right',
      timeOut: 2500,
      extendedTimeOut: 1000,
      closeButton: true,
      progressBar: true,
      easeTime: 300,
      tapToDismiss: true,
      toastClass: 'ngx-toastr custom-toast'
    })
  ],
  providers: [
    provideHttpClient(withInterceptorsFromDi()),
    provideAnimations(), // ✅ This replaces BrowserAnimationsModule
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: ErrorHandler, useClass: GlobalErrorHandler }
  ],
  bootstrap: [App]
})
export class AppModule {}

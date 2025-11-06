import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { provideHttpClient } from '@angular/common/http';
import { NgChartsModule } from 'ng2-charts';
 
import { ReportRoutingModule } from './report-routing-module';
import { ReportGenerate } from './report-generate/report-generate';
import { ReportList } from './report-list/report-list';
import { ReportDetail } from './report-detail/report-detail';
import { ReactiveFormsModule } from '@angular/forms';
import { FormsModule } from '@angular/forms';
@NgModule({
  declarations: [
    ReportGenerate,
    ReportList,
    ReportDetail
  ],
  imports: [
    CommonModule,
    FormsModule,
      ReactiveFormsModule,
    NgChartsModule,
    ReportRoutingModule
  ],
  providers: [
    provideHttpClient() // ✅ This replaces HttpClientModule
  ]
})
export class ReportModule { }
 
 
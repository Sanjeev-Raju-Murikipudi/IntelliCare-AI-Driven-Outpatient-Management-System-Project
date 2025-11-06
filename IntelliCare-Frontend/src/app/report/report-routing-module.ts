import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ReportGenerate } from './report-generate/report-generate';
import { ReportList } from './report-list/report-list';
import { ReportDetail } from './report-detail/report-detail';
 
const routes: Routes = [
  {
    path: '',
    redirectTo: 'generate',
    pathMatch: 'full'
  },
  {
    path: 'generate',
    component: ReportGenerate
  },
  {
    path: 'list',
    component: ReportList
  },
  {
    path: 'detail/:id',
    component: ReportDetail
  }
];
 
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ReportRoutingModule { }
 
 
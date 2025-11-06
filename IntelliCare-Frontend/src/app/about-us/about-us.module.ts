import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AboutUs } from './about-us';
 
@NgModule({
  declarations: [AboutUs],
  imports: [CommonModule],
  exports: [AboutUs] // Export it to use it elsewhere
})
export class AboutHospitalModule { }
 
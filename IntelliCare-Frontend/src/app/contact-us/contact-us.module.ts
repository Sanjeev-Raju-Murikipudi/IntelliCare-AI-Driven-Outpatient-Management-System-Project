import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common'; // Required for common directives like *ngIf, *ngFor
 
// Import the component created previously
import { ContactUs } from './contact-us';
import { RouterModule } from '@angular/router'; // Useful if the module defines its own routes
 
@NgModule({
  // 1. Declare the component that belongs to this module
  declarations: [
    ContactUs
  ],
  imports: [
    // This is required for feature modules to use standard directives
    CommonModule,
    RouterModule
  ],
  // 2. Export the component so other modules can use it
  exports: [
    ContactUs
  ]
})
export class ContactUsModule { }
 
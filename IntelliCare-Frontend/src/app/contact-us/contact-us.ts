import { Component } from '@angular/core';
 
@Component({
  selector: 'app-contact-us',
  standalone: false,
  templateUrl: './contact-us.html',
  styleUrl: './contact-us.css',
})
export class ContactUs {
  // Data matching the implied content from design images (Prescripto example)
  contactInfo = {
    heading: 'OUR OFFICE',
    addressLine1: '54709 Willms Station',
    addressLine2: 'Suite 350, Washington, USA',
    tel: 'Tel: (415) 555-0132',
    email: 'Email: intellicare108@gmail.com'
  };
 
  careersInfo = {
    heading: 'CAREERS AT IntelliCare',
    description: 'Learn more about our teams and job openings.'
  };
 
}
 
 
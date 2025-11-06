import { Component, signal } from '@angular/core';
import { NgForm } from '@angular/forms';
@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrls: ['./app.css']
})
export class App {
  protected readonly title = signal('IntelliCare_Frontend');
        onSubmit(form: NgForm): void {
    if (form.valid) {
      console.log('Form submitted:', form.value);
      alert('Registration successful!');
    } else {
      console.log('Form invalid');
    }
  }
 
  }


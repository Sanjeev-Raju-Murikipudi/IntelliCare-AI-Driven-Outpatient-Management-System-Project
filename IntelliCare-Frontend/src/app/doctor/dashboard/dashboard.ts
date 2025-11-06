import { Component } from '@angular/core';
import { Router } from '@angular/router';
 
@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css'] // ✅ corrected from 'styleUrl'
})
export class Dashboard {
  loggingOut = false;
 
  constructor(private router: Router) {}
 
  handleLogout() {
    this.loggingOut = true;
 
    // Simulate logout delay or call actual logout logic
    setTimeout(() => {
      this.router.navigate(['/logout']);
    }, 1500);
  }
}
 
 
import { Component, OnInit } from '@angular/core';
import { DashboardService } from '../../services/dashboard.service';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class Dashboard implements OnInit {
  patientsCount = 0;
  doctorsCount = 0;
  appointmentsCount = 0;
  latestAppointments: any[] = [];

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
  this.dashboardService.getAllPatients().subscribe(data => this.patientsCount = data.length);
  this.dashboardService.getAllDoctors().subscribe(data => this.doctorsCount = data.length);
  this.dashboardService.getAllDoctorsAppointmentsToday().subscribe(data => {
    this.appointmentsCount = data.length;

    // Sort by scheduledTime in descending order
    this.latestAppointments = data
      .sort((a, b) => new Date(b.scheduledTime).getTime() - new Date(a.scheduledTime).getTime())
      .slice(0, 3); // Show latest 3
  });
}

}
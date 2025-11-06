import { Component, OnInit } from '@angular/core';
import { DoctorService } from '../../services/doctor.service';

@Component({
  selector: 'app-home-page',
  standalone: false,
  templateUrl: './home-page.html',
  styleUrls: ['./home-page.css'],
})
export class HomePage implements OnInit {
  topDoctors: any[] = [];
  isLoading = false;
  error: string | null = null;

  reviews = [
    {
      name: 'Sanjeev Raju',
      review:
        'The scheduling system is incredibly efficient, cutting down my wait time significantly. Dr. James was excellent and the e-prescription was ready immediately. Highly recommend IntelliCare.',
      rating: 5,
      photo:
        'https://t4.ftcdn.net/jpg/03/26/98/51/360_F_326985142_1aaKcEjMQW6ULp6oI9MYuv8lN9f8sFmj.jpg',
    },
    {
      name: 'Chand Basha',
      review:
        'Booking appointments has never been easier. The doctor utilization reports must be working well because the clinic flow is seamless now. Great job!',
      rating: 5,
      photo:
        'https://plus.unsplash.com/premium_photo-1672239496412-ab605befa53f?ixlib=rb-4.1.0&amp;ixid=M3wxMjA3fDB8MHxzZWFyY2h8NXx8bWFsZXxlbnwwfHwwfHx8MA%3D%3D&amp;fm=jpg&amp;q=60&amp;w=3000%27',
    },
    {
      name: 'Swamy',
      review:
        'Detailed notes and diagnosis records are easily accessible. The staff were friendly, and the consultation process felt thorough and organized. Truly AI-driven care.',
      rating: 4,
      photo:
        'https://t4.ftcdn.net/jpg/03/26/98/51/360_F_326985142_1aaKcEjMQW6ULp6oI9MYuv8lN9f8sFmj.jpg',
    },
  ];

  constructor(private doctorService: DoctorService) {}

  ngOnInit(): void {
    this.loadTopDoctors();
  }

  loadTopDoctors(): void {
    this.isLoading = true;
    this.error = null;
    this.doctorService.getAllDoctors().subscribe({
      next: (data) => {
        console.log('Fetched doctors:', data);
        this.topDoctors = (data || []).slice(0, 8);
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error fetching doctors', err);
        this.error = 'Failed to load doctors.';
        this.isLoading = false;
      },
    });
  }
}
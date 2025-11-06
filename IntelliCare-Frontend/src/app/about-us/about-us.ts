import { Component } from '@angular/core';
 
@Component({
  selector: 'app-about-us',
  standalone: false,
  templateUrl: './about-us.html',
  styleUrl: './about-us.css',
})
export class AboutUs {
  // Information about the hospital
  hospitalName = 'IntelliCare';
  tagline = 'AI-Driven Outpatient Management System';
 
  // Placeholder text summarizing the system's purpose based on the LLD
  summaryText = `Welcome to ${this.hospitalName}, Your Trusted Partner in Healthcare Technology. We are an ${this.tagline} designed to streamline outpatient workflows in multi-specialty hospitals and clinics. Our system facilitates patient registration, appointment scheduling, consultation tracking, prescription management, and analytics for operational efficiency.`;
 
  features = [
    'Manages doctor schedules and real-time queue tracking.',
    'Records doctor notes, diagnoses, and generates e-prescriptions.',
    'Provides insights into patient flow, doctor utilization, and revenue.',
    'Ensures secure access via role-based access (Admin, Doctor, Receptionist).'
  ];
 
  // <<-- NEW PROPERTY FOR VISION -->>
  visionText = 'Our vision is to create a seamless healthcare experience for every user. We aim to bridge the gap between patients and healthcare providers, making it easier for you to access the care you need, when you need it.';
 
  // <<-- NEW DATA FOR "WHY CHOOSE US" -->>
  benefits = [
    {
      title: 'EFFICIENCY:',
      description: 'Streamlined Appointment Scheduling That Fits Into Your Busy Lifestyle.'
    },
    {
      title: 'CONVENIENCE:',
      description: 'Access To A Network Of Trusted Healthcare Professionals In Your Area.'
    },
    {
      title: 'PERSONALIZATION:',
      description: 'Tailored Recommendations And Reminders To Help You Stay On Top Of Your Health.'
    }
  ];
 
  // <<-- NEW DATA FOR REVIEWS -->>
  reviews = [
    { name: 'Sanjeev Raju', review: 'The scheduling system is incredibly efficient, cutting down my wait time significantly. Dr. James was excellent and the e-prescription was ready immediately. Highly recommend IntelliCare.', rating: 5, photo: 'https://t4.ftcdn.net/jpg/03/26/98/51/360_F_326985142_1aaKcEjMQW6ULp6oI9MYuv8lN9f8sFmj.jpg' },
    { name: 'Chand Basha', review: 'Booking appointments has never been easier. The doctor utilization reports must be working well because the clinic flow is seamless now. Great job!', rating: 5, photo: 'https://plus.unsplash.com/premium_photo-1672239496412-ab605befa53f?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8NXx8bWFsZXxlbnwwfHwwfHx8MA%3D%3D&fm=jpg&q=60&w=3000%27' },
    { name: 'Swamy', review: 'Detailed notes and diagnosis records are easily accessible. The staff were friendly, and the consultation process felt thorough and organized. Truly AI-driven care.', rating: 4, photo: 'https://img.freepik.com/premium-photo/stunning-portrait-handsome-indian-male-model-professional-studio-setting-fashion-advertising-projects_1247367-94485.jpg?w=360%27' },
    { name: 'Swetha', review: 'The secure access and role-based controls gave me confidence in my data privacy. My cardiologist appointment was on time, and the staff was professional.', rating: 5, photo: 'https://img.freepik.com/free-photo/young-woman-blue-sweater-autumn-park_1303-11368.jpg?semt=ais_hybrid&w=740&q=80%27' },
    { name: 'Vennela', review: 'I appreciated the queue tracking feature—it kept me informed. The system is intuitive for a non-tech user. Doctors are knowledgeable.', rating: 4, photo: 'https://img.freepik.com/premium-photo/portrait-happy-multi-ethnic-business_936686-27271.jpg?semt=ais_hybrid&w=740&q=80%27' },
    { name: 'Pawan Kalyan', review: 'Excellent experience from registration to prescription pickup. The doctors use the system efficiently, making the visit quick and pleasant.', rating: 5, photo: 'https://cdn.telugu360.com/wp-content/uploads/2023/04/pawan-kalyan.jpg' },
    { name: 'Ram pothineni', review: 'The prescription management is flawless; I received my medication without any paperwork. A fantastic outpatient system for a busy person!', rating: 5, photo: 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTeiMZSsYY0J4FhDDZb0aT-NWJxSZ1_Bkym28cqqokmcpsA86itlk1qCWfx3KYQxdkrSd8&usqp=CAU%27' },
    { name: 'Sandeep Kishan', review: 'Insights into patient flow clearly translate to a better experience. Waiting room time was minimal. Highly impressed with the management.', rating: 4, photo: 'https://www.behindwoods.com/tamil-actor/sundeep-kishan/sundeep-kishan-stills-photos-pictures-35.jpg' },
  ];
 
}
 
 
 
 
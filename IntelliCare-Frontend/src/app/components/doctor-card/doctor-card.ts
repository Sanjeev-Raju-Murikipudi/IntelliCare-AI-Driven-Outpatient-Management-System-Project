import { Component ,Input} from '@angular/core';

export interface Doctor {
  name: string;
  specialty: string;
  isAvailable: boolean;
  imageUrl: string;
}

@Component({
  selector: 'app-doctor-card',
  standalone: false,
  templateUrl: './doctor-card.html',
  styleUrl: './doctor-card.css',
})
export class DoctorCard {
@Input() doctor: Doctor | undefined;
}

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MyAppointments } from './my-appointments';
import { AppointmentService } from '../../services/appointment.service2';
import { of, throwError } from 'rxjs';

// Mock Bootstrap Modal and Toast
(globalThis as any).bootstrap = {
  Modal: class {
    constructor(public el: any) {}
    show() {}
    hide() {}
    static getInstance(el: any) {
      return new this(el);
    }
  },
  Toast: class {
    constructor(public el: any) {}
    show() {}
  }
};

describe('MyAppointments', () => {
  let component: MyAppointments;
  let fixture: ComponentFixture<MyAppointments>;
  let mockService: jasmine.SpyObj<AppointmentService>;

  beforeEach(async () => {
  mockService = jasmine.createSpyObj('AppointmentService', [
    'getMyAppointments',
    'cancelAppointment',
    'getDoctorAvailability',
    'rescheduleAppointment',
    'getQueue'
  ]);

  // ✅ Provide default return value to prevent subscribe error
  mockService.getMyAppointments.and.returnValue(of([]));

  await TestBed.configureTestingModule({
    declarations: [MyAppointments],
    providers: [{ provide: AppointmentService, useValue: mockService }]
  }).compileComponents();

  fixture = TestBed.createComponent(MyAppointments);
  component = fixture.componentInstance;
  fixture.detectChanges(); // triggers ngOnInit
});

  it('should create component', () => {
    expect(component).toBeTruthy();
  });

  it('should load appointments on init', () => {
    const mockAppointments = [{ doctorName: 'Dr. A', appointmentID: 1 }];
    mockService.getMyAppointments.and.returnValue(of(mockAppointments));

    component.loadMyAppointments();

    expect(component.appointments).toEqual(mockAppointments);
    expect(component.isLoading).toBeFalse();
  });

  it('should handle error when loading appointments', () => {
    mockService.getMyAppointments.and.returnValue(throwError(() => new Error('Error')));

    component.loadMyAppointments();

    expect(component.error).toBe('Oops You have no appointments');
    expect(component.isLoading).toBeFalse();
  });

  it('should open cancel modal', () => {
    const appointment = { appointmentID: 1 };
    spyOn(document, 'getElementById').and.returnValue(document.createElement('div'));
    component.openCancelModal(appointment);
    expect(component.appointmentToCancel).toBe(appointment);
  });

  it('should confirm cancel and reload appointments on success', () => {
    const appointment = { appointmentID: 1 };
    component.appointmentToCancel = appointment;
    mockService.cancelAppointment.and.returnValue(of({}));
    spyOn(component, 'loadMyAppointments');
    spyOn(component, 'showToast');
    spyOn(component, 'closeModal');

    component.confirmCancel();

    expect(component.loadMyAppointments).toHaveBeenCalled();
    expect(component.showToast).toHaveBeenCalledWith('Appointment cancelled successfully', 'success');
    expect(component.closeModal).toHaveBeenCalledWith('cancelModal');
  });

  it('should show error toast on cancel failure', () => {
    const appointment = { appointmentID: 1 };
    component.appointmentToCancel = appointment;
    mockService.cancelAppointment.and.returnValue(throwError(() => new Error('Error')));
    spyOn(component, 'showToast');
    spyOn(component, 'closeModal');

    component.confirmCancel();

    expect(component.showToast).toHaveBeenCalledWith('Failed to cancel appointment', 'danger');
    expect(component.closeModal).toHaveBeenCalledWith('cancelModal');
  });

  // Add similar tests for showReschedule, openRescheduleModal, confirmRescheduleAction, viewQueue, etc.
});

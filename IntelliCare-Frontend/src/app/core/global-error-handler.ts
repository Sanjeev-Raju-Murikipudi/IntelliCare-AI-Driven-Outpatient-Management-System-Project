import { ErrorHandler, Injectable } from '@angular/core';
 
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  handleError(error: any): void {
    console.error('🔥 Global Error:', error);
 
    // Optional: show toast or log to server
    // this.toastr.error('Something went wrong', 'Error');
  }
}
 
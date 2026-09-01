import { Injectable } from '@angular/core';
import Swal, { SweetAlertIcon, SweetAlertResult } from 'sweetalert2';

@Injectable({
  providedIn: 'root'
})
export class AlertService {

  // Global styling configuration matching TSLMS Tailwind theme
  private baseOptions = {
    customClass: {
      confirmButton: 'btn-primary px-4 py-2 mx-2',
      cancelButton: 'btn-secondary px-4 py-2 mx-2',
      popup: 'rounded-xl shadow-xl border border-gray-100',
      title: 'text-lg font-semibold text-gray-800',
      htmlContainer: 'text-sm text-gray-600'
    },
    buttonsStyling: false
  };

  /**
   * Show a simple success toast/alert
   */
  success(message: string, title: string = 'Success'): Promise<SweetAlertResult> {
    return Swal.fire({
      ...this.baseOptions,
      title,
      text: message,
      icon: 'success',
      timer: 2000,
      showConfirmButton: false
    });
  }

  /**
   * Show an error alert
   */
  error(message: string, title: string = 'Error'): Promise<SweetAlertResult> {
    return Swal.fire({
      ...this.baseOptions,
      title,
      text: message,
      icon: 'error',
      confirmButtonText: 'OK'
    });
  }

  /**
   * Show an info alert
   */
  info(message: string, title: string = 'Information'): Promise<SweetAlertResult> {
    return Swal.fire({
      ...this.baseOptions,
      title,
      html: message,
      icon: 'info',
      confirmButtonText: 'OK'
    });
  }

  /**
   * Show a message detailing something like an email (no icon, left aligned)
   */
  mail(messageHtml: string, title: string): Promise<SweetAlertResult> {
    return Swal.fire({
      ...this.baseOptions,
      title,
      html: `<div class="text-left mt-4 px-2">${messageHtml}</div>`,
      showCloseButton: true,
      confirmButtonText: 'Close',
      customClass: {
        ...this.baseOptions.customClass,
        title: 'text-xl font-bold text-gray-800 text-left border-b pb-3 mb-2',
        confirmButton: 'btn-secondary px-4 py-2 mt-4',
        htmlContainer: 'text-sm text-gray-600'
      }
    });
  }

  /**
   * Show a confirmation dialog. Returns a Promise that resolves to true if confirmed.
   */
  async confirm(text: string, title: string = 'Are you sure?', confirmButtonText: string = 'Yes', icon: SweetAlertIcon = 'warning'): Promise<boolean> {
    const result = await Swal.fire({
      ...this.baseOptions,
      title,
      text,
      icon,
      showCancelButton: true,
      confirmButtonText,
      cancelButtonText: 'Cancel'
    });
    return result.isConfirmed;
  }

  /**
   * Show a prompt dialog to capture user input
   */
  async prompt(title: string, text: string = '', inputPlaceholder: string = ''): Promise<string | null> {
    const result = await Swal.fire({
      ...this.baseOptions,
      title,
      text,
      input: 'text',
      inputPlaceholder,
      showCancelButton: true,
      confirmButtonText: 'Submit',
      cancelButtonText: 'Cancel',
      inputValidator: (value) => {
        if (!value) {
          return 'You need to write something!';
        }
        return null;
      }
    });
    return result.isConfirmed ? result.value : null;
  }
}

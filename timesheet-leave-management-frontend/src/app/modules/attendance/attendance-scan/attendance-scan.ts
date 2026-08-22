import {
  Component,
  OnInit,
  OnDestroy,
  Input,
  Output,
  EventEmitter,
  AfterViewInit,
} from '@angular/core';
import { NgIf } from '@angular/common';
import { AttendanceService } from '../../../services/attendance';
import { AlertService } from '../../../services/alert.service';

@Component({
  selector: 'app-attendance-scan',
  imports: [NgIf],
  templateUrl: './attendance-scan.html',
  styleUrl: './attendance-scan.css',
})
export class AttendanceScan implements OnInit, AfterViewInit, OnDestroy {
  /** 'clock-in' or 'clock-out' */
  @Input() scanType: 'clock-in' | 'clock-out' = 'clock-in';
  @Output() scanSuccess = new EventEmitter<void>();
  @Output() close = new EventEmitter<void>();

  scanning = false;
  processing = false;
  errorMessage = '';
  successMessage = '';

  private html5QrCode: any = null;

  constructor(
    private attendanceService: AttendanceService,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {}

  ngAfterViewInit(): void {
    this.startScanner();
  }

  startScanner(): void {
    this.errorMessage = '';
    this.scanning = true;

    // Dynamically import html5-qrcode
    import('html5-qrcode').then(({ Html5Qrcode }) => {
      this.html5QrCode = new Html5Qrcode('qr-reader-element');
      const config = { fps: 10, qrbox: { width: 350, height: 350 } };

      this.html5QrCode
        .start(
          { facingMode: 'environment' },
          config,
          (decodedText: string) => {
            this.onScanSuccess(decodedText);
          },
          () => {
            // scan error — ignore, keep scanning
          }
        )
        .catch(() => {
          this.scanning = false;
          this.errorMessage = 'Camera access denied or not available.';
        });
    }).catch(() => {
      this.scanning = false;
      this.errorMessage = 'QR scanner library not available.';
    });
  }

  private onScanSuccess(qrPayload: string): void {
    if (this.processing) return;
    this.processing = true;
    this.stopScanner();

    const request$ =
      this.scanType === 'clock-in'
        ? this.attendanceService.scanIn({ qrPayload })
        : this.attendanceService.scanOut({ qrPayload });

    request$.subscribe({
      next: (res) => {
        this.processing = false;
        if (res.success) {
          this.successMessage =
            this.scanType === 'clock-in'
              ? 'Clocked in successfully!'
              : 'Clocked out successfully!';
          this.scanSuccess.emit();
          setTimeout(() => this.close.emit(), 1500);
        } else {
          this.errorMessage = res.message || 'Scan failed.';
        }
      },
      error: (err) => {
        this.processing = false;
        this.errorMessage = err.error?.message || 'Scan failed. Please try again.';
      },
    });
  }

  stopScanner(): void {
    if (this.html5QrCode) {
      this.html5QrCode.stop().catch(() => {});
      this.html5QrCode = null;
    }
    this.scanning = false;
  }

  onClose(): void {
    this.stopScanner();
    this.close.emit();
  }

  ngOnDestroy(): void {
    this.stopScanner();
  }
}

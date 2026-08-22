import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { AuthService } from './auth';
import { NotificationService } from './notification';

@Injectable({
  providedIn: 'root',
})
export class SignalrService implements OnDestroy {
  private connection: any = null;
  private connected$ = new BehaviorSubject<boolean>(false);

  public isConnected$ = this.connected$.asObservable();

  constructor(
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  /**
   * Start the SignalR connection to the notification hub.
   * Uses a type-safe dynamic import — gracefully degrades if the package
   * is not installed (the app works without real-time notifications).
   */
  startConnection(): void {
    if (this.connection) return;

    const token = this.authService.getToken();
    if (!token) return;

    // Attempt to load @microsoft/signalr at runtime.
    // If the package is not installed the catch block silently skips real-time.
    this.loadSignalR()
      .then((signalR) => {
        if (!signalR) return;

        this.connection = new signalR.HubConnectionBuilder()
          .withUrl('/hubs/notifications', {
            accessTokenFactory: () => this.authService.getToken() ?? '',
          })
          .withAutomaticReconnect()
          .configureLogging(signalR.LogLevel.Warning)
          .build();

        this.connection.on('ReceiveNotification', () => {
          this.notificationService.getUnreadCount().subscribe();
        });

        this.connection.onreconnected(() => this.connected$.next(true));
        this.connection.onclose(() => this.connected$.next(false));

        this.connection
          .start()
          .then(() => this.connected$.next(true))
          .catch(() => this.connected$.next(false));
      })
      .catch(() => {
        // Package not available — real-time notifications disabled
      });
  }

  private async loadSignalR(): Promise<any> {
    try {
      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore — optional peer dependency
      return await import(/* webpackIgnore: true */ '@microsoft/signalr');
    } catch {
      return null;
    }
  }

  stopConnection(): void {
    if (this.connection) {
      this.connection.stop();
      this.connection = null;
      this.connected$.next(false);
    }
  }

  ngOnDestroy(): void {
    this.stopConnection();
  }
}

import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf, NgClass, DatePipe } from '@angular/common';
import { NotificationService } from '../../../services/notification';
import { Notification } from '../../../models/notification';
import { AlertService } from '../../../services/alert.service';

@Component({
  selector: 'app-notification-list',
  imports: [NgFor, NgIf, NgClass, DatePipe],
  templateUrl: './notification-list.html',
  styleUrl: './notification-list.css'
})
export class NotificationList implements OnInit {
  notifications: Notification[] = []; loading = true; errorMessage = '';

  constructor(private notifService: NotificationService, private alertService: AlertService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.notifService.getNotifications().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) {
          this.notifications = res.data?.items || [];
        } else {
          this.errorMessage = res.message;
        }
      },
      error: (err) => { this.loading = false; this.errorMessage = err.error?.message || 'Failed.'; }
    });
  }

  markRead(n: Notification): void {
    // Show the detailed message like a mail
    const formattedBody = n.message ? n.message.replace(/\n/g, '<br>') : 'No details provided.';
    this.alertService.mail(formattedBody, n.title);

    if (n.isRead) return;
    this.notifService.markRead(n.id).subscribe({ next: () => { n.isRead = true; } });
  }

  markAllRead(): void {
    this.notifService.markAllRead().subscribe({ next: () => { this.notifications.forEach(n => n.isRead = true); } });
  }

  get unreadCount(): number { return this.notifications.filter(n => !n.isRead).length; }
}

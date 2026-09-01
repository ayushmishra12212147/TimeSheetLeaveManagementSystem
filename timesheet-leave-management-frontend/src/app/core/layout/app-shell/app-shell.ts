import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Sidebar } from '../sidebar/sidebar';
import { Topbar } from '../topbar/topbar';
import { SignalrService } from '../../../services/signalr';

@Component({
  selector: 'app-app-shell',
  imports: [RouterOutlet, Sidebar, Topbar],
  templateUrl: './app-shell.html',
  styleUrl: './app-shell.css'
})
export class AppShell implements OnInit {

  sidebarCollapsed = false;

  constructor(private signalrService: SignalrService) {}

  ngOnInit(): void {
    // Start real-time notification connection when the shell loads
    this.signalrService.startConnection();
  }

  onToggleSidebar(): void {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }
}

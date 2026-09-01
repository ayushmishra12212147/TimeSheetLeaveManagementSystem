import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { NgFor, NgIf } from '@angular/common';
import { AuthService } from '../../../services/auth';
import { NAV_ITEMS, NavItem } from '../../../constants/permissions';
import { Role } from '../../../constants/roles';

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive, NgFor, NgIf],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css'
})
export class Sidebar implements OnInit {

  @Input() collapsed = false;
  @Output() collapsedChange = new EventEmitter<boolean>();

  sections: { name: string; items: NavItem[] }[] = [];
  userRole: string = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.userRole = this.authService.getUserRole() || Role.Employee;
    this.buildSections();
  }

  private buildSections(): void {
    const visibleItems = NAV_ITEMS.filter(item =>
      item.roles.includes(this.userRole as Role)
    );

    const sectionMap = new Map<string, NavItem[]>();
    for (const item of visibleItems) {
      const list = sectionMap.get(item.section) || [];
      list.push(item);
      sectionMap.set(item.section, list);
    }

    this.sections = Array.from(sectionMap.entries()).map(
      ([name, items]) => ({ name, items })
    );
  }

  logout(): void {
    this.authService.logout().subscribe({
      next: () => this.router.navigate(['/login']),
      error: () => {
        this.authService.clearSession();
        this.router.navigate(['/login']);
      }
    });
  }
}
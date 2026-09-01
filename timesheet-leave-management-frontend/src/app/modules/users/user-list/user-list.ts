import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf, NgClass } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserService } from '../../../services/user';
import { DepartmentService } from '../../../services/department';
import { User } from '../../../models/user';
import { Department } from '../../../models/department';
import { AlertService } from '../../../services/alert.service';

@Component({
  selector: 'app-user-list',
  imports: [NgFor, NgIf, NgClass, ReactiveFormsModule, RouterLink],
  templateUrl: './user-list.html',
  styleUrl: './user-list.css'
})
export class UserList implements OnInit {
  users: User[] = []; departments: Department[] = []; managers: User[] = [];
  loading = true; errorMessage = '';
  showForm = false; editingId: string | null = null; userForm!: FormGroup; formLoading = false;

  constructor(private fb: FormBuilder, private userService: UserService, private deptService: DepartmentService, private alertService: AlertService) {}

  ngOnInit(): void { this.initForm(); this.loadDepartments(); this.loadUsers();
    
    //  this.userForm.get('gender')?.valueChanges.subscribe(value => {
    // alert('Selected Gender: ' + value);
    // });



  }

  private initForm(): void {
    this.userForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      role: ['Employee', [Validators.required]],
      gender: ['', [Validators.required]],
      departmentId: ['', [Validators.required]],
      managerId: [''],
    });


  }

  loadUsers(): void {
    this.loading = true;
    this.userService.getUsers().subscribe({
      next: (res) => { this.loading = false; if (res.success) { this.users = res.data || []; this.managers = this.users.filter(u => u.role === 'Manager' || u.role === 'HRAdmin'); } else this.errorMessage = res.message; },
      error: (err) => { this.loading = false; this.errorMessage = err.error?.message || 'Failed to load.'; }
    });
  }

  private loadDepartments(): void {
    this.deptService.getDepartments().subscribe({ next: (res) => { if (res.success) this.departments = res.data || []; } });
  }

  openCreateForm(): void { this.editingId = null; this.userForm.reset({ role: 'Employee', managerId: '' }); this.showForm = true; }

  openEditForm(user: User): void {
    this.editingId = user.id;
    this.userForm.patchValue({
      fullName: user.fullName,
      email: user.email,
      role: user.role,
      gender: user.gender || '',
      departmentId: user.departmentId,
      managerId: user.managerId || '',
    });
    this.showForm = true;
  }

  closeForm(): void { this.showForm = false; this.editingId = null; }

  submitForm(): void {
    

    if (this.userForm.invalid) { this.userForm.markAllAsTouched(); return; }
    this.formLoading = true;
    const val = this.userForm.value;
    const request$ = this.editingId
      ? this.userService.updateUser(this.editingId, { fullName: val.fullName, email: val.email, role: val.role, gender: val.gender, departmentId: val.departmentId })
      : this.userService.createUser({ fullName: val.fullName, email: val.email, role: val.role, gender: val.gender, departmentId: val.departmentId, managerId: val.managerId || undefined });
    request$.subscribe({ next: () => { this.formLoading = false; this.closeForm(); this.loadUsers(); }, error: (err) => { this.formLoading = false; this.alertService.error(err.error?.message || 'Failed.'); } });
  }

  isFieldInvalid(f: string): boolean { const c = this.userForm.get(f); return !!(c && c.invalid && c.touched); }
}

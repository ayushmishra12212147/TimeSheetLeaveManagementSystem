import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DepartmentService } from '../../../services/department';
import { Department } from '../../../models/department';
import { AlertService } from '../../../services/alert.service';

@Component({
  selector: 'app-department-list',
  imports: [NgFor, NgIf, ReactiveFormsModule],
  templateUrl: './department-list.html',
  styleUrl: './department-list.css'
})
export class DepartmentList implements OnInit {
  departments: Department[] = []; loading = true; errorMessage = '';
  showForm = false; editingId: string | null = null; deptForm!: FormGroup; formLoading = false;

  constructor(private fb: FormBuilder, private deptService: DepartmentService, private alertService: AlertService) {}

  ngOnInit(): void { this.deptForm = this.fb.group({ name: ['', [Validators.required, Validators.minLength(2)]] }); this.loadDepts(); }

  loadDepts(): void {
    this.loading = true;
    this.deptService.getDepartments().subscribe({
      next: (res) => { this.loading = false; if (res.success) this.departments = res.data || []; else this.errorMessage = res.message; },
      error: (err) => { this.loading = false; this.errorMessage = err.error?.message || 'Failed.'; }
    });
  }

  openCreate(): void { this.editingId = null; this.deptForm.reset(); this.showForm = true; }
  openEdit(d: Department): void { this.editingId = d.id; this.deptForm.patchValue({ name: d.name }); this.showForm = true; }
  closeForm(): void { this.showForm = false; this.editingId = null; }

  submit(): void {
    if (this.deptForm.invalid) { this.deptForm.markAllAsTouched(); return; }
    this.formLoading = true;
    const req$ = this.editingId ? this.deptService.updateDepartment(this.editingId, this.deptForm.value) : this.deptService.createDepartment(this.deptForm.value);
    req$.subscribe({ next: () => { this.formLoading = false; this.closeForm(); this.loadDepts(); }, error: (err) => { this.formLoading = false; this.alertService.error(err.error?.message || 'Failed.'); } });
  }


}

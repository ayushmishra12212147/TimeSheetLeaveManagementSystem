import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf, NgClass, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProjectService } from '../../../services/project';
import { Project } from '../../../models/project';
import { AlertService } from '../../../services/alert.service';

@Component({
  selector: 'app-project-list',
  imports: [NgFor, NgIf, NgClass, DatePipe, ReactiveFormsModule],
  templateUrl: './project-list.html',
  styleUrl: './project-list.css'
})
export class ProjectList implements OnInit {

  projects: Project[] = [];
  loading = true;
  errorMessage = '';
  showForm = false;
  editingId: string | null = null;
  projectForm!: FormGroup;
  formLoading = false;

  constructor(
    private fb: FormBuilder,
    private projectService: ProjectService,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadProjects();
  }

  private initForm(): void {
    this.projectForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      code: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(10)]],
      description: [''],
      isActive: [true],
    });
  }

  loadProjects(): void {
    this.loading = true;
    this.projectService.getProjects().subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) this.projects = res.data || [];
        else this.errorMessage = res.message;
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to load projects.';
      }
    });
  }

  openCreateForm(): void {
    this.editingId = null;
    this.projectForm.reset({ isActive: true });
    this.showForm = true;
  }

  openEditForm(project: Project): void {
    this.editingId = project.id;
    this.projectForm.patchValue({
      name: project.name,
      code: project.code,
      description: project.description || '',
      isActive: project.isActive
    });
    this.showForm = true;
  }

  closeForm(): void {
    this.showForm = false;
    this.editingId = null;
  }

  submitForm(): void {
    if (this.projectForm.invalid) {
      this.projectForm.markAllAsTouched();
      return;
    }

    this.formLoading = true;

    const request$ = this.editingId
      ? this.projectService.updateProject(this.editingId, this.projectForm.value)
      : this.projectService.createProject(this.projectForm.value);

    request$.subscribe({
      next: () => {
        this.formLoading = false;
        this.closeForm();
        this.loadProjects();
      },
      error: (err) => {
        this.formLoading = false;
        this.alertService.error(err.error?.message || 'Operation failed.');
      }
    });
  }



  isFieldInvalid(field: string): boolean {
    const control = this.projectForm.get(field);
    return !!(control && control.invalid && control.touched);
  }
}

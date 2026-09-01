using Microsoft.EntityFrameworkCore;
using TimesheetService.Data;
using TimesheetService.DTOs;
using TimesheetService.Exceptions;
using TimesheetService.Models;

namespace TimesheetService.Services
{
    public class ProjectService : IProjectService
    {
        private readonly TimesheetDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;

        public ProjectService(TimesheetDbContext dbContext, ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
        }

        public async Task<IReadOnlyCollection<ProjectResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var isHrAdmin = string.Equals(_currentUserService.GetRole(), "HRAdmin", StringComparison.OrdinalIgnoreCase);

            var query = _dbContext.Projects.AsNoTracking();
            if (!isHrAdmin)
            {
                query = query.Where(x => x.IsActive);
            }

            var projects = await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
            return projects.Select(MapProject).ToList();
        }

        public async Task<ProjectResponseDto> CreateAsync(CreateProjectDto dto, CancellationToken cancellationToken = default)
        {
            EnsureHrAdmin();
            await EnsureCodeAvailableAsync(dto.Code, null, cancellationToken);

            var now = DateTime.UtcNow;
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Code = dto.Code.Trim().ToUpperInvariant(),
                Description = NormalizeOptionalText(dto.Description),
                IsActive = dto.IsActive,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _dbContext.Projects.Add(project);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return MapProject(project);
        }

        public async Task<ProjectResponseDto> UpdateAsync(Guid id, UpdateProjectDto dto, CancellationToken cancellationToken = default)
        {
            EnsureHrAdmin();

            var project = await GetProjectAsync(id, cancellationToken);
            await EnsureCodeAvailableAsync(dto.Code, id, cancellationToken);

            project.Name = dto.Name.Trim();
            project.Code = dto.Code.Trim().ToUpperInvariant();
            project.Description = NormalizeOptionalText(dto.Description);
            project.IsActive = dto.IsActive;
            project.UpdatedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return MapProject(project);
        }

        public async Task<ProjectResponseDto> ToggleActiveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            EnsureHrAdmin();

            var project = await GetProjectAsync(id, cancellationToken);
            project.IsActive = !project.IsActive;
            project.UpdatedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return MapProject(project);
        }

        private void EnsureHrAdmin()
        {
            if (!string.Equals(_currentUserService.GetRole(), "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Only HRAdmin can manage projects.");
            }
        }

        private async Task<Project> GetProjectAsync(Guid id, CancellationToken cancellationToken)
        {
            var project = await _dbContext.Projects.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (project == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "Project not found.");
            }

            return project;
        }

        private async Task EnsureCodeAvailableAsync(string code, Guid? currentId, CancellationToken cancellationToken)
        {
            var normalizedCode = code.Trim().ToUpperInvariant();
            var exists = await _dbContext.Projects.AnyAsync(
                x => x.Code == normalizedCode && (!currentId.HasValue || x.Id != currentId.Value),
                cancellationToken);

            if (exists)
            {
                throw new ApiException(StatusCodes.Status409Conflict, $"Project code {normalizedCode} already exists.");
            }
        }

        private static string? NormalizeOptionalText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static ProjectResponseDto MapProject(Project project)
        {
            return new ProjectResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                Code = project.Code,
                Description = project.Description,
                IsActive = project.IsActive,
                UpdatedAtUtc = project.UpdatedAtUtc
            };
        }
    }
}

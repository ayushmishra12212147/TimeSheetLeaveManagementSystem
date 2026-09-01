using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetService.DTOs;
using TimesheetService.Helpers;
using TimesheetService.Services;

namespace TimesheetService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/projects")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var projects = await _projectService.GetAllAsync(cancellationToken);
            return Ok(new ApiResponse<IReadOnlyCollection<ProjectResponseDto>>(projects, "Projects fetched successfully."));
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectDto dto, CancellationToken cancellationToken)
        {
            var project = await _projectService.CreateAsync(dto, cancellationToken);
            return Ok(new ApiResponse<ProjectResponseDto>(project, "Project created successfully."));
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectDto dto, CancellationToken cancellationToken)
        {
            var project = await _projectService.UpdateAsync(id, dto, cancellationToken);
            return Ok(new ApiResponse<ProjectResponseDto>(project, "Project updated successfully."));
        }

        [Authorize(Roles = "HRAdmin")]
        [HttpPost("{id:guid}/toggle")]
        public async Task<IActionResult> ToggleActive(Guid id, CancellationToken cancellationToken)
        {
            var project = await _projectService.ToggleActiveAsync(id, cancellationToken);
            return Ok(new ApiResponse<ProjectResponseDto>(project, "Project status updated successfully."));
        }
    }
}

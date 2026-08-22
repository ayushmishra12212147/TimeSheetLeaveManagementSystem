using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using TimesheetService.Clients;
using TimesheetService.Data;
using TimesheetService.Messaging;
using TimesheetService.Middleware;
using TimesheetService.Options;
using TimesheetService.Services;
using TimesheetService.Validators;
using Shared.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<ServiceEndpointsOptions>(builder.Configuration.GetSection(ServiceEndpointsOptions.SectionName));

builder.Services.AddDbContext<TimesheetDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TimesheetDb")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITimesheetConfigService, TimesheetConfigService>();
builder.Services.AddScoped<ITimesheetEntryService, TimesheetEntryService>();
builder.Services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();
builder.Services.AddHostedService<TimesheetAutoApproveWorker>();
builder.Services.AddHostedService<ManagerAssignmentChangedConsumer>();

builder.Services.AddHttpClient<IEmployeeDirectoryClient, EmployeeDirectoryClient>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateTimesheetEntryDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.AddIndianStandardTimeConverters());

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT settings are missing.");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter JWT access token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Timesheet Service API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", null),
            new List<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TimesheetDbContext>();
    dbContext.Database.Migrate();
    await TimesheetDbSeeder.SeedAsync(dbContext);
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

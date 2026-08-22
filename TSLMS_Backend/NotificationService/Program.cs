using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using NotificationService.Data;
using NotificationService.Hubs;
using NotificationService.Messaging;
using NotificationService.Middleware;
using NotificationService.Options;
using NotificationService.Services;
using NotificationService.Validators;
using Shared.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.Configure<FrontendUrlOptions>(builder.Configuration.GetSection(FrontendUrlOptions.SectionName));

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NotificationDb")));

builder.Services.AddScoped<INotificationService, NotificationService.Services.NotificationService>();
builder.Services.AddScoped<INotificationDispatchService, NotificationDispatchService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<ISignalRNotifier, SignalRNotifier>();

builder.Services.AddHostedService<UserCreatedConsumer>();
builder.Services.AddHostedService<PasswordResetRequestedConsumer>();
builder.Services.AddHostedService<LeaveSubmittedConsumer>();
builder.Services.AddHostedService<LeaveApprovedConsumer>();
builder.Services.AddHostedService<LeaveRejectedConsumer>();
builder.Services.AddHostedService<LeaveCancelledConsumer>();
builder.Services.AddHostedService<TimesheetSubmittedConsumer>();
builder.Services.AddHostedService<TimesheetApprovedConsumer>();
builder.Services.AddHostedService<TimesheetRejectedConsumer>();
builder.Services.AddHostedService<AttendanceClockInConsumer>();
builder.Services.AddHostedService<AttendanceClockOutConsumer>();
builder.Services.AddHostedService<ReportRequestedConsumer>();
builder.Services.AddHostedService<ReportApprovedConsumer>();
builder.Services.AddHostedService<ReportRejectedConsumer>();

builder.Services.AddValidatorsFromAssemblyContaining<NotificationQueryDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.AddSignalR()
    .AddJsonProtocol(options => options.PayloadSerializerOptions.AddIndianStandardTimeConverters());
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

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrWhiteSpace(accessToken) &&
                    path.StartsWithSegments("/hubs/notifications"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
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
        Title = "Notification Service API",
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
    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    dbContext.Database.Migrate();
    await NotificationDbSeeder.SeedAsync(dbContext);
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();

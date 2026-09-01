using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.DTOs;
using NotificationService.Exceptions;
using NotificationService.Models;

namespace NotificationService.Services
{
    public class NotificationService : INotificationService
    {
        private readonly NotificationDbContext _dbContext;

        public NotificationService(NotificationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<NotificationListResponseDto> GetNotificationsAsync(Guid userId, NotificationQueryDto query, CancellationToken cancellationToken)
        {
            var notificationQuery = _dbContext.Notifications
                .AsNoTracking()
                .Where(x => x.RecipientUserId == userId);

            if (query.IsRead.HasValue)
            {
                notificationQuery = notificationQuery.Where(x => x.IsRead == query.IsRead.Value);
            }

            var totalCount = await notificationQuery.CountAsync(cancellationToken);
            var notifications = await notificationQuery
                .OrderByDescending(x => x.CreatedAtUtc)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);
            var items = notifications
                .Select(MapNotification)
                .ToList();

            return new NotificationListResponseDto
            {
                Items = items,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken)
        {
            var notification = await _dbContext.Notifications
                .FirstOrDefaultAsync(x => x.Id == notificationId && x.RecipientUserId == userId, cancellationToken);

            if (notification == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "Notification not found.");
            }

            if (notification.IsRead)
            {
                return;
            }

            notification.IsRead = true;
            notification.ReadAtUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken)
        {
            var notifications = await _dbContext.Notifications
                .Where(x => x.RecipientUserId == userId && !x.IsRead)
                .ToListAsync(cancellationToken);

            if (notifications.Count == 0)
            {
                return;
            }

            var now = DateTime.UtcNow;
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAtUtc = now;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<UnreadCountDto> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken)
        {
            var unreadCount = await _dbContext.Notifications
                .CountAsync(x => x.RecipientUserId == userId && !x.IsRead, cancellationToken);

            return new UnreadCountDto
            {
                UnreadCount = unreadCount
            };
        }

        public async Task<NotificationPreferenceDto> GetPreferencesAsync(Guid userId, CancellationToken cancellationToken)
        {
            var preference = await GetOrCreatePreferenceAsync(userId, cancellationToken);
            return MapPreference(preference);
        }

        public async Task<NotificationPreferenceDto> UpdatePreferencesAsync(Guid userId, UpdateNotificationPreferenceDto dto, CancellationToken cancellationToken)
        {
            var preference = await GetOrCreatePreferenceAsync(userId, cancellationToken);
            preference.EmailNotificationsEnabled = dto.EmailNotificationsEnabled;
            preference.InAppNotificationsEnabled = dto.InAppNotificationsEnabled;
            preference.UpdatedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return MapPreference(preference);
        }

        public async Task<IReadOnlyCollection<NotificationTemplateDto>> GetTemplatesAsync(CancellationToken cancellationToken)
        {
            var templates = await _dbContext.NotificationTemplates
                .AsNoTracking()
                .OrderBy(x => x.EventKey)
                .ThenBy(x => x.Channel)
                .ToListAsync(cancellationToken);

            return templates
                .Select(x => new NotificationTemplateDto
                {
                    Id = x.Id,
                    EventKey = x.EventKey,
                    Channel = x.Channel.ToString(),
                    Name = x.Name,
                    SubjectTemplate = x.SubjectTemplate,
                    BodyTemplate = x.BodyTemplate,
                    IsCritical = x.IsCritical,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    UpdatedAtUtc = x.UpdatedAtUtc
                })
                .ToList();
        }

        public async Task<NotificationTemplateDto> UpdateTemplateAsync(Guid id, UpdateNotificationTemplateDto dto, CancellationToken cancellationToken)
        {
            var template = await _dbContext.NotificationTemplates.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (template == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "Notification template not found.");
            }

            if (template.IsCritical && !dto.IsActive)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Critical notification templates cannot be disabled.");
            }

            template.Name = dto.Name.Trim();
            template.SubjectTemplate = dto.SubjectTemplate.Trim();
            template.BodyTemplate = dto.BodyTemplate.Trim();
            template.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            template.IsActive = dto.IsActive;
            template.UpdatedAtUtc = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new NotificationTemplateDto
            {
                Id = template.Id,
                EventKey = template.EventKey,
                Channel = template.Channel.ToString(),
                Name = template.Name,
                SubjectTemplate = template.SubjectTemplate,
                BodyTemplate = template.BodyTemplate,
                IsCritical = template.IsCritical,
                Description = template.Description,
                IsActive = template.IsActive,
                UpdatedAtUtc = template.UpdatedAtUtc
            };
        }

        private async Task<NotificationPreference> GetOrCreatePreferenceAsync(Guid userId, CancellationToken cancellationToken)
        {
            var preference = await _dbContext.NotificationPreferences
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

            if (preference != null)
            {
                return preference;
            }

            preference = new NotificationPreference
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EmailNotificationsEnabled = false,
                InAppNotificationsEnabled = true,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _dbContext.NotificationPreferences.Add(preference);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return preference;
        }

        private static NotificationPreferenceDto MapPreference(NotificationPreference preference)
        {
            return new NotificationPreferenceDto
            {
                EmailNotificationsEnabled = preference.EmailNotificationsEnabled,
                InAppNotificationsEnabled = preference.InAppNotificationsEnabled,
                UpdatedAtUtc = preference.UpdatedAtUtc
            };
        }

        public static NotificationResponseDto MapNotification(Notification notification)
        {
            return new NotificationResponseDto
            {
                Id = notification.Id,
                Type = notification.Type.ToString(),
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                IsImportant = notification.IsImportant,
                CreatedAtUtc = notification.CreatedAtUtc,
                ReadAtUtc = notification.ReadAtUtc,
                ActionUrl = notification.ActionUrl,
                EntityType = notification.EntityType,
                EntityId = notification.EntityId
            };
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace AutoNext.Platform.AccessControl.API.Models.DTOs
{
    public class PermissionResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int RoleCount { get; set; }
    }

    public class PermissionListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class AssignPermissionDto
    {
        [Required]
        public Guid RoleId { get; set; }

        [Required]
        public List<Guid> PermissionIds { get; set; } = new List<Guid>();
    }

    public class UserPermissionDto
    {
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new List<string>();
        public List<RolePermissionSummaryDto> RolePermissions { get; set; } = new List<RolePermissionSummaryDto>();
    }

    public class RolePermissionSummaryDto
    {
        public string RoleName { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new List<string>();
    }
}

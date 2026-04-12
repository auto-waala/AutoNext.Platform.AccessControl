namespace AutoNext.Platform.AccessControl.API.Models.DTOs
{
    public class RoleResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string RoleType { get; set; } = string.Empty;
        public bool IsSystemRole { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
        public int UserCount { get; set; }
    }
}

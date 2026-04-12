using System.ComponentModel.DataAnnotations;

namespace AutoNext.Platform.AccessControl.API.Models.DTOs
{
    public class UserUpdateDto
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public string? UserType { get; set; }

        public bool? IsActive { get; set; }

        public List<Guid>? RoleIds { get; set; }

        public Dictionary<string, object>? Metadata { get; set; }
    }
}

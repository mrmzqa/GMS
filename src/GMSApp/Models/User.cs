using System;
using System.ComponentModel.DataAnnotations;

namespace GarageApp.Models
{
    public enum UserRole
    {
        User,
        Admin,
        SuperAdmin
    }

    public class User
    {
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.User;

        public bool IsActive { get; set; } = true;
    }
}
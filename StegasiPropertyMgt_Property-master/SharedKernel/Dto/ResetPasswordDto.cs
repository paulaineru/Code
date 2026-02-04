using System.ComponentModel.DataAnnotations;

namespace SharedKernel.Dto
{
    public class ResetPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
} 
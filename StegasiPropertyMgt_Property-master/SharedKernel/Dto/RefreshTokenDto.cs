

using System.ComponentModel.DataAnnotations;

namespace  SharedKernel.Dto
{
    /// <summary>
    /// Data transfer object for refresh token requests
    /// </summary>
    public class RefreshTokenDto
    {
        /// <summary>
        /// The refresh token string
        /// </summary>
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// The access token that needs to be refreshed
        /// </summary>
        [Required(ErrorMessage = "Access token is required")]
        public string AccessToken { get; set; } = string.Empty;
    }
} 
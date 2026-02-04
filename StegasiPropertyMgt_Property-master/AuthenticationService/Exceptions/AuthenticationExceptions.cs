using System;

namespace AuthenticationService.Exceptions
{
    public class AuthenticationException : Exception
    {
        public string ErrorCode { get; }
        public string CorrelationId { get; }

        public AuthenticationException(string message, string errorCode, string correlationId)
            : base(message)
        {
            ErrorCode = errorCode;
            CorrelationId = correlationId;
        }
    }

    public class InvalidCredentialsException : AuthenticationException
    {
        public InvalidCredentialsException(string correlationId)
            : base("Invalid username or password", "AUTH_001", correlationId)
        {
        }
    }

    public class UserNotFoundException : AuthenticationException
    {
        public UserNotFoundException(string correlationId)
            : base("User not found", "AUTH_002", correlationId)
        {
        }
    }

    public class UserAlreadyExistsException : AuthenticationException
    {
        public UserAlreadyExistsException(string correlationId)
            : base("User already exists", "AUTH_003", correlationId)
        {
        }
    }

    public class InvalidTokenException : AuthenticationException
    {
        public InvalidTokenException(string correlationId)
            : base("Invalid or expired token", "AUTH_004", correlationId)
        {
        }
    }

    public class RefreshTokenExpiredException : AuthenticationException
    {
        public RefreshTokenExpiredException(string correlationId)
            : base("Refresh token has expired", "AUTH_005", correlationId)
        {
        }
    }

    public class AccountLockedException : AuthenticationException
    {
        public AccountLockedException(string correlationId)
            : base("Account is locked due to multiple failed login attempts", "AUTH_006", correlationId)
        {
        }
    }

    public class PasswordResetRequiredException : AuthenticationException
    {
        public PasswordResetRequiredException(string correlationId)
            : base("Password reset is required", "AUTH_007", correlationId)
        {
        }
    }
} 
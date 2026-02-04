using System;

namespace SharedKernel.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException()
            : base("The requested resource was not found.")
        {
        }

        public NotFoundException(string message)
            : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public NotFoundException(string resourceType, string resourceId)
            : base($"The {resourceType} with ID {resourceId} was not found.")
        {
        }
    }
} 
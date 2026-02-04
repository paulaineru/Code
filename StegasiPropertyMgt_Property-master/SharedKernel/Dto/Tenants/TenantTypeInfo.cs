namespace SharedKernel.Dto.Tenants
{
    /// <summary>
    /// DTO for tenant type information returned by the API
    /// </summary>
    public class TenantTypeInfo
    {
        /// <summary>
        /// Numeric ID of the tenant type
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the tenant type
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the tenant type
        /// </summary>
        public string Description { get; set; }
    }
} 
namespace SharedKernel.Dto
{
    public class PropertyApprovalResponseDto
    {
        public Guid PropertyId { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public Guid ApprovedBy { get; set; }
        public DateTime ApprovedAt { get; set; }
        public string NextStage { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
} 
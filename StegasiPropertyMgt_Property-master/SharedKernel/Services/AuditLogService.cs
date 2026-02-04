// SharedKernel/Services/AuditLogService.cs
using Elasticsearch.Net;
using Nest;
using System;
using System.Threading.Tasks;
using SharedKernel.Services;
using SharedKernel.Models;
using Microsoft.Extensions.Configuration;

namespace SharedKernel.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IElasticClient _elasticClient;
        private readonly string _indexPrefix;

        public AuditLogService(IElasticClient elasticClient, IConfiguration configuration)
        {
            _elasticClient = elasticClient;
            _indexPrefix = configuration["Elasticsearch:IndexPrefix"] ?? "audit-logs";
        }

        public async Task RecordActionAsync(string action, Guid? entityId, Guid? userId, string details, Guid moduleId)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                Action = action,
                EntityId = entityId,
                UserId = userId,
                Details = details,
                ModuleId = moduleId,
                Timestamp = DateTime.UtcNow
            };

            var indexName = $"{_indexPrefix}-{DateTime.UtcNow:yyyy.MM.dd}";
            await _elasticClient.IndexAsync(auditLog, idx => idx.Index(indexName));
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(Guid? entityId = null, Guid? userId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var mustClauses = new List<QueryContainer>();

            if (entityId.HasValue)
            {
                mustClauses.Add(new TermQuery
                {
                    Field = "entityId",
                    Value = entityId.Value
                });
            }

            if (userId.HasValue)
            {
                mustClauses.Add(new TermQuery
                {
                    Field = "userId",
                    Value = userId.Value
                });
            }

            if (startDate.HasValue || endDate.HasValue)
            {
                var rangeQuery = new DateRangeQuery
                {
                    Field = "timestamp"
                };

                if (startDate.HasValue)
                    rangeQuery.GreaterThanOrEqualTo = startDate.Value;

                if (endDate.HasValue)
                    rangeQuery.LessThanOrEqualTo = endDate.Value;

                mustClauses.Add(rangeQuery);
            }

            var searchRequest = new SearchRequest<AuditLog>
            {
                Query = new BoolQuery
                {
                    Must = mustClauses
                }
            };

            var response = await _elasticClient.SearchAsync<AuditLog>(searchRequest);
            return response.Documents;
        }
    }
}
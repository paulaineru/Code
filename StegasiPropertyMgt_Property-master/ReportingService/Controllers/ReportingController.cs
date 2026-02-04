// ReportingService/Controllers/ReportController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReportingService.Data;
using ReportingService.Repository;
using SharedKernel.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ReportingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ReportingDbContext _context;
        private readonly IPropertyRepository _propertyRepository;

        public ReportController(ReportingDbContext context, IPropertyRepository propertyRepository)
        {
            _context = context;
            _propertyRepository = propertyRepository;
        }

        [HttpGet("occupancy-report")]
        public async Task<IActionResult> GetOccupancyReport([FromQuery] Guid? propertyId = null)
        {
            var properties = await _propertyRepository.GetAllAsync();
            if (propertyId.HasValue)
            {
                properties = properties.Where(p => p.Id == propertyId.Value).ToList();
            }

            var occupancyReports = properties.Select(property =>
            {
                int totalUnits = 1;
                int occupiedUnits = 0;

                // If property has a Units collection
                var unitsProp = property.GetType().GetProperty("Units");
                if (unitsProp != null)
                {
                    var units = unitsProp.GetValue(property) as System.Collections.IEnumerable;
                    if (units != null)
                    {
                        var unitList = units.Cast<object>().ToList();
                        totalUnits = unitList.Count;
                        occupiedUnits = unitList.Count(u =>
                        {
                            var tenantIdProp = u.GetType().GetProperty("TenantId");
                            return tenantIdProp != null && tenantIdProp.GetValue(u) != null;
                        });
                    }
                }
                // If property has a Clusters collection
                else
                {
                    var clustersProp = property.GetType().GetProperty("Clusters");
                    if (clustersProp != null)
                    {
                        var clusters = clustersProp.GetValue(property) as System.Collections.IEnumerable;
                        if (clusters != null)
                        {
                            var clusterList = clusters.Cast<object>().ToList();
                            totalUnits = clusterList.Count;
                            occupiedUnits = clusterList.Count(c =>
                            {
                                var tenantIdProp = c.GetType().GetProperty("TenantId");
                                return tenantIdProp != null && tenantIdProp.GetValue(c) != null;
                            });
                        }
                    }
                    // Fallback for single-unit property
                    else if (property.GetType().GetProperty("TenantId")?.GetValue(property) != null)
                    {
                        occupiedUnits = 1;
                    }
                }

                return new SharedKernel.Models.OccupancyReport
                {
                    PropertyId = property.Id,
                    PropertyName = property.Name,
                    TotalUnits = totalUnits,
                    OccupiedUnits = occupiedUnits
                };
            }).ToList();

            return Ok(occupancyReports);
        }

        [HttpGet("financial-report")]
        public async Task<IActionResult> GetFinancialReport([FromQuery] Guid? propertyId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var billsQuery = _context.Bills.AsQueryable();
            var maintenanceQuery = _context.MaintenanceTickets.AsQueryable();

            if (propertyId.HasValue)
            {
                billsQuery = billsQuery.Where(b => b.PropertyId == propertyId);
                maintenanceQuery = maintenanceQuery.Where(m => m.PropertyId == propertyId);
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                billsQuery = billsQuery.Where(b => b.DueDate >= startDate && b.DueDate <= endDate);
                maintenanceQuery = maintenanceQuery.Where(m => m.CreatedAt >= startDate && m.CreatedAt <= endDate);
            }

            var bills = await billsQuery.ToListAsync();
            var maintenanceTickets = await maintenanceQuery.ToListAsync();

            var financialReport = new SharedKernel.Models.FinancialReport
            {
                PropertyId = propertyId ?? Guid.Empty,
                PropertyName = propertyId.HasValue ? (await _propertyRepository.GetByIdAsync(propertyId.Value))?.Name : "All Properties",
                TotalRevenue = bills.Sum(b => b.Amount),
                OutstandingAmounts = bills.Where(b => !b.IsPaid).Sum(b => b.Amount),
                MaintenanceCosts = maintenanceTickets.Sum(t => t.RepairCost)
            };

            return Ok(financialReport);
        }
    }
}
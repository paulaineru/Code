using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PropertyManagementService.Services;
using SharedKernel.Dto;
using SharedKernel.Models;
using SharedKernel.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PropertyManagementService.Controllers
{
    [ApiController]
    [Route("api/property-features")]
    [Produces("application/json")]
    public class PropertyFeaturesController : ControllerBase
    {
        private readonly IPropertyFeaturesService _propertyFeaturesService;
        private readonly ILogger<PropertyFeaturesController> _logger;

        public PropertyFeaturesController(
            IPropertyFeaturesService propertyFeaturesService,
            ILogger<PropertyFeaturesController> logger)
        {
            _propertyFeaturesService = propertyFeaturesService ?? throw new ArgumentNullException(nameof(propertyFeaturesService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("certifications/{propertyId}")]
        [Authorize(Roles = "Estates Officer,Property Manager")]
        [ProducesResponseType(typeof(PropertyCertificationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPropertyCertifications(Guid propertyId)
        {
            try
            {
                var certifications = await _propertyFeaturesService.GetPropertyCertificationsAsync(propertyId);
                return Ok(certifications);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Property not found: {PropertyId}", propertyId);
                return NotFound(new ErrorResponse { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving property certifications for property {PropertyId}", propertyId);
                return StatusCode(500, new ErrorResponse { Error = "An error occurred while retrieving property certifications" });
            }
        }

        [HttpGet("compliance/{propertyId}")]
        [Authorize(Roles = "Estates Officer,Property Manager")]
        [ProducesResponseType(typeof(PropertyComplianceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPropertyCompliance(Guid propertyId)
        {
            try
            {
                var compliance = await _propertyFeaturesService.GetPropertyComplianceAsync(propertyId);
                return Ok(compliance);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Property not found: {PropertyId}", propertyId);
                return NotFound(new ErrorResponse { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving property compliance for property {PropertyId}", propertyId);
                return StatusCode(500, new ErrorResponse { Error = "An error occurred while retrieving property compliance" });
            }
        }

        [HttpGet("regulations/{propertyId}")]
        [Authorize(Roles = "Estates Officer,Property Manager")]
        [ProducesResponseType(typeof(PropertyRegulationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPropertyRegulations(Guid propertyId)
        {
            try
            {
                var regulations = await _propertyFeaturesService.GetPropertyRegulationsAsync(propertyId);
                return Ok(regulations);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Property not found: {PropertyId}", propertyId);
                return NotFound(new ErrorResponse { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving property regulations for property {PropertyId}", propertyId);
                return StatusCode(500, new ErrorResponse { Error = "An error occurred while retrieving property regulations" });
            }
        }

        [HttpGet("standards/{propertyId}")]
        [Authorize(Roles = "Estates Officer,Property Manager")]
        [ProducesResponseType(typeof(PropertyStandardDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPropertyStandards(Guid propertyId)
        {
            try
            {
                var standards = await _propertyFeaturesService.GetPropertyStandardsAsync(propertyId);
                return Ok(standards);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Property not found: {PropertyId}", propertyId);
                return NotFound(new ErrorResponse { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving property standards for property {PropertyId}", propertyId);
                return StatusCode(500, new ErrorResponse { Error = "An error occurred while retrieving property standards" });
            }
        }

        [HttpGet("features/{propertyId}")]
        [Authorize(Roles = "Estates Officer,Property Manager,Tenant")]
        [ProducesResponseType(typeof(PropertyFeatureDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPropertyFeatures(Guid propertyId)
        {
            try
            {
                var features = await _propertyFeaturesService.GetPropertyFeaturesAsync(propertyId);
                return Ok(features);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Property not found: {PropertyId}", propertyId);
                return NotFound(new ErrorResponse { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving property features for property {PropertyId}", propertyId);
                return StatusCode(500, new ErrorResponse { Error = "An error occurred while retrieving property features" });
            }
        }

        [HttpGet("services/{propertyId}")]
        [Authorize(Roles = "Estates Officer,Property Manager,Tenant")]
        [ProducesResponseType(typeof(PropertyServiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPropertyServices(Guid propertyId)
        {
            try
            {
                var services = await _propertyFeaturesService.GetPropertyServicesAsync(propertyId);
                return Ok(services);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Property not found: {PropertyId}", propertyId);
                return NotFound(new ErrorResponse { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving property services for property {PropertyId}", propertyId);
                return StatusCode(500, new ErrorResponse { Error = "An error occurred while retrieving property services" });
            }
        }
    }
} 
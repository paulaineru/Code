# Tenant Management Service API Guide

## Overview
This guide provides comprehensive documentation for the Tenant Management Service API, including all endpoints, request/response examples, and usage instructions.

## Base URL
```
http://localhost:8181/tenant
```

## Authentication
All API endpoints require JWT authentication. Include the token in the Authorization header:
```
Authorization: Bearer <your_jwt_token>
```

## API Endpoints

### 1. Tenant Management

#### Get Tenant Types
- **GET** `/api/tenant/types`
- **Authorization**: None (Public endpoint)
- **Description**: Retrieve all available tenant types

**Response:**
```json
{
  "success": true,
  "message": "Tenant types retrieved successfully",
  "data": [
    {
      "id": 0,
      "name": "Individual",
      "description": "Personal/individual tenants - Single person renting a property"
    },
    {
      "id": 1,
      "name": "CorporateOrganisation",
      "description": "Business/corporate entities - Companies, businesses, or organizations"
    },
    {
      "id": 2,
      "name": "GovernmentAgency",
      "description": "Government departments or agencies - Public sector organizations"
    }
  ],
  "timestamp": "2024-01-01T00:00:00Z"
}
```

#### Create Tenant
- **POST** `/api/tenant`
- **Authorization**: Estates Officer, Property Manager
- **Description**: Create a new tenant

**Request Body:**
```json
{
  "name": "Acme Corporation",
  "primaryEmail": "contact@acmecorp.com",
  "primaryTelephone": "+256-123-456-789",
  "businessRegistrationNumber": "REG123456789",
  "taxIdentificationNumber": "TIN987654321",
  "tenantType": 1,
  "billingEntity": "Acme Corporation Ltd",
  "contacts": [
    {
      "type": "Email",
      "value": "accounts@acmecorp.com"
    },
    {
      "type": "Phone",
      "value": "+256-123-456-790"
    }
  ]
}
```

**Tenant Types:**
- `0`: Individual
- `1`: CorporateOrganisation
- `2`: GovernmentAgency

#### Get Tenant by ID
- **GET** `/api/tenant/{id}`
- **Authorization**: Estates Officer, Property Manager
- **Description**: Retrieve tenant information by ID

#### Update Tenant
- **PUT** `/api/tenant/{id}`
- **Authorization**: Estates Officer, Property Manager
- **Description**: Update tenant information

**Request Body:**
```json
{
  "name": "Acme Corporation Updated",
  "email": "updated@acmecorp.com",
  "taxIdentificationNumber": "TIN987654321-UPDATED",
  "billingEntity": "Acme Corporation Ltd - Updated",
  "contacts": [
    {
      "type": "Email",
      "value": "new-accounts@acmecorp.com"
    },
    {
      "type": "Phone",
      "value": "+256-123-456-791"
    }
  ]
}
```

#### Delete Tenant
- **DELETE** `/api/tenant/{id}`
- **Authorization**: Estates Officer, Property Manager
- **Description**: Delete a tenant

#### Initiate Termination
- **POST** `/api/tenant/{leaseId}/initiate-termination`
- **Authorization**: Tenant, Property Manager
- **Description**: Initiate lease termination process

**Request Body:**
```json
{
  "outstandingAmount": 5000.00,
  "securityDepositDeduction": 1000.00
}
```

#### Terminate Lease
- **POST** `/api/tenant/{id}/terminate-lease`
- **Authorization**: Tenant, Property Manager
- **Description**: Terminate a specific lease

**Request Body:**
```json
{
  "leaseAgreementId": "lease-guid-here",
  "outstandingAmount": 5000.00,
  "securityDepositDeduction": 1000.00
}
```

### 2. Lease Management

#### Create Lease
- **POST** `/api/lease/create`
- **Description**: Create a new lease agreement

**Request Body:**
```json
{
  "propertyId": "property-guid-here",
  "tenantId": "tenant-guid-here",
  "startDate": "2025-01-01T00:00:00Z",
  "endDate": "2025-12-31T23:59:59Z",
  "terms": "Standard lease agreement with monthly rent payment"
}
```

#### Get Leases by Tenant
- **GET** `/api/lease/tenant/{tenantId}`
- **Description**: Retrieve all leases for a specific tenant

#### Update Lease Status
- **PUT** `/api/lease/{id}/update-status`
- **Description**: Update the status of a lease agreement

**Request Body:**
```json
{
  "newStatus": "Active"
}
```

#### Approve Lease
- **PUT** `/api/lease/{id}/approve`
- **Description**: Approve or reject a lease agreement

**Request Body:**
```json
{
  "approvalStatus": "Approved",
  "approverId": "approver-guid-here"
}
```

#### Get Lease by ID
- **GET** `/api/lease/{id}`
- **Description**: Retrieve a specific lease by ID

### 3. Booking Management

#### Create Booking
- **POST** `/api/booking/create`
- **Description**: Create a new property booking

**Request Body:**
```json
{
  "propertyId": "property-guid-here",
  "tenantId": "tenant-guid-here",
  "startDate": "2025-01-01T00:00:00Z",
  "endDate": "2025-12-31T23:59:59Z"
}
```

**Validation Rules:**
- Start date must be in the future (not in the past)
- End date must be after start date
- Property ID must be valid and the property must be available for booking

#### Get Booking by ID
- **GET** `/api/booking/{id}`
- **Description**: Retrieve a specific booking by ID

#### Get Bookings by Tenant
- **GET** `/api/booking/tenant/{tenantId}`
- **Description**: Retrieve all bookings for a specific tenant

#### Update Booking Status
- **PUT** `/api/booking/{id}/update-status`
- **Description**: Update the status of a booking

**Request Body:**
```json
{
  "newStatus": 1
}
```

**Booking Status Values:**
- `0`: Pending
- `1`: Confirmed
- `2`: Cancelled
- `3`: Completed

### 4. Renewal Management

#### Submit Renewal Request
- **POST** `/api/renewal/submit-renewal`
- **Authorization**: Tenant, Estates Officer, Property Manager
- **Description**: Submit a lease renewal request

**Request Body:**
```json
{
  "propertyId": "property-guid-here",
  "tenantId": "tenant-guid-here",
  "newTerms": "Updated lease terms with new conditions",
  "newMonthlyRent": 2500.00
}
```

#### Get Renewal Request
- **GET** `/api/renewal/{id}`
- **Authorization**: Tenant, Estates Officer, Property Manager
- **Description**: Retrieve a specific renewal request by ID

### 5. Termination Management

#### Submit Termination Request
- **POST** `/api/termination/submit-termination`
- **Authorization**: Tenant, Estates Officer, Property Manager
- **Description**: Submit a lease termination request

**Request Body:**
```json
{
  "propertyId": "property-guid-here",
  "reason": "Relocation to new office",
  "outstandingAmount": 5000.00,
  "securityDepositDeduction": 1000.00,
  "leaseAgreementId": "lease-guid-here"
}
```

#### Get Termination Process
- **GET** `/api/termination/{id}`
- **Authorization**: Tenant, Estates Officer, Property Manager
- **Description**: Retrieve a specific termination process by ID

## Response Format

All API responses follow a standard format:

```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    // Response data here
  },
  "timestamp": "2024-01-01T00:00:00Z"
}
```

## Error Responses

### 400 Bad Request
```json
{
  "success": false,
  "message": "Invalid request data",
  "errors": [
    "Field 'name' is required",
    "Invalid email format"
  ]
}
```

### 401 Unauthorized
```json
{
  "success": false,
  "message": "Authentication required"
}
```

### 403 Forbidden
```json
{
  "success": false,
  "message": "Insufficient permissions"
}
```

### 404 Not Found
```json
{
  "success": false,
  "message": "Resource not found"
}
```

### 500 Internal Server Error
```json
{
  "success": false,
  "message": "An error occurred while processing the request"
}
```

## Using the Postman Collection

### Setup Instructions

1. **Import the Collection**
   - Open Postman
   - Click "Import" and select the `TenantManagementService.postman_collection.json` file

2. **Configure Environment Variables**
   - Create a new environment in Postman
   - Add the following variables:
     - `base_url`: `http://localhost:8181/tenant`
     - `auth_token`: Your JWT authentication token
     - `tenant_id`: Sample tenant ID (replace with actual)
     - `property_id`: Sample property ID (replace with actual)
     - `lease_id`: Sample lease ID (replace with actual)
     - `booking_id`: Sample booking ID (replace with actual)
     - `renewal_id`: Sample renewal ID (replace with actual)
     - `termination_id`: Sample termination ID (replace with actual)
     - `approver_id`: Sample approver ID (replace with actual)

3. **Get Authentication Token**
   - Use the Authentication Service to obtain a JWT token
   - Update the `auth_token` variable with your token

4. **Test the Endpoints**
   - Start with creating a tenant
   - Use the returned tenant ID for subsequent requests
   - Follow the workflow: Tenant → Property → Lease → Booking → Renewal/Termination

### Testing Workflow

1. **Create a Tenant**
   - Use the "Create Tenant" endpoint
   - Save the returned tenant ID

2. **Create a Lease**
   - Use the "Create Lease" endpoint with the tenant ID
   - Save the returned lease ID

3. **Create a Booking**
   - Use the "Create Booking" endpoint
   - Save the returned booking ID

4. **Test Renewal/Termination**
   - Use the renewal or termination endpoints as needed

## Notes

- All dates should be in ISO 8601 format (UTC)
- GUIDs should be valid UUID format
- Ensure proper authorization roles for each endpoint
- The service integrates with other microservices (Property, Billing, Notification)
- All operations are audited and logged
- Notifications are sent for critical operations

## Support

For technical support or questions about the API, please refer to the service documentation or contact the development team. 
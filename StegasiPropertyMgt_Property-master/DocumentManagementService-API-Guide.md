# Document Management Service API Guide

## Overview

The Document Management Service provides a comprehensive, configurable document management system for the Stegasi Property Management platform. It supports dynamic document types, categories, and sub-categories that can be managed without code changes.

## Base URL

```
http://localhost:8181/document/api
```

## Authentication

All endpoints require JWT authentication. Include the token in the Authorization header:

```
Authorization: Bearer <your_jwt_token>
```

## Features

- **Configurable Document Types**: Create and manage document types (Property, Tenant, Lease, etc.)
- **Dynamic Categories**: Add categories within document types (Ownership, Application, Agreement, etc.)
- **Sub-Categories**: Further classify documents (Title Deed, ID Card, etc.)
- **Process Requirements**: Define required documents for different business processes
- **File Validation**: Configurable file type and size restrictions
- **Expiry Management**: Track document expiry dates and warnings
- **Verification Workflow**: Document verification and approval process
- **Completeness Checking**: Verify document completeness for processes
- **Statistics & Reporting**: Document usage and storage statistics

## Document Types Management

### Get All Document Types
```
GET /document/types
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "name": "Property",
      "description": "Property-related documents",
      "isActive": true,
      "displayOrder": 1,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": null,
      "categoryCount": 4,
      "documentCount": 25
    }
  ]
}
```

### Create Document Type
```
POST /document/types
```

**Request Body:**
```json
{
  "name": "Custom Type",
  "description": "A custom document type",
  "isActive": true,
  "displayOrder": 12
}
```

### Update Document Type
```
PUT /document/types/{documentTypeId}
```

### Delete Document Type
```
DELETE /document/types/{documentTypeId}
```

## Document Categories Management

### Get Categories by Type
```
GET /document/categories/type/{documentTypeId}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "documentTypeId": "guid",
      "documentTypeName": "Property",
      "name": "Ownership",
      "description": "Property ownership documents",
      "code": "PROP_OWN",
      "isActive": true,
      "isRequired": true,
      "displayOrder": 1,
      "validationRules": "{\"maxFileSize\": 10485760}",
      "expiryWarningDays": 30,
      "allowedFileTypes": "pdf,jpg,jpeg,png",
      "maxFileSizeInBytes": 10485760,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": null,
      "subCategoryCount": 3,
      "documentCount": 10
    }
  ]
}
```

### Create Document Category
```
POST /document/categories
```

**Request Body:**
```json
{
  "documentTypeId": "guid",
  "name": "Custom Category",
  "description": "A custom document category",
  "code": "CUSTOM_CAT",
  "isActive": true,
  "isRequired": false,
  "displayOrder": 1,
  "validationRules": "{\"maxFileSize\": 5242880, \"allowedExtensions\": [\"pdf\", \"jpg\"]}",
  "expiryWarningDays": 30,
  "allowedFileTypes": "pdf,jpg,jpeg,png",
  "maxFileSizeInBytes": 5242880
}
```

### Update Document Category
```
PUT /document/categories/{categoryId}
```

### Delete Document Category
```
DELETE /document/categories/{categoryId}
```

## Document Sub-Categories Management

### Get Sub-Categories by Category
```
GET /document/subcategories/category/{categoryId}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "documentCategoryId": "guid",
      "documentCategoryName": "Ownership",
      "name": "Title Deed",
      "description": "Property title deed",
      "code": "OWN_TITLE",
      "isActive": true,
      "isRequired": true,
      "displayOrder": 1,
      "validationRules": "{\"maxFileSize\": 10485760}",
      "expiryWarningDays": 30,
      "allowedFileTypes": "pdf",
      "maxFileSizeInBytes": 10485760,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": null,
      "documentCount": 5
    }
  ]
}
```

### Create Document Sub-Category
```
POST /document/subcategories
```

**Request Body:**
```json
{
  "documentCategoryId": "guid",
  "name": "Custom Sub-Category",
  "description": "A custom document sub-category",
  "code": "CUSTOM_SUB",
  "isActive": true,
  "isRequired": false,
  "displayOrder": 1,
  "validationRules": "{\"maxFileSize\": 2621440, \"allowedExtensions\": [\"pdf\"]}",
  "expiryWarningDays": 15,
  "allowedFileTypes": "pdf",
  "maxFileSizeInBytes": 2621440
}
```

### Update Document Sub-Category
```
PUT /document/subcategories/{subCategoryId}
```

### Delete Document Sub-Category
```
DELETE /document/subcategories/{subCategoryId}
```

## Document Management

### Upload Document
```
POST /document/upload
```

**Request (multipart/form-data):**
```
documentTypeId: guid
documentCategoryId: guid
documentSubCategoryId: guid (optional)
propertyId: guid (optional)
tenantId: guid (optional)
leaseId: guid (optional)
maintenanceTicketId: guid (optional)
bookingId: guid (optional)
renewalRequestId: guid (optional)
terminationProcessId: guid (optional)
billId: guid (optional)
paymentId: guid (optional)
isRequired: true/false
expiryDate: 2024-12-31 (optional)
description: "Document description"
tags: ["tag1", "tag2"] (JSON array)
comments: "Additional comments"
file: [file upload]
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "documentTypeId": "guid",
    "documentCategoryId": "guid",
    "documentSubCategoryId": "guid",
    "documentTypeName": "Property",
    "documentCategoryName": "Ownership",
    "documentSubCategoryName": "Title Deed",
    "fileName": "title_deed.pdf",
    "fileUrl": "guid_title_deed.pdf",
    "contentType": "application/pdf",
    "fileSize": 1048576,
    "uploadedAt": "2024-01-01T00:00:00Z",
    "uploadedBy": "guid",
    "expiryDate": "2024-12-31T00:00:00Z",
    "isRequired": true,
    "isVerified": false,
    "verifiedAt": null,
    "verifiedBy": null,
    "status": "Active",
    "version": "1.0",
    "tags": "[\"important\", \"legal\"]",
    "description": "Property title deed",
    "comments": "Original title deed",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": null,
    "propertyId": "guid",
    "tenantId": null,
    "leaseId": null
  },
  "message": "Document uploaded successfully"
}
```

### Get Document by ID
```
GET /document/{documentId}
```

### Search Documents
```
POST /document/search
```

**Request Body:**
```json
{
  "searchTerm": "title deed",
  "documentTypeId": "guid",
  "documentCategoryId": "guid",
  "documentSubCategoryId": "guid",
  "status": "Active",
  "propertyId": "guid",
  "tenantId": "guid",
  "leaseId": "guid",
  "maintenanceTicketId": "guid",
  "bookingId": "guid",
  "renewalRequestId": "guid",
  "terminationProcessId": "guid",
  "billId": "guid",
  "paymentId": "guid",
  "isRequired": true,
  "isVerified": false,
  "uploadedFrom": "2024-01-01T00:00:00Z",
  "uploadedTo": "2024-12-31T23:59:59Z",
  "expiresFrom": "2024-01-01T00:00:00Z",
  "expiresTo": "2024-12-31T23:59:59Z",
  "page": 1,
  "pageSize": 20
}
```

### Update Document
```
PUT /document/{documentId}
```

**Request Body:**
```json
{
  "description": "Updated document description",
  "tags": "[\"updated\", \"document\"]",
  "comments": "Document has been updated",
  "expiryDate": "2024-12-31T00:00:00Z",
  "isRequired": true,
  "status": "Active"
}
```

### Delete Document
```
DELETE /document/{documentId}
```

### Verify Document
```
POST /document/verify
```

**Request Body:**
```json
{
  "documentId": "guid",
  "isVerified": true,
  "comments": "Document verified successfully"
}
```

## Document Requirements Management

### Get Document Requirements
```
GET /document/requirements/{processType}?subProcess={subProcess}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "processType": "TenantApplication",
      "subProcess": "Residential",
      "documentTypeId": "guid",
      "documentCategoryId": "guid",
      "documentSubCategoryId": "guid",
      "documentTypeName": "Tenant",
      "documentCategoryName": "Identity",
      "documentSubCategoryName": "National ID",
      "isRequired": true,
      "description": "National ID is required for tenant verification",
      "validationRules": "{\"maxFileSize\": 5242880}",
      "expiryWarningDays": 30,
      "displayOrder": 1,
      "isActive": true
    }
  ]
}
```

### Create Document Requirement
```
POST /document/requirements
```

**Request Body:**
```json
{
  "processType": "CustomProcess",
  "subProcess": "CustomSubProcess",
  "documentTypeId": "guid",
  "documentCategoryId": "guid",
  "documentSubCategoryId": "guid",
  "isRequired": true,
  "displayOrder": 1,
  "description": "Custom document requirement",
  "validationRules": "{\"maxFileSize\": 5242880}",
  "expiryWarningDays": 30,
  "isActive": true
}
```

## Document Completeness & Validation

### Check Document Completeness
```
GET /document/completeness/{processType}/{entityId}?subProcess={subProcess}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "isComplete": false,
    "requiredDocuments": [
      {
        "id": "guid",
        "processType": "TenantApplication",
        "documentTypeName": "Tenant",
        "documentCategoryName": "Identity",
        "documentSubCategoryName": "National ID",
        "isRequired": true,
        "description": "National ID is required"
      }
    ],
    "missingDocuments": [
      {
        "id": "guid",
        "processType": "TenantApplication",
        "documentTypeName": "Tenant",
        "documentCategoryName": "Financial",
        "documentSubCategoryName": "Bank Statement",
        "isRequired": true,
        "description": "Bank statements are required"
      }
    ],
    "expiredDocuments": [],
    "pendingVerification": []
  }
}
```

### Get Expiring Documents
```
GET /document/expiring?daysThreshold=30
```

### Get Documents Requiring Verification
```
GET /document/pending-verification
```

## Document Statistics & Reporting

### Get Document Statistics
```
GET /document/statistics
```

**Response:**
```json
{
  "success": true,
  "data": {
    "totalDocuments": 150,
    "activeDocuments": 140,
    "expiredDocuments": 5,
    "pendingVerification": 5,
    "requiredDocuments": 50,
    "totalStorageUsed": 1073741824,
    "documentsByType": {
      "Property": 45,
      "Tenant": 35,
      "Lease": 25,
      "Maintenance": 20,
      "Financial": 15,
      "Legal": 10
    },
    "documentsByCategory": {
      "Ownership": 20,
      "Application": 15,
      "Agreement": 25,
      "Identity": 10,
      "Financial": 15
    }
  }
}
```

### Get Document Hierarchy
```
GET /document/hierarchy
```

**Response:**
```json
{
  "success": true,
  "data": {
    "documentTypes": [
      {
        "id": "guid",
        "name": "Property",
        "description": "Property-related documents",
        "isActive": true,
        "displayOrder": 1,
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": null,
        "categoryCount": 4,
        "documentCount": 25
      }
    ]
  }
}
```

## Error Responses

All endpoints return standardized error responses:

```json
{
  "success": false,
  "message": "Error description",
  "data": null
}
```

Common HTTP status codes:
- `200 OK`: Success
- `201 Created`: Resource created successfully
- `400 Bad Request`: Invalid request data
- `401 Unauthorized`: Missing or invalid authentication
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

## Configuration

### File Upload Limits
- Maximum file size: Configurable per category (default: 10MB)
- Allowed file types: Configurable per category
- Supported formats: PDF, JPG, JPEG, PNG, DOC, DOCX

### Validation Rules
Validation rules are stored as JSON and can include:
- `maxFileSize`: Maximum file size in bytes
- `allowedExtensions`: Array of allowed file extensions
- `requiredFields`: Required metadata fields
- `expiryWarningDays`: Days before expiry to warn

### Process Types
Common process types include:
- `PropertyRegistration`: Property registration process
- `TenantApplication`: Tenant application process
- `LeaseCreation`: Lease creation process
- `MaintenanceRequest`: Maintenance request process
- `RenewalProcess`: Lease renewal process
- `TerminationProcess`: Lease termination process

## Integration Examples

### Tenant Application Process
1. Get required documents for tenant application
2. Upload tenant documents (ID, financial statements, etc.)
3. Check document completeness
4. Verify uploaded documents
5. Proceed with application approval

### Property Registration Process
1. Get required documents for property registration
2. Upload property documents (title deed, survey plan, etc.)
3. Check document completeness
4. Verify uploaded documents
5. Complete property registration

### Lease Creation Process
1. Get required documents for lease creation
2. Upload lease agreement and related documents
3. Check document completeness
4. Verify uploaded documents
5. Execute lease agreement

## Best Practices

1. **Use Configurable Categories**: Create specific categories for different document types
2. **Set Validation Rules**: Configure appropriate file size and type restrictions
3. **Implement Expiry Tracking**: Set expiry warning days for time-sensitive documents
4. **Use Process Requirements**: Define required documents for each business process
5. **Regular Verification**: Implement document verification workflow
6. **Monitor Statistics**: Use statistics endpoints for system monitoring
7. **Handle Errors Gracefully**: Implement proper error handling for file uploads
8. **Secure File Storage**: Ensure proper access controls for uploaded files

## Support

For technical support or questions about the Document Management Service API, please refer to the system documentation or contact the development team. 
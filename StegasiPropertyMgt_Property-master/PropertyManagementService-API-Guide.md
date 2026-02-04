# Property Management Service API Guide

## Overview

The Property Management Service handles property registration, management, approval workflows, and amenity management. This service provides both public endpoints (accessible without authentication) and protected endpoints that require JWT authentication.

## Base URL

- **Development**: `http://localhost:8181/property`
- **Production**: `https://your-domain.com/property`

## Authentication

### Getting a JWT Token

Before using protected endpoints, you need to obtain a JWT token from the Authentication Service:

```bash
POST http://localhost:8181/auth/api/auth/login
Content-Type: application/json

{
  "email": "your-email@example.com",
  "password": "your-password"
}
```

**Response:**
```json
{
  "code": "200",
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "expiresIn": 3600
  }
}
```

### Using JWT Tokens

Include the JWT token in the Authorization header for protected endpoints:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Public Endpoints (No Authentication Required)

### 1. Get Property by ID

Retrieve a specific property by its ID.

```http
GET /api/property/{propertyId}
```

**Parameters:**
- `propertyId` (UUID): The unique identifier of the property

**Example:**
```bash
curl -X GET "http://localhost:8181/property/api/property/60240f34-c196-42ab-abce-a834b59bc4da" \
  -H "accept: application/json"
```

**Response:**
```json
{
  "code": "200",
  "message": "Property retrieved successfully",
  "data": {
    "id": "60240f34-c196-42ab-abce-a834b59bc4da",
    "name": "Cozy Bungalow Home",
    "address": "321 Quiet Lane, Residential Area",
    "propertyType": "Bungalow",
    "approvalStatus": "Approved",
    "numberOfBedrooms": 3,
    "numberOfBathrooms": 2,
    "rentPrice": 4000.00,
    "salePrice": 900000.00,
    "isRentable": true,
    "isSaleable": true
  }
}
```

### 2. Get All Properties

Retrieve all properties in the system.

```http
GET /api/property
```

**Example:**
```bash
curl -X GET "http://localhost:8181/property/api/property" \
  -H "accept: application/json"
```

### 3. Authentication Test

Debug endpoint to test authentication and token issues.

```http
GET /api/property/auth-test
```

**Example:**
```bash
curl -X GET "http://localhost:8181/property/api/property/auth-test" \
  -H "accept: application/json"
```

**Response:**
```json
{
  "code": "200",
  "message": "Authentication test completed",
  "data": {
    "hasAuthorizationHeader": true,
    "isBearerToken": true,
    "authorizationHeader": "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "tokenLength": 150,
    "userAuthenticated": true,
    "userName": "user@example.com",
    "claims": [
      {
        "type": "sub",
        "value": "user-id"
      },
      {
        "type": "role",
        "value": "Estates Officer"
      }
    ]
  }
}
```

## Protected Endpoints (Authentication Required)

### Property Management

#### 1. Register New Property

Register a new property for approval workflow.

```http
POST /api/property
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

**Required Role:** Estates Officer, Property Manager

**Request Body:**
```json
{
  "name": "Modern Office Building",
  "address": "123 Business District, City Center",
  "propertyType": "Commercial",
  "yearOfCommissionOrPurchase": 2020,
  "fairValue": 2500000.00,
  "insurableValue": 2250000.00,
  "ownershipStatus": "Owned",
  "salePrice": 2800000.00,
  "isRentable": true,
  "isSaleable": true,
  "rentPrice": 15000.00,
  "numberOfStories": 5,
  "commercialSpecifications": {
    "floorSpace": 5000.00,
    "parkingSpaces": 20,
    "officeUnits": 15,
    "meetingRooms": 5,
    "receptionArea": true,
    "securitySystem": true,
    "elevator": true,
    "airConditioning": true
  }
}
```

**Property Types:**
- `Residential` - Houses, apartments, etc.
- `Commercial` - Office buildings, retail spaces, etc.
- `Industrial` - Warehouses, factories, etc.
- `Bungalow` - Single-story residential properties
- `Apartment` - Multi-unit residential buildings

#### 2. Get Properties by Type

Retrieve properties filtered by type.

```http
GET /api/property/type/{propertyType}
Authorization: Bearer {jwt_token}
```

**Required Role:** Tenant, Sales Officer, Sales Manager

**Parameters:**
- `propertyType` (string): The type of property to filter by

**Example:**
```bash
curl -X GET "http://localhost:8181/property/api/property/type/Commercial" \
  -H "Authorization: Bearer your_jwt_token_here" \
  -H "accept: application/json"
```

#### 3. Get Properties by Filter

Retrieve properties with status and type filters.

```http
GET /api/property/properties?status={status}&type={propertyType}
Authorization: Bearer {jwt_token}
```

**Required Role:** Estates Officer, Property Manager, Tenant, Sales Officer, Sales Manager

**Query Parameters:**
- `status` (optional): Filter by property status (Available, Rented, Sold, etc.)
- `type` (optional): Filter by property type (Residential, Commercial, Industrial, etc.)

#### 4. Update Property

Update an existing property.

```http
PUT /api/property/{propertyId}
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

**Required Role:** Estates Officer

#### 5. Delete Property

Delete a property from the system.

```http
DELETE /api/property/{propertyId}
Authorization: Bearer {jwt_token}
```

**Required Role:** Estates Officer

### Property Approval Workflow

#### 1. Approve/Reject Property

Approve or reject a pending property in the approval workflow.

```http
PUT /api/property/{propertyId}/approval
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

**Required Role:** Property Manager, Estates Officer

**Request Body:**
```json
{
  "status": "Approved",
  "comments": "Property meets all requirements and is approved for listing."
}
```

**Approval Statuses:**
- `Approved` - Property is approved
- `Rejected` - Property is rejected
- `MoreInfo` - Request more information

### Amenity Management

#### 1. Create Amenity

Create a new amenity.

```http
POST /api/property/amenities
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

**Required Role:** Admin, Property Manager

**Request Body:**
```json
{
  "name": "Swimming Pool",
  "description": "Outdoor swimming pool with changing rooms",
  "category": "Recreation"
}
```

#### 2. Get Amenity by ID

Retrieve a specific amenity by ID.

```http
GET /api/property/amenities/{amenityId}
```

**Note:** This is a public endpoint that doesn't require authentication.

#### 3. Get All Amenities

Retrieve all amenities.

```http
GET /api/property/amenities
```

**Note:** This is a public endpoint that doesn't require authentication.

#### 4. Update Amenity

Update an existing amenity.

```http
PUT /api/property/amenities/{amenityId}
Authorization: Bearer {jwt_token}
Content-Type: application/json
```

**Required Role:** Admin, Property Manager

#### 5. Delete Amenity

Delete an amenity.

```http
DELETE /api/property/amenities/{amenityId}
Authorization: Bearer {jwt_token}
```

**Required Role:** Admin

#### 6. Associate Amenity with Property

Associate an amenity with a specific property.

```http
POST /api/property/properties/{propertyId}/amenities/{amenityId}
Authorization: Bearer {jwt_token}
```

**Required Role:** Admin, Property Manager

#### 7. Dissociate Amenity from Property

Remove an amenity association from a property.

```http
DELETE /api/property/properties/{propertyId}/amenities/{amenityId}
Authorization: Bearer {jwt_token}
```

**Required Role:** Admin, Property Manager

### Statistics

#### 1. Get Property Statistics

Retrieve property statistics and analytics.

```http
GET /api/property/statistics
Authorization: Bearer {jwt_token}
```

**Required Role:** Sales Officer, Sales Manager, Tenant, Property Manager, Estates Officer

## Error Responses

### Common Error Codes

- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Missing or invalid authentication token
- `403 Forbidden` - Insufficient permissions for the requested operation
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

### Error Response Format

```json
{
  "code": "400",
  "message": "Bad Request",
  "data": {
    "error": "Detailed error message"
  }
}
```

## JWT Token Issues

### Common Problems

1. **Malformed Token**: Ensure the token is a valid JWT with three parts separated by dots
2. **Expired Token**: Tokens expire after 60 minutes by default
3. **Wrong Issuer/Audience**: Ensure the token was issued by the correct authentication service
4. **Missing Role**: Ensure the user has the required role for the endpoint

### Debugging Token Issues

Use the `/api/property/auth-test` endpoint to debug authentication problems:

```bash
curl -X GET "http://localhost:8181/property/api/property/auth-test" \
  -H "Authorization: Bearer your_token_here" \
  -H "accept: application/json"
```

This endpoint will show:
- Whether an authorization header is present
- If it's a valid Bearer token
- Token length and format
- User authentication status
- All claims in the token

## Postman Collection

Import the `PropertyManagementService.postman_collection.json` file into Postman for easy testing.

### Environment Variables

Set up these environment variables in Postman:

- `base_url`: `http://localhost:8181/property`
- `auth_base_url`: `http://localhost:8181/auth`
- `auth_token`: Your JWT token (obtained from login)
- `property_id`: Sample property ID for testing
- `amenity_id`: Sample amenity ID for testing

### Getting Started

1. Import the Postman collection
2. Set up environment variables
3. Call the login endpoint to get a JWT token
4. Update the `auth_token` variable with the received token
5. Test the endpoints

## Notes

- Public endpoints (Get Property, Get All Properties, Get Amenities) don't require authentication
- Protected endpoints require valid JWT tokens with appropriate roles
- The service uses role-based access control (RBAC)
- All timestamps are in UTC
- Property IDs and other UUIDs should be valid GUIDs 
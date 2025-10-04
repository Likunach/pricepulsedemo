# PricePulse Database Setup

## Overview
This document provides instructions for setting up the complete database structure for the PricePulse pricing platform with all 13 tables.

## Database Schema
The database includes the following 13 tables:
1. **users** - Main user accounts
2. **profiles** - User profile information
3. **addresses** - Company addresses
4. **company_profiles** - Company information
5. **competitors** - Competitor tracking
6. **own_products** - User's own products
7. **competitor_products** - Competitor product mappings
8. **prices** - Price tracking data
9. **contacts** - User contact information
10. **authentications** - Authentication settings
11. **sessions** - User sessions
12. **registration_verifications** - Email verification
13. **user_roles** - User role management

## Setup Instructions

### 1. Prerequisites
- PostgreSQL server running on localhost
- .NET 9.0 SDK installed
- Entity Framework Core tools

### 2. Database Connection
Update your PostgreSQL password in `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=PricePulseDB;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### 3. Run Migrations
Execute the following commands in PowerShell/Command Prompt:

```bash
# Navigate to project directory
cd "C:\Users\Likun\OneDrive\Desktop\PricePulse\PricePulse"

# Install EF tools if not already installed
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration to create database
dotnet ef database update
```

### 4. Verify Database Creation
Check that all 13 tables are created in your PostgreSQL database:
- users
- profiles
- addresses
- company_profiles
- competitors
- own_products
- competitor_products
- prices
- contacts
- authentications
- sessions
- registration_verifications
- user_roles

## Entity Relationships
- **User** (1:1) **Profile**
- **User** (1:1) **CompanyProfile**
- **User** (1:1) **Contact**
- **User** (1:1) **Authentication**
- **User** (1:many) **Session**
- **User** (1:many) **RegistrationVerification**
- **User** (1:many) **UserRole**
- **User** (1:many) **OwnProduct**
- **CompanyProfile** (1:1) **Address**
- **CompanyProfile** (1:many) **Competitor**
- **CompanyProfile** (1:many) **OwnProduct**
- **Competitor** (1:many) **CompetitorProduct**
- **OwnProduct** (1:many) **CompetitorProduct**
- **OwnProduct** (1:many) **Price**
- **CompetitorProduct** (1:many) **Price**

## Files Created
- `Models/` - All 13 entity models
- `Data/PricePulseDbContext.cs` - Entity Framework DbContext
- Updated `Program.cs` - Database configuration
- Updated `appsettings.json` and `appsettings.Development.json` - Connection strings
- Updated `PricePulse.csproj` - EF packages

## Next Steps
Once the database is set up, you can proceed to:
1. Create DAOs (Data Access Objects)
2. Implement API endpoints
3. Add authentication middleware
4. Integrate with Semrush API
5. Add OpenAI integration for pricing analysis
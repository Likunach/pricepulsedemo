# PricePulse DAO Implementation Guide

## Overview
This document provides a comprehensive guide to the Data Access Object (DAO) layer implemented for the PricePulse pricing platform. The DAO layer provides a clean abstraction over Entity Framework operations with additional business logic methods.

## Architecture

### Base DAO Pattern
All DAOs inherit from a base `IBaseDAO<T>` interface and `BaseDAO<T>` implementation, providing:
- Standard CRUD operations
- Pagination support
- Filtering and searching
- Async operations throughout
- Proper Entity Framework navigation property handling

### Directory Structure
```
DAOs/
├── Interfaces/
│   ├── IBaseDAO.cs
│   ├── IUserDAO.cs
│   ├── IProfileDAO.cs
│   ├── ICompanyProfileDAO.cs
│   ├── IAddressDAO.cs
│   ├── IOwnProductDAO.cs
│   ├── IPriceDAO.cs
│   ├── ICompetitorDAO.cs
│   ├── ICompetitorProductDAO.cs
│   ├── IContactDAO.cs
│   ├── IAuthenticationDAO.cs
│   ├── ISessionDAO.cs
│   ├── IUserRoleDAO.cs
│   └── IRegistrationVerificationDAO.cs
└── Implementations/
    ├── BaseDAO.cs
    ├── UserDAO.cs
    ├── ProfileDAO.cs
    ├── CompanyProfileDAO.cs
    ├── AddressDAO.cs
    ├── OwnProductDAO.cs
    ├── PriceDAO.cs
    ├── CompetitorDAO.cs
    ├── CompetitorProductDAO.cs
    ├── ContactDAO.cs
    ├── AuthenticationDAO.cs
    ├── SessionDAO.cs
    ├── UserRoleDAO.cs
    └── RegistrationVerificationDAO.cs
```

## DAO Features

### Base DAO Operations
All DAOs provide these standard operations:
- `CreateAsync(entity)` - Create single entity
- `CreateManyAsync(entities)` - Create multiple entities
- `GetByIdAsync(id)` - Get by primary key
- `GetAllAsync()` - Get all entities
- `FindAsync(predicate)` - Find entities matching condition
- `FindFirstAsync(predicate)` - Find first entity matching condition
- `ExistsAsync(predicate)` - Check if entity exists
- `CountAsync()` / `CountAsync(predicate)` - Count entities
- `UpdateAsync(entity)` - Update single entity
- `UpdateManyAsync(entities)` - Update multiple entities
- `DeleteAsync(id)` / `DeleteAsync(entity)` - Delete operations
- `DeleteManyAsync(predicate)` - Delete multiple entities
- `GetPagedAsync(pageNumber, pageSize)` - Pagination
- `GetOrderedAsync(orderBy, ascending)` - Ordered results

### Specialized DAO Methods

#### UserDAO
- `GetByEmailAsync(email)` - Find user by email
- `GetUserWithProfileAsync(userId)` - User with profile data
- `GetUserWithCompanyProfileAsync(userId)` - User with company data
- `GetUserWithFullDetailsAsync(userId)` - User with all related data
- `GetActiveUsersAsync()` - Only active users
- `GetUsersByStatusAsync(status)` - Users by account status
- `EmailExistsAsync(email)` - Check email availability
- `UpdateLastLoginAsync(userId)` - Update login timestamp
- `UpdateAccountStatusAsync(userId, status)` - Change account status
- `GetUsersRegisteredAfterAsync(date)` - Recent registrations
- `GetUsersWithRoleAsync(roleName)` - Users with specific role

#### ProfileDAO
- `GetByUserIdAsync(userId)` - Profile by user ID
- `GetProfileWithUserAsync(profileId)` - Profile with user data
- `SearchByNameAsync(searchTerm)` - Search by first/last name
- `ProfileExistsForUserAsync(userId)` - Check if profile exists
- `UpdateProfileAsync(userId, firstName, lastName, bio)` - Update profile

#### CompanyProfileDAO
- `GetByUserIdAsync(userId)` - Company by user ID
- `GetCompanyWithAddressAsync(companyId)` - Company with address
- `GetCompanyWithFullDetailsAsync(companyId)` - Company with all data
- `SearchByCompanyNameAsync(searchTerm)` - Search companies
- `GetCompaniesByIndustryAsync(industry)` - Filter by industry
- `CompanyExistsForUserAsync(userId)` - Check if company exists
- `UpdateCompanyDetailsAsync(companyId, name, description, industry)` - Update company

#### OwnProductDAO
- `GetByUserIdAsync(userId)` - Products by user
- `GetByCompanyProfileIdAsync(companyProfileId)` - Products by company
- `GetProductWithDetailsAsync(productId)` - Product with full details
- `GetProductWithPricesAsync(productId)` - Product with price history
- `GetProductWithCompetitorsAsync(productId)` - Product with competitors
- `SearchByProductNameAsync(searchTerm)` - Search products
- `ProductExistsForUserAsync(userId, productName)` - Check product exists
- `GetProductsWithRecentPricesAsync(days)` - Products with recent pricing data

#### PriceDAO
- `GetByOwnProductIdAsync(ownProductId)` - Prices for own product
- `GetByCompetitorProductIdAsync(competitorProductId)` - Competitor prices
- `GetLatestPriceForProductAsync(ownProductId)` - Most recent price
- `GetLatestPriceForCompetitorProductAsync(competitorProductId)` - Latest competitor price
- `GetPriceHistoryAsync(ownProductId, fromDate, toDate)` - Price history range
- `GetCompetitorPriceHistoryAsync(competitorProductId, fromDate, toDate)` - Competitor price history
- `GetPricesInRangeAsync(minPrice, maxPrice)` - Prices within range
- `GetAveragePriceForProductAsync(ownProductId, days)` - Average price calculation
- `GetAveragePriceForCompetitorProductAsync(competitorProductId, days)` - Avg competitor price
- `GetRecentPricesAsync(days)` - Recent price updates

#### SessionDAO
- `GetByUserIdAsync(userId)` - User sessions
- `GetBySessionTokenAsync(sessionToken)` - Session by token
- `GetActiveSessionsAsync()` - All active sessions
- `GetExpiredSessionsAsync()` - All expired sessions
- `SessionExistsAsync(sessionToken)` - Check session validity
- `ExpireSessionAsync(sessionToken)` - Expire single session
- `ExpireAllUserSessionsAsync(userId)` - Expire all user sessions
- `CleanupExpiredSessionsAsync()` - Remove expired sessions
- `GetUserActiveSessionsAsync(userId)` - User's active sessions

#### UserRoleDAO
- `GetByUserIdAsync(userId)` - User's roles
- `GetByRoleNameAsync(roleName)` - Users with role
- `UserHasRoleAsync(userId, roleName)` - Check role assignment
- `AssignRoleToUserAsync(userId, roleName)` - Assign role
- `RemoveRoleFromUserAsync(userId, roleName)` - Remove role
- `GetUserRolesAsync(userId)` - Get role names for user
- `GetUsersInRoleAsync(roleName)` - Users with specific role
- `CountUsersInRoleAsync(roleName)` - Count users in role

#### RegistrationVerificationDAO
- `GetByUserIdAsync(userId)` - Verifications for user
- `GetByVerificationCodeAsync(verificationCode)` - Find by code
- `GetPendingVerificationsAsync()` - Unverified, not expired
- `GetExpiredVerificationsAsync()` - Expired verifications
- `VerificationExistsAsync(verificationCode)` - Check code exists
- `MarkAsVerifiedAsync(verificationCode)` - Complete verification
- `CleanupExpiredVerificationsAsync()` - Remove expired records
- `GetLatestVerificationForUserAsync(userId)` - Most recent verification

## Usage Examples

### Basic CRUD Operations
```csharp
// Dependency injection in controller/service
public class UserService
{
    private readonly IUserDAO _userDAO;
    
    public UserService(IUserDAO userDAO)
    {
        _userDAO = userDAO;
    }
    
    // Create user
    public async Task<User> CreateUserAsync(User user)
    {
        return await _userDAO.CreateAsync(user);
    }
    
    // Get user by ID with navigation properties
    public async Task<User?> GetUserAsync(int userId)
    {
        return await _userDAO.GetByIdAsync(userId); // Includes Profile and Contact
    }
    
    // Search users
    public async Task<IEnumerable<User>> SearchUsersAsync(string email)
    {
        return await _userDAO.FindAsync(u => u.Email.Contains(email));
    }
}
```

### Advanced Operations
```csharp
// Get user with full details
var userWithDetails = await _userDAO.GetUserWithFullDetailsAsync(userId);

// Get price history for analysis
var priceHistory = await _priceDAO.GetPriceHistoryAsync(productId, 
    DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow);

// Calculate average prices
var avgPrice = await _priceDAO.GetAveragePriceForProductAsync(productId, 30);

// Session management
await _sessionDAO.ExpireAllUserSessionsAsync(userId);
var activeSessions = await _sessionDAO.GetUserActiveSessionsAsync(userId);

// Role management
await _userRoleDAO.AssignRoleToUserAsync(userId, "Admin");
var hasRole = await _userRoleDAO.UserHasRoleAsync(userId, "Admin");
```

### Pagination and Ordering
```csharp
// Paginated results
var pagedUsers = await _userDAO.GetPagedAsync(pageNumber: 1, pageSize: 20);

// Ordered results
var usersByName = await _userDAO.GetOrderedAsync(u => u.Email, ascending: true);

// Combined filtering, ordering, and pagination
var recentUsers = await _userDAO.GetPagedAsync(1, 10, 
    u => u.RegistrationDate >= DateTime.UtcNow.AddDays(-30));
```

## Navigation Properties

Each DAO is designed to intelligently load navigation properties:
- **Base operations** (GetByIdAsync, GetAllAsync) include essential related data
- **Specialized methods** include specific navigation properties as needed
- **Full detail methods** load comprehensive related data for complete views
- **Performance-focused methods** only load necessary data to minimize queries

## Dependency Injection

All DAOs are registered in `Program.cs`:
```csharp
builder.Services.AddScoped<IUserDAO, UserDAO>();
builder.Services.AddScoped<IProfileDAO, ProfileDAO>();
// ... all other DAOs
```

## Best Practices

1. **Use interfaces** for dependency injection and testing
2. **Async operations** throughout for better performance
3. **Navigation properties** are pre-loaded where logical
4. **Business logic** is contained in specialized DAO methods
5. **Error handling** should be implemented in service layer
6. **Unit of work** pattern can be implemented above DAOs if needed
7. **Caching** can be added at the service layer

## Testing

DAOs can be easily unit tested by:
1. Using in-memory database for integration tests
2. Mocking the interfaces for unit tests
3. Creating test data builders for complex entities
4. Testing both success and failure scenarios

## Performance Considerations

- Navigation properties are selectively loaded to avoid N+1 queries
- Pagination is built-in for large data sets
- Indexes should be added to frequently queried columns
- Consider implementing caching at the service layer for read-heavy operations
- Use projections for reports that don't need full entities

## Next Steps

1. Implement service layer above DAOs
2. Add comprehensive error handling
3. Implement logging and monitoring
4. Add caching strategies
5. Create API controllers that use the DAOs
6. Add comprehensive unit and integration tests

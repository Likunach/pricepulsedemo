# Competitor Discovery Feature

## Overview

The Competitor Discovery feature integrates with Semrush API to automatically identify and track competitors for your business. This feature follows the flowchart you provided and includes both automatic discovery and manual entry capabilities.

## Features Implemented

### 1. Semrush API Integration
- **Service**: `SemrushService.cs`
- **API Key**: Configured in `appsettings.Development.json`
- **Endpoints**: 
  - Domain organic competitors
  - Domain ranking details
- **Caching**: 24-hour cache for competitor data

### 2. Competitor Discovery Flow
- **Input**: Company domain (e.g., apple.com)
- **Process**: 
  1. Retrieve competitor data from Semrush
  2. Present data to client for confirmation
  3. Allow manual entry if needed
  4. Save confirmed competitors to company profile

### 3. User Interface
- **Discovery Page**: `/Competitor` - Main competitor discovery interface
- **List Page**: `/Competitor/List` - View and manage discovered competitors
- **Manual Entry**: Inline form for adding competitors manually
- **Confirmation Flow**: Checkbox selection for confirming competitors

### 4. Background Monitoring
- **Service**: `CompetitorMonitoringService.cs`
- **Frequency**: Every 24 hours
- **Function**: Automatically refresh competitor data
- **Integration**: Updates existing competitor metrics

## File Structure

```
Services/
├── SemrushService.cs              # Semrush API integration
├── CompetitorMonitoringService.cs # Background monitoring
└── OpenAIService.cs              # Existing product discovery

Controllers/
└── CompetitorController.cs        # Competitor management

ViewModels/
└── CompetitorViewModel.cs         # Data models

Views/Competitor/
├── Index.cshtml                   # Discovery interface
└── List.cshtml                    # Competitor list

Program.cs                         # Service registration
appsettings.Development.json       # API configuration
```

## API Configuration

The Semrush API key is configured in `appsettings.Development.json`:

```json
{
  "Semrush": {
    "ApiKey": "26b2b9f11cf4d65fc35f5a20d1b2bb1b"
  }
}
```

## Usage Flow

### 1. Discover Competitors
1. Navigate to `/Competitor`
2. Enter your company domain
3. Click "Discover Competitors"
4. Review discovered competitors
5. Select competitors to confirm
6. Click "Confirm Selected Competitors"

### 2. Manual Entry
1. If no competitors found, click "Add Manually"
2. Fill in competitor details:
   - Competitor Name
   - Website URL
   - Product Listing Page
   - Notes
3. Click "Save Manual Competitors"

### 3. Manage Competitors
1. Navigate to `/Competitor/List?domain=yourdomain.com`
2. View competitor metrics and status
3. Refresh data or add more competitors
4. Analyze competitor products

## Data Models

### CompetitorInfo
```csharp
public class CompetitorInfo
{
    public string Domain { get; set; }
    public int CommonKeywords { get; set; }
    public int OrganicKeywords { get; set; }
    public int OrganicTraffic { get; set; }
    public decimal OrganicCost { get; set; }
    public int AdwordsKeywords { get; set; }
    public int AdwordsTraffic { get; set; }
    public decimal AdwordsCost { get; set; }
    public bool IsConfirmed { get; set; }
    public DateTime DiscoveredAt { get; set; }
    public string? ProductListingPage { get; set; }
    public string? Notes { get; set; }
}
```

## Integration Points

### 1. Product Discovery Integration
- Competitors can be analyzed for products
- Direct link from competitor list to product analysis
- URL parameter: `?competitorDomain=example.com`

### 2. Navigation Integration
- Added "Competitor Discovery" to main navigation
- Consistent styling with existing design
- Responsive layout

### 3. Background Processing
- Automatic competitor data refresh
- Configurable monitoring frequency
- Error handling and logging

## Error Handling

- **API Failures**: Graceful fallback to manual entry
- **Network Issues**: Retry logic with exponential backoff
- **Invalid Data**: Validation and user feedback
- **Cache Misses**: Automatic refresh from API

## Performance Features

- **Caching**: 24-hour cache for competitor data
- **Async Operations**: Non-blocking API calls
- **Pagination**: Handle large competitor lists
- **Background Processing**: Offload heavy operations

## Security Considerations

- **API Key**: Stored in configuration, not in code
- **Input Validation**: Sanitize all user inputs
- **Rate Limiting**: Respect Semrush API limits
- **Authentication**: Require user login for access

## Future Enhancements

1. **Database Integration**: Store competitors in PostgreSQL
2. **Advanced Analytics**: Competitor trend analysis
3. **Alerts**: Notify on significant changes
4. **Export**: CSV/Excel export of competitor data
5. **API Rate Limiting**: Implement proper rate limiting
6. **Competitor Categories**: Group competitors by type
7. **Historical Data**: Track competitor changes over time

## Testing

To test the feature:

1. **Start the application**
2. **Navigate to `/Competitor`**
3. **Enter a domain** (e.g., apple.com)
4. **Click "Discover Competitors"**
5. **Review the results**
6. **Test manual entry** if no competitors found
7. **Check the competitor list** at `/Competitor/List`

## Troubleshooting

### Common Issues

1. **No competitors found**: 
   - Check if domain is valid
   - Verify Semrush API key
   - Try manual entry

2. **API errors**:
   - Check network connection
   - Verify API key validity
   - Check Semrush API status

3. **Slow loading**:
   - Check cache configuration
   - Monitor API response times
   - Consider reducing data scope

## Configuration Options

```json
{
  "Semrush": {
    "ApiKey": "your-api-key",
    "CacheHours": 24,
    "MaxCompetitors": 20,
    "TimeoutSeconds": 30
  }
}
```

This implementation provides a comprehensive competitor discovery system that integrates seamlessly with your existing PricePulse application while following the flowchart requirements you provided.


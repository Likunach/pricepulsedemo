# Competitor Discovery Feature

## Overview

The Competitor Discovery feature provides manual competitor management for your business. This feature includes manual entry capabilities for adding and tracking competitors.

## Features Implemented

### 1. Manual Competitor Management
- **Service**: Manual entry forms
- **Configuration**: No external API required
- **Features**: 
  - Manual competitor entry
  - Competitor details management
- **Storage**: Local competitor data

### 2. Competitor Management Flow
- **Input**: Company domain (e.g., apple.com)
- **Process**: 
  1. Present manual entry form
  2. User enters competitor details
  3. Save competitors to company profile
  4. Manage competitor list

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
├── CompetitorMonitoringService.cs # Background monitoring
└── OpenAIService.cs              # Existing product discovery

Controllers/
└── CompetitorController.cs        # Competitor management

ViewModels/
└── CompetitorViewModel.cs         # Data models

Views/Competitor/
├── Index.cshtml                   # Manual entry interface
└── List.cshtml                    # Competitor list

Program.cs                         # Service registration
```

## Configuration

No external API configuration is required for manual competitor management.

## Usage Flow

### 1. Add Competitors
1. Navigate to `/Competitor`
2. Enter your company domain
3. Click "Add Competitors"
4. Fill in competitor details manually
5. Save competitors

### 2. Manual Entry
1. Click "Add Competitors" to show manual entry form
2. Fill in competitor details:
   - Competitor Name
   - Website URL
   - Product Listing Page
   - Notes
3. Click "Save Competitors"

### 3. Manage Competitors
1. Navigate to `/Competitor/List?domain=yourdomain.com`
2. View competitor details and status
3. Add more competitors or edit existing ones
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

- **Invalid Data**: Validation and user feedback
- **Form Errors**: Clear error messages
- **Data Validation**: Input sanitization

## Performance Features

- **Form Handling**: Efficient form processing
- **Data Storage**: Local competitor data management
- **Pagination**: Handle large competitor lists
- **Background Processing**: Offload heavy operations

## Security Considerations

- **Input Validation**: Sanitize all user inputs
- **Data Security**: Secure data handling
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
   - Use manual entry form
   - Add competitors manually

2. **Form errors**:
   - Check form validation
   - Verify required fields
   - Check data format

3. **Slow loading**:
   - Check form processing
   - Monitor data operations
   - Consider reducing data scope

## Configuration Options

No external API configuration is required for manual competitor management.

This implementation provides a comprehensive competitor management system that integrates seamlessly with your existing PricePulse application for manual competitor tracking and analysis.


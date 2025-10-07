# PricePulse - Competitive Price Monitoring System

PricePulse is a comprehensive competitive price monitoring and analysis system built with ASP.NET Core MVC. It helps businesses track competitor pricing, analyze market trends, and make informed pricing decisions.

## Features

- **User Authentication**: Secure user registration and login with Google and Microsoft OAuth
- **Company Management**: Multi-company support with user roles and permissions
- **Product Discovery**: AI-powered product discovery and competitor analysis
- **Price Monitoring**: Automated competitor price tracking and analysis
- **Dashboard**: Comprehensive analytics and reporting dashboard
- **Email Notifications**: Automated email alerts for price changes

## Prerequisites

- .NET 9.0 SDK
- PostgreSQL database
- OpenAI API key
- Manual competitor management
- Email service credentials (Gmail SMTP recommended)

## Setup Instructions

### 1. Clone the Repository

```bash
git clone https://github.com/Likunach/pricepulsedemo.git
cd pricepulsedemo
```

### 2. Database Setup

1. Install PostgreSQL on your system
2. Create a new database named `PricePulseDB`
3. Update the connection string in your configuration

### 3. Configuration

1. Copy `appsettings.Example.json` to `appsettings.Development.json`
2. Fill in your actual API keys and credentials:

```json
{
  "EmailSettings": {
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "your-email@gmail.com"
  },
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    },
    "Microsoft": {
      "ClientId": "your-microsoft-client-id",
      "ClientSecret": "your-microsoft-client-secret"
    }
  },
  "OpenAI": {
    "ApiKey": "your-openai-api-key"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=PricePulseDB;Username=postgres;Password=your-db-password"
  }
}
```

### 4. Run Database Migrations

```bash
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run
```

The application will be available at `http://localhost:5000`

## API Keys Setup

### OpenAI API Key
1. Visit [OpenAI Platform](https://platform.openai.com/)
2. Create an account and generate an API key
3. Add the key to your configuration

### Google OAuth
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing
3. Enable Google+ API
4. Create OAuth 2.0 credentials
5. Add your domain to authorized origins

### Microsoft OAuth
1. Visit [Azure Portal](https://portal.azure.com/)
2. Register a new application
3. Configure authentication settings
4. Generate client secret

### Manual Competitor Management
1. Navigate to the Competitor section
2. Add competitors manually using the form
3. Manage your competitor list

## Security Notes

- Never commit actual API keys or passwords to version control
- Use environment variables for production deployments
- Regularly rotate your API keys
- Use strong, unique passwords for all services

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request


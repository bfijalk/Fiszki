# Fiszki

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Status](https://img.shields.io/badge/status-beta-green)](#project-status)
[![License](https://img.shields.io/badge/license-MIT-blue)](#license)

> AI-powered flashcard creation and spaced-repetition learning platform built with Blazor Server, PostgreSQL, and OpenRouter integration.

A modern web application that revolutionizes flashcard creation by combining AI assistance with proven spaced repetition techniques. Generate high-quality study materials from your content in seconds, then master them through scientifically-backed learning sessions.

---

## Table of Contents

1. [Features](#features)
2. [Tech Stack](#tech-stack)
3. [Getting Started](#getting-started)
4. [Project Structure](#project-structure)
5. [Development](#development)
6. [Testing Strategy](#testing-strategy)
7. [CI/CD Pipeline](#cicd-pipeline)
8. [Deployment](#deployment)
9. [Environment Configuration](#environment-configuration)
10. [Architecture](#architecture)
11. [Security & Privacy](#security--privacy)
12. [Contributing](#contributing)
13. [Roadmap](#roadmap)
14. [License](#license)

---

## Features

### 🤖 AI-Powered Generation
- **Smart Content Analysis**: Paste any text (50+ characters) and let AI create relevant flashcards
- **OpenRouter Integration**: Leverages advanced language models for high-quality question/answer pairs
- **Review & Edit**: Accept, modify, or reject AI suggestions before saving
- **Multi-language Support**: Generate flashcards in multiple languages

### 📚 Manual Creation
- **Full Control**: Create custom flashcards with complete control over content
- **Rich Text Support**: Add detailed questions and answers
- **Tagging System**: Organize cards with custom tags for better management
- **Bulk Operations**: Efficient management of large flashcard collections

### 🧠 Spaced Repetition Learning (Planned)
- **Scientific Approach**: Will be based on proven spaced repetition algorithms
- **Progress Tracking**: Planned monitoring of learning progress with detailed statistics
- **Adaptive Scheduling**: Future feature for cards to appear at optimal intervals for retention
- **Interactive Study Sessions**: Basic flip-card interface available, advanced features planned

### 👤 User Management
- **Secure Authentication**: Account creation with password hashing
- **Personal Collections**: Isolated user data with full privacy
- **Data Control**: Complete account and data deletion capabilities
- **Session Management**: Secure login/logout functionality

### 📊 Analytics & Insights (Planned)
- **Learning Statistics**: Planned tracking of generated vs manual cards
- **Study Metrics**: Future monitoring of session frequency and performance
- **Visual Feedback**: Basic indicators for card types and sources available
- **Progress Reports**: Planned feature to understand learning patterns

---

## Tech Stack

### Backend
- **.NET 8**: Latest version with modern C# features
- **Blazor Server**: Server-side rendering with real-time UI updates
- **Entity Framework Core**: ORM with PostgreSQL provider
- **PostgreSQL**: Robust, scalable database solution
- **BCrypt.Net**: Secure password hashing

### Frontend
- **Blazor Components**: Reusable, maintainable UI components
- **MudBlazor**: Material Design component library
- **Bootstrap**: Responsive CSS framework
- **Custom CSS**: Tailored styling for optimal UX

### AI Integration
- **OpenRouter**: Gateway to multiple LLM providers
- **HTTP Client**: Efficient API communication
- **JSON Processing**: Structured AI response handling

### Testing & Quality
- **xUnit**: Primary testing framework
- **bUnit**: Blazor component testing
- **FluentValidation**: Input validation
- **AutoFixture**: Test data generation
- **FluentAssertions**: Readable test assertions

---

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 16+](https://www.postgresql.org/download/)
- [Git](https://git-scm.com/)

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/bfijalk/Fiszki.git
cd Fiszki
```

2. **Install dependencies**
```bash
dotnet restore
```

3. **Set up the database**
```bash
# Install PostgreSQL (macOS with Homebrew)
brew install postgresql@16
brew services start postgresql@16

# Create database
createdb fiszki_dev
```

4. **Configure environment variables**
```bash
# Set up user secrets for development
dotnet user-secrets init
dotnet user-secrets set "OpenRouter:ApiKey" "your-openrouter-api-key"
dotnet user-secrets set "ConnectionStrings:FiszkiDatabase" "Host=localhost;Database=fiszki_dev;Username=your_username"
```

5. **Run the application**
```bash
dotnet run
```

6. **Open in browser**
Navigate to `https://localhost:5001` (or the URL shown in console)

---

## Project Structure

```
Fiszki/
├── Components/                 # Blazor components
│   ├── Pages/                 # Page components
│   │   ├── Generate/          # AI generation workflow
│   │   ├── Flashcards.razor   # Card management
│   │   ├── Login.razor        # Authentication
│   │   └── ...
│   └── Layout/                # Layout components
├── Fiszki.Database/           # Data layer
│   ├── Entities/              # EF Core entities
│   ├── Configurations/        # Entity configurations
│   ├── Migrations/            # Database migrations
│   └── FiszkiDbContext.cs     # Database context
├── Fiszki.Services/           # Business logic
│   ├── Services/              # Domain services
│   ├── Models/                # DTOs and view models
│   ├── Commands/              # Command pattern
│   └── Validation/            # FluentValidation
├── Fiszki.Tests/              # Unit tests
├── Fiszki.FunctionalTests/    # Integration & E2E tests
└── wwwroot/                   # Static assets
```

---

## Development

### Running Tests
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test categories
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
```

### Database Migrations
```bash
# Add new migration
dotnet ef migrations add YourMigrationName --project Fiszki.Database

# Update database
dotnet ef database update --project Fiszki.Database
```

### Code Quality
```bash
# Format code
dotnet format

# Analyze code
dotnet analyze
```

---

## Testing Strategy

### Unit Tests
- **Services**: Business logic validation
- **Validators**: Input validation rules
- **Utilities**: Helper functions and algorithms
- **Coverage**: 80%+ for core business logic

### Integration Tests
- **Database**: EF Core operations with real PostgreSQL
- **API**: Service layer integration
- **Authentication**: Login/logout workflows

### Component Tests
- **Blazor Components**: UI behavior and rendering
- **User Interactions**: Form submissions and navigation
- **State Management**: Component state changes

### Functional Tests
- **End-to-End**: Complete user workflows
- **Cross-browser**: Chrome, Firefox, Safari compatibility
- **Performance**: Load testing and optimization

---

## CI/CD Pipeline

### Fully Integrated CI/CD Process

Our application features a comprehensive **double-gate CI/CD pipeline** that ensures maximum quality and reliability before any code reaches production.

#### Continuous Integration
- **Automated Testing**: Multi-stage testing pipeline with **independent job execution**
- **Build Stage**: .NET compilation and artifact generation
- **Unit Testing Stage**: Fast, isolated service layer validation with xUnit
- **Integration Testing Stage**: Database interactions with EF Core
- **E2E Testing Stage**: Comprehensive browser automation with Playwright
- **Test Environment**: Application runs in **isolated test mode with in-memory database**
- **Browser Automation**: **Real user interaction simulation** across multiple browsers
- **Code Quality**: Static analysis, linting validation, and comprehensive coverage reports
- **Build Verification**: Ensures successful compilation across environments
- **Security Scanning**: Dependency vulnerability checks and security validation

#### Double-Gate Validation Strategy
1. **Gate 1 - Pull Request Validation**:
   - ✅ **Build Stage**: Compilation and artifact generation
   - ✅ **Unit Testing Stage**: Service layer validation with xUnit (independent job)
   - ✅ **Integration Testing Stage**: Database interactions with EF Core (independent job)
   - ✅ **E2E Testing Stage (Playwright)**: Full application workflow testing (independent job)
   - ✅ Quality gate validation (blocks merge if any stage fails)

2. **Gate 2 - Pre-Deployment Validation**:
   - ✅ **Build Stage**: Complete rebuild and artifact generation
   - ✅ **Unit Testing Stage**: Re-execution of service layer tests (independent job)
   - ✅ **Integration Testing Stage**: Re-execution of database interaction tests (independent job)  
   - ✅ **E2E Testing Stage (Playwright)**: Re-execution of full workflow tests (independent job)
   - ✅ **Application startup verification in test mode**
   - ✅ **Deployment readiness validation**

#### Pipeline Stages Breakdown

##### 🔨 Build Stage
- .NET 8 SDK setup and environment preparation
- NuGet package restoration and dependency resolution
- Release configuration compilation
- Build artifact generation and packaging

##### 🧪 Unit Testing Stage
- **Purpose**: Fast feedback on business logic and service layer
- **Framework**: xUnit with FluentAssertions and AutoFixture
- **Scope**: Service layer validation, input validation rules, utility functions
- **Execution**: Independent job running in parallel after build
- **Coverage**: 80%+ target for core business logic
- **Results**: TRX reports and code coverage analysis

##### 🎭 E2E Testing Stage (Playwright)
- **Purpose**: Full application workflow validation with real browser automation
- **Framework**: Playwright with Chrome/Firefox support
- **Environment**: Isolated application instance with **in-memory database**
- **Scope**: Complete user workflows from login to flashcard management
- **Execution**: Independent job requiring **application startup in test mode**
- **Coverage**: Critical user paths and cross-browser compatibility
- **Results**: Test artifacts, screenshots, and application logs

##### 🚀 Deployment Stage
- **Azure Web App deployment** to production slot
- Database migration execution and validation
- Post-deployment health checks and monitoring
- Zero-downtime rolling deployment strategy

#### Continuous Deployment
- **Azure App Service**: Automatic deployment to Azure cloud on successful validation
- **Production Ready**: Live application available immediately after comprehensive testing
- **Database Migrations**: Automated schema updates during deployment
- **Health Checks**: Post-deployment verification and monitoring
- **Zero-Downtime**: Rolling deployment strategy

#### End-to-End Testing Details
- **Playwright Integration**: Modern browser automation framework
- **Test Environment**: Isolated application instance with **in-memory database**
- **Real Browser Testing**: Chrome/Firefox automation with **actual user interactions**
- **Application Modes**: Full testing in dedicated **test mode configuration**
- **Coverage**: Complete user workflows from login to flashcard management
- **Environment Variables**: Secure handling of test secrets and API keys

#### Monitoring & Alerts
- **Application Performance**: Response time and error tracking via Azure Monitor
- **Infrastructure**: Server health and resource usage monitoring
- **Deployment Status**: Real-time feedback on deployment success/failure
- **Test Results**: Comprehensive reporting with artifacts and logs

### Current Implementation
- **Trigger Strategy**: 
  - **Pull Requests**: Build + Unit + Integration + E2E tests (validation gate)
  - **Main Branch Merge**: Full double-validation + deployment to production
- **Platform**: Azure App Services hosting with PostgreSQL managed database
- **Testing Framework**: xUnit + Playwright + EF Core InMemory for comprehensive coverage
- **Monitoring**: Azure Application Insights integration with custom dashboards

---

## Deployment

### Automated Deployment (Current)
The application uses a fully automated CI/CD pipeline:

1. **Code Push**: Developer merges to main branch
2. **Automated Build**: Azure DevOps/GitHub Actions builds the application
3. **Testing**: Full test suite execution
4. **Database Updates**: Automatic migration deployment
5. **Azure Deployment**: Live deployment to Azure App Services
6. **Health Verification**: Automated post-deployment checks

### Live Application
- **Production URL**: Available on Azure App Services
- **Database**: Azure PostgreSQL managed database
- **SSL/TLS**: Automatic HTTPS certificate management
- **Scaling**: Auto-scaling based on demand

### Deployment Architecture
- **App Service**: Blazor Server application hosting
- **Database**: Azure Database for PostgreSQL
- **Storage**: Azure Blob Storage for static assets (planned)
- **CDN**: Azure CDN for performance optimization (planned)

---

## Environment Configuration

### Required Environment Variables
```bash
# Database
ConnectionStrings__FiszkiDatabase="Host=localhost;Database=fiszki;Username=user;Password=pass"

# AI Integration
OpenRouter__ApiKey="your-openrouter-api-key"
OpenRouter__BaseUrl="https://openrouter.ai/api/v1"

# Application
ASPNETCORE_ENVIRONMENT="Production"
ASPNETCORE_URLS="https://+:443;http://+:80"
```

### Optional Configuration
```bash
# Logging
Serilog__MinimumLevel="Information"

# Security
DataProtection__ApplicationName="Fiszki"
```

---

## Architecture

### Clean Architecture Principles
- **Separation of Concerns**: Clear layer boundaries
- **Dependency Inversion**: Abstractions over implementations
- **Testability**: Easy unit and integration testing
- **Maintainability**: Modular, extensible design

### Data Flow
1. **UI Layer**: Blazor components handle user interactions
2. **Service Layer**: Business logic and validation
3. **Data Layer**: Entity Framework and PostgreSQL
4. **External APIs**: OpenRouter for AI generation

### Security Model
- **Authentication**: Secure password-based authentication
- **Authorization**: User-specific data isolation
- **Data Protection**: Encrypted sensitive information
- **Input Validation**: Comprehensive input sanitization

---

## Security & Privacy

### Data Protection
- **Encryption**: Passwords hashed with BCrypt
- **Isolation**: User data strictly separated
- **Deletion**: Complete data removal on account deletion
- **Minimal Data**: Only necessary information collected

### GDPR Compliance
- **Right to Delete**: Full account and data removal
- **Data Portability**: Export functionality planned
- **Consent Management**: Clear privacy policies
- **Audit Trail**: User action logging

---

## Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details on:
- Code style and standards
- Pull request process
- Issue reporting
- Development setup

### Development Workflow
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add/update tests
5. Submit a pull request

---

## Roadmap

### Current Phase (v1.0)
- ✅ Core flashcard functionality
- ✅ AI-powered generation
- ✅ User authentication
- ✅ Basic study sessions
- 🔄 Performance optimization
- 🔄 UI/UX improvements

### Next Phase (v1.1)
- 📋 Advanced spaced repetition algorithms
- 📋 Enhanced statistics and analytics
- 📋 Mobile-responsive improvements
- 📋 Bulk import/export features

### Future Enhancements
- 📋 Mobile applications (iOS/Android)
- 📋 Collaborative study groups
- 📋 Advanced search and filtering
- 📋 Gamification elements
- 📋 Multi-format content support
- 📋 Public API for integrations

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## Support

- **Documentation**: [Wiki](https://github.com/bfijalk/Fiszki/wiki)
- **Issues**: [GitHub Issues](https://github.com/bfijalk/Fiszki/issues)
- **Discussions**: [GitHub Discussions](https://github.com/bfijalk/Fiszki/discussions)

---

*Built with ❤️ using .NET 8, Blazor Server, and modern web technologies.*

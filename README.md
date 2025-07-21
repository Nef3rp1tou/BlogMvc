# BlogMvc - ASP.NET Core Blog Platform

A modern blog management platform built with ASP.NET Core MVC, featuring clean architecture, role-based authorization, and comprehensive API documentation.

## 🚀 Features

### Core Functionality
- **Blog Post Management**: Create, read, update, and delete blog posts
- **User Authentication**: JWT-based API authentication and cookie-based web authentication
- **Role-Based Authorization**: Three user roles with different permission levels
- **Search Functionality**: Search posts by title
- **Responsive Design**: Bootstrap-powered UI that works on all devices

### Technical Features
- **Clean Architecture**: Separation of concerns with Repository and Service patterns
- **Result Pattern**: Consistent error handling without exceptions
- **Dual Controllers**: Separate API and MVC controllers for different use cases
- **Comprehensive Logging**: Request/response logging middleware for monitoring
- **API Documentation**: Full Swagger/OpenAPI documentation
- **In-Memory Database**: Easy setup without external dependencies

## 🏗️ Architecture

The project follows Clean Architecture principles with clear separation of concerns:

```
BlogMvc/
├── Controllers/
│   ├── Api/                    # RESTful API controllers
│   │   ├── AuthController.cs   # JWT authentication endpoints
│   │   └── BlogPostApiController.cs
│   ├── BlogPostController.cs   # MVC controllers for web UI
│   ├── HomeController.cs
│   └── BaseController.cs       # Shared controller logic
├── Data/
│   ├── ApplicationDbContext.cs # Entity Framework context
│   └── Seed/
│       └── DataSeeder.cs       # Automatic data seeding
├── DTOs/                       # Data Transfer Objects for API
├── Extensions/                 # Service and middleware extensions
├── Middleware/                 # Custom middleware (logging)
├── Models/                     # Domain models and ViewModels
├── Repositories/               # Data access layer
├── Services/                   # Business logic layer
├── Validators/                 # FluentValidation rules
└── Views/                      # Razor views for web UI
```

## 👥 User Roles

The platform implements three distinct user roles:

| Role | Permissions |
|------|-------------|
| **Guest** | View blog posts and details only |
| **User** | View posts + Create new posts |
| **Admin** | Full access: View, Create, Edit, Delete all posts |

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: Entity Framework Core with In-Memory provider
- **Authentication**: ASP.NET Core Identity + JWT Bearer tokens
- **Validation**: FluentValidation
- **Documentation**: Swagger/OpenAPI
- **Frontend**: Bootstrap 5, HTML5, CSS3, JavaScript
- **Logging**: Built-in ASP.NET Core logging with custom middleware

## 📊 Database

### Why In-Memory Database?

This project uses Entity Framework's In-Memory database provider for the following reasons:
- **Easy Setup**: No external database configuration required
- **Development Speed**: Instant startup without database migrations
- **Portability**: Runs anywhere without SQL Server dependencies
- **Demo-Friendly**: Perfect for showcasing the application

> **Note**: In a production environment, this would typically use SQL Server, PostgreSQL, or another persistent database provider.

### Automatic Data Seeding

The application automatically seeds the database with:
- **3 User Roles**: Guest, User, Admin
- **4 Sample Users**: 1 Admin + 3 regular users
- **11 Blog Posts**: Sample content demonstrating various features

**Seeded User Accounts:**
```
Admin: admin@blogmvc.com / Admin123!
Users: john.doe@example.com / User123!
       jane.smith@example.com / User123!
       bob.wilson@example.com / User123!
```

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- IDE (Visual Studio 2022, VS Code, or JetBrains Rider)

### Installation & Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/Nef3rp1tou/BlogMvc.git
   cd BlogMvc
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the application**
   ```bash
   dotnet run
   ```

4. **Access the application**
   - **Web Interface**: https://localhost:7043 (or http://localhost:5043)
   - **API Documentation**: https://localhost:7043/swagger

### First Steps

1. **Browse as Guest**: Visit the homepage to see blog posts
2. **Login**: Use any of the seeded accounts (see credentials above)
3. **Create Posts**: Log in as User or Admin to create new blog posts
4. **Explore API**: Check out the Swagger documentation for API endpoints
5. **Test Authorization**: Try different roles to see permission differences

## 🔧 API Usage

### Authentication

1. **Get JWT Token**
   ```bash
   POST /api/Auth/Login
   Content-Type: application/json

   {
     "email": "john.doe@example.com",
     "password": "User123!"
   }
   ```

2. **Use Token in Requests**
   ```bash
   Authorization: Bearer YOUR_JWT_TOKEN
   ```

### Key Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/BlogPostApi/GetAllPosts` | List all posts | No |
| GET | `/api/BlogPostApi/GetPostById/{id}` | Get specific post | No |
| POST | `/api/BlogPostApi/CreatePost` | Create new post | Yes (User/Admin) |
| PUT | `/api/BlogPostApi/UpdatePost` | Update existing post | Yes (Owner/Admin) |
| DELETE | `/api/BlogPostApi/DeletePost/{id}` | Delete post | Yes (Owner/Admin) |

## 📝 Logging & Monitoring

The application includes comprehensive request/response logging:

```
🔵 GET /api/BlogPostApi/GetAllPosts from 127.0.0.1
📝 Request Body: {"title":"New Post","content":"Content here"}
🟢 POST /api/BlogPostApi/CreatePost -> 201 (87ms)
📤 Response Body: {"isSuccess":true,"value":{"id":13,...}}
```

Features:
- Request/response timing
- API body logging for debugging
- Slow request detection (>2 seconds)
- Static file filtering to reduce noise

## 🎯 Design Decisions

### Dual Controller Architecture
- **API Controllers** (`/api/*`): Pure data endpoints with JSON responses
- **MVC Controllers** (`/*`): HTML rendering with server-side logic
- **Benefits**: Clean separation, better testability, API-first design

### Result Pattern Implementation
```csharp
public async Task<Result<BlogPost>> GetPostAsync(int id)
{
    var post = await _repository.GetByIdAsync(id);
    return post != null 
        ? Result<BlogPost>.Success(post)
        : Result<BlogPost>.Failure(Error.NotFound("Post not found"));
}
```

### Role-Based Security
- **Web Pages**: Cookie-based authentication with ASP.NET Core Identity
- **API Endpoints**: JWT Bearer token authentication
- **Flexible Authorization**: Both schemes supported for maximum compatibility

## 🧪 Testing the Application

### Web Interface Testing
1. Navigate to different pages as Guest/User/Admin
2. Try creating, editing, and deleting posts
3. Test search functionality
4. Verify role-based access restrictions

### API Testing
1. Use Swagger UI for interactive testing
2. Test authentication flow (register → login → use token)
3. Verify CRUD operations with different user roles
4. Check error handling with invalid requests

## 📦 Project Structure Details

### Key Components

**Models & DTOs**
- `BlogPost`: Core domain entity
- `CreatePostViewModel/Dto`: Input validation models
- `BlogPostViewModel`: Display models with computed properties

**Services & Repositories**
- `IBlogService`: Business logic interface
- `IBlogRepository`: Data access interface
- Clean separation between data access and business rules

**Middleware**
- `SimpleRequestLoggingMiddleware`: Custom logging for monitoring
- Configurable request/response body logging

## 🔮 Future Enhancements

Potential improvements for production use:
- **Persistent Database**: SQL Server/PostgreSQL integration
- **File Upload**: Image support for blog posts
- **Caching**: Redis integration for performance
- **Email Service**: User notifications and confirmations
- **Comments System**: User engagement features
- **Rich Text Editor**: Enhanced content creation
- **Admin Dashboard**: User and content management

## 📄 License

This project is available under the MIT License. See LICENSE file for details.

## 👨‍💻 Author

**Your Name**
- GitHub: [@Nef3rp1tou](https://github.com/Nef3rp1tou)
- Project Link: [https://github.com/Nef3rp1tou/BlogMvc](https://github.com/Nef3rp1tou/BlogMvc)


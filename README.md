# TanukiPanel 🎋

A powerful, cross-platform desktop application for GitLab operations. Manage projects, commits, issues, and registries all from one elegant interface.

![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![Avalonia](https://img.shields.io/badge/Avalonia-11.3.9-purple)
![License](https://img.shields.io/badge/license-GPLv2-green)
![Platforms](https://img.shields.io/badge/platforms-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)

---

## ✨ Features

- **🔍 Project Discovery** - Browse, search, and manage your GitLab projects
- **📝 Commit History** - View commits with advanced filtering and pagination
- **⚙️ Issue Management** - Create, read, and manage project issues effortlessly
- **📦 Registry Access** - Explore container and package registries
- **🔑 Secure Configuration** - Store GitLab API keys locally with encrypted persistence
- **🌐 Cross-Platform** - Native support for Windows, Linux, and macOS
- **🎨 Modern UI** - GNOME-inspired design system with intuitive navigation
- **⚡ Fast & Responsive** - Built on Avalonia for optimal performance

---

## 🛠️ Tech Stack

| Component | Technology |
|-----------|-----------|
| **Framework** | Avalonia 11.3.9 |
| **Language** | C# |
| **Runtime** | .NET 9.0 |
| **Architecture** | MVVM (Model-View-ViewModel) |
| **DI Framework** | Built-in .NET Dependency Injection |
| **API** | GitLab API v4 |

---

## 🚀 Getting Started

### Prerequisites

- **.NET 9.0 SDK** - [Download here](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- **GitLab Account** with API access
- A valid **GitLab Personal Access Token**

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/WeWeBunnyX/Tanuki-Panel.git
   cd TanukiPanel
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

---

## 📖 Usage

### First Time Setup

1. Launch TanukiPanel
2. On the Welcome screen, click on the **API Key Guide** to find your GitLab personal access token
3. Enter your API key in the configuration screen
4. Select your desired scopes and permissions
5. Start exploring your GitLab projects!

### Main Features

#### Projects View
- Browse all your GitLab projects
- Search by project name or description
- View project metadata (visibility, star count, owner)
- Navigate to project details with one click

#### Commits View
- Filter commits by date range
- Paginate through commit history (20 commits per page)
- View commit details (author, date, message)
- Direct links to GitLab commit pages

#### Issues View
- Create new issues within the app
- View all project issues
- Close or reopen existing issues
- Filter and search through issues
- Pagination support (15 issues per page)

#### Registry View
- Access container registries
- Browse package registries
- View package versions and metadata

---

## 🏗️ Project Architecture

### MVVM Pattern

```
Views (UI Layer)
    ↓ (Data Binding)
ViewModels (Business Logic)
    ↓ (Commands & Data)
Services (Domain Logic)
    ↓
Models (Data Entities)
```

### Directory Structure

```
TanukiPanel/
├── Views/              # Avalonia UI components
├── ViewModels/         # MVVM business logic
├── Services/           # API, navigation, persistence
├── Models/             # Domain entities
├── Assets/             # Static resources
└── Program.cs          # Application entry point
```

### Key Components

**Views** - User interface built with Avalonia:
- MainWindow, WelcomeView, ProjectsView, CommitView, IssuesView, etc.

**ViewModels** - Implements commands and state management:
- ProjectsViewModel, IssuesViewModel, CommitViewModel, etc.

**Services** - Handles business logic and integrations:
- `GitLabApiService` - REST client for GitLab API
- `NavigationService` - MVVM navigation
- `ApiKeyPersistence` - Secure credential storage
- `ToastService` - User notifications

**Models** - Data entities:
- User, Project, Commit, Issue, Package, etc.

---

## 🔐 API Key Management

TanukiPanel securely manages your GitLab API key:

1. Keys are stored locally in `tanuki_api_key.json`
2. Timestamped for tracking
3. Used only for GitLab API authentication
4. Never transmitted or shared

### Creating a GitLab Personal Access Token

1. Go to [GitLab Settings](https://gitlab.com/-/user_settings/personal_access_tokens)
2. Click "Add new token"
3. Select scopes: `api`, `read_user`, `read_repository`, `read_registry`
4. Click "Create personal access token"
5. Copy the token and paste it in TanukiPanel

---

## 🎯 Development

### Building from Source

```bash
# Restore dependencies
dotnet restore

# Build in Debug mode
dotnet build

# Build in Release mode
dotnet build -c Release

# Run tests (if available)
dotnet test
```

### Project File

The project uses `TanukiPanel.csproj` configured for:
- Target Framework: `.NET 9.0`
- Nullable reference types enabled
- Platform-specific runtime identifiers

---

## 🤝 Contributing

Contributions are welcome! Here's how to contribute:

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Code Style

- Follow C# naming conventions (PascalCase for public members)
- Use MVVM patterns consistently
- Write descriptive commit messages
- Keep UI logic in ViewModels, not Views

---

## 🐛 Troubleshooting

### Application won't start
- Ensure .NET 9.0 SDK is installed: `dotnet --version`
- Try cleaning and rebuilding: `dotnet clean && dotnet build`

### API Key not working
- Verify your GitLab personal access token is valid
- Check that required scopes are enabled
- Ensure token hasn't expired on GitLab

### Issues loading projects
- Check your internet connection
- Verify GitLab API is accessible
- Check application logs for error details

---

## 📋 Requirements

- **OS**: Windows 10+, Ubuntu 20.04+ (or any other GNU/Linux distribution), macOS 10.15+
- **.NET Runtime**: 9.0 or higher
- **Disk Space**: ~200 MB for installation

---

## 📄 License

This project is licensed under the GNU General Public License v2.0 (GPLv2) - see the LICENSE file for details.

---

## 🙋 Support

Having issues? 

- Check the [GitHub Issues](https://github.com/WeWeBunnyX/Tanuki-Panel/issues)
- Create a new issue with details about your problem

---

## 🎉 Acknowledgments

- Built with [Avalonia](https://avaloniaui.net/) - Cross-platform MVVM UI framework
- Uses GitLab API v4
- Inspired by desktop-first developer experience
- Community Toolkit for MVVM support

---

**Made with ❤️ for GitLab developers**

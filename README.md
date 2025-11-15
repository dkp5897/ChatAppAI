# ChatAppAI

ChatAppAI is a simple .NET-based chat application project designed to demonstrate AI integration and robust service patterns using Polly for resilience.

## Features
- Basic chat service structure
- .NET 9.0 project
- Polly integration for resilience and transient-fault handling

## Project Structure
- `Program.cs`: Entry point for the application
- `ChatService.cs`: Core chat service logic
- `ChatAppAI.csproj`: Project configuration

## Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- Git

### Build and Run
```bash
git clone https://github.com/dkp5897/ChatAppAI.git
cd ChatAppAI
dotnet restore
dotnet build
dotnet run
```

## Usage
Modify and extend the `ChatService` class to add chat features or AI integrations as needed.

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
[MIT](LICENSE)

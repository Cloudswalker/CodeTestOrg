# AsyncLog – Asynchronous Logging Component

This is a test assignment implementation of a lightweight, asynchronous logging component, written in C# (.NET), following SOLID principles and focused on reliability, performance, and testability.

---

## ✅ Features

- **Non-blocking logging** via background processing queue (`BlockingCollection`)
- **Asynchronous writing** to file with `ILogWriter`
- **File rotation** at midnight (based on log line timestamps)
- **Graceful or immediate shutdown:**
  - `StopWithFlush()` – flushes remaining logs before exit
  - `StopWithoutFlush()` – exits immediately, skips pending logs
- **Fault-tolerant** – errors during writing are swallowed, app remains stable
- **Thread-safe**
- **Fully tested** – with real file-based integration tests

---

## 🧱 Project Structure

/LogComponent
	├── Interfaces/
	│ ├── ILog.cs
	│ └── ILogWriter.cs
	├── AsyncLog.cs
	├── FileLogWriter.cs
	└── LogLine.cs (record type)

/Application
	└── Program.cs (example usage)

/LogComponent.Tests
	└── AsyncLogTests.cs
	└──FaultyLogWriter.cs

---

## 🔧 Configuration

Logging path is configured via `appsettings.json` (optional in console app):

```json
{
  "Logging": {
    "Directory": "C:\\LogTest"
  }
}

```

Can be injected via IConfiguration.

---

## 🧪 Tests

Test coverage includes:

- ✔ Log line is written to file
- ✔ File is rotated after midnight
- ✔ StopWithFlush() and StopWithoutFlush() behaviors
- ✔ No exceptions thrown on double dispose or writing after shutdown
- ✔ Long log lines are fully written
- ✔ File-based integration tests using FileLogWriter

---

## ▶ Example usage

```csharp
ILog logger = new AsyncLog(new FileLogWriter("C:\\LogTest"));
logger.Write("Hello, log!");
logger.StopWithFlush();
```

Or via DI:

```csharp
services.AddSingleton<ILogWriter>(_ => new FileLogWriter("C:\\LogTest"));
services.AddTransient<ILog, AsyncLog>();
```

---

## 📌 Notes

- ✔ Built with testability and extensibility in mind
- ✔ Easy to extend with other log writers (e.g., console, DB, cloud)
- ✔ Uses only .NET built-in APIs

---

## 👨‍💻 Author's Notes

I have followed the task's requirements and delivered a maintainable and testable solution that prioritizes performance and application stability.
If there’s anything else you'd like to see — just let me know!
# Developing Libation

Libation is built using .NET and Avalonia UI. To get started with development, you'll need to set up your environment.

## Prerequisites

- **.NET SDK**: The project currently targets `.net10.0`. You will need the latest .NET SDK.
- **IDE**: We recommend [Visual Studio Code](https://code.visualstudio.com/), [JetBrains Rider](https://www.jetbrains.com/rider/), or Visual Studio.
- **Git**: For version control.

## Setup

1.  **Clone the repository**:

    ```bash
    git clone https://github.com/rmcrackan/Libation.git
    cd Libation
    ```

2.  **Restore dependencies**:
    ```bash
    dotnet restore
    ```

## Running Libation Locally

You can run Libation directly from the source code using the .NET CLI or your IDE.

### Using .NET CLI

To run the desktop application (Avalonia):

1.  Navigate to the `Source/LibationAvalonia` directory:

    ```bash
    cd Source/LibationAvalonia
    ```

2.  Run the application:
    ```bash
    dotnet run
    ```

### Using Visual Studio / Rider

1.  Open `Libation.sln` (or open the root folder).
2.  Set `LibationAvalonia` as the startup project.
3.  Press Run/Debug.

### Troubleshooting

- **Assets/Cover Art**: If you encounter issues with missing assets, ensure you have run `git submodule update --init --recursive` if applicable, although Libation typically manages assets within the project.
- **Port/Network**: Libation makes network requests to Audible and other services. Ensure your firewall allows the application to connect.

## Linux Specifics

For Linux users, we have a specific guide using Nix:

- [Linux Development Setup with Nix](./nix-linux-setup.md)

## Documentation Specifics

For Documentaion, we have a specific guide using VitePress:

- [Website & Docs Development](./website.md)

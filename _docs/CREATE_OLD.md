|              |                                                       |
| ------------ | ----------------------------------------------------- |
| Copyright    | © VEXIT ® 2025 , www.vexit.com , Tomorrow is today... |
| Author       | Vex Tatarevic                                         |
| Date Created | 2025-10-26                                            |
| Date Updated | 2025-11-02                                            |

# Vexit.CliEngine - Creation Guide


## Install Required Development Tools

| Software                                             | Description                                                                                                                                                                                                |
| ---------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **VERSION CONTROL**                                  |                                                                                                                                                                                                            |
| [Git](https://git-scm.com/downloads)                 | - **Version control system** - necessary for managing code changes and history, having multiple people working on the same codebase, backing up code to server and downloading it to multiple dev machines |
|                                                      | - **GitBash** (that comes with Git installation) is a terminal that allows using bash script on Windows OS                                                                                                 |
| **CODE EDITORS**                                     |                                                                                                                                                                                                            |
| [VS Code](https://code.visualstudio.com/)            | (Optional) - **Code Editor** for cross platform. It has AI assistant tool GitHub Copilot built in.                                                                                                         |
| [VS Codium]( https://vscodium.com)                   | (Optional) - Same as VS Code but without Microsoft tracking aka Telemetry                                                                                                                                  |
| [Cursor](https://www.cursor.com/)                    | (Recommended) - **AI-First Code Editor** built from VS Code source, so you can use all VS Code extensions and features. Has Privacy Mode. Best for AI-powered development.                                 |
| [Visual Studio](https://visualstudio.microsoft.com/) | (Recommended for debugging on Windows) - Best Code Editor for debugging of C# and .Net development                                                                                                         |
| **RUNTIMES**                                         |                                                                                                                                                                                                            |
| [.NET SDK](https://dotnet.microsoft.com/download)    | .NET Software Development Kit and runtime -  Required for backend development in C# and .Net. <br><br> NOTE: not required if you installed Visual Studio which already includes .NET                       |


## Configure Code Editor

You should have installed one of the code editors (IDEs) below.

| IDE       | Download                       |
| --------- | ------------------------------ |
| Cursor AI | https://www.cursor.com/        |
| VS Codium | https://vscodium.com/          |
| VS Code   | https://code.visualstudio.com/ |

### Set Up Shell Command

After you have installed one of the code editors (IDEs) above, follow the steps below to configure it.

- Open the Code Editor

**NOTE:** on Windows, VS Code typically installs the `code` command automatically during setup.For all other configurations (VS Code on Linux, Cursor on Windows etc.) you need to install the `shell command` via **Command Pallete** in order to open the code editor via shell command.

- Install **shell command** - this allows you to start the code editor via shell command
  - Open IDE (VS Code / Cursor / VS Codium)
  - Open **Command Palette** : <kbd>Ctrl</kbd> <kbd>Shift</kbd> <kbd>P</kbd>
  - Type `shell command` and select:
    - if using **VS Code:** `Shell Command: Install 'code' command in PATH`
    - if using **Cursor:** `Shell Command: Install 'cursor' command in PATH`
    - if using **VS Codium:** `Shell Command: Install 'codium' command in PATH`

- On Windows - Set **Git Bash** as default terminal inside the code editor
  - Open IDE (VS Code / Cursor / VS Codium)
  - Open **Command Palette** : <kbd>Ctrl</kbd> <kbd>Shift</kbd> <kbd>P</kbd>
  - Type `terminal` and select: `Terminal: Select Default Profile`
  - Select **Git Bash**

- Close the code editor

### Set Up Default Terminal

On Windows:

- Select Git Bash as default profile
  - Open Command Palette : <kbd>CTRL</kbd> <kbd>SHIFT</kbd> <kbd>P</kbd> - this will focus your cursor in the search box at the top of the window
  - Type "**terminal**" - You should see "Terminal: Select Default Profile"
  - Select **Terminal: Select Default Profile**
  - Select **Git Bash**
  - Close the command palette


## Configure Bash Profile

### Add Commands for Create and Open File

We will add two bash functions which make it easy to copy paste file creation and file opening commands:
- `touchopen path/to/file` - creates a file and opens it in the code editor
- `open path/to/file` - opens a file in the code editor


Run this command in your terminal to add the above mentioned 2 bash functions to your shell profile:

```bash
# Copy and paste this ENTIRE block at once then hit Enter to execute:
TOUCHOPEN_FUNCS="
function open() { cursor \"\$1\"; }
function touchopen() { touch \"\$1\"; open \"\$1\"; }
" && \
echo "$TOUCHOPEN_FUNCS" >> ~/.bash_profile && \
source ~/.bash_profile
```

> **IMPORTANT:** In the above bash command block:
> - Replace `cursor` with your IDE (Code Editor) command

  | IDE           | Command  |
  | ------------- | -------- |
  | **Cursor**    | `cursor` |
  | **VS Code**   | `code`   |
  | **VS Codium** | `codium` |
> - Replace the file path `~/.bash_profile` with the path to your shell profile file depending on operating system and shell you are using as per below:
  
  | Operating System | Shell                        | Shell Profile File |
  | ---------------- | ---------------------------- | ------------------ |
  | Windows          | Git Bash, MSYS, or similar   | `~/.bash_profile`  |
  | Linux            | bash shell                   | `~/.bashrc`        |
  | Linux            | zsh shell                    | `~/.zshrc`         |
  | macOS            | bash                         | `~/.bash_profile`  |
  | macOS            | zsh - default since Catalina | `~/.zshrc`         |

## Create Workspace

Open terminal (GitBash on Windows) and type commands to create workspace

```bash
mkdir ~/dev/vexit
cd ~/dev/vexit
mkdir src
cd src
```
Now you should be inside `~/dev/vexit/src` folder.


## Create .NET Class Library Project

```bash
dotnet new classlib --name Vexit.CliEngine     # Create class library project
```
## Add NuGet Packages

```bash
dotnet add Vexit.CliEngine/ package Microsoft.Extensions.DependencyInjection
dotnet add Vexit.CliEngine/ package Microsoft.Extensions.Hosting.Abstractions
```

- Verify project contains the added packages:

  ```bash
  dotnet list Vexit.CliEngine/ package
  ```

- You should see the added packages in the output.

## Generate .gitignore file

We need to generate a .gitignore file to ignore the bin and obj directories, but also a bunch of other files and directories that are not needed in the repository. .NET CLI provides a template for .gitignore file that has a lot of useful defaults, but you should make sure to add any other files and directories that should not be checked in to the repository.

```bash
dotnet new gitignore --output Vexit.CliEngine/
```

- You should see the .gitignore file generated in the root of the project.


## Build and Test

After adding dependencies, build the project to ensure everything compiles correctly:

```bash
dotnet build Vexit.CliEngine/
```

You should see a successful build output. If you encounter any errors, verify the NuGet packages were added correctly: `dotnet list Vexit.CliEngine/ package`.

## Open in Code Editor

- Open the project in the code editor:

  ```bash
  open Vexit.CliEngine/
  ```
  - This should work if you correctly [configured shell command](#set-up-shell-command) in your code editor and [configured bash profile](#configure-bash-profile) convenience functions `open` and `touchopen`.

- Open integrated terminal: <kbd>Ctrl</kbd> <kbd>`</kbd>
  - This should open the terminal in the bottom of the code editor
  - Now you can type commands in the terminal

## Add Project Assets

- Open Command Palette: <kbd>Ctrl</kbd> <kbd>Shift</kbd> <kbd>P</kbd>

- Type `assets` and select: `.NET: Generate Assets for Build and Debug`

> **Note:** In multi-project workspaces, this generates assets at the workspace level (`.vscode` folder), not per-project. For single-project development, assets are created in the project folder.


## Set Up and Use CliUtil

We will use the CliUtil class to write to the console.

We will configure CliUtil to be available everywhere in the project under a short alias called `Cli`.

It is important to use Cli.Write* methods to write to the console instead of Console.WriteLine, because Cli.Write* methods separate user interface output from machine-readable data output that can be consumed by other tools.

- Copy CliUtil.cs into Utils folder

- Create GlobalUsings.cs file in the root of the project

  ```bash
  touchopen GlobalUsings.cs
  ```

- Add CliUtil as alias Cli to the GlobalUsings.cs file

  ```csharp
  global using Cli = Vexit.CliEngine.Utils.CliUtil;
  ```

Now you can call Cli.Write* methods to write to the console in any files in the project.

- Write for user interface output:

  ```csharp
  Cli.WriteLn("Hello, World!");                        // Writes "Hello, World!" to the console in white color
  Cli.WriteLn("Hello, World!", ConsoleColor.Magenta);  // Writes "Hello, World!" to the console in magenta color
  Cli.WriteSuccess("Hello, World!");                   // Writes "Hello, World!" to the console in green color
  Cli.WriteWarning("Hello, World!");                   // Writes "Hello, World!" to the console in yellow color
  Cli.WriteError("Hello, World!");                     // Writes "Hello, World!" to the console in red color
  Cli.WriteInfo("Hello, World!");                      // Writes "Hello, World!" to the console in blue color
  Cli.WriteDim("Hello, World!");                       // Writes "Hello, World!" to the console in dark gray color
  ```

  - This writes to the **STDERR stream** for user interface output. This is useful for human-facing output that needs to be read by the user.
  - Keeping STDERR separate from STDOUT is a good practice to avoid mixing human-facing output with machine-readable data output.

- Write for machine-readable data output:

  ```csharp
  Cli.WriteData(new { Name = "Nikola Tesla" }, DataFormat.Json); // Writes "Nikola Tesla" to the console in JSON format
  Cli.WriteData(new { Name = "Nikola Tesla" }, DataFormat.Text); // Writes "Nikola Tesla" to the console in Text format
  ```

  - This writes to the **STDOUT stream** for machine-readable data output. This is useful for scripts and CI/CD pipelines that need to consume the data output from the command.

## Create Test Project

### Run the Script

You can automatically create the test project with all dependencies configured by running the script:

**On Linux/Mac** - Make the script executable first (not required on Windows/GitBash):

```bash
chmod +x create-tests-project.sh
```

**Run the script:**

```bash
./create-tests-project.sh Vexit.CliEngine
```

This script will:
- Create the test project with correct naming
- Add all required NuGet packages (including Microsoft.NET.Test.Sdk for .NET 9.0 compatibility)
- Add project reference to the source project
- Create a dummy test file
- Build and run tests to verify setup

**Alternatively, continue to manual setup below.**

### Manual Setup

We will create a separate test project for unit tests.

- Create the test project:

  ```bash
  dotnet new classlib --name Vexit.CliEngine.Tests
  ```

### Generate .gitignore file

```bash
dotnet new gitignore --output Vexit.CliEngine.Tests/
```

- You should see the .gitignore file generated in the root of the test project.

### Add NuGet Packages

```bash
dotnet add Vexit.CliEngine.Tests/ package Microsoft.NET.Test.Sdk                    # Test SDK required for .NET compatibility with test frameworks
dotnet add Vexit.CliEngine.Tests/ package xunit                                     # xUnit testing framework
dotnet add Vexit.CliEngine.Tests/ package xunit.runner.visualstudio                 # Test runner for Visual Studio and dotnet test
dotnet add Vexit.CliEngine.Tests/ package Moq                                       # Mocking framework for creating test doubles and isolating dependencies
dotnet add Vexit.CliEngine.Tests/ package FluentAssertions                          # Fluent API for more readable and expressive test assertions
```

- Verify project contains the added packages:

  ```bash
  dotnet list Vexit.CliEngine.Tests/ package
  ```

  > **Note:** We use `classlib` template and add xUnit packages manually, instead of creating from `xunit` template (dotnet new xunit), to make sure xUnit version is the latest available. `Microsoft.NET.Test.Sdk` is required for .NET 9.0 compatibility with Moq and other test dependencies.

### Add Project References

- Add project reference to Vexit.CliEngine library:

  ```bash
  dotnet add Vexit.CliEngine.Tests/ reference Vexit.CliEngine/Vexit.CliEngine.csproj
  ```

### Build Test Project

```bash
dotnet build Vexit.CliEngine.Tests/
```

- You should see a successful build output.

### Run Tests

Below are a few different ways to run tests:

- Run tests - this command shows just the summary of the test execution

  ```bash
  dotnet test Vexit.CliEngine.Tests/
  ```
- Run tests list - shows all test names

  ```bash
  dotnet test Vexit.CliEngine.Tests/ --list-tests
  ```

- Run tests with detailed console output

  ```bash
  dotnet test Vexit.CliEngine.Tests/ --logger "console;verbosity=normal"
  ```

- Run tests and generate an HTML report inside `TestResults/TestResults.html`

  ```bash
  dotnet test Vexit.CliEngine.Tests/ --logger "html;LogFileName=TestResults.html"
  ```

---

*© VEXIT ® 2025 | All rights reserved. | [www.vexit.com](https://www.vexit.com) | Tomorrow is today...®*
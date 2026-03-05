|              |                                                       |
| ------------ | ----------------------------------------------------- |
| Copyright    | © 2026 VEXIT ® , Tomorrow is today... , www.vexit.com |
| Author       | Vex Tatarevic                                         |
| Date Created | 2026-03-04                                            |
| Date Updated |                                                       |

# Vexit.CliEngine - Creation Guide

## Create Workspace

Open terminal (GitBash on Windows) and type commands to create workspace

```bash
mkdir ~/dev
cd ~/dev
```
Now you should be inside `~/dev` folder.

> **NOTE:** From this point forward, all commands should be run from the workspace root folder !

## Create .NET Class Library Project

```bash
dotnet new classlib --name Vexit.CliEngine # Create class library project
```

## Organize Project Structure

Move project files to src folder and create required directories:

```bash
# Create src directory first
mkdir -p Vexit.CliEngine/src

# Move all files from Vexit.CliEngine into Vexit.CliEngine/src
mv Vexit.CliEngine/* Vexit.CliEngine/src/

# Create _docs directory
mkdir -p Vexit.CliEngine/_docs
```

Your project structure should now be:
```
~/dev/Vexit.CliEngine/
├── _docs/
└── src/
```

## Add NuGet Packages

```bash
dotnet add Vexit.CliEngine/src package Microsoft.Extensions.DependencyInjection # Required for dependency injection container used by CliEngine
dotnet add Vexit.CliEngine/src package Microsoft.Extensions.Hosting.Abstractions # Required for hosting abstractions for IHostApplicationBuilder
```

- Verify package list:

  ```bash
  dotnet list Vexit.CliEngine/src package
  ```

## Add Project References

```bash
dotnet add Vexit.CliEngine/src reference Vexit/src
```

## Generate .gitignore file

```bash
dotnet new gitignore --output Vexit.CliEngine/src
```

- You should see the .gitignore file generated in the Vexit.CliEngine/src folder.



## Set Initial Version

- Set the initial version in **Vexit.CliEngine.csproj**:

  ```xml
  <PropertyGroup>
    <Version>1.0.0</Version>  // <= Add this line
  </PropertyGroup>
  ```

  **OR** run this script to add it automatically:

  ```bash
  # Add version to Vexit.CliEngine.csproj
  sed -i '/<\/PropertyGroup>/i\    <Version>1.0.0<\/Version>' Vexit.CliEngine/src/Vexit.CliEngine.csproj
  ```


## Create Test Project - Automatically

Run the script:

```bash
Vexit.Scripts/create-dotnet-unittests-project.sh Vexit.CliEngine/src
```
- This will create Vexit.CliEngine.Tests project in the Vexit.CliEngine directory.

Move the test project to the tests folder:

```bash
# Move Vexit.CliEngine.Tests to tests folder
mv Vexit.CliEngine/Vexit.CliEngine.Tests Vexit.CliEngine/tests
```

Your project structure should now be:
```
~/dev/Vexit.CliEngine/
├── _docs/
├── src/
└── tests/          # Contains Vexit.CliEngine.Tests project files
```


## Create Test Project - Manually

### Create Project

We will create a separate test project for unit tests.

- Create the test project inside Vexit.CliEngine directory:

  ```bash
  dotnet new classlib --output Vexit.CliEngine/Vexit.CliEngine.Tests
  ```

  > **NOTE:** We use `classlib` template (instead of xunit template) and add xUnit packages manually, instead of creating from `xunit` template (dotnet new xunit), to make sure xUnit version is the latest available. `Microsoft.NET.Test.Sdk` is required for .NET compatibility with Moq and other test dependencies.

- Delete the initial template file `Class1.cs`

  ```bash
  rm Vexit.CliEngine/Vexit.CliEngine.Tests/Class1.cs
  ```


### Organize Test Project Structure

Move the test project to the tests folder:

```bash
# Move Vexit.CliEngine.Tests to tests folder
mv Vexit.CliEngine/Vexit.CliEngine.Tests Vexit.CliEngine/tests
```

Your project structure should now be:
```
~/dev/Vexit.CliEngine/
├── _docs/
├── src/
└── tests/          # Contains Vexit.CliEngine.Tests project files
```

### Generate .gitignore file

```bash
dotnet new gitignore --output Vexit.CliEngine/tests
```

- You should see the .gitignore file generated in the Vexit.CliEngine/tests folder.

### Add NuGet Packages

```bash
dotnet add Vexit.CliEngine/tests package Microsoft.NET.Test.Sdk                    # Test SDK required for .NET compatibility with test frameworks
dotnet add Vexit.CliEngine/tests package xunit                                     # xUnit testing framework
dotnet add Vexit.CliEngine/tests package xunit.runner.visualstudio                 # Test runner for Visual Studio and dotnet test
dotnet add Vexit.CliEngine/tests package Moq                                       # Mocking framework for creating test doubles and isolating dependencies
dotnet add Vexit.CliEngine/tests package FluentAssertions                          # Fluent API for more readable and expressive test assertions
```

- Verify project contains the added packages:

  ```bash
  dotnet list Vexit.CliEngine/tests package
  ```


### Add Project References

```bash
dotnet add Vexit.CliEngine/tests reference Vexit.CliEngine/src
```

### Add Dummy Test

Create a basic test file to verify the test setup is working:

```bash
# Create dummy test file to verify test setup
cat > Vexit.CliEngine/tests/DummyTest.cs << 'EOF'
using Xunit;

namespace Vexit.CliEngine.Tests;

public class DummyTest
{
    [Fact]
    public void Should_Pass()
    {
        // Arrange
        var expected = true;

        // Act
        var actual = true;

        // Assert
        Assert.Equal(expected, actual);
    }
}
EOF
```

### Build Test Project

```bash
dotnet build Vexit.CliEngine/tests
```

- You should see a successful build output.

### Run Tests

Below are a few different ways to run tests:

- Run tests - this command shows just the summary of the test execution

  ```bash
  dotnet test Vexit.CliEngine/tests
  ```

- Run tests list - shows all test names

  ```bash
  dotnet test Vexit.CliEngine/tests --list-tests
  ```

- Run tests with detailed console output

  ```bash
  dotnet test Vexit.CliEngine/tests --logger "console;verbosity=normal"
  ```

- Run tests and generate an HTML report inside `TestResults/TestResults.html`

  ```bash
  dotnet test Vexit.CliEngine/tests --logger "html;LogFileName=TestResults.html"
  ```


---

*© VEXIT ® 2026 | All rights reserved. | [www.vexit.com](https://www.vexit.com) | Tomorrow is today...®*
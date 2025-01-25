# Ensure.Generator

A .NET tool for generating test code from specification files.

## Installation

```bash
dotnet tool install --global Ensure.Generator
```

## Usage

1. Create a specification file (e.g., `example.spec.md`) in your test project
2. Run the generator:

```bash
ensure -s path/to/specs -o path/to/output -n namespace
```

## Features

- Generates test code from markdown specification files
- Supports BDD-style specifications
- Easy integration with existing test projects

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details. 
# ApiCallHandler Library

## Overview

ApiCallHandler is a lightweight C# library designed to simplify API interactions. It supports state management, caching, and a clean, modular design.

## Features

- GET and POST request handling.
- Built-in caching with configurable TTL.
- State management for reusable API calls.

## Installation

Include the `ApiCallHandler` namespace in your project.

## Usage

```csharp
using ApiCallHandler;

var apiHandler = new ApiHandler();
string response = await apiHandler.GetAsync("https://api.example.com/data");
apiHandler.Dispose();
```

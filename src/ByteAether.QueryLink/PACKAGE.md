# QueryLink
*from ByteAether*

[![License](https://img.shields.io/github/license/ByteAether/QueryLink?logo=github&label=License)](https://github.com/ByteAether/QueryLink/blob/main/LICENSE)
[![NuGet Version](https://img.shields.io/nuget/v/ByteAether.QueryLink?logo=nuget&label=Version)](https://www.nuget.org/packages/ByteAether.QueryLink/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ByteAether.QueryLink?logo=nuget&label=Downloads)](https://www.nuget.org/packages/ByteAether.QueryLink/)
[![GitHub Build Status](https://img.shields.io/github/actions/workflow/status/ByteAether/QueryLink/build-and-test.yml?logo=github&label=Build%20%26%20Test)](https://github.com/ByteAether/QueryLink/actions/workflows/build-and-test.yml)

QueryLink is a NuGet package designed to simplify the integration of UI components such as datagrids and datatables with backend `IQueryable`-based data sources. This library provides a seamless way to link these two parts of a system with minimal code, making it easier to manage filters and sorting operations.

## Features

![.NET 8.0](https://img.shields.io/badge/.NET-8.0-brightgreen)
![.NET 6.0](https://img.shields.io/badge/.NET-6.0-brightgreen)
![.NET Standard 2.1](https://img.shields.io/badge/.NET-Standard_2.1-yellow)

- **Filter Definitions:** Define filters with various operators to refine your data queries.
- **Order Definitions:** Specify sorting criteria to order your data.
- **Overrides:** Customize filter and order operations with expression-based overrides.
- **Query String Conversion:** Easily convert filter and order definitions to and from query strings.
- **IQueryable Extensions:** Apply filter and order definitions directly to `IQueryable` sources.

## Installation

Install the latest stable package via NuGet:

```sh
dotnet add package ByteAether.QueryLink
```

Use the `--version` option to specify a [preview version](https://www.nuget.org/packages/ByteAether.QueryLink/absoluteLatest) to install.

## Usage

### Definitions

The `Definitions` class allows you to specify filters and orders for your data queries.

This example demonstrates how to create filter and order definitions using the `Definitions` class.

```csharp
var definitions = new Definitions
{
    Filters = [
        new("Name", FilterOperator.Eq, "John"),
        new("Age", FilterOperator.Gt, 30)
    ],
    Orders = [
        new("Name", false),
        new("Age", true)
    ]
};
```

### Overrides

The `Overrides` class allows you to customize filter and order operations using expression-based overrides.

This example shows how to create overrides for filter and order operations using the `Overrides` class.

```csharp
var overrides = new Overrides<Person>
{
    Filter = [
        new(p => p.Name, p => p.FullName)
    ],
    Order = [
        new(p => p.Name, p => p.FullName)
    ]
};
```

### Query String Conversion

Convert filter and order definitions to and from query strings using the `HttpExtensions` class.

This example demonstrates how to convert filter and order definitions to and from query strings using the `HttpExtensions` class.

```csharp
string queryString = definitions.ToQueryString();
Definitions parsedDefinitions = Definitions.FromQueryString(queryString);
```

### Applying Definitions to IQueryable

Apply filter and order definitions directly to `IQueryable` sources using the `QueryableExtensions` class.

This example shows how to apply filter and order definitions to an `IQueryable` source using the `QueryableExtensions` class.

```csharp
IQueryable<Person> query = dbContext.People.AsQueryable();
query = query.Apply(definitions, overrides);
```

## Filter Operators

The library provides a variety of filter operators to refine your data queries. Here is a list of all the available filter operators:

- **Eq `=`:** Equals
- **Neq `!=`:** Not equals
- **Gt `>`:** Greater than
- **Gte `>=`:** Greater than or equal to
- **Lt `<`:** Less than
- **Lte `<=`:** Less than or equal to
- **Has `=*`:** Contains
- **Nhas `!*`:** Does not contain
- **In `[]`:** In a list
- **Nin `![]`:** Not in a list
- **Sw `^`:** Starts with
- **Nsw `!^`:** Does not start with
- **Ew `$`:** Ends with
- **New `!$`:** Does not end with

## License

This project is licensed under the MIT License.

---

QueryLink simplifies the integration of UI components with backend data sources, making it easier to manage filters and sorting operations with minimal code.

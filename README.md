# QueryLink
*from **ByteAether***

[![Build Status](https://github.com/ByteAether/QueryLink/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/ByteAether/QueryLink/actions/workflows/build-and-test.yml)
[![NuGet](https://img.shields.io/nuget/v/ByteAether.QueryLink)](https://www.nuget.org/packages/ByteAether.QueryLink/)


QueryLink is a NuGet package designed to simplify the integration of UI components such as datagrids and datatables with backend `IQueryable`-based data sources. This library provides a seamless way to link these two parts of a system with minimal code, making it easier to manage filters and sorting operations.

## Features

- **Filter Definitions:** Define filters with various operators to refine your data queries.
- **Order Definitions:** Specify sorting criteria to order your data.
- **Overrides:** Customize filter and order operations with expression-based overrides.
- **Query String Conversion:** Easily convert filter and order definitions to and from query strings.
- **IQueryable Extensions:** Apply filter and order definitions directly to `IQueryable` sources.

## Installation

You can install the package via NuGet:

```sh
dotnet add package ByteAether.QueryLink
```

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

## Examples

### Filtering and Sorting

This example demonstrates filtering and sorting using the `Definitions` class and applying them to an `IQueryable` source.

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

IQueryable<Person> query = dbContext.People.AsQueryable();
query = query.Apply(definitions);
```

### Using Overrides

This example shows how to use overrides to customize filter and order operations and apply them to an `IQueryable` source.

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

IQueryable<Person> query = dbContext.People.AsQueryable();
query = query.Apply(definitions, overrides);
```

### Comprehensive Example: Using MudBlazor DataGrid and EF Core

This example demonstrates how to integrate QueryLink with MudBlazor DataGrid and EF Core. The `LoadServerData` method reads the state of the MudBlazor DataGrid, creates a QueryLink definition set out of it, and sends the definitions over an HTTP API using `ToQueryString` and `FromQueryString`. The `PersonService` class contains the overrides and applies the definitions to the `IQueryable` source. The `PeopleController` handles the API requests, reads the full query string from the request, and returns the filtered and sorted data. The produced query string is directly included in the URL, and the definitions are parsed from the full query string.

```csharp
// Define your EF Core DbContext and entity
public class ApplicationDbContext : DbContext
{
    public DbSet<Person> People { get; set; }
}

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public string FullName => $"{Name} Doe";
}

// In your service or controller
public class PersonService
{
    private readonly ApplicationDbContext _context;

    public PersonService(ApplicationDbContext context)
    {
        _context = context;
    }

    public IQueryable<Person> GetPeople(Definitions definitions)
    {
        var overrides = new Overrides<Person>
        {
            Filter = [
                new(p => p.Name, p => p.FullName)
            ],
            Order = [
                new(p => p.Name, p => p.FullName)
            ]
        };

        var query = _context.People.AsQueryable();
        return query.Apply(definitions, overrides);
    }
}

// In your API controller
[ApiController]
[Route("api/[controller]")]
public class PeopleController : ControllerBase
{
    private readonly PersonService _personService;

    public PeopleController(PersonService personService)
    {
        _personService = personService;
    }

    [HttpGet]
    public IActionResult GetPeople()
    {
        var queryString = Request.QueryString.ToString();
        var definitions = Definitions.FromQueryString(queryString);
        var people = _personService.GetPeople(definitions);
        return Ok(people);
    }
}

// In your Blazor component
@page "/people"
@inject HttpClient Http

<MudDataGrid
    T="Person"
    Items="people"
    Hover="true"
    Sortable="true"
    Filterable="true"
    Striped="true"
    Pagination="true"
    ServerData="LoadServerData"
>
    <ToolBarContent>
        <MudText typo="Typo.h6">People</MudText>
    </ToolBarContent>
    <Columns>
        <Column T="Person" Field="@nameof(Person.Name)" Title="Name" Sortable="true" Filterable="true" />
        <Column T="Person" Field="@nameof(Person.Age)" Title="Age" Sortable="true" Filterable="true" />
    </Columns>
</MudDataGrid>

@code {
    private IEnumerable<Person> people = new List<Person>();

    private async Task<GridData<Person>> LoadServerData(GridState<Person> state)
    {
        var definitions = new Definitions
        {
            Filters = state.Filters.Select(f => new FilterDefinition<object?>(f.Field, GetFilterOperator(f.Operator), f.Value)).ToList(),
            Orders = state.Sorts.Select(s => new OrderDefinition(s.Field, s.Direction == SortDirection.Descending)).ToList()
        };

        var queryString = definitions.ToQueryString();
        var response = await Http.GetFromJsonAsync<List<Person>>($"api/people?{queryString}");

        var totalItems = response.Count();
        var items = response.Skip(state.Page * state.PageSize).Take(state.PageSize).ToList();

        return new GridData<Person> { Items = items, TotalItems = totalItems };
    }

    private FilterOperator GetFilterOperator(FilterOperator mudOperator)
    {
        return mudOperator switch
        {
            FilterOperator.Contains => FilterOperator.Has,
            FilterOperator.Equals => FilterOperator.Eq,
            FilterOperator.GreaterThan => FilterOperator.Gt,
            FilterOperator.GreaterThanOrEqual => FilterOperator.Gte,
            FilterOperator.LessThan => FilterOperator.Lt,
            FilterOperator.LessThanOrEqual => FilterOperator.Lte,
            FilterOperator.NotEqual => FilterOperator.Neq,
            FilterOperator.StartsWith => FilterOperator.Sw,
            FilterOperator.EndsWith => FilterOperator.Ew,
            _ => throw new ArgumentOutOfRangeException(nameof(mudOperator), mudOperator, null)
        };
    }
}
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

## FAQ

### Why is there no pagination support?

Pagination depends heavily on the underlying data persistence technology and requires specific implementations for each technology. It is easy to write your own pagination logic and apply it to `IQueryable` on top of what our library provides.

### How can I create my own custom conditions?

The full functionality of LINQ is still available. You are free to write any `.Where()` conditions and apply them to `IQueryable`. Our library does not block you from doing that.

### I need projections; the raw data models are not enough for me.

You can use any library that can map objects from one to another. Our library does not limit you in any way and will work with the dataset you provide in the form of `IQueryable<T>`, whatever the `T` may be.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request if you have any suggestions or improvements.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

QueryLink simplifies the integration of UI components with backend data sources, making it easier to manage filters and sorting operations with minimal code.

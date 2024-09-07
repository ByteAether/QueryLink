using System.Web;
using static ByteAether.QueryLink.Definitions;

namespace ByteAether.QueryLink.Test;
public class HttpExtensionsTests
{
	[Fact]
	public void ToQueryString_And_FromQueryString_ShouldHandleEqOperator()
	{
		var definitions = new Definitions
		{
			Filters =
			[
				new("Name", FilterOperator.Eq, "Alice")
			]
		};

		var queryString = definitions.ToQueryString();
		Assert.Equal("filter[]=Name=Alice", HttpUtility.UrlDecode(queryString));

		var parsedDefinitions = QueryStringExtensions.FromQueryString(queryString);
		AssertDefinitionsEqual(definitions, parsedDefinitions);
	}

	[Fact]
	public void ToQueryString_And_FromQueryString_ShouldHandleNeqOperator()
	{
		var definitions = new Definitions
		{
			Filters =
			[
				new("Name", FilterOperator.Neq, "Alice")
			]
		};

		var queryString = definitions.ToQueryString();
		Assert.Equal("filter[]=Name!=Alice", HttpUtility.UrlDecode(queryString));

		var parsedDefinitions = QueryStringExtensions.FromQueryString(queryString);
		AssertDefinitionsEqual(definitions, parsedDefinitions);
	}

	[Fact]
	public void ToQueryString_And_FromQueryString_ShouldHandleGtOperator()
	{
		var definitions = new Definitions
		{
			Filters =
			[
				new("Age", FilterOperator.Gt, 30)
			]
		};

		var queryString = definitions.ToQueryString();
		Assert.Equal("filter[]=Age>30", HttpUtility.UrlDecode(queryString));

		var parsedDefinitions = QueryStringExtensions.FromQueryString(queryString);
		AssertDefinitionsEqual(definitions, parsedDefinitions);
	}

	[Fact]
	public void ToQueryString_And_FromQueryString_ShouldHandleGteOperator()
	{
		var definitions = new Definitions
		{
			Filters =
			[
				new("Age", FilterOperator.Gte, 30)
			]
		};

		var queryString = definitions.ToQueryString();
		Assert.Equal("filter[]=Age>=30", HttpUtility.UrlDecode(queryString));

		var parsedDefinitions = QueryStringExtensions.FromQueryString(queryString);
		AssertDefinitionsEqual(definitions, parsedDefinitions);
	}

	[Fact]
	public void ToQueryString_And_FromQueryString_ShouldHandleLtOperator()
	{
		var definitions = new Definitions
		{
			Filters =
			[
				new("Age", FilterOperator.Lt, 30)
			]
		};

		var queryString = definitions.ToQueryString();
		Assert.Equal("filter[]=Age<30", HttpUtility.UrlDecode(queryString));

		var parsedDefinitions = QueryStringExtensions.FromQueryString(queryString);
		AssertDefinitionsEqual(definitions, parsedDefinitions);
	}

	[Fact]
	public void ToQueryString_And_FromQueryString_ShouldHandleLteOperator()
	{
		var definitions = new Definitions
		{
			Filters =
			[
				new("Age", FilterOperator.Lte, 30)
			]
		};

		var queryString = definitions.ToQueryString();
		Assert.Equal("filter[]=Age<=30", HttpUtility.UrlDecode(queryString));

		var parsedDefinitions = QueryStringExtensions.FromQueryString(queryString);
		AssertDefinitionsEqual(definitions, parsedDefinitions);
	}

	[Fact]
	public void ToQueryString_And_FromQueryString_ShouldHandleHasOperator()
	{
		var definitions = new Definitions
		{
			Filters =
			[
				new("Tags", FilterOperator.Has, "tag1")
			]
		};

		var queryString = definitions.ToQueryString();
		Assert.Equal("filter[]=Tags=*tag1", HttpUtility.UrlDecode(queryString));

		var parsedDefinitions = QueryStringExtensions.FromQueryString(queryString);
		AssertDefinitionsEqual(definitions, parsedDefinitions);
	}

	[Fact]
	public void ToQueryString_And_FromQueryString_ShouldHandleNhasOperator()
	{
		var definitions = new Definitions
		{
			Filters =
			[
				new("Tags", FilterOperator.Nhas, "tag1")
			]
		};

		var queryString = definitions.ToQueryString();
		Assert.Equal("filter[]=Tags!*tag1", HttpUtility.UrlDecode(queryString));

		var parsedDefinitions = QueryStringExtensions.FromQueryString(queryString);
		AssertDefinitionsEqual(definitions, parsedDefinitions);
	}

	[Fact]
	public void ToQueryString_And_FromQueryString_ShouldHandleInOperator()
	{
		var definitions = new Definitions
		{
			Filters =
			[
				new("Age", FilterOperator.In, new[] { 20, 30, 40 })
			]
		};

		var queryString = definitions.ToQueryString();
		Assert.Equal("filter[]=Age[][20,30,40]", HttpUtility.UrlDecode(queryString));

		var parsedDefinitions = QueryStringExtensions.FromQueryString(queryString);
		AssertDefinitionsEqual(definitions, parsedDefinitions);
	}

	[Fact]
	public void ToQueryString_And_FromQueryString_ShouldHandleNinOperator()
	{
		var definitions = new Definitions
		{
			Filters =
			[
				new("Age", FilterOperator.Nin, new[] { 20, 30, 40 })
			]
		};

		var queryString = definitions.ToQueryString();
		Assert.Equal("filter[]=Age![][20,30,40]", HttpUtility.UrlDecode(queryString));

		var parsedDefinitions = QueryStringExtensions.FromQueryString(queryString);
		AssertDefinitionsEqual(definitions, parsedDefinitions);
	}

	[Fact]
	public void ToQueryString_And_FromQueryString_ShouldHandleSwOperator()
	{
		var definitions = new Definitions
		{
			Filters =
			[
				new("Name", FilterOperator.Sw, "Alice")
			]
		};

		var queryString = definitions.ToQueryString();
		Assert.Equal("filter[]=Name^Alice", HttpUtility.UrlDecode(queryString));

		var parsedDefinitions = QueryStringExtensions.FromQueryString(queryString);
		AssertDefinitionsEqual(definitions, parsedDefinitions);
	}

	[Fact]
	public void ToQueryString_And_FromQueryString_ShouldHandleNswOperator()
	{
		var definitions = new Definitions
		{
			Filters =
			[
				new("Name", FilterOperator.Nsw, "Alice")
			]
		};

		var queryString = definitions.ToQueryString();
		Assert.Equal("filter[]=Name!^Alice", HttpUtility.UrlDecode(queryString));

		var parsedDefinitions = QueryStringExtensions.FromQueryString(queryString);
		AssertDefinitionsEqual(definitions, parsedDefinitions);
	}

	[Fact]
	public void ToQueryString_And_FromQueryString_ShouldHandleEwOperator()
	{
		var definitions = new Definitions
		{
			Filters =
			[
				new("Name", FilterOperator.Ew, "Alice")
			]
		};

		var queryString = definitions.ToQueryString();
		Assert.Equal("filter[]=Name$Alice", HttpUtility.UrlDecode(queryString));

		var parsedDefinitions = QueryStringExtensions.FromQueryString(queryString);
		AssertDefinitionsEqual(definitions, parsedDefinitions);
	}

	[Fact]
	public void ToQueryString_And_FromQueryString_ShouldHandleNewOperator()
	{
		var definitions = new Definitions
		{
			Filters =
			[
				new("Name", FilterOperator.New, "Alice")
			]
		};

		var queryString = definitions.ToQueryString();
		Assert.Equal("filter[]=Name!$Alice", HttpUtility.UrlDecode(queryString));

		var parsedDefinitions = QueryStringExtensions.FromQueryString(queryString);
		AssertDefinitionsEqual(definitions, parsedDefinitions);
	}

	[Fact]
	public void ToQueryString_And_FromQueryString_ShouldHandleMultipleFiltersAndOrders()
	{
		var definitions = new Definitions
		{
			Filters =
			[
				new("Name", FilterOperator.Eq, "Alice"),
				new("Age", FilterOperator.Gt, 30)
			],
			Orders =
			[
				new("Date", false),
				new("Name", true)
			]
		};

		var queryString = definitions.ToQueryString();
		Assert.Equal("filter[]=Name=Alice&filter[]=Age>30&order=Date,-Name", HttpUtility.UrlDecode(queryString));

		var parsedDefinitions = QueryStringExtensions.FromQueryString(queryString);
		AssertDefinitionsEqual(definitions, parsedDefinitions);
	}

	[Fact]
	public void ToQueryString_And_FromQueryString_ShouldHandleEmptyFiltersAndOrders()
	{
		var definitions = new Definitions
		{
			Filters = [],
			Orders = []
		};

		var queryString = definitions.ToQueryString();
		Assert.Equal("", queryString);

		var parsedDefinitions = QueryStringExtensions.FromQueryString(queryString);
		AssertDefinitionsEqual(definitions, parsedDefinitions);
	}

	private static void AssertDefinitionsEqual(Definitions expected, Definitions actual)
	{
		Assert.Equal(expected.Filters.Count(), actual.Filters.Count());
		for (var i = 0; i < expected.Filters.Count(); i++)
		{
			var expectedFilter = expected.Filters.ElementAt(i);
			var actualFilter = actual.Filters.ElementAt(i);
			Assert.Equal(expectedFilter.Name, actualFilter.Name);
			Assert.Equal(expectedFilter.Operation, actualFilter.Operation);
			Assert.Equal(expectedFilter.Value, actualFilter.Value);
		}

		Assert.Equal(expected.Orders.Count(), actual.Orders.Count());
		for (var i = 0; i < expected.Orders.Count(); i++)
		{
			var expectedOrder = expected.Orders.ElementAt(i);
			var actualOrder = actual.Orders.ElementAt(i);
			Assert.Equal(expectedOrder.Name, actualOrder.Name);
			Assert.Equal(expectedOrder.IsReversed, actualOrder.IsReversed);
		}
	}

}

using static ByteAether.QueryLink.Definitions;

namespace ByteAether.QueryLink.Test;

public class QueryableExtensionsTests
{
	public class TestEntity
	{
		public int Id { get; set; }
		public string Name { get; set; } = null!;
		public DateTime Date { get; set; }
		public List<string> Tags { get; set; } = [];
	}

	private static readonly List<TestEntity> _data =
	[
		new TestEntity { Id = 1, Name = "Alice", Date = new DateTime(2023, 3, 1), Tags = ["tag1", "tag2"] },
		new TestEntity { Id = 2, Name = "Bob", Date = new DateTime(2023, 2, 1), Tags = ["tag2", "tag3"] },
		new TestEntity { Id = 3, Name = "Charlie", Date = new DateTime(2023, 1, 1), Tags = ["tag3", "tag4"] }
	];

	[Fact]
	public void Apply_Filter_Equal_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters = [
				new ("Name",FilterOperator.Eq, "Bob" )
			]
		};

		var result = data.Apply(definitions);

		Assert.Single(result);
		Assert.Equal("Bob", result.Single().Name);
	}

	[Fact]
	public void Apply_Filter_NotEqual_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters =
			[
				new ("Name", FilterOperator.Neq, "Alice")
			]
		};

		var result = data.Apply(definitions);

		Assert.Equal(2, result.Count());
		Assert.All(result, x => Assert.NotEqual("Alice", x.Name));
	}

	[Fact]
	public void Apply_Filter_GreaterThan_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters =
			[
				new ("Id", FilterOperator.Gt, 1)
			]
		};

		var result = data.Apply(definitions);

		Assert.Equal(2, result.Count());
		Assert.All(result, x => Assert.True(x.Id > 1));
	}

	[Fact]
	public void Apply_Filter_GreaterThanOrEqual_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters =
			[
				new ("Id", FilterOperator.Gte, 2)
			]
		};

		var result = data.Apply(definitions);

		Assert.Equal(2, result.Count());
		Assert.All(result, x => Assert.True(x.Id >= 2));
	}

	[Fact]
	public void Apply_Filter_LessThan_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters =
			[
				new ("Id", FilterOperator.Lt, 3)
			]
		};

		var result = data.Apply(definitions);

		Assert.Equal(2, result.Count());
		Assert.All(result, x => Assert.True(x.Id < 3));
	}

	[Fact]
	public void Apply_Filter_LessThanOrEqual_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters =
			[
				new ("Id", FilterOperator.Lte, 2)
			]
		};

		var result = data.Apply(definitions);

		Assert.Equal(2, result.Count());
		Assert.All(result, x => Assert.True(x.Id <= 2));
	}

	[Fact]
	public void Apply_Filter_Contains_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters =
			[
				new ("Name", FilterOperator.Has, "li")
			]
		};

		var result = data.Apply(definitions);

		Assert.Equal(2, result.Count());
		Assert.All(result, x => Assert.Contains("li", x.Name));
	}

	[Fact]
	public void Apply_Filter_NotContains_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters =
			[
				new ("Name", FilterOperator.Nhas, "li")
			]
		};

		var result = data.Apply(definitions);

		Assert.Single(result);
		Assert.DoesNotContain("li", result.Single().Name);
	}

	[Fact]
	public void Apply_Filter_StartsWith_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters =
			[
				new ("Name", FilterOperator.Sw, "A")
			]
		};

		var result = data.Apply(definitions);

		Assert.Single(result);
		Assert.StartsWith("A", result.Single().Name);
	}

	[Fact]
	public void Apply_Filter_NotStartsWith_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters =
			[
				new ("Name", FilterOperator.Nsw, "A")
			]
		};

		var result = data.Apply(definitions);
		Assert.Equal(2, result.Count());
		Assert.All(result, x => Assert.False(x.Name.StartsWith("A")));
	}

	[Fact]
	public void Apply_Filter_EndsWith_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters =
			[
				new ("Name", FilterOperator.Ew, "e")
			]
		};

		var result = data.Apply(definitions);

		Assert.Equal(2, result.Count());
		Assert.All(result, x => Assert.EndsWith("e", x.Name));
	}

	[Fact]
	public void Apply_Filter_NotEndsWith_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters =
			[
				new ("Name", FilterOperator.New, "e")
			]
		};

		var result = data.Apply(definitions);

		Assert.Single(result);
		Assert.False(result.Single().Name.EndsWith("e"));
	}

	[Fact]
	public void Apply_Filter_In_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters =
			[
				new ("Id", FilterOperator.In, new[] { 1, 3 })
			]
		};

		var result = data.Apply(definitions);

		Assert.Equal(2, result.Count());
		Assert.All(result, x => Assert.Contains(x.Id, new[] { 1, 3 }));
	}

	[Fact]
	public void Apply_Filter_NotIn_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters =
			[
				new ("Id", FilterOperator.Nin, new[] { 1, 3 })
			]
		};

		var result = data.Apply(definitions);

		Assert.Single(result);
		Assert.DoesNotContain(result.Single().Id, new[] { 1, 3 });
	}

	[Fact]
	public void Apply_Filter_Has_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters =
			[
				new ("Tags", FilterOperator.Has, "tag2")
			]
		};

		var result = data.Apply(definitions);

		Assert.Equal(2, result.Count());
		Assert.All(result, x => Assert.Contains("tag2", x.Tags));
	}

	[Fact]
	public void Apply_Filter_NHas_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters =
			[
				new ("Tags", FilterOperator.Nhas, "tag2")
			]
		};

		var result = data.Apply(definitions);

		Assert.Single(result);
		Assert.DoesNotContain("tag2", result.Single().Tags);
	}

	[Fact]
	public void Apply_Orders_ShouldOrderCorrectly()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Orders =
			[
				new ("Date", false)
			]
		};

		var result = data.Apply(definitions);

		Assert.Equal(3, result.Count());
		Assert.Equal(3, result.First().Id);
		Assert.Equal(1, result.Last().Id);
	}

	[Fact]
	public void Apply_FiltersAndOrders_ShouldFilterAndOrderCorrectly()
	{
		var data = _data
			.Append(new TestEntity { Id = 4, Name = "Alice", Date = new DateTime(2023, 4, 1) })
			.AsQueryable();

		var definitions = new Definitions
		{
			Filters =
			[
				new ("Name", FilterOperator.Eq, "Alice")
			],
			Orders =
			[
				new ("Date", true)
			]
		};

		var result = data.Apply(definitions);

		Assert.Equal(2, result.Count());
		Assert.All(result, x => Assert.Equal("Alice", x.Name));
		Assert.Equal(4, result.First().Id);
		Assert.Equal(1, result.Last().Id);
	}

	[Fact]
	public void Apply_WithOverrides_ShouldFilterCorrectlyWithOverrides()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters =
			[
				new ("Name", FilterOperator.Eq, "Bobby")
			]
		};

		var overrides = new Overrides<TestEntity>
		{
			Filter =
			[
				new(x => x.Name, x => x.Name == "Bob" ? "Bobby" : x.Name)
			]
		};

		var result = data.Apply(definitions, overrides);

		Assert.Single(result);
		Assert.Equal("Bob", result.First().Name);
	}

	[Fact]
	public void Apply_WithOverrides_ShouldFilterAndOrderCorrectlyWithOverrides()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters = [
				new("Id", FilterOperator.Gt, 100)
			],
			Orders = [
				new("Id", true)
			]
		};

		var overrides = new Overrides<TestEntity>
		{
			Filter = [
				new(x => x.Id, x => x.Id + 100)
			],
			Order =
			[
				new(x => x.Id, x => x.Id * -1)
			]
		};

		var result = data.Apply(definitions, overrides);

		Assert.Equal(3, result.Count());
		Assert.Equal("Alice", result.First().Name);
		Assert.Equal("Charlie", result.Last().Name);
	}

	[Fact]
	public void Apply_WithOverridesChangeType_ShouldOrderCorrectlyWithOverrides()
	{
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters = [
				new("Id", FilterOperator.Sw, "a")
			],
			Orders = [
				new("Id", true)
			]
		};

		var overrides = new Overrides<TestEntity>
		{
			Filter =
			[
				new(x => x.Id, x => "a" + x.ToString())
			],
			Order =
			[
				new(x => x.Id, x => (100 - x.Id).ToString() + "b")
			]
		};

		var result = data.Apply(definitions, overrides);

		Assert.Equal(3, result.Count());
		Assert.Equal("Alice", result.First().Name);
		Assert.Equal("Charlie", result.Last().Name);
	}
}

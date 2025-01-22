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
		public Address Address { get; set; } = null!;
		public Status Status { get; set; }
	}

	public class Address
	{
		public string Street { get; set; } = null!;
		public City City { get; set; } = null!;
	}

	public class City
	{
		public string Name { get; set; } = null!;
	}

	public enum Status { Active, Inactive }

	private static readonly List<TestEntity> _data =
	[
		new TestEntity {
				Id = 1,
				Name = "Alice",
				Date = new DateTime(2023, 3, 1),
				Tags = ["tag1", "tag2"],
				Address = new Address {
					Street = "Main St",
					City = new City { Name = "Metropolis" }
				},
				Status = Status.Active
			},
			new TestEntity {
				Id = 2,
				Name = "Bob",
				Date = new DateTime(2023, 2, 1),
				Tags = ["tag2", "tag3"],
				Address = new Address {
					Street = "Oak Rd",
					City = new City { Name = "Gotham" }
				},
				Status = Status.Inactive
			},
			new TestEntity {
				Id = 3,
				Name = "Charlie",
				Date = new DateTime(2023, 1, 1),
				Tags = ["tag3", "tag4"],
				Address = new Address {
					Street = "Pine Ave",
					City = new City { Name = "Star City" }
				},
				Status = Status.Active
			}
	];

	#region Filter Tests
	[Fact]
	public void Apply_NoFilters_ReturnsOriginalData()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions { Filters = [] };

		var result = data.Apply(definitions);

		Assert.Equal(_data.Count, result.Count());
	}

	[Fact]
	public void Apply_SingleFilter_ReturnsFilteredResults()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("Name", FilterOperator.Eq, "Alice")]
		};

		var result = data.Apply(definitions).ToList();

		Assert.Single(result);
		Assert.Equal("Alice", result[0].Name);
	}

	[Fact]
	public void Apply_MultipleFilters_CombinesWithAndLogic()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [
				new("Tags", FilterOperator.Has, "tag2"),
				new("Status", FilterOperator.Eq, Status.Active)
			]
		};

		var result = data.Apply(definitions).ToList();

		Assert.Single(result);
		Assert.Equal(1, result[0].Id);
	}

	[Fact]
	public void Apply_FilterOverride_AppliesOverride()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("Status", FilterOperator.Eq, "Active")]
		};

		var overrides = new Overrides<TestEntity>
		{
			Filter = [
				new (x => x.Status, x => x.Status == Status.Active ? "Active" : "Inactive")
			]
		};

		var result = data.Apply(definitions, overrides).ToList();

		Assert.Equal(2, result.Count);
		Assert.All(result, x => Assert.Equal(Status.Active, x.Status));
	}

	[Fact]
	public void Apply_NestedPropertyFilter_WorksCorrectly()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("Address.Street", FilterOperator.Sw, "Main")]
		};

		var result = data.Apply(definitions).ToList();

		Assert.Single(result);
		Assert.Equal("Main St", result[0].Address.Street);
	}
	#endregion

	#region Order Tests
	[Fact]
	public void Apply_NoOrders_ReturnsOriginalOrder()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions { Orders = [] };

		var result = data.Apply(definitions).ToList();

		Assert.Equal(_data.Select(x => x.Id), result.Select(x => x.Id));
	}

	[Fact]
	public void Apply_SingleOrder_OrdersCorrectly()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Orders = [new("Date", true)] // Descending
		};

		var result = data.Apply(definitions).ToList();

		Assert.Equal([1, 2, 3], result.Select(x => x.Id));
	}

	[Fact]
	public void Apply_MultipleOrders_OrdersCorrectly()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Orders = [
				new("Status", false), // Ascending
                new("Id", true)       // Descending
			]
		};

		var result = data.Apply(definitions).ToList();

		Assert.Equal([3, 1, 2], result.Select(x => x.Id));
	}

	[Fact]
	public void Apply_OrderOverride_AppliesOverride()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Orders = [new("Address.City.Name", false)]
		};

		var overrides = new Overrides<TestEntity>
		{
			Order = [new(x => x.Address.City.Name, x => x.Address.Street)]
		};

		var result = data.Apply(definitions, overrides).ToList();

		Assert.Equal("Main St", result[0].Address.Street);
		Assert.Equal("Oak Rd", result[1].Address.Street);
		Assert.Equal("Pine Ave", result[2].Address.Street);
	}

	[Fact]
	public void Apply_NestedPropertyOrder_WorksCorrectly()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Orders = [new("Address.City.Name", true)]
		};

		var result = data.Apply(definitions).ToList();

		Assert.Equal("Star City", result[0].Address.City.Name);
		Assert.Equal("Metropolis", result[1].Address.City.Name);
		Assert.Equal("Gotham", result[2].Address.City.Name);
	}
	#endregion

	#region Combined Tests
	[Fact]
	public void Apply_FiltersAndOrders_CombinesCorrectly()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("Status", FilterOperator.Eq, Status.Active)],
			Orders = [new("Date", true)]
		};

		var result = data.Apply(definitions).ToList();

		Assert.Equal(2, result.Count);
		Assert.Equal(1, result[0].Id);
		Assert.Equal(3, result[1].Id);
	}

	[Fact]
	public void Apply_ComplexOverride_AppliesBothFilterAndOrderOverrides()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("Status", FilterOperator.Eq, "Active")],
			Orders = [new("Address.Street", false)]
		};

		var overrides = new Overrides<TestEntity>
		{
			Filter = [
				new (x => x.Status, x => x.Status == Status.Inactive ? "Active" : "Inactive")
			],
			Order = [
				new (x => x.Address.Street, x => x.Name)
			]
		};

		var result = data.Apply(definitions, overrides).ToList();

		Assert.Single(result);
		Assert.Equal("Bob", result[0].Name);
	}
	#endregion

	#region Filter Operator Coverage
	[Fact]
	public void Apply_Filter_Neq_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("Status", FilterOperator.Neq, Status.Active)]
		};

		var result = data.Apply(definitions).ToList();

		Assert.Single(result);
		Assert.Equal(Status.Inactive, result[0].Status);
	}

	[Fact]
	public void Apply_Filter_Gt_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("Id", FilterOperator.Gt, 1)]
		};

		var result = data.Apply(definitions).ToList();

		Assert.Equal(2, result.Count);
		Assert.Contains(result, x => x.Id == 2);
		Assert.Contains(result, x => x.Id == 3);
	}

	[Fact]
	public void Apply_Filter_Gte_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("Id", FilterOperator.Gte, 2)]
		};

		var result = data.Apply(definitions).ToList();

		Assert.Equal(2, result.Count);
		Assert.Contains(result, x => x.Id == 2);
		Assert.Contains(result, x => x.Id == 3);
	}

	[Fact]
	public void Apply_Filter_Lt_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("Date", FilterOperator.Lt, new DateTime(2023, 2, 1))]
		};

		var result = data.Apply(definitions).ToList();

		Assert.Single(result);
		Assert.Equal(3, result[0].Id);
	}

	[Fact]
	public void Apply_Filter_Lte_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("Date", FilterOperator.Lte, new DateTime(2023, 2, 1))]
		};

		var result = data.Apply(definitions).ToList();

		Assert.Equal(2, result.Count);
		Assert.Contains(result, x => x.Id == 2);
		Assert.Contains(result, x => x.Id == 3);
	}

	[Fact]
	public void Apply_Filter_Nhas_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("Tags", FilterOperator.Nhas, "tag2")]
		};

		var result = data.Apply(definitions).ToList();

		Assert.Single(result);
		Assert.DoesNotContain(result, x => x.Tags.Contains("tag2"));
	}

	[Fact]
	public void Apply_Filter_Nin_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("Id", FilterOperator.Nin, new[] { 1, 3 })]
		};

		var result = data.Apply(definitions).ToList();

		Assert.Single(result);
		Assert.Equal(2, result[0].Id);
	}

	[Fact]
	public void Apply_Filter_Nsw_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("Name", FilterOperator.Nsw, "A")]
		};

		var result = data.Apply(definitions).ToList();

		Assert.Equal(2, result.Count);
		Assert.DoesNotContain(result, x => x.Name.StartsWith("A"));
	}

	[Fact]
	public void Apply_Filter_Ew_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("Name", FilterOperator.Ew, "e")]
		};

		var result = data.Apply(definitions).ToList();

		Assert.Equal(2, result.Count);
		Assert.Contains(result, x => x.Name == "Alice");
		Assert.Contains(result, x => x.Name == "Charlie");
	}

	[Fact]
	public void Apply_Filter_New_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("Name", FilterOperator.New, "e")]
		};

		var result = data.Apply(definitions).ToList();

		Assert.Single(result);
		Assert.Equal("Bob", result[0].Name);
	}

	[Fact]
	public void Apply_Filter_In_ShouldFilterCorrectly()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("Status", FilterOperator.In, new[] { Status.Active })]
		};

		var result = data.Apply(definitions).ToList();

		Assert.Equal(2, result.Count);
		Assert.All(result, x => Assert.Equal(Status.Active, x.Status));
	}
	#endregion

	#region Edge Cases
	[Fact]
	public void Apply_EmptyDefinitions_ReturnsOriginalData()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions();

		var result = data.Apply(definitions).ToList();

		Assert.Equal(_data.Count, result.Count);
	}

	[Fact]
	public void Apply_InvalidPropertyPath_ThrowsArgumentException()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("Invalid.Path", FilterOperator.Eq, "value")]
		};

		Assert.Throws<ArgumentException>(() => data.Apply(definitions).ToList());
	}

	[Fact]
	public void Apply_EnumFilter_WorksWithStringValues()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("Status", FilterOperator.Eq, "Active")]
		};

		var result = data.Apply(definitions).ToList();

		Assert.Equal(2, result.Count);
		Assert.All(result, x => Assert.Equal(Status.Active, x.Status));
	}

	[Fact]
	public void Apply_EnumFilter_WithInvalidString_ThrowsException()
	{
		var data = _data.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("Status", FilterOperator.Eq, "InvalidStatus")]
		};

		Assert.Throws<InvalidOperationException>(() => data.Apply(definitions).ToList());
	}

	public class EdgeCaseEntity
	{
		public int[] ArrayProperty { get; set; } = [];
		public List<int> ListProperty { get; set; } = [];
		public object UnsupportedProperty => new();
	}

	[Fact]
	public void Apply_InvalidOverrideSelector_ThrowsArgumentException()
	{
		// Arrange
		var data = _data.AsQueryable();
		var definitions = new Definitions();
		var overrides = new Overrides<TestEntity>
		{
			Filter = [
				new Overrides<TestEntity>.Override<object, object>(
					x => 42, // Invalid non-property selector
					x => true
				)
			]
		};

		// Act & Assert
		Assert.Throws<ArgumentException>(() => data.Apply(definitions, overrides));
	}

	[Fact]
	public void Apply_InvalidFilterOperator_ThrowsException()
	{
		// Arrange
		var data = _data.AsQueryable();

		var definitions = new Definitions
		{
			Filters = [new("Id", (FilterOperator)int.MaxValue, 1)]
		};

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => data.Apply(definitions).ToList());
	}

	[Fact]
	public void Apply_TypeMismatchContains_ThrowsException()
	{
		// Arrange
		var data = new List<EdgeCaseEntity> { new() { ListProperty = [1, 2, 3] } }.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("ListProperty", FilterOperator.Has, "string")]
		};

		// Act & Assert
		Assert.Throws<ArgumentException>(() => data.Apply(definitions).ToList());
	}

	[Fact]
	public void Apply_ArrayPropertyFilter_WorksCorrectly()
	{
		// Arrange
		var data = new List<EdgeCaseEntity>
		{
			new() { ArrayProperty = [1, 2] },
			new() { ArrayProperty = [3, 4] }
		}.AsQueryable();

		var definitions = new Definitions
		{
			Filters = [new("ArrayProperty", FilterOperator.Has, 3)]
		};

		// Act
		var result = data.Apply(definitions).ToList();

		// Assert
		Assert.Single(result);
		Assert.Contains(3, result[0].ArrayProperty);
	}

	[Fact]
	public void Apply_UnsupportedContainsOperation_ThrowsException()
	{
		// Arrange
		var data = new List<EdgeCaseEntity> { new() }.AsQueryable();
		var definitions = new Definitions
		{
			Filters = [new("UnsupportedProperty", FilterOperator.Has, "test")]
		};

		// Act & Assert
		Assert.Throws<InvalidOperationException>(() => data.Apply(definitions).ToList());
	}
	#endregion
}
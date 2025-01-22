using System.Linq.Expressions;
using System.Reflection;
using static ByteAether.QueryLink.Definitions;

namespace ByteAether.QueryLink;

/// <summary>
/// Provides extension methods for applying filter and order definitions to an IQueryable.
/// </summary>
public static class QueryableExtensions
{
	/// <summary>
	/// Applies the filter and order definitions to the source queryable.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the source queryable.</typeparam>
	/// <param name="source">The source queryable to apply the definitions to.</param>
	/// <param name="definitions">The definitions containing filters and orders.</param>
	/// <param name="overrides">Optional overrides for filters and orders.</param>
	/// <returns>The modified queryable with filters and orders applied.</returns>
	public static IQueryable<T> Apply<T>(
		this IQueryable<T> source,
		Definitions definitions,
		Overrides<T>? overrides = null
	) => source
		.FilterByDefs(definitions.Filters, overrides?.Filter ?? [])
		.OrderByDefs(definitions.Orders, overrides?.Order ?? []);

	private static string GetFieldName(Expression expression)
	{
		expression = GetTrueExpression(expression);

		return expression switch
		{
			MemberExpression memberExpression => GetMemberPath(memberExpression),
			_ => throw new ArgumentException(
				"Invalid expression type. Only MemberExpression is supported for overrides.",
				nameof(expression)
			)
		};
	}

	private static string GetMemberPath(MemberExpression? expression)
	{
		var path = new List<string>();
		while (expression != null)
		{
			path.Add(expression.Member.Name);
			expression = expression.Expression as MemberExpression;
		}
		path.Reverse();
		return string.Join(".", path);
	}

	private static Expression GetTrueExpression(Expression expression)
		=> expression switch
		{
			UnaryExpression unaryExpression => GetTrueExpression(unaryExpression.Operand),
			_ => expression
		};

	private static (Expression, ParameterExpression[]) BuildPropertyExpression<T>(
		ParameterExpression param,
		string propertyPath,
		Dictionary<string, Overrides<T>.Override<object, object>> overridesByPath
	)
	{
		if (overridesByPath.TryGetValue(propertyPath, out var or))
		{
			return (GetTrueExpression(or.ValueReplace.Body), [.. or.ValueReplace.Parameters]);
		}

		Expression body = param;
		foreach (var part in propertyPath.Split('.'))
		{
			var prop = body.Type.GetProperty(part) ??
				throw new ArgumentException($"Property {part} in path {propertyPath} not found on {body.Type.Name}");
			body = Expression.Property(body, prop);
		}

		return (body, [param]);
	}

	#region Filtering
	private static Expression GetFilterExpression(
		Expression valueSelector,
		ConstantExpression checkedValue,
		FilterOperator operation
	) => operation switch
	{
		FilterOperator.Eq => Expression.Equal(valueSelector, checkedValue),
		FilterOperator.Neq => Expression.NotEqual(valueSelector, checkedValue),

		FilterOperator.Gt => Expression.GreaterThan(valueSelector, checkedValue),
		FilterOperator.Gte => Expression.GreaterThanOrEqual(valueSelector, checkedValue),

		FilterOperator.Lt => Expression.LessThan(valueSelector, checkedValue),
		FilterOperator.Lte => Expression.LessThanOrEqual(valueSelector, checkedValue),

		FilterOperator.Has => ExpressionContains(valueSelector, checkedValue),
		FilterOperator.Nhas => Expression.Not(ExpressionContains(valueSelector, checkedValue)),

		FilterOperator.In => ExpressionContains(checkedValue, valueSelector),
		FilterOperator.Nin => Expression.Not(ExpressionContains(checkedValue, valueSelector)),

		FilterOperator.Sw => Expression.Call(valueSelector, typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!, checkedValue),
		FilterOperator.Nsw => Expression.Not(Expression.Call(valueSelector, typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!, checkedValue)),

		FilterOperator.Ew => Expression.Call(valueSelector, typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string)])!, checkedValue),
		FilterOperator.New => Expression.Not(Expression.Call(valueSelector, typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string)])!, checkedValue)),

		_ => throw new InvalidOperationException($"Operation {operation} is not supported!")
	};

	private static readonly MethodInfo _stringContains = typeof(string)
		.GetMethod(nameof(string.Contains), [typeof(string)])!;
	private static readonly MethodInfo _enumerableContains = typeof(Enumerable)
		.GetMethods()
		.Single(x => x.Name == nameof(Enumerable.Contains) && x.GetParameters().Length == 2);

	private static MethodCallExpression ExpressionContains(Expression containerValue, Expression searchValue)
	{
		try
		{
			// Handle string.Contains first
			if (containerValue.Type == typeof(string))
			{
				return Expression.Call(containerValue, _stringContains, searchValue);
			}

			// Fallback to Enumerable.Contains
			var containsMethodGeneric = _enumerableContains.MakeGenericMethod(searchValue.Type);
			return Expression.Call(null, containsMethodGeneric, containerValue, searchValue);
		}
		catch (Exception e)
		{
			throw new InvalidOperationException(
				$"Type {containerValue.Type} does not support Contains for {searchValue.Type}",
				e
			);
		}
	}

	private static IQueryable<T> FilterByDefs<T>(
		this IQueryable<T> source,
		IEnumerable<FilterDefinition<object?>> filterDefs,
		IEnumerable<Overrides<T>.Override<object, object>> overrides
	)
	{
		var param = Expression.Parameter(typeof(T), "x");
		var overridesByPath = overrides.ToDictionary(x => GetFieldName(x.Selector.Body), x => x);

		foreach (var def in filterDefs)
		{
			var (expBody, expParams) = BuildPropertyExpression(param, def.Name, overridesByPath);

			// Get the actual type we need to use for comparison
			var (targetType, value) = ResolveTargetTypeAndValue(expBody.Type, def.Value, def.Operation);

			var checkedValue = Expression.Constant(value, targetType);
			var conditionExpression = GetFilterExpression(expBody, checkedValue, def.Operation);
			var conditionLambda = Expression.Lambda<Func<T, bool>>(conditionExpression, expParams);

			source = source.Where(conditionLambda);
		}

		return source;
	}

	private static (Type TargetType, object? Value) ResolveTargetTypeAndValue(
		Type propertyType,
		object? value,
		FilterOperator operation
	)
	{
		// Handle enum conversion from string first
		if (propertyType.IsEnum && value is string stringValue)
		{
			try
			{
				return (propertyType, Enum.Parse(propertyType, stringValue, ignoreCase: true));
			}
			catch
			{
				throw new InvalidOperationException(
					$"Value '{stringValue}' is not valid for enum type {propertyType.Name}");
			}
		}

		// Existing logic for IN/NIN operations
		if (operation is FilterOperator.In or FilterOperator.Nin)
		{
			return (
				value?.GetType() ?? typeof(object),
				value
			);
		}

		// Handle collection properties
		if (propertyType.IsGenericType &&
			propertyType.GetGenericTypeDefinition() == typeof(List<>))
		{
			var elementType = propertyType.GetGenericArguments()[0];
			return (elementType, value);
		}

		if (propertyType.IsArray)
		{
			var elementType = propertyType.GetElementType();
			return (elementType!, value);
		}

		// Default case
		return (propertyType, value);
	}
	#endregion

	#region Sorting
	private static IQueryable<T> OrderByDefs<T>(
		this IQueryable<T> source,
		IEnumerable<OrderDefinition> orderDefs,
		IEnumerable<Overrides<T>.Override<object, object>> overrides
	)
	{
		var param = Expression.Parameter(typeof(T), "x");
		var overridesByPath = overrides.ToDictionary(x => GetFieldName(x.Selector.Body), x => x);

		// Check if source has existing orderings
		var hasExistingOrder = source.Expression.Type == typeof(IOrderedQueryable<T>);

		foreach (var def in orderDefs)
		{
			var (expBody, expParams) = BuildPropertyExpression(param, def.Name, overridesByPath);
			var selector = Expression.Lambda<Func<T, object>>(
				Expression.Convert(expBody, typeof(object)), expParams
			);

			if (hasExistingOrder)
			{
				// Append to existing order with ThenBy/ThenByDescending
				source = def.IsReversed
					? ((IOrderedQueryable<T>)source).ThenByDescending(selector)
					: ((IOrderedQueryable<T>)source).ThenBy(selector);
			}
			else
			{
				// Apply first order with OrderBy/OrderByDescending
				source = def.IsReversed
					? Queryable.OrderByDescending(source, selector)
					: Queryable.OrderBy(source, selector);

				hasExistingOrder = true;
			}
		}

		return source;
	}
	#endregion
}
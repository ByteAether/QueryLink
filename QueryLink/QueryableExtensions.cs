using System.Linq.Expressions;
using System.Reflection;
using static QueryLink.Definitions;

namespace QueryLink;

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
		=> expression switch
		{
			MemberExpression memberExpression => memberExpression.Member.Name,
			UnaryExpression unaryExpression => GetFieldName(unaryExpression.Operand),
			_ => throw new ArgumentException(
				"Invalid expression type. Only MemberExpression and UnaryExpression are supported.",
				nameof(expression)
			)
		};

	private static Expression GetTrueExpression(Expression expression)
		=> expression switch
		{
			UnaryExpression unaryExpression => GetTrueExpression(unaryExpression.Operand),
			_ => expression
		};

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
	private static readonly MethodInfo _ofType = typeof(Enumerable)
		.GetMethods()
		.Single(m => m.Name == nameof(Enumerable.OfType) && m.GetParameters().Length == 1);

	private static MethodCallExpression ExpressionContains(Expression containerValue, Expression searchValue)
	{
		try
		{
			if (containerValue.Type == typeof(string))
			{
				return Expression.Call(containerValue, _stringContains, searchValue);
			}
			else
			{
				var ofTypeMethod = _ofType.MakeGenericMethod(searchValue.Type);
				var container = Expression.Call(ofTypeMethod, containerValue);

				var containsMethod = _enumerableContains.MakeGenericMethod(searchValue.Type);

				return Expression.Call(null, containsMethod, container, searchValue);
			}
		}
		catch (Exception e)
		{
			throw new InvalidOperationException(
				$"Type {containerValue.Type} does not contain a 'Contains' method for type {searchValue.Type}",
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

		foreach (var def in filterDefs)
		{
			var prop = typeof(T).GetProperty(def.Name)
				?? throw new ArgumentException($"Property {def.Name} does not exist in queryable object of {typeof(T).Name}!");

			Expression expBody = Expression.MakeMemberAccess(param, prop);
			ParameterExpression[] expParams = [param];

			var or = overrides.FirstOrDefault(
				x => GetFieldName(x.Selector.Body) == def.Name
			);

			if (or != null)
			{
				expBody = GetTrueExpression(or.ValueReplace.Body);
				expParams = or.ValueReplace.Parameters.ToArray();
			}

			var conditionExpression = GetFilterExpression(expBody, Expression.Constant(def.Value), def.Operation);
			var conditionLambda = Expression.Lambda<Func<T, bool>>(conditionExpression, expParams);

			source = source.Where(conditionLambda);
		}

		return source;
	}
	#endregion

	#region Sorting
	private static readonly MethodInfo _orderByMethod = typeof(Queryable).GetMethods()
		.First(x => x.Name == nameof(Queryable.OrderBy) && x.GetParameters().Length == 2);
	private static readonly MethodInfo _orderByDescendingMethod = typeof(Queryable).GetMethods()
		.First(x => x.Name == nameof(Queryable.OrderByDescending) && x.GetParameters().Length == 2);
	private static readonly MethodInfo _thenByMethod = typeof(Queryable).GetMethods()
		.First(x => x.Name == nameof(Queryable.ThenBy) && x.GetParameters().Length == 2);
	private static readonly MethodInfo _thenByDescendingMethod = typeof(Queryable).GetMethods()
		.First(x => x.Name == nameof(Queryable.ThenByDescending) && x.GetParameters().Length == 2);

	private static IQueryable<T> OrderByDefs<T>(
		this IQueryable<T> source,
		IEnumerable<OrderDefinition> orderDefs,
		IEnumerable<Overrides<T>.Override<object, object>> overrides
	)
	{
		var param = Expression.Parameter(typeof(T), "x");

		foreach (var def in orderDefs)
		{
			var prop = typeof(T).GetProperty(def.Name)
				?? throw new ArgumentException($"Property {def.Name} does not exist in queryable object of {typeof(T).Name}!");

			Expression expBody = Expression.Property(param, prop);
			ParameterExpression[] expParams = [param];

			var or = overrides.FirstOrDefault(
				x => GetFieldName(x.Selector.Body) == def.Name
			);

			if (or != null)
			{
				expBody = GetTrueExpression(or.ValueReplace.Body);
				expParams = or.ValueReplace.Parameters.ToArray();
			}

			var selector = Expression.Lambda(expBody, expParams);

			var method = (source is IOrderedQueryable<T>, def.IsReversed) switch
			{
				(true, false) => _orderByMethod,
				(true, true) => _orderByDescendingMethod,
				(false, false) => _thenByMethod,
				(false, true) => _thenByDescendingMethod
			};

			var genericMethod = method.MakeGenericMethod(typeof(T), expBody.Type);

			var call = Expression.Call(null, genericMethod, source.Expression, Expression.Quote(selector));

			source = source.Provider.CreateQuery<T>(call);
		}

		return source;
	}
	#endregion
}
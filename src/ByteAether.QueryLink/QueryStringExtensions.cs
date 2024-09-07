using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using static ByteAether.QueryLink.Definitions;

namespace ByteAether.QueryLink;

/// <summary>
/// Provides extension methods for converting Definitions to and from query strings.
/// </summary>
public static class QueryStringExtensions
{
	private static readonly (FilterOperator Operator, string StringValue)[] _filterOperatorPairs
		= ((FilterOperator[])Enum.GetValues(typeof(FilterOperator)))
		.Select(x => (
			Operator: x,
			StringValue: typeof(FilterOperator)
				.GetField(x.ToString())?
				.GetCustomAttribute<DescriptionAttribute>()?
				.Description ?? x.ToString()
		))
		.ToArray();

	private static readonly Regex _filterSplitter = new(
		$"({string.Join(
			'|',
			_filterOperatorPairs
				.Select(x => x.StringValue)
				.OrderByDescending(x => x.Length)
				.Select(Regex.Escape)
		)})",
		RegexOptions.Compiled
	);

	/// <summary>
	/// Converts the definitions to a query string.
	/// </summary>
	/// <param name="definitions">The definitions to convert.</param>
	/// <param name="filterKey">The key for filter definitions in the query string. Default is "filter".</param>
	/// <param name="orderKey">The key for order definitions in the query string. Default is "order".</param>
	/// <returns>The query string representation of the definitions.</returns>
	public static string ToQueryString(
		this Definitions definitions,
		string filterKey = "filter",
		string orderKey = "order"
	) => new StringBuilder()
		.Append(
			string.Join('&', definitions.Filters
			.Select(ToQueryString)
			.Select(x => $"{filterKey}[]={x}")
			)
		)
		.Append(definitions.Orders.Any() ? $"&{orderKey}=" : string.Empty)
		.Append(
			string.Join(',', definitions.Orders
				.Select(ToQueryString)
			)
		)
		.ToString();

	/// <summary>
	/// Creates a Definitions object from a query string.
	/// </summary>
	/// <param name="queryString">The query string to parse.</param>
	/// <param name="filterKey">The key for filter definitions in the query string. Default is "filter".</param>
	/// <param name="orderKey">The key for order definitions in the query string. Default is "order".</param>
	/// <returns>The Definitions object created from the query string.</returns>
	public static Definitions FromQueryString(
		string queryString,
		string filterKey = "filter",
		string orderKey = "order"
	)
	{
		var parsedQuery = HttpUtility.ParseQueryString(queryString);
		var queryParams = parsedQuery.AllKeys
			.SelectMany(x => parsedQuery.GetValues(x)!.Select(y => (Key: x, Value: y)));

		var filters = new List<FilterDefinition<object?>>();
		var orders = new List<OrderDefinition>();

		foreach (var queryParam in queryParams)
		{
			if (queryParam.Key == $"{filterKey}[]")
			{
				var valParts = _filterSplitter.Split(queryParam.Value, 2);
				var op = OperatorFromString(valParts[1]);
				var filterValue = StringValueParser.Parse(valParts[2]);

				filters.Add(new FilterDefinition<object?>(
					valParts[0],
					OperatorFromString(valParts[1]),
					filterValue
				));
			}
			else if (queryParam.Key == orderKey)
			{
				orders.AddRange(queryParam.Value
					.Split(',')
					.Select(part =>
					{
						var isReversed = part[0] == '-';
						var fieldName = part.TrimStart('-', '+');
						return new OrderDefinition(fieldName, isReversed);
					})
				);
			}
		}

		return new Definitions()
		{
			Filters = filters,
			Orders = orders
		};
	}

	private static string ToQueryString(OrderDefinition def)
		=> HttpUtility.UrlEncode((def.IsReversed ? "-" : string.Empty) + def.Name);

	private static string ToQueryString<T>(FilterDefinition<T> def)
	{
		var valueSet =
			new[] { FilterOperator.Has, FilterOperator.Nhas, FilterOperator.In, FilterOperator.Nin }.Contains(def.Operation)
			&& def.Value!.GetType() != typeof(string)
				? "[" + string.Join(',', (def.Value as IEnumerable)!.OfType<object>().Select(x => x.ToString()?.Replace(",", "\\,"))) + "]"
				: def.Value?.ToString();

		return HttpUtility.UrlEncode($"{def.Name}{OperatorToString(def.Operation)}{valueSet}");
	}

	private static string OperatorToString(FilterOperator op)
		=> _filterOperatorPairs.Single(x => x.Operator == op).StringValue;

	private static FilterOperator OperatorFromString(string operatorString)
		=> _filterOperatorPairs.Single(x => x.StringValue == operatorString).Operator;
}

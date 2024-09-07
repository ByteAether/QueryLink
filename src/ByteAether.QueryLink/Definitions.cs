using System.ComponentModel;

namespace ByteAether.QueryLink;

/// <summary>
/// A collection of filter and order definitions.
/// </summary>
public class Definitions
{
	/// <summary>
	/// Gets or sets the collection of filter definitions.
	/// </summary>
	public IEnumerable<FilterDefinition<object?>> Filters { get; set; } = [];

	/// <summary>
	/// Gets or sets the collection of order definitions.
	/// </summary>
	public IEnumerable<OrderDefinition> Orders { get; set; } = [];

	/// <summary>
	/// Defines the operators used in filter definitions.
	/// </summary>
	public enum FilterOperator
	{
		/// <summary>
		/// Equals operator.
		/// </summary>
		[Description("=")] Eq,

		/// <summary>
		/// Not equals operator.
		/// </summary>
		[Description("!=")] Neq,

		/// <summary>
		/// Greater than operator.
		/// </summary>
		[Description(">")] Gt,

		/// <summary>
		/// Greater than or equals operator.
		/// </summary>
		[Description(">=")] Gte,

		/// <summary>
		/// Less than operator.
		/// </summary>
		[Description("<")] Lt,

		/// <summary>
		/// Less than or equals operator.
		/// </summary>
		[Description("<=")] Lte,

		/// <summary>
		/// Has operator.
		/// </summary>
		[Description("=*")] Has,

		/// <summary>
		/// Not has operator.
		/// </summary>
		[Description("!*")] Nhas,

		/// <summary>
		/// In operator.
		/// </summary>
		[Description("[]")] In,

		/// <summary>
		/// Not in operator.
		/// </summary>
		[Description("![]")] Nin,

		/// <summary>
		/// Starts with operator.
		/// </summary>
		[Description("^")] Sw,

		/// <summary>
		/// Not starts with operator.
		/// </summary>
		[Description("!^")] Nsw,

		/// <summary>
		/// Ends with operator.
		/// </summary>
		[Description("$")] Ew,

		/// <summary>
		/// Not ends with operator.
		/// </summary>
		[Description("!$")] New,
	}

	/// <summary>
	/// A base filter definition.
	/// </summary>
	public abstract record FilterDefinition();

	/// <summary>
	/// A filter definition with a specific type.
	/// </summary>
	/// <typeparam name="T">The type of the filter value.</typeparam>
	/// <param name="Name">The name of the filter.</param>
	/// <param name="Operation">The filter operation.</param>
	/// <param name="Value">The value of the filter.</param>
	public record FilterDefinition<T>(string Name, FilterOperator Operation, T Value)
		: FilterDefinition;

	/// <summary>
	/// An order definition.
	/// </summary>
	/// <param name="Name">The name of the order.</param>
	/// <param name="IsReversed">Indicates whether the order is reversed.</param>
	public record OrderDefinition(string Name, bool IsReversed);
}

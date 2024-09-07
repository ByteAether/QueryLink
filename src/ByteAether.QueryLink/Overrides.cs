using System.Linq.Expressions;

namespace ByteAether.QueryLink;

/// <summary>
/// Overrides for filter and order definitions.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public class Overrides<T>
{
	/// <summary>
	/// Gets or sets the collection of filter overrides.
	/// </summary>
	public IEnumerable<Override<object, object>> Filter { get; set; } = [];

	/// <summary>
	/// Gets or sets the collection of order overrides.
	/// </summary>
	public IEnumerable<Override<object, object>> Order { get; set; } = [];

	/// <summary>
	/// An override for a filter or order definition.
	/// </summary>
	/// <typeparam name="TVal">The type of the value to be overridden.</typeparam>
	/// <typeparam name="TValReplace">The type of the value to replace with.</typeparam>
	/// <param name="Selector">The selector expression for the value to be overridden.</param>
	/// <param name="ValueReplace">The expression for the value to replace with.</param>
	public record Override<TVal, TValReplace>(
		Expression<Func<T, TVal>> Selector,
		Expression<Func<T, TValReplace>> ValueReplace
	);
}

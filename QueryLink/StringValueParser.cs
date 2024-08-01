using System.Globalization;
using System.Text.RegularExpressions;

namespace QueryLink;
internal static partial class StringValueParser
{
	private static readonly List<Func<string, object?>> _parsers = [
		s => int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : null,
		s => double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : null,
		s => bool.TryParse(s, out var val) ? val : null,
		s => Guid.TryParse(s, out var val) ? val : null,
		s => TimeSpan.TryParse(s, out var val) ? val : null,
		s => DateTime.TryParse(s, out var val) ? val : null,
		s => TryParseArray(s, out var val) ? val : null,
		s => s.Trim()
	];

	public static object? Parse(string s)
		=> s == null
			? null
			: _parsers
				.Select(p => p(s))
				.First(p => p != null)!;

	private static bool TryParseArray(string s, out object? outVal)
	{
		outVal = null;
		if (!s.StartsWith('[') || !s.EndsWith(']'))
		{
			return false;
		}

		var elements = GetCSVSplitRegex()
			.Split(s[1..^1])
			.Select(x => x.Replace("\\,", ","))
			.ToList();

		if (elements.Count == 1 && elements.First() == string.Empty)
		{
			outVal = Array.Empty<object>();
			return true;
		}

		var vals = _parsers
			.Select(p =>
			{
				var parsed = elements.Select(e => p(e));
				return parsed.Any(x => x == null)
					? (IEnumerable<object>?)null
					: parsed;
			})
			.FirstOrDefault(x => x != null)
			?.Select(x => x!)
			.ToArray();

		if (vals == null)
		{
			return false;
		}

		var typedVals = Array.CreateInstance(vals.First().GetType(), vals.Length);
		Array.Copy(vals, typedVals, vals.Length);

		outVal = typedVals;

		return true;
	}

#if NET8_0_OR_GREATER
	[GeneratedRegex(@"(?<!\\),(?![^\[\]]*\])", RegexOptions.Compiled)]
	private static partial Regex GetCSVSplitRegex();
#else
	private static readonly Regex _regex = new(@"(?<!\\),(?![^\[\]]*\])", RegexOptions.Compiled);
	private static Regex GetCSVSplitRegex() => _regex;
#endif
}
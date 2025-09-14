using Brigadier.NET.Context;
using Brigadier.NET.Exceptions;
using Brigadier.NET.Suggestion;

namespace Brigadier.NET.ArgumentTypes;
public class EnumArgumentType<T> : IArgumentType<T> where T : struct, Enum
{
	public T Parse(IStringReader reader)
	{
		var input = reader.ReadUnquotedString();
		if (Enum.TryParse<T>(input, ignoreCase: true, out var level))
		{
			return level;
		}

		throw CommandSyntaxException.BuiltInExceptions.LiteralIncorrect().Create(input);
	}

	public IEnumerable<string> Examples => Enum.GetNames(typeof(T));

	public Task<Suggestions> ListSuggestions<TSource>(CommandContext<TSource> context, SuggestionsBuilder builder)
	{
		var remaining = builder.RemainingLowerCase;

		// Suggest enum names matching the current partial input (case-insensitive)
		foreach (var name in Enum.GetNames(typeof(T)))
		{
			if (string.IsNullOrEmpty(remaining) || name.StartsWith(remaining, StringComparison.OrdinalIgnoreCase))
			{
				builder.Suggest(name);
			}
		}

		return builder.BuildFuture();
	}
}

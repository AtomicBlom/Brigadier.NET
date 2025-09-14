using Brigadier.NET.Context;
using Brigadier.NET.Exceptions;
using Brigadier.NET.Suggestion;

namespace Brigadier.NET.ArgumentTypes;

[PublicAPI]
public interface IArgumentType<out T> 
{
	/// <exception cref="CommandSyntaxException"></exception>
	T Parse(IStringReader reader);

	T Parse<TSource>(StringReader reader, TSource source)
	{
		return Parse(reader);
	}

	Task<Suggestions> ListSuggestions<TSource>(CommandContext<TSource> context, SuggestionsBuilder builder)
	{
		return Suggestions.Empty();
	}

	IEnumerable<string> Examples => [];
}
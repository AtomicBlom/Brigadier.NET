using Brigadier.NET.Context;
using Brigadier.NET.Suggestion;

namespace Brigadier.NET.ArgumentTypes;

[PublicAPI]
public class BoolArgumentType : IArgumentType<bool>
{
	private static readonly IEnumerable<string> BoolExamples = ["true", "false"];

	internal BoolArgumentType()
	{
	}

	public bool Parse(IStringReader reader)
	{
		return reader.ReadBoolean();
	}

	public Task<Suggestions> ListSuggestions<TSource>(CommandContext<TSource> context, SuggestionsBuilder builder)
	{
		if ("true".StartsWith(builder.RemainingLowerCase))
		{
			builder.Suggest("true");
		}
		if ("false".StartsWith(builder.RemainingLowerCase))
		{
			builder.Suggest("false");
		}
		return builder.BuildAsync();
	}

	public IEnumerable<string> Examples => BoolExamples;
}
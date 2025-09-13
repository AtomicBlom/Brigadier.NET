using System.Collections.Generic;
using System.Threading.Tasks;
using Brigadier.NET.Context;
using Brigadier.NET.Suggestion;

namespace Brigadier.NET.ArgumentTypes
{
	public class BoolArgumentType : ArgumentType<bool>
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
			return builder.BuildFuture();
		}

		public IEnumerable<string> Examples => BoolExamples;
	}
}

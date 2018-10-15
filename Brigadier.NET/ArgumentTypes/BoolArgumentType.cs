using System.Collections.Generic;
using System.Threading.Tasks;
using Brigadier.NET.Context;
using Brigadier.NET.Suggestion;

namespace Brigadier.NET.ArgumentTypes
{
	public class BoolArgumentType : ArgumentType<bool>
	{
		private static readonly IEnumerable<string> BoolExamples = new[] {"true", "false"};

		internal BoolArgumentType()
		{
		}

		public override bool Parse(IStringReader reader)
		{
			return reader.ReadBoolean();
		}

		public override Suggestions ListSuggestions<TSource>(CommandContext<TSource> context, SuggestionsBuilder builder)
		{
			if ("true".StartsWith(builder.Remaining.ToLower()))
			{
				builder.Suggest("true");
			}
			if ("false".StartsWith(builder.Remaining.ToLower()))
			{
				builder.Suggest("false");
			}
			return builder.Build();
		}

		public override IEnumerable<string> Examples => BoolExamples;
	}
}

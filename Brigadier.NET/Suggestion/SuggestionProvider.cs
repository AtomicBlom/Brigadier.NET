using System.Threading.Tasks;
using Brigadier.NET.Context;

namespace Brigadier.NET.Suggestion
{
	public delegate Suggestions SuggestionProvider<TSource>(CommandContext<TSource> context, SuggestionsBuilder builder);
}

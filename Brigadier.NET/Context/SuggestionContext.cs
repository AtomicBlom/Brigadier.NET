using Brigadier.NET.Tree;

namespace Brigadier.NET.Context;

[PublicAPI]
public class SuggestionContext<TSource>
{
	public readonly CommandNode<TSource> Parent;
	public readonly int StartPos;

	public SuggestionContext(CommandNode<TSource> parent, int startPos)
	{
		Parent = parent;
		StartPos = startPos;
	}
}
using System.Collections.Generic;
using Brigadier.NET.Tree;

namespace Brigadier.NET
{
	public delegate void AmbiguityConsumer<TSource>(CommandNode<TSource> parent, CommandNode<TSource> child, CommandNode<TSource> sibling,
		IEnumerable<string> inputs);

}

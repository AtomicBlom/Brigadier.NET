using Brigadier.NET.Context;
using Brigadier.NET.Exceptions;

namespace Brigadier.NET
{
	///<exception cref="CommandSyntaxException" />
	public delegate TSource SingleRedirectModifier<TSource>(CommandContext<TSource> context);
}

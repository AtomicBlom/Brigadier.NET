using Brigadier.NET.Context;

namespace Brigadier.NET
{
	public delegate IList<TSource> RedirectModifier<TSource>(CommandContext<TSource> context);
}

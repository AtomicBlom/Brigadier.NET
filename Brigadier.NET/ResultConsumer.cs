using Brigadier.NET.Context;

namespace Brigadier.NET
{
	public delegate void ResultConsumer<TSource>(CommandContext<TSource> context, bool success, int result);
}

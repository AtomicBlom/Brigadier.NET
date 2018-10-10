using Brigadier.NET.Context;

namespace Brigadier.NET
{
    public delegate int Command<TSource>(CommandContext<TSource> context);
}

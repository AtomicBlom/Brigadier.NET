namespace Brigadier.NET.Builder
{
	// ReSharper disable once UnusedTypeParameter
	// TSource is used for inferring argument builder generic parameters, Resharper doesn't seem to realize this.
	public interface IRootArgumentContext<TSource>
	{
	}

	public interface IChildArgumentContext<TSource> : IRootArgumentContext<TSource>
	{

	}

	public struct ArgumentContext<TSource> : IChildArgumentContext<TSource>
	{

	}
}

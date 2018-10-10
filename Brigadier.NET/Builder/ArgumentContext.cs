namespace Brigadier.NET.Builder
{
	// ReSharper disable once UnusedTypeParameter
	// TSource is used for inferring argument builder generic parameters, Resharper doesn't seem to realize this.
	public interface IArgumentContext<TSource>
	{

	}

	public struct ArgumentContext<TSource> : IArgumentContext<TSource>
	{

	}
}

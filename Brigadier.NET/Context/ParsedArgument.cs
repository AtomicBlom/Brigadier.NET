namespace Brigadier.NET.Context;

[PublicAPI]
public interface IParsedArgument
{
	object Result { get; }
}

[PublicAPI]
public struct ParsedArgument<TSource, T> : IParsedArgument, IEquatable<ParsedArgument<TSource, T>> where T : notnull
{
	private readonly T _result;

	public ParsedArgument(int start, int end, T result)
	{
		Range = StringRange.Between(start, end);
		_result = result;
	}

	public StringRange Range { get; }

	public object Result => _result;

	public override bool Equals(object? obj)
	{
		return obj is ParsedArgument<TSource, T> other 
		       && Equals(other);
	}

	public bool Equals(ParsedArgument<TSource, T> other)
	{
		return Range.Equals(other.Range) 
		       && EqualityComparer<T>.Default.Equals(_result, other._result);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Range, _result);
	}
}
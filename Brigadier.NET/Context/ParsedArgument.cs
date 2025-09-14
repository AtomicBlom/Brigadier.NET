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
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		return obj is ParsedArgument<TSource, T> other && Equals(other);
	}

	public bool Equals(ParsedArgument<TSource, T> other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		return Equals(Range, other.Range) && EqualityComparer<T>.Default.Equals(_result, other._result);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Range, _result);
	}
}
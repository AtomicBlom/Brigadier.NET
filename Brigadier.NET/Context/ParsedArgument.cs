using System;
using System.Collections.Generic;
using Brigadier.NET.Util;

namespace Brigadier.NET.Context
{

	public interface IParsedArgument
	{
		object Result { get; }
	}

	public struct ParsedArgument<TSource, T> : IParsedArgument, IEquatable<ParsedArgument<TSource, T>>
	{
		private readonly T _result;

		public ParsedArgument(int start, int end, T result)
		{
			Range = StringRange.Between(start, end);
			_result = result;
		}

		public StringRange Range { get; }

		public object Result => _result;

		public override bool Equals(object obj)
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
			return HashCode.Start
				.Hash(Range)
				.Hash(_result);
		}
	}
}

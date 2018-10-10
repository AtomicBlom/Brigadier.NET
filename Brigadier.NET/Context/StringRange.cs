using System;
using Brigadier.NET.Util;

namespace Brigadier.NET.Context
{
	public struct StringRange
	{
		public StringRange(int start, int end)
		{
			Start = start;
			End = end;
		}

		public static StringRange At(int pos)
		{
			return new StringRange(pos, pos);
		}

		public static StringRange Between(int start, int end)
		{
			return new StringRange(start, end);
		}

		public static StringRange Encompassing(StringRange a, StringRange b)
		{
			return new StringRange(Math.Min(a.Start, b.Start), Math.Max(a.End, b.End));
		}

		public int Start { get; }

		public int End { get; }

		public string Get(IImmutableStringReader reader)
		{
			return reader.String.Substring(Start, End - Start);
		}

		public string Get(string source)
		{
			return source.Substring(Start, End - Start);
		}


		public bool IsEmpty => Start == End;

		public int Length => End - Start;

		public override bool Equals(object o)
		{
			if (!(o is StringRange)) {
				return false;
			}
			var that = (StringRange)o;
			return Start == that.Start && End == that.End;
		}

		public override int GetHashCode()
		{
			return HashCode.Start
				.Hash(Start)
				.Hash(End);
		}

		public override string ToString()
		{
			return $"StringRange{{start={Start}, end={End}}}";
		}
	}
}

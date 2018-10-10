using System;
using Brigadier.NET.Context;
using Brigadier.NET.Util;

namespace Brigadier.NET.Suggestion
{
	public class IntegerSuggestion : Suggestion, IEquatable<IntegerSuggestion>
	{
		public IntegerSuggestion(StringRange range, int value, IMessage tooltip = null)
			: base(range, value.ToString(), tooltip)
		{
			Value = value;
		}

		public int Value { get; }

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is IntegerSuggestion other 
			       && Equals(other);
		}

		public bool Equals(IntegerSuggestion other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Value == other.Value 
			       && base.Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Start
				.Hash(Value);
		}

		public override string ToString()
		{
			return $"IntegerSuggestion{{value={Value}, range={Range}, text='{Text}', tooltip='{Tooltip}'}}";
		}

		public override int CompareTo(Suggestion o)
		{
			if (o is IntegerSuggestion integerSuggestion)
			{
				return integerSuggestion.Value.CompareTo(Value);
			}
			return base.CompareTo(o);
		}

		public override int CompareToIgnoreCase(Suggestion b)
		{
			return CompareTo(b);
		}
	}
}

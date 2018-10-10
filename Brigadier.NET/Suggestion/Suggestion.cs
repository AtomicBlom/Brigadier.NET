using System;
using System.Text;
using Brigadier.NET.Context;
using Brigadier.NET.Util;

namespace Brigadier.NET.Suggestion
{
	public class Suggestion : IComparable<Suggestion>, IEquatable<Suggestion>
	{
		public Suggestion(StringRange range, string text, IMessage tooltip = null)
		{
			Range = range;
			Text = text;
			Tooltip = tooltip;
		}

		public StringRange Range { get; }

		public string Text { get; }

		public IMessage Tooltip { get; }

		public string Apply(string input)
		{
			if (Range.Start == 0 && Range.End == input.Length)
			{
				return Text;
			}
			var result = new StringBuilder();
			if (Range.Start > 0)
			{
				result.Append(input.Substring(0, Range.Start));
			}
			result.Append(Text);
			if (Range.End < input.Length)
			{
				result.Append(input.Substring(Range.End));
			}
			return result.ToString();
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is Suggestion other 
			       && Equals(other);
		}

		public bool Equals(Suggestion other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(Range, other.Range) 
			       && string.Equals(Text, other.Text) 
			       && Equals(Tooltip, other.Tooltip);
		}

		public override int GetHashCode()
		{
			return HashCode.Start
				.Hash(Range)
				.Hash(Text)
				.Hash(Tooltip);
		}

		public override string ToString()
		{
			return $"Suggestion{{range={Range}, text='{Text}', tooltip='{Tooltip}}}";
		}

		public virtual int CompareTo(Suggestion o)
		{
			return String.Compare(Text, o.Text, StringComparison.Ordinal);
		}

		public virtual int CompareToIgnoreCase(Suggestion b)
		{
			return string.Compare(Text, b.Text, StringComparison.OrdinalIgnoreCase);
		}

		public Suggestion Expand(string command, StringRange range)
		{
			if (range.Equals(Range))
			{
				return this;
			}

			var result = new StringBuilder();
			if (range.Start < Range.Start)
			{
				result.Append(command.Substring(range.Start, Range.Start - range.Start));
			}
			result.Append(Text);
			if (range.End > Range.End)
			{
				result.Append(command.Substring(Range.End, range.End - Range.End));
			}
			return new Suggestion(range, result.ToString(), Tooltip);
		}
	}
}

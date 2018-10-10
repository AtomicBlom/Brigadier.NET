using System.Collections.Generic;
using Brigadier.NET.Context;
using Brigadier.NET.Exceptions;

namespace Brigadier.NET.ArgumentTypes
{
	public class LongArgumentType : ArgumentType<long>
	{
		private static readonly IEnumerable<string> LongExamples = new [] {"0", "123", "-123"};

		internal LongArgumentType(long minimum, long maximum)
		{
			Minimum = minimum;
			Maximum = maximum;
		}

		public long Minimum { get; }

		public long Maximum { get; }

		/// <exception cref="CommandSyntaxException" />
		public override long Parse(IStringReader reader)
		{
			var start = reader.Cursor;
			var result = reader.ReadLong();
	        if (result < Minimum) {
				reader.Cursor = start;
				throw CommandSyntaxException.BuiltInExceptions.LongTooLow().CreateWithContext(reader, result, Minimum);
			}
	        if (result > Maximum) {
				reader.Cursor = start;
				throw CommandSyntaxException.BuiltInExceptions.LongTooHigh().CreateWithContext(reader, result, Maximum);
			}
	        return result;
		}

		public override bool Equals(object o)
		{
			if (this == o) return true;
			if (!(o is LongArgumentType)) return false;

			var that = (LongArgumentType)o;
			return Maximum == that.Maximum && Minimum == that.Minimum;
		}

		public override int GetHashCode()
		{
			return 31 * Minimum.GetHashCode() + Maximum.GetHashCode();
		}

		public override string ToString()
		{
			if (Minimum == long.MinValue && Maximum == long.MaxValue)
			{
				return "longArg()";
			}
			else if (Maximum == long.MaxValue)
			{
				return $"longArg({Minimum})";
			}
			else
			{
				return $"longArg({Minimum}, {Maximum})";
			}
		}

		public override IEnumerable<string> Examples => LongExamples;
	}
}

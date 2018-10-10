using System.Collections.Generic;
using Brigadier.NET.Exceptions;

namespace Brigadier.NET.ArgumentTypes
{
	public class IntegerArgumentType : ArgumentType<int>
	{
		private static readonly IEnumerable<string> IntegerExamples = new [] {"0", "123", "-123"};

		internal IntegerArgumentType(int minimum, int maximum)
		{
			Minimum = minimum;
			Maximum = maximum;
		}

		public int Minimum { get; }

		public int Maximum { get; }

		///<exception cref="CommandSyntaxException" />
		public override int Parse(IStringReader reader)
		{
			var start = reader.Cursor;
			var result = reader.ReadInt();
	        if (result < Minimum)
	        {
				reader.Cursor = start;
				throw CommandSyntaxException.BuiltInExceptions.IntegerTooLow().CreateWithContext(reader, result, Minimum);
			}
	        if (result > Maximum)
	        {
		        reader.Cursor = start;
				throw CommandSyntaxException.BuiltInExceptions.IntegerTooHigh().CreateWithContext(reader, result, Maximum);
			}
	        return result;
		}

		public override bool Equals(object o)
		{
			if (this == o) return true;
			if (!(o is IntegerArgumentType)) return false;

			var that = (IntegerArgumentType)o;
			return Maximum == that.Maximum && Minimum == that.Minimum;
		}

		public override int GetHashCode()
		{
			return 31 * Minimum + Maximum;
		}

		public override string ToString()
		{
			if (Minimum == int.MinValue && Maximum == int.MaxValue)
			{
				return "integer()";
			}
			else if (Maximum == int.MaxValue)
			{
				return "integer(" + Minimum + ")";
			}
			else
			{
				return "integer(" + Minimum + ", " + Maximum + ")";
			}
		}

		public override IEnumerable<string> Examples => IntegerExamples;
	}
}

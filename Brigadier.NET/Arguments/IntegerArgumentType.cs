using System.Collections.Generic;
using Brigadier.NET.Context;
using Brigadier.NET.Exceptions;

namespace Brigadier.NET.Arguments
{
	public class IntegerArgumentType : ArgumentType<int>
	{
		private static readonly IEnumerable<string> IntegerExamples = new [] {"0", "123", "-123"};

		private IntegerArgumentType(int minimum, int maximum)
		{
			Minimum = minimum;
			Maximum = maximum;
		}

		public static IntegerArgumentType Integer(int min = int.MinValue, int max = int.MaxValue)
		{
			return new IntegerArgumentType(min, max);
		}

		public static int GetInteger<TSource>(CommandContext<TSource> context, string name)
		{
			return context.GetArgument<int>(name);
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

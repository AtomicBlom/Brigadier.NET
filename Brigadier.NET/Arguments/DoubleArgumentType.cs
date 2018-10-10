using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Brigadier.NET.Context;
using Brigadier.NET.Exceptions;

namespace Brigadier.NET.Arguments
{
	public class DoubleArgumentType : ArgumentType<double>
	{
		private static readonly IEnumerable<string> DoubleExamples = new [] {"0", "1.2", ".5", "-1", "-.5", "-1234.56"};

		private DoubleArgumentType(double minimum, double maximum)
		{
			Minimum = minimum;
			Maximum = maximum;
		}

		public static DoubleArgumentType DoubleArg(double min = -double.MaxValue, double max = double.MaxValue)
		{
			return new DoubleArgumentType(min, max);
		}

		public static double GetDouble<TSource>(CommandContext<TSource> context, string name)
		{
			return context.GetArgument<double>(name);
		}

		public double Minimum { get; }

		public double Maximum { get; }


		/// <exception cref="CommandSyntaxException" />
		public override double Parse(IStringReader reader) 
		{
			var start = reader.Cursor;
			var result = reader.ReadDouble();
	        if (result < Minimum)
	        {
		        reader.Cursor = start;
				throw CommandSyntaxException.BuiltInExceptions.DoubleTooLow().CreateWithContext(reader, result, Minimum);
			}
	        if (result > Maximum) {
				reader.Cursor = start;
				throw CommandSyntaxException.BuiltInExceptions.DoubleTooHigh().CreateWithContext(reader, result, Maximum);
			}
	        return result;
		}

		[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
		public override bool Equals(object o)
		{
			if (this == o) return true;
			if (!(o is DoubleArgumentType)) return false;

			var that = (DoubleArgumentType)o;
			return Maximum == that.Maximum && Minimum == that.Minimum;
		}

		public override int GetHashCode()
		{
			return (int)(31 * Minimum + Maximum);
		}

		[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
		public override string ToString()
		{
			if (Minimum == -double.MaxValue && Maximum == double.MaxValue)
			{
				return "double()";
			}
			else if (Maximum == double.MaxValue)
			{
				return $"double({Minimum:#.0})";
			}
			else
			{
				return $"double({Minimum:#.0}, {Maximum:#.0})";
			}
		}

		public override IEnumerable<string> Examples => DoubleExamples;
	}
}

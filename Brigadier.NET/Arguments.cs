using System;
using Brigadier.NET.ArgumentTypes;
using Brigadier.NET.Context;

namespace Brigadier.NET
{
	public class Arguments
	{
		public static IntegerArgumentType Integer(int min = Int32.MinValue, int max = Int32.MaxValue)
		{
			return new IntegerArgumentType(min, max);
		}

		public static int GetInteger<TSource>(CommandContext<TSource> context, string name)
		{
			return context.GetArgument<int>(name);
		}

		public static BoolArgumentType Bool()
		{
			return new BoolArgumentType();
		}

		public static bool GetBool<TSource>(CommandContext<TSource> context, string name)
		{
			return context.GetArgument<bool>(name);
		}

		public static DoubleArgumentType Double(double min = -System.Double.MaxValue, double max = System.Double.MaxValue)
		{
			return new DoubleArgumentType(min, max);
		}

		public static double GetDouble<TSource>(CommandContext<TSource> context, string name)
		{
			return context.GetArgument<double>(name);
		}

		public static FloatArgumentType Float(float min = -Single.MaxValue, float max = Single.MaxValue)
		{
			return new FloatArgumentType(min, max);
		}

		public static float GetFloat<TSource>(CommandContext<TSource> context, string name)
		{
			return context.GetArgument<float>(name);
		}

		public static LongArgumentType Long(long min = Int64.MinValue, long max = Int64.MaxValue)
		{
			return new LongArgumentType(min, max);
		}

		public static long GetLong<TSource>(CommandContext<TSource> context, string name)
		{
			return context.GetArgument<long>(name);
		}

		public static StringArgumentType Word()
		{
			return new StringArgumentType(StringArgType.SingleWord);
		}

		public static StringArgumentType String()
		{
			return new StringArgumentType(StringArgType.QuotablePhrase);
		}

		public static StringArgumentType GreedyString()
		{
			return new StringArgumentType(StringArgType.GreedyPhrase);
		}

		public static string GetString<TSource>(CommandContext<TSource> context, string name)
		{
			return context.GetArgument<string>(name);
		}
	}
}

using Brigadier.NET.Exceptions;

namespace Brigadier.NET.ArgumentTypes;

[PublicAPI]
public class FloatArgumentType : IArgumentType<float>
{
	private static readonly IEnumerable<string> FloatExamples = ["0", "1.2", ".5", "-1", "-.5", "-1234.56"];

	internal FloatArgumentType(float minimum, float maximum)
	{
		Minimum = minimum;
		Maximum = maximum;
	}

	public float Minimum { get; }
	public float Maximum { get; }

	/// <exception cref="CommandSyntaxException"></exception>
	public float Parse(IStringReader reader)
	{
		var start = reader.Cursor;
		var result = reader.ReadFloat();
		if (result < Minimum) {
			reader.Cursor = start;
			throw CommandSyntaxException.BuiltInExceptions.FloatTooLow().CreateWithContext(reader, result, Minimum);
		}
		if (result > Maximum) {
			reader.Cursor = start;
			throw CommandSyntaxException.BuiltInExceptions.FloatTooHigh().CreateWithContext(reader, result, Maximum);
		}
		return result;
	}

	public IEnumerable<string> Examples => FloatExamples;


	[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
	public override bool Equals(object? o)
	{
		if (this == o) return true;
		if (o is not FloatArgumentType that) return false;

		return Maximum == that.Maximum && Minimum == that.Minimum;
	}

	public override int GetHashCode()
	{
		return (int)(31 * Minimum + Maximum);
	}

	[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
	public override string ToString()
	{
		if (Minimum == -float.MaxValue && Maximum == float.MaxValue)
		{
			return "float()";
		}
		else if (Maximum == float.MaxValue)
		{
			return $"float({Minimum:#.0})";
		}
		else
		{
			return $"float({Minimum:#.0}, {Maximum:#.0})";
		}
	}
}
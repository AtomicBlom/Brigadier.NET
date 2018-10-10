using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Brigadier.NET.Context;
using Brigadier.NET.Exceptions;

namespace Brigadier.NET.ArgumentTypes
{
	public class FloatArgumentType : ArgumentType<float>
	{
		private static readonly IEnumerable<string> FloatExamples = new [] {"0", "1.2", ".5", "-1", "-.5", "-1234.56"};

		private readonly float _minimum;
		private readonly float _maximum;

		internal FloatArgumentType(float minimum, float maximum)
		{
			_minimum = minimum;
			_maximum = maximum;
		}

		public float Minimum() => _minimum;

		public float Maximum() => _maximum;

		/// <exception cref="CommandSyntaxException"></exception>
		public override float Parse(IStringReader reader)
		{
			var start = reader.Cursor;
			var result = reader.ReadFloat();
	        if (result < _minimum) {
				reader.Cursor = start;
				throw CommandSyntaxException.BuiltInExceptions.FloatTooLow().CreateWithContext(reader, result, _minimum);
			}
	        if (result > _maximum) {
				reader.Cursor = start;
				throw CommandSyntaxException.BuiltInExceptions.FloatTooHigh().CreateWithContext(reader, result, _maximum);
			}
	        return result;
		}

		[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
		public override bool Equals(object o)
		{
			if (this == o) return true;
			if (!(o is FloatArgumentType)) return false;

			var that = (FloatArgumentType)o;
			return _maximum == that._maximum && _minimum == that._minimum;
		}

		public override int GetHashCode()
		{
			return (int)(31 * _minimum + _maximum);
		}

		[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
		public override string ToString()
		{
			if (_minimum == -float.MaxValue && _maximum == float.MaxValue)
			{
				return "float()";
			}
			else if (_maximum == float.MaxValue)
			{
				return $"float({_minimum:#.0})";
			}
			else
			{
				return $"float({_minimum:#.0}, {_maximum:#.0})";
			}
		}

		public override IEnumerable<string> Examples => FloatExamples;
	}
}

using System.Collections.Generic;

namespace Brigadier.NET.Util
{
	internal struct HashCode
	{
		private readonly int _value;

		private HashCode(int value) => _value = value;

		public static HashCode Start { get; } = new HashCode(17);

		public static implicit operator int(HashCode hash) => hash._value;

		public HashCode Hash<T>(T obj)
		{
			var h = EqualityComparer<T>.Default.GetHashCode(obj);
			return unchecked(new HashCode((_value * 397) ^ h));
		}

		public override int GetHashCode() => _value;
	}

}

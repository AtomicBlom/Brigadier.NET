using System;
using Brigadier.NET.Tree;
using FluentAssertions;
using FluentAssertions.Numeric;

namespace Brigadier.NET.Tests
{
	class EqualsTester
	{
		public EqualsTester AddEqualityGroup<T>(params T[] equivalents)
		{
			foreach (var equivalent in equivalents)
			{
				foreach (var equivalent1 in equivalents)
				{
					if (ReferenceEquals(equivalent, equivalent1)) continue;
					equivalent.Should().Be(equivalent1);
				}
			}

			return this;
		}

		public void TestEquals()
		{
			//No-Op. 'cause I'm lazy
		}
	}

	public static class TestNodeExtensions
	{
		public static ComparableTypeAssertions<CommandNode<TArg>> Should<TArg>(this CommandNode<TArg> obj)
		{
			return ((IComparable<CommandNode<TArg>>)obj).Should();
		}
	}
}

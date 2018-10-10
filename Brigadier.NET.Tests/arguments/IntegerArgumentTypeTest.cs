// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Brigadier.NET.Exceptions;
using FluentAssertions;
using Xunit;
using static Brigadier.NET.Arguments;

namespace Brigadier.NET.Tests.arguments
{
	public class IntegerArgumentTypeTest {
		[Fact]
		public void Parse(){
			var reader = new StringReader("15");
			Integer().Parse(reader).Should().Be(15);
			reader.CanRead().Should().Be(false);
		}

		[Fact]
		public void parse_tooSmall(){
			var reader = new StringReader("-5");
			Integer(0, 100).Invoking(l => l.Parse(reader))
				.Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.IntegerTooLow())
				.Where(ex => ex.Cursor == 0);
		}

		[Fact]
		public void parse_tooBig(){
			var reader = new StringReader("5");
			Integer(-100, 0).Invoking(l => l.Parse(reader))
				.Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.IntegerTooHigh())
				.Where(ex => ex.Cursor == 0);
		}

		[Fact]
		public void TestEquals(){
			new EqualsTester()
				.AddEqualityGroup(Integer(), Integer())
				.AddEqualityGroup(Integer(-100, 100), Integer(-100, 100))
				.AddEqualityGroup(Integer(-100, 50), Integer(-100, 50))
				.AddEqualityGroup(Integer(-50, 100), Integer(-50, 100))
				.TestEquals();
		}

		[Fact]
		public void TestToString(){
			Integer().ToString().Should().BeEquivalentTo("integer()");
			Integer(-100).ToString().Should().BeEquivalentTo("integer(-100)");
			Integer(-100, 100).ToString().Should().BeEquivalentTo("integer(-100, 100)");
			Integer(int.MinValue, 100).ToString().Should().BeEquivalentTo("integer(-2147483648, 100)");
		}
	}
}
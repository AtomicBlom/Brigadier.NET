// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Brigadier.NET.Arguments;
using Brigadier.NET.Exceptions;
using FluentAssertions;
using Xunit;

namespace Brigadier.NET.Tests.arguments
{
	public class DoubleArgumentTypeTest {
		[Fact]
		public void Parse(){
			var reader = new StringReader("15");
			DoubleArgumentType.DoubleArg().Parse(reader).Should().Be(15.0);
			reader.CanRead().Should().Be(false);
		}

		[Fact]
		public void parse_tooSmall(){
			var reader = new StringReader("-5");
			DoubleArgumentType.DoubleArg(0, 100).Invoking(l => l.Parse(reader))
				.Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DoubleTooLow())
				.Where(ex => ex.Cursor == 0);
		}

		[Fact]
		public void parse_tooBig(){
			var reader = new StringReader("5");
			DoubleArgumentType.DoubleArg(-100, 0).Invoking(l => l.Parse(reader))
				.Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DoubleTooHigh())
				.Where(ex => ex.Cursor == 0);
		}

		[Fact]
		public void TestEquals(){
			new EqualsTester()
				.AddEqualityGroup(DoubleArgumentType.DoubleArg(), DoubleArgumentType.DoubleArg())
				.AddEqualityGroup(DoubleArgumentType.DoubleArg(-100, 100), DoubleArgumentType.DoubleArg(-100, 100))
				.AddEqualityGroup(DoubleArgumentType.DoubleArg(-100, 50), DoubleArgumentType.DoubleArg(-100, 50))
				.AddEqualityGroup(DoubleArgumentType.DoubleArg(-50, 100), DoubleArgumentType.DoubleArg(-50, 100))
				.TestEquals();
		}

		[Fact]
		public void TestToString(){
			DoubleArgumentType.DoubleArg().ToString().Should().BeEquivalentTo("double()");
			DoubleArgumentType.DoubleArg(-100).ToString().Should().BeEquivalentTo("double(-100.0)");
			DoubleArgumentType.DoubleArg(-100, 100).ToString().Should().BeEquivalentTo("double(-100.0, 100.0)");
			DoubleArgumentType.DoubleArg(int.MinValue, 100).ToString().Should().BeEquivalentTo("double(-2147483648.0, 100.0)");
		}
	}
}
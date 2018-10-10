// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Brigadier.NET.Exceptions;
using FluentAssertions;
using Xunit;

namespace Brigadier.NET.Tests.arguments
{
	public class DoubleArgumentTypeTest {
		[Fact]
		public void Parse(){
			var reader = new StringReader("15");
			Arguments.Double().Parse(reader).Should().Be(15.0);
			reader.CanRead().Should().Be(false);
		}

		[Fact]
		public void parse_tooSmall(){
			var reader = new StringReader("-5");
			Arguments.Double(0, 100).Invoking(l => l.Parse(reader))
				.Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DoubleTooLow())
				.Where(ex => ex.Cursor == 0);
		}

		[Fact]
		public void parse_tooBig(){
			var reader = new StringReader("5");
			Arguments.Double(-100, 0).Invoking(l => l.Parse(reader))
				.Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.DoubleTooHigh())
				.Where(ex => ex.Cursor == 0);
		}

		[Fact]
		public void TestEquals(){
			new EqualsTester()
				.AddEqualityGroup(Arguments.Double(), Arguments.Double())
				.AddEqualityGroup(Arguments.Double(-100, 100), Arguments.Double(-100, 100))
				.AddEqualityGroup(Arguments.Double(-100, 50), Arguments.Double(-100, 50))
				.AddEqualityGroup(Arguments.Double(-50, 100), Arguments.Double(-50, 100))
				.TestEquals();
		}

		[Fact]
		public void TestToString(){
			Arguments.Double().ToString().Should().BeEquivalentTo("double()");
			Arguments.Double(-100).ToString().Should().BeEquivalentTo("double(-100.0)");
			Arguments.Double(-100, 100).ToString().Should().BeEquivalentTo("double(-100.0, 100.0)");
			Arguments.Double(int.MinValue, 100).ToString().Should().BeEquivalentTo("double(-2147483648.0, 100.0)");
		}
	}
}
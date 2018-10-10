// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Brigadier.NET.Arguments;
using Brigadier.NET.Exceptions;
using FluentAssertions;
using Xunit;

namespace Brigadier.NET.Tests.arguments
{
	public class FloatArgumentTypeTest {
		[Fact]
		public void Parse(){
			var reader = new StringReader("15");
			FloatArgumentType.FloatArg().Parse(reader).Should().Be(15f);
			reader.CanRead().Should().Be(false);
		}

		[Fact]
		public void parse_tooSmall(){
			var reader = new StringReader("-5");
			FloatArgumentType.FloatArg(0, 100).Invoking(l => l.Parse(reader))
				.Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.FloatTooLow())
				.Where(ex => ex.Cursor == 0);
		}

		[Fact]
		public void parse_tooBig(){
			var reader = new StringReader("5");
			FloatArgumentType.FloatArg(-100, 0).Invoking(l => l.Parse(reader))
				.Should().Throw<CommandSyntaxException>()
				.Where(ex => ex.Type == CommandSyntaxException.BuiltInExceptions.FloatTooHigh())
				.Where(ex => ex.Cursor == 0);
		}

		[Fact]
		public void TestEquals(){
			new EqualsTester()
				.AddEqualityGroup(FloatArgumentType.FloatArg(), FloatArgumentType.FloatArg())
				.AddEqualityGroup(FloatArgumentType.FloatArg(-100, 100), FloatArgumentType.FloatArg(-100, 100))
				.AddEqualityGroup(FloatArgumentType.FloatArg(-100, 50), FloatArgumentType.FloatArg(-100, 50))
				.AddEqualityGroup(FloatArgumentType.FloatArg(-50, 100), FloatArgumentType.FloatArg(-50, 100))
				.TestEquals();
		}

		[Fact]
		public void TestToString(){
			FloatArgumentType.FloatArg().ToString().Should().BeEquivalentTo("float()");
			FloatArgumentType.FloatArg(-100).ToString().Should().BeEquivalentTo("float(-100.0)");
			FloatArgumentType.FloatArg(-100, 100).ToString().Should().BeEquivalentTo("float(-100.0, 100.0)");
			//WTF? I actually expected the answer to be -2147483648.0
			FloatArgumentType.FloatArg(int.MinValue, 100).ToString().Should().BeEquivalentTo("float(-2147484000.0, 100.0)");
		}
	}
}
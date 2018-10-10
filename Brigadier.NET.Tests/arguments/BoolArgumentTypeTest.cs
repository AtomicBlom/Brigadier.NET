// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Brigadier.NET.Arguments;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Brigadier.NET.Tests.arguments
{
	public class BoolArgumentTypeTest {
		private readonly BoolArgumentType _type;

		public BoolArgumentTypeTest()
		{
			_type = BoolArgumentType.Bool();
		}

		[Fact]
		public void Parse(){
			var reader = Substitute.For<IStringReader>();
			reader.ReadBoolean().Returns(true);
			_type.Parse(reader).Should().Be(true);

			reader.Received().ReadBoolean();
		}
	}
}
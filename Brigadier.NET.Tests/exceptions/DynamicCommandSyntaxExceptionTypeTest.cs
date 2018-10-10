// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using Brigadier.NET.Exceptions;
using FluentAssertions;
using Xunit;

namespace Brigadier.NET.Tests.exceptions
{
	public class DynamicCommandSyntaxExceptionTypeTest {
		private readonly DynamicCommandExceptionType _type;

		public DynamicCommandSyntaxExceptionTypeTest()
		{
			_type = new DynamicCommandExceptionType(name => new LiteralMessage("Hello, " + name + "!"));
		}

		[Fact]
		public void CreateWithContext(){
			var reader = new StringReader("Foo bar")
			{
				Cursor = 5
			};
			var exception = _type.CreateWithContext(reader, "World");
			exception.Type.Should().Be(_type);
			exception.Input.Should().Be("Foo bar");
			exception.Cursor.Should().Be(5);
		}
	}
}
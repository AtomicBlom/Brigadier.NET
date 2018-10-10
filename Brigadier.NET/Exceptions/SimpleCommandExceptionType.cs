namespace Brigadier.NET.Exceptions
{
	public class SimpleCommandExceptionType : ICommandExceptionType
	{
		private readonly IMessage _message;

		public SimpleCommandExceptionType(IMessage message)
		{
			_message = message;
		}

		public CommandSyntaxException Create()
		{
			return new CommandSyntaxException(this, _message);
		}

		public CommandSyntaxException CreateWithContext(IImmutableStringReader reader)
		{
			return new CommandSyntaxException(this, _message, reader.String, reader.Cursor);
		}

		public override string ToString()
		{
			return _message.String;
		}
	}
}

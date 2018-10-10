namespace Brigadier.NET.Exceptions
{
	public class DynamicCommandExceptionType : ICommandExceptionType
	{
		private readonly Function _function;

		public DynamicCommandExceptionType(Function function)
		{
			_function = function;
		}

		public CommandSyntaxException Create(object a)
		{
			return new CommandSyntaxException(this, _function(a));
		}

		public CommandSyntaxException CreateWithContext(IImmutableStringReader reader, object a)
		{
			return new CommandSyntaxException(this, _function(a), reader.String, reader.Cursor);
		}

		public delegate IMessage Function(object a);
	}
}

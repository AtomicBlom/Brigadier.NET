namespace Brigadier.NET.Exceptions
{
	public class DynamicNCommandExceptionType : ICommandExceptionType
	{
		private readonly Function _function;

		public DynamicNCommandExceptionType(Function function)
		{
			_function = function;
		}

		public CommandSyntaxException Create(params object[] args)
		{
			return new CommandSyntaxException(this, _function(args));
		}

		public CommandSyntaxException CreateWithContext(IImmutableStringReader reader, params object[] args)
		{
			return new CommandSyntaxException(this, _function(args), reader.String, reader.Cursor);
		}

		public delegate IMessage Function(object[] args);
	}
}

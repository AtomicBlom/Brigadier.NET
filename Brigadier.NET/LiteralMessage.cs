namespace Brigadier.NET
{
	public class LiteralMessage : IMessage
	{
		public LiteralMessage(string message)
		{
			String = message;
		}
		
		public string String { get; }

		public override string ToString()
		{
			return String;
		}
	}
}

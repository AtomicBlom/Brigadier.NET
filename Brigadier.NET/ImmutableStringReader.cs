namespace Brigadier.NET
{
	public interface IImmutableStringReader
	{
		string String { get; }

		int RemainingLength { get; }

		int TotalLength { get; }

		int Cursor { get; }

		string Read { get; }

		string Remaining { get; }

		bool CanRead(int length);

		bool CanRead();

		char Peek();

		char Peek(int offset);
	}
}

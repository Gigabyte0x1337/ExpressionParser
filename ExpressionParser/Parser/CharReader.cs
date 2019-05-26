namespace ExpressionParser
{
    public class CharReader
    {
        public string Text { get; }

        public int Position { get; set; }

        public char Current => Text.Length > Position ? Text[Position] : (char)0;

        public CharReader(string text)
        {
            Text = text;
        }

        public string GetRange(int start, int end)
        {
            return Text.Substring(start, end - start);
        }

        public void Next()
        {
            Position++;
        }
    }
}

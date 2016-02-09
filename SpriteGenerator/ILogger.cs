namespace SpriteGenerator
{
    internal interface ILogger
    {
        void Clear();
        void Log(string text, bool appendLine = false);
        void Log(string text, bool appendLine, params object[] format);
        void Log(string text, params object[] format);
    }
}
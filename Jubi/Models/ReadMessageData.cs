namespace Jubi.Models
{
    public abstract class ReadMessageData
    {
        public string Error { get; protected set; }

        public abstract bool TryParse(string str, out object result);
    }

    public class ReadMessageData<T> : ReadMessageData
    {
        public delegate bool ConvertDelegate(string str, out T result);

        public ConvertDelegate TryParseDelegate;

        public ReadMessageData(string error, ConvertDelegate convertDelegate)
        {
            TryParseDelegate = convertDelegate;
            Error = error;
        }

        public override bool TryParse(string str, out object result)
        {
            var response = TryParseDelegate(str, out T tResult);
            result = tResult;

            return response;
        }
    }
}
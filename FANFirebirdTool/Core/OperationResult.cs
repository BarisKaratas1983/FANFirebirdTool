namespace FANFirebirdTool.Core
{
    public sealed class OperationResult
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public static OperationResult Ok(string message)
        {
            return new OperationResult { Success = true, Message = message };
        }
        public static OperationResult Fail(string message)
        {
            return new OperationResult { Success = false, Message = message };
        }
    }
}
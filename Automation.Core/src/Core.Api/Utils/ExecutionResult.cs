namespace Core.Api.Utils
{
    public class ExecutionResult<TResult, TError>
        where TError : class
    {
        public ExecutionResult(TResult result, TError error)
        {
            Result = result;
            Error = error;
        }

        public TResult Result { get; private set; }

        public TError Error { get; private set; }

        public virtual bool HasError => !ReferenceEquals(Error, null);
    }
}

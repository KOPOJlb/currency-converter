namespace CurrencyConverter.ApplicationServices
{
    public static class TaskExtensions
    {
        public static async Task<TResult> ThrowIfNull<TResult, TKey>(this Task<TResult?> task, TKey id) where TResult : class
        {
            var result = await task;
            return result is null ? throw new EntityNotFoundException($"Entity {typeof(TResult)} with id {id} was not found") : result;
        }
    }

    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string message) : base(message) { }
        public EntityNotFoundException(string message, Exception e) : base(message, e) { }
    }
}

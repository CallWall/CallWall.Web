using System.Threading.Tasks;

namespace CallWall.Web.InMemoryRepository
{
    public static class TaskEx
    {
        public static Task<T> ToTask<T>(this T value)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(value);
            return tcs.Task;
        }
    }
}
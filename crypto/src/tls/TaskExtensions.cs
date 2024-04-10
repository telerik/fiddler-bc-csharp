using System;
using System.Threading;
using System.Threading.Tasks;

namespace Org.BouncyCastle.Tls
{
    public static class TaskExtensions
	{
        public static  IAsyncResult ToBegin<TResult>(this Task<TResult> task, AsyncCallback callback, object state)
        {
            var tcs = new TaskCompletionSource<TResult>(state, TaskCreationOptions.RunContinuationsAsynchronously);
            var oldContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            try
            {
                CompleteAsync(task, callback, tcs);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(oldContext);
            }

            return tcs.Task;
        }

        public static TResult ToEnd<TResult>(this IAsyncResult asyncResult)
        {
            return ((Task<TResult>)asyncResult).GetAwaiter().GetResult();
        }

        public static IAsyncResult ToBegin(this Task task, AsyncCallback callback, object state)
        {
            var tcs = new TaskCompletionSource<bool>(state, TaskCreationOptions.RunContinuationsAsynchronously);
            var oldContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            try
            {
                CompleteAsync(task, callback, tcs);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(oldContext);
            }

            return tcs.Task;
        }

        public static void ToEnd(this IAsyncResult asyncResult)
        {
            ((Task)asyncResult).GetAwaiter().GetResult();
        }

        private static async void CompleteAsync<TResult>(Task<TResult> task, AsyncCallback callback, TaskCompletionSource<TResult> tcs)
        {
            try
            {
                tcs.TrySetResult(await task.ConfigureAwait(false));
            }
            catch (OperationCanceledException ex)
            {
                tcs.TrySetCanceled(ex.CancellationToken);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
            finally
            {
                callback?.Invoke(tcs.Task);
            }
        }

        private static async void CompleteAsync(Task task, AsyncCallback callback, TaskCompletionSource<bool> tcs)
        {
            try
            {
                await task.ConfigureAwait(false);
                tcs.TrySetResult(false);
            }
            catch (OperationCanceledException ex)
            {
                tcs.TrySetCanceled(ex.CancellationToken);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
            finally
            {
                callback?.Invoke(tcs.Task);
            }
        }
    }
}
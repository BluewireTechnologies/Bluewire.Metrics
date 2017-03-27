using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ReshapeMetrics.Remote
{
    public class PostQueue
    {
        public Uri TargetUri { get; }

        public PostQueue(Uri targetUri)
        {
            this.TargetUri = targetUri;
        }

        private readonly TaskCompletionSource<object> completedAddingTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<object> completedTcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly AutoResetEvent queuedEvent = new AutoResetEvent(false);
        private readonly Queue<PostItem> queue = new Queue<PostItem>();

        public async Task<PostItem> TryDequeue(TimeSpan deadline, CancellationToken token)
        {
            var waitLimit = Task.Delay(deadline, token);
            while (true)
            {
                lock (queue)
                {
                    if (queue.Count > 0) return queue.Dequeue();
                    if (IsAddingComplete)
                    {
                        completedTcs.TrySetResult(null);
                        return null;
                    }
                }
                var complete = await Task.WhenAny(queuedEvent.AsTask(token), waitLimit);
                if (complete == waitLimit) return null;
            }
        }

        public void Enqueue(PostItem item)
        {
            lock (queue)
            {
                if (IsAddingComplete) throw new InvalidOperationException("Queue has been completed. No more items may be queued.");
                queue.Enqueue(item);
                queuedEvent.Set();
            }
        }

        public void CompleteAdding()
        {
            lock (queue)
            {
                completedAddingTcs.TrySetResult(null);
                queuedEvent.Set();
            }
        }

        public Task CompletedTask => completedTcs.Task;

        public bool IsAddingComplete => completedAddingTcs.Task.IsCompleted;
        public bool IsComplete => completedTcs.Task.IsCompleted;
    }
}

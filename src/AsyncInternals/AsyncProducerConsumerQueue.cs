using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncInternals
{
    public class AsyncProducerConsumerQueue<T> : IDisposable
    {
        private readonly Action<T> _consumerAction;
        private readonly BlockingCollection<T> _queue;
        private readonly CancellationTokenSource _cancelTokenSrc;
        // ReSharper disable once NotAccessedField.Local
        private readonly Task _processQueueTask;
        private readonly FixedSizedQueue<long> _timings;
        private bool m_isDisposed;

        public AsyncProducerConsumerQueue(Action<T> consumerAction)
        {
            _consumerAction = consumerAction ?? throw new ArgumentNullException(nameof(consumerAction));

            _queue = new BlockingCollection<T>(new ConcurrentQueue<T>());
            _cancelTokenSrc = new CancellationTokenSource();

            _timings = new FixedSizedQueue<long>(25);

            // not sure which of these options is best?
            //new Thread(() => ConsumeLoop(_cancelTokenSrc.Token)).Start();

            //var task3 = new Task(() => ConsumeLoop(_cancelTokenSrc.Token), TaskCreationOptions.LongRunning);
            //task3.Start();

            _processQueueTask = Task.Factory.StartNew(() => ConsumerLoop(_cancelTokenSrc.Token), TaskCreationOptions.LongRunning);
            
        }

        public void ProduceJob(T value)
        {
            _queue.Add(value);
        }

        public int OutstandingTaskCount { get; private set; }

        public DateTimeOffset LastJobDateTime { get; private set; }
        public long LastJobTimeTakenMs { get; private set; }
        public long RollingAverageTimeMs { get; private set; }
        public long TotalJobsCompleted { get; private set; }

        public long EstimatedCurrentCompletionTimeMs => RollingAverageTimeMs * OutstandingTaskCount;

        public void WaitForProcessingCompletion(long timeoutInMs = 10000, bool cancelAfterTimeout = false)
        {
            var timeoutDeadline = DateTimeOffset.Now.AddMilliseconds(timeoutInMs);
            while (DateTimeOffset.Now < timeoutDeadline)
            {
                Task.Factory.StartNew(() => { Thread.Sleep(250); }).Wait();
                if (OutstandingTaskCount + 1 == 0) // TODO think through this logic
                    return;
            }
            // What should happen here? Cancellation?
            if (cancelAfterTimeout)
            {
                _cancelTokenSrc.Cancel();
            }
        }

        private void ConsumerLoop(CancellationToken cancelToken)
        {
            // requirements:
            // * outstanding queue length
            // * average processing time
            // * last processed time
            // * varying priorities?
            // * Drain/Pause?
            // * Configurable reporting Action (to report using properties of T)?

            while (!cancelToken.IsCancellationRequested)
            {
                try
                {
                    OutstandingTaskCount = _queue.Count;

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    var item = _queue.Take(cancelToken);
                    _consumerAction(item);

                    stopwatch.Stop();

                    TotalJobsCompleted += 1;
                    RecordTiming(stopwatch.ElapsedMilliseconds);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
        }

        private void RecordTiming(long ellapsedMilliseconds)
        {
            // keep last n times and calc average time over last 10 and last 100
            _timings.Enqueue(ellapsedMilliseconds);

            long sumOfTimings = 0;
            foreach (var timing in _timings)
            {
                sumOfTimings += timing;
            }
            RollingAverageTimeMs = sumOfTimings / _timings.Count;
            LastJobDateTime = DateTimeOffset.Now;
            LastJobTimeTakenMs = ellapsedMilliseconds;
        }        
        
        protected virtual void Dispose(bool disposing)
        {
            if (!m_isDisposed)
            {
                if (disposing)
                {
                    _cancelTokenSrc.Cancel();
                    _cancelTokenSrc.Dispose();
                    _queue.Dispose();
                }

                m_isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
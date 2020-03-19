using AsyncInternals;
using NUnit.Framework;
using System;
using System.Threading;

namespace DataStore.Tests
{
    [TestFixture]
    public class AsyncProducerConsumerQueueTests
    {
        [Test]
        public void PropertiesShouldReturnZeroWhenNoJobsHaveBeenDone()
        {
            void TestAction(int x)
            {
                Thread.Sleep(x);
            }

            var sut = new AsyncProducerConsumerQueue<int>(TestAction);

            Assert.That(sut.LastJobTimeTakenMs, Is.EqualTo(0));
            Assert.That(sut.RollingAverageTimeMs, Is.EqualTo(0));
            Assert.That(sut.EstimatedCurrentCompletionTimeMs, Is.EqualTo(0));
            Assert.That(sut.TotalJobsCompleted, Is.EqualTo(0));
        }

        [Test]
        public void StatsShouldBeCorrectAfterWaitForProcessingCompletionRun()
        {
            var actionCounter = 0;

            void TestAction(int x)
            {
                Thread.Sleep(x);
                actionCounter += 1;
            }

            var sut = new AsyncProducerConsumerQueue<int>(TestAction);

            sut.ProduceJob(1500);
            sut.ProduceJob(50);

            sut.WaitForProcessingCompletion();
            
            Assert.That(actionCounter, Is.EqualTo(2));
            Assert.That(sut.TotalJobsCompleted, Is.EqualTo(2));
            Assert.That(sut.LastJobTimeTakenMs, Is.InRange(50, 80));
            Assert.That(sut.RollingAverageTimeMs, Is.InRange(750, 850));
            Assert.That(sut.EstimatedCurrentCompletionTimeMs, Is.EqualTo(0));

            sut.Dispose();
        }

        [Test]
        public void WaitForProcessingCompletionWithCancellationShouldCancel()
        {
            var actionCounter = 0;

            void TestAction(int x)
            {
                Thread.Sleep(x);
                actionCounter += 1;
            }

            var sut = new AsyncProducerConsumerQueue<int>(TestAction);

            sut.ProduceJob(50);
            sut.ProduceJob(50);
            sut.ProduceJob(50);
            sut.ProduceJob(50);
            sut.ProduceJob(50);
            sut.ProduceJob(50);
            sut.ProduceJob(50);

            sut.WaitForProcessingCompletion(100, true);

            Assert.That(actionCounter, Is.InRange(3, 5));
            Assert.That(sut.TotalJobsCompleted, Is.InRange(3, 5));

            sut.Dispose();
        }

        [Test]
        public void RemainingProcessingTimeShouldBeReported()
        {
            var actionCounter = 0;

            void TestAction(int x)
            {
                Thread.Sleep(x);
                actionCounter += 1;
            }

            var sut = new AsyncProducerConsumerQueue<int>(TestAction);

            sut.ProduceJob(50);
            sut.ProduceJob(50);
            sut.ProduceJob(50);
            sut.ProduceJob(50);
            sut.ProduceJob(50);
            sut.ProduceJob(50);

            Thread.Sleep(100);
            Assert.That(sut.EstimatedCurrentCompletionTimeMs, Is.InRange(200, 1000));
            Thread.Sleep(1000);

            Assert.That(actionCounter, Is.EqualTo(6));
            Assert.That(sut.TotalJobsCompleted, Is.EqualTo(6));
            Assert.That(sut.LastJobTimeTakenMs, Is.InRange(45, 100));
            Assert.That(sut.RollingAverageTimeMs, Is.InRange(45, 120));
            Assert.That(sut.EstimatedCurrentCompletionTimeMs, Is.EqualTo(0));

            sut.Dispose();
        }
    }
}

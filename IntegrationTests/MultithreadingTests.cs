using ChannelFactory.Abstract;
using ChannelReader.Abstract;
using EventChannelBuilder;
using EventChannelBuilder.Abstract;
using EventLogger;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Channels;

namespace IntegrationTests
{
    public class MultithreadingTests
    {
        /// <summary>
        /// implementation of event for testing
        /// </summary>
        private class CustomEvent : IGenericEvent<int>
        {
            public int EventType => 1;
        }

        /// <summary>
        /// implementation of event handler for testing
        /// </summary>
        private class CustomEventHandler : IGenericEventHandler<int>
        {
            public int EventType => 1;

            public async Task HandleAsync(IGenericEvent<int> @event, CancellationToken token = default)
            {
                //take some time for simulate payload
                await Task.Delay(1, token);
                await Task.CompletedTask;
            }
        }

        /// <summary>
        /// produce channel builder
        /// </summary>
        /// <typeparam name="T" <see cref="IComparable"/>>type of event</typeparam>
        /// <returns>bounded channel</returns>
        private static IChannelBuilder<T> ProduceChannelBuilder<T>()
            where T : IComparable

        {
            var logger = Substitute.For<IGenericEventDispatcherLogger>();
            var provider = new ServiceCollection()
                .AddSingleton(logger)
                .AddEventChannelBuilder<T>(Assembly.GetExecutingAssembly())
                .BuildServiceProvider();
            return provider.GetRequiredService<IChannelBuilder<T>>();
        }

        /// <summary>
        /// produce BoundedChannel
        /// </summary>
        /// <typeparam name="T" <see cref="IComparable"/>>type of event</typeparam>
        /// <returns>bounded channel</returns>
        private static IChannel<T> ProduceBoundedChannel<T>(int capacity, int readersCount, BoundedChannelFullMode fullMode)
            where T : IComparable
        {
            var builder = ProduceChannelBuilder<T>();
            return builder.Bounded()
                          .WithCapacity(capacity)
                          .WithReadersCount(readersCount)
                          .WithMultipleWriters()
                          .WithFullMode(fullMode)
                          .Build();
        }

        /// <summary>
        /// produce UnboundedChannel
        /// </summary>
        /// <typeparam name="T" <see cref="IComparable"/>>type of event</typeparam>
        /// <returns>unbounded channel</returns>
        private static IChannel<T> ProduceUnboundedChannel<T>(int readersCount)
            where T : IComparable
        {
            var builder = ProduceChannelBuilder<T>();
            return builder.Unbounded()
                          .WithReadersCount(readersCount)
                          .WithMultipleWriters()
                          .Build();
        }

        /// <summary>
        /// produce test data 
        /// </summary>
        /// <param name="channel" <see cref="IChannel{T}">>bounded or unbounded channel</param>
        /// <param name="unexpectedExceptions">container for save exception</param>
        /// <returns>container with test data</returns>
        private static List<Task> ProduceTestTasks(IChannel<int> channel,
            ConcurrentBag<Exception> unexpectedExceptions, ConcurrentBag<Exception> expectedExceptions)
        {
            var random = new Random();
            return [.. Enumerable.Range(0, 100_000).Select( i =>
            {
                var rnd = random.Next(100);
                if (rnd < 33)
                {
                    return Task.Run(()=>{
                        // We don't expect exceptions, but if they occur, we'll save them
                        try { channel.Start(); } catch (Exception e) { unexpectedExceptions.Add(e); }
                        Task.Delay(1);
                    });
                }
                else if (rnd > 33 && rnd < 66)
                {
                    return Task.Run(()=>{
                        // We don't expect exceptions, but if they occur, we'll save them
                        try { channel.Stop(); } catch (Exception e) { unexpectedExceptions.Add(e); }
                        Task.Delay(1);
                    });
                }
                else
                {
                    return Task.Run(async ()=>{
                        // We expect exceptions from Enqueue, we'll save them for check later
                        try { await channel.Enqueue(new CustomEvent()); }
                        catch (Exception e) { expectedExceptions.Add(e); }
                        await Task.Delay(1);
                    });
                }
            })];
        }

        /// <summary>
        /// Check type of exception helper
        /// </summary>
        /// <param name="exception">exception</param>
        /// <returns>if typeof exception is expected return true else return false</returns>
        private static bool IsExpectedException(Exception exception)
        {
            var exceptionType = exception.GetType();
            return exceptionType == typeof(ChannelClosedException)
                   || exceptionType == typeof(TaskCanceledException);
        }

        /// <summary>
        /// Stress test for channel operations under high concurrency conditions.
        /// Tests both bounded and unbounded channels with varying reader counts
        /// to ensure thread safety and absence of deadlocks during concurrent
        /// Start, Stop, and Enqueue operations.
        /// </summary>
        /// <param name="boundedChannel">
        /// If true, tests a bounded channel with specified capacity; 
        /// if false, tests an unbounded channel
        /// </param>
        /// <param name="readersCount">
        /// Number of reader threads to use for processing events in the channel
        /// </param>
        /// <returns>A task that represents the asynchronous stress test operation</returns>
        /// <remarks>
        /// This test performs 100,000 random operations (Start, Stop, Enqueue) across
        /// multiple threads to simulate heavy load conditions. It verifies:
        /// - No deadlocks occur during concurrent access
        /// - No unexpected exceptions are thrown
        /// - Channel remains functional after stress testing
        /// - All tasks complete successfully
        /// </remarks>
        [Theory]
        [InlineData(true, 1)]
        [InlineData(true, 4)]
        [InlineData(false, 1)]
        [InlineData(false, 4)]
        public async Task StressTest(bool boundedChannel, int readersCount)
        {
            var channel = boundedChannel
                ? ProduceBoundedChannel<int>(10, readersCount, BoundedChannelFullMode.Wait)
                : ProduceUnboundedChannel<int>(readersCount);

            var unexpectedExceptions = new ConcurrentBag<Exception>();
            var expectedExceptions = new ConcurrentBag<Exception>();
            var tasks = ProduceTestTasks(channel, unexpectedExceptions, expectedExceptions);

            //Action
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
            var completedTask = await Task.WhenAny(Task.WhenAll(tasks), timeoutTask);
            // After stress test, make sure we can start and stop channel without errors
            channel.Stop();
            channel.Start();
            var enqueueResult = channel.Enqueue(new CustomEvent());

            //Asserts
            Assert.False(timeoutTask.IsCompleted, "Test timed out - possible deadlock");
            Assert.Empty(unexpectedExceptions);
            Assert.True(enqueueResult.IsCompletedSuccessfully);
            Assert.All(expectedExceptions, e => Assert.True(IsExpectedException(e)));
            Assert.All(tasks, t => Assert.Equal(TaskStatus.RanToCompletion, t.Status));
        }
    }
}

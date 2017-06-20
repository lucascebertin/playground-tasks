using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Playground.Tasks
{
    class Program
    {
        static void Main(string[] args)
        {
            TestAsyncTasks.Run(10, 2);
            TestTasks.Run(10, 2);
            TestReactiveExtensions.Run(50, 2);
        }
    }

    public class TestReactiveExtensions
    {
        static async Task RandomConsoleOut(int n)
        {
            var randomGen = new Random();
            var randomValue = randomGen.Next(1, 5);
            await Task.Delay(TimeSpan.FromSeconds(randomValue));
            await Console.Out.WriteLineAsync($"{n}: {randomValue}");
        }

        public static void Run(int times, int merge)
        {
            Observable.Range(0, times)
                .Select(n =>
                    Observable.FromAsync(() =>
                        RandomConsoleOut(n + 1)))
                .Merge(merge)
                .Wait();
        }
    }


    public class TestAsyncTasks
    {
        static private SemaphoreSlim _semaphore;

        public static void Run(int tasksNumber, int maxConcurrency)
        {
            _semaphore = new SemaphoreSlim(maxConcurrency);

            var tasks = Enumerable.Range(0, tasksNumber)
                .Select(i =>
                    new Task<Task>(async () =>
                    {
                        await Console.Out.WriteLineAsync($"{i}");
                        await Task.Delay(5000);
                    })
                );

            foreach (var task in tasks)
            {
                _semaphore.Wait();

                task.Start();
                task.Wait();
                var innerTask = task.Result;

                innerTask.ContinueWith(t =>
                    Task.Run(() =>
                        _semaphore.Release()));
            }

            while (_semaphore.CurrentCount != maxConcurrency)
                Thread.Sleep(100);
        }

        public static void SemaphoreLock()
        {
            _semaphore = new SemaphoreSlim(2);
            _semaphore.Wait();
            _semaphore.Wait();
            _semaphore.Wait();
        }
    }


    public class TestTasks
    {
        static private SemaphoreSlim _semaphore;

        public static void Run(int tasksNumber, int maxConcurrency)
        {
            _semaphore = new SemaphoreSlim(maxConcurrency);

            var tasks = Enumerable.Range(0, tasksNumber)
                .Select(i =>
                    new Task(
                        () =>
                        {
                            Console.Out.WriteLine($"{i}");
                            Thread.Sleep(5000);
                        }
                    )
                );

            foreach (var task in tasks)
            {
                _semaphore.Wait();

                task.Start();

                task.ContinueWith(t =>
                    Task.Run(() =>
                        _semaphore.Release()));
            }

            //var waitingTasks = tasks.Where(t => t.Status == TaskStatus.Running).ToArray();
            //Task.WaitAll(waitingTasks);

            while (_semaphore.CurrentCount != maxConcurrency)
                Thread.Sleep(100);
        }

        public static void SemaphoreLock()
        {
            _semaphore = new SemaphoreSlim(2);
            _semaphore.Wait();
            _semaphore.Wait();
            _semaphore.Wait();
        }
    }
}

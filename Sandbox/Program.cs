﻿using System;
using Rocks.Benchmark;

namespace Sandbox
{
    public class Program
    {
        public static void Main()
        {
            try
            {
                //ProfilingLibrary.Setup(() => null, configure: x => x.ErrorLogger = Console.WriteLine);

                //using (var connection = ConfigurationManager.ConnectionStrings["Realty"].CreateDbConnection())
                //{
                //    var id = connection.Query<int>("select top 1 Id from dbo.Objects with (nolock) order by Id desc").FirstOrNull();

                //    Console.WriteLine("id: {0}", id);
                //}

                //DoAsync().Wait();

                var benchmarks = new BenchmarkList(100, 100);

                //benchmarks.Add("Callstack", () => GetCallStackMethods());
                //benchmarks.Add("Environment.StackTrace", () =>
                //                                         {
                //                                             var x = Environment.StackTrace;
                //                                         });

                //benchmarks.Add("Disposable",
                //               () =>
                //               {
                //                   using (new MeasureScope())
                //                       SpinWait.SpinUntil(() => false, 1);
                //               });

                //benchmarks.Add("Manual",
                //               () =>
                //               {
                //                   var stopwatch = Stopwatch.StartNew();
                //                   SpinWait.SpinUntil(() => false, 1);
                //                   stopwatch.Stop();
                //               });

                //benchmarks.Add("FastMember",
                //               () =>
                //               {
                //                   var result = new List<string>();

                //                   var obj = new { a = 1, b = "aaa" };
                //                   var object_accessor = ObjectAccessor.Create(obj);
                //                   var type_accessor = TypeAccessor.Create(obj.GetType());
                //                   foreach (var p in type_accessor.GetMembers())
                //                   {
                //                       var v = object_accessor[p.Name];
                //                       result.Add(p.Name);
                //                       result.Add(v.ToString());
                //                   }
                //               });


                //benchmarks.Add("Dictionary",
                //               () =>
                //               {
                //                   var result = new List<string>();

                //                   var obj = new Dictionary<string, object> { { "a", 1 }, { "b", "aaa" } };
                //                   foreach (var p in obj)
                //                   {
                //                       result.Add(p.Key);
                //                       result.Add(p.Value.ToString());
                //                   }
                //               });

                benchmarks.RunAll(false).WriteToConsole();
            }
                // ReSharper disable once CatchAllClause
            catch (Exception ex)
            {
                Console.WriteLine("\n\n{0}\n\n", ex);
            }
        }


        ///// <summary>
        /////     Enumerates the information about all methods in the current call stack.
        ///// </summary>
        ///// <returns></returns>
        //[NotNull, MethodImpl(MethodImplOptions.NoInlining)]
        //public static IEnumerable<MethodBase> GetCallStackMethods()
        //{
        //    var stack_frames = new StackTrace().GetFrames();

        //    if (stack_frames != null)
        //        return stack_frames.Select(x => x.GetMethod());

        //    return new[] { new StackFrame().GetMethod() };
        //}


        //private class MeasureScope : IDisposable
        //{
        //    public Stopwatch Stopwatch { get; } = Stopwatch.StartNew();


        //    public void Dispose()
        //    {
        //        this.Stopwatch.Stop();
        //    }
        //}


        //private static readonly AsyncLocal<string> CurrentSession = new AsyncLocal<string>();


        //private static Task DoAsync()
        //{
        //    var tasks = Enumerable.Range(0, 5).Select(x => StartTaskAsync(x.ToString()));

        //    return Task.WhenAll(tasks);
        //}


        //private static Task StartTaskAsync(string name)
        //{
        //    return Task.Run(() =>
        //                    {
        //                        Console.WriteLine($"Creating new task with name {name}. Current session = {CurrentSession.Value}");
        //                        CurrentSession.Value = name;
        //                        return TaskAsync(name, name);
        //                    });
        //}


        //private static async Task TaskAsync(string name, string expectedSession)
        //{
        //    if (CurrentSession.Value != expectedSession)
        //        Console.WriteLine($"ERROR! Task {name} has incorrect session {CurrentSession.Value} (expected {expectedSession})");

        //    //Console.WriteLine($"Task {name}: start (session {CurrentSession.Value})");

        //    var p = RandomizationExtensions.Random.NextDouble();
        //    if (p <= 0.50)
        //        await Task.Delay(RandomizationExtensions.Random.Next(200, 2000)).ConfigureAwait(false);
        //    else if (p <= 0.75)
        //        await Task.Run(() => TaskAsync(name + "_sub2", expectedSession)).ConfigureAwait(false);
        //    else
        //        await StartTaskAsync(name + "_sub").ConfigureAwait(false);

        //    //Console.WriteLine($"Task {name}: end (session {CurrentSession.Value})");

        //    if (CurrentSession.Value != expectedSession)
        //        Console.WriteLine($"ERROR! Task {name} has incorrect session {CurrentSession.Value} (expected {expectedSession})");
        //}
    }
}
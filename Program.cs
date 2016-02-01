using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace AwaitCoroutines
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var co = Coroutine.Run<double, int>(c => Foo(c, "hello"));

            int previous = 42;
            while (!co.IsCompleted)
            {
                int next = co.Next(previous);
                Console.WriteLine("{1} -> {2} ({0})", co.State, previous, next);
                previous = next;
            }
        }

        static async Task<int> Foo(Coroutine<double, int> co, string s)
        {
            Console.WriteLine("{0}!", s);
            int value = (int)Math.Floor(co.Input);
            while (value > 0)
            {
                value = (int)(await co.Yield(value - 2));
            }
            return 0;
        }
    }
}

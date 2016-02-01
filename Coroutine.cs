using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace AwaitCoroutines
{
    public static class Coroutine
    {
        public static Coroutine<Input, Generate> Run<Input, Generate>(
            Func<Coroutine<Input, Generate>, Task<Generate>> coroutine)
        {
            var co = new Coroutine<Input, Generate>();
            co.continuation = () =>
            {
                co.task = coroutine(co);
            };
            return co;
        }
    }

    public enum CoroutineState
    {
        NotStarted,
        Suspended,
        Running,
        Completed,
    }

    public sealed class Coroutine<TInput, TGenerate>
    {
        TGenerate generate;
        TInput input;

        internal Action continuation;
        internal Task<TGenerate> task;

        public CoroutineState State { get; private set; }

        public bool IsCompleted
        {
            get { return this.State == CoroutineState.Completed; }
        }

        public TInput Input
        {
            get { return input; }
        }

        public Coroutine() {}

        public Awaitable Yield(TGenerate generate)
        {
            if (this.State != CoroutineState.Running)
                throw new CoroutineException("Cannot yield from not running coroutine");
            this.generate = generate;
            return new Awaitable(this);
        }

        public TGenerate Next(TInput input = default(TInput))
        {
            if (this.State == CoroutineState.NotStarted ||
                this.State == CoroutineState.Suspended)
            {
                this.input = input;
                this.State = CoroutineState.Running;
                this.continuation();
                return SetResultFromContinuation();
            }
            else
            {
                throw new CoroutineException("Cannot progress coroutine in {this.State} state");
            }
        }

        private TGenerate SetResultFromContinuation()
        {
            if (task.IsCompleted)
            {
                this.State = CoroutineState.Completed;
                return task.Result;
            }
            else
            {
                this.State = CoroutineState.Suspended;
                return this.generate;
            }
        }

        public struct Awaitable : INotifyCompletion
        {
            Coroutine<TInput, TGenerate> co;

            internal Awaitable(Coroutine<TInput, TGenerate> co)
            {
                this.co = co;
            }

            public Awaitable GetAwaiter()
            {
                return this;
            }

            public bool IsCompleted 
            {
                get { return false; } 
            }

            public void OnCompleted(Action continuation) 
            {
                co.continuation = continuation;
            }

            public TInput GetResult()
            {
                return co.input;
            }
        }
    }


    [Serializable]
    public class CoroutineException : Exception
    {
        public CoroutineException() {}
        public CoroutineException(string message)
            : base(message) {}
        public CoroutineException(string message, Exception inner)
            : base(message, inner) {}
        protected CoroutineException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) {}
    }
}


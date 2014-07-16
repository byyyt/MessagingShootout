using System;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using Disruptor.Scheduler;

namespace MessagingShootout.Scenarios.Disruptor
{
    [Scenario("Disruptor Dsl with 3 Fork/Join Consumers")]
    public class DisruptorDslForkJoinScenario : Scenario<Message>
    {
        private class Handler : IEventHandler<Message>
        {
            private readonly TaskCompletionSource<bool> _tcs;
            private readonly string _name;

            public Handler(TaskCompletionSource<bool> tcs, string name)
            {
                _tcs = tcs;
                _name = name;
            }

            public void OnNext(Message data, long sequence, bool endOfBatch)
            {
                Count++;
                if (data.Terminate)
                {
                    Console.WriteLine("Consumer {0} received {1:#,#;;0} messages.", _name, Count);

                    if(_tcs != null)
                        _tcs.SetResult(true);
                }
            }

            public int Count { get; private set; }
        }

        private readonly Disruptor<Message> _disruptor;
        private readonly Handler _handlerOne;
        private readonly Handler _handlerTwo;
        private readonly Handler _handlerThree;

        private RingBuffer<Message> _ringBuffer;

        private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        public DisruptorDslForkJoinScenario()
        {
            _disruptor = new Disruptor<Message>(() => new Message(), new SingleThreadedClaimStrategy(4096), new YieldingWaitStrategy(), new RoundRobinThreadAffinedTaskScheduler(3));
            _handlerOne = new Handler(null, "One");
            _handlerTwo = new Handler(null, "Two");
            _handlerThree = new Handler(_tcs, "Three");

            _disruptor
                .HandleEventsWith(_handlerOne, _handlerTwo)
                .HandleEventsWith(_handlerThree);
        }

        protected override Task StartScenarioTask()
        {
            _ringBuffer = _disruptor.Start();
            return _tcs.Task.ContinueWith(t => _disruptor.Shutdown());
        }

        public override void Publish(Message message)
        {
            var num = _ringBuffer.Next();
            _ringBuffer[num].Value = message.Value;
            _ringBuffer[num].Terminate = message.Terminate;

            _ringBuffer.Publish(num);
        }
    }
}
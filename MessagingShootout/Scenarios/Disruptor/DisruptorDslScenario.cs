using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using Disruptor.Scheduler;

namespace MessagingShootout.Scenarios.Disruptor
{
    [Scenario("Disruptor Dsl with 1 Consumer")]
    public class DisruptorDslScenario : Scenario<Message>
    {
        private class Handler : IEventHandler<Message>
        {
            private readonly TaskCompletionSource<bool> _tcs;

            public Handler(TaskCompletionSource<bool> tcs)
            {
                _tcs = tcs;
            }

            public void OnNext(Message data, long sequence, bool endOfBatch)
            {
                Count++;
                if (data.Terminate)
                {
                    Console.WriteLine("Consumer received {0:#,#;;0} messages.", Count);
                    _tcs.SetResult(true);
                }
            }

            public int Count { get; private set; }
        }

        private readonly Disruptor<Message> _disruptor;
        private readonly Handler _handler;
        private RingBuffer<Message> _ringBuffer;

        private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();
        
        public DisruptorDslScenario()
        {
            _disruptor = new Disruptor<Message>(() => new Message(), new SingleThreadedClaimStrategy(4096), new YieldingWaitStrategy(), new RoundRobinThreadAffinedTaskScheduler(1));
            _handler = new Handler(_tcs);

            _disruptor.HandleEventsWith(_handler);
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

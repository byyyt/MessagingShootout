using System;
using System.Collections.Concurrent;

namespace MessagingShootout.Scenarios.ConcurrentQueue
{
    [Scenario("Concurrent Queue with 1 Consumer")]
    public class ConcurrentQueueScenario : SingleConsumerScenario<Message>
    {
        private readonly ConcurrentQueue<Message> _queue = new ConcurrentQueue<Message>();

        public override void Publish(Message message)
        {
            _queue.Enqueue(message);
        }

        protected override void ConsumerOne()
        {
            int count = 0;
            Message msg;

            while (true)
            {
                var received = _queue.TryDequeue(out msg);
                if (received)
                {
                    count++;
                    if (msg.Terminate)
                        break;
                }
            }

            Console.WriteLine("Consumer received {0:#,#;;0} messages.", count);
        }
    }
}
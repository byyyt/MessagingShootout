using System;
using System.Collections.Concurrent;

namespace MessagingShootout.Scenarios.BlockingCollection
{
    [Scenario("BlockingCollection (backed by ConcurrentQueue) with 1 Consumer")]
    public class BlockingCollectionScenario : SingleConsumerScenario<Message>
    {
        private readonly BlockingCollection<Message> _queue = new BlockingCollection<Message>();

        public override void Publish(Message message)
        {
            _queue.Add(message);
        }

        protected override void ConsumerOne()
        {
            int count = 0;
            Message msg;

            while (true)
            {
                var received = _queue.TryTake(out msg);
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
using System;

namespace MessagingShootout.Scenarios.LockedQueue
{
    [Scenario("Locked Queue with 1 Consumer")]
    public class LockedQueueScenario : SingleConsumerScenario<Message>
    {
        private readonly LockedQueue<Message> _queue = new LockedQueue<Message>();

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
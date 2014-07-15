using System;

namespace MessagingShootout.Scenarios
{
    [Scenario("Locked Queue with 3 Fork/Join Consumers")]
    public class LockedQueueForkJoinScenario : ThreeConsumerForkJoinScenario<Message>
    {
        private readonly LockedQueue<Message> _consumerOneIn = new LockedQueue<Message>();
        private readonly LockedQueue<Message> _consumerTwoIn = new LockedQueue<Message>();

        private readonly LockedQueue<Message> _consumerOneOut = new LockedQueue<Message>();
        private readonly LockedQueue<Message> _consumerTwoOut = new LockedQueue<Message>();

        public override void Publish(Message message)
        {
            _consumerOneIn.Enqueue(message);
            _consumerTwoIn.Enqueue(message);
        }

        private void ConsumeAndPublish(LockedQueue<Message> @in, LockedQueue<Message> @out, string name)
        {
            int count = 0;
            Message msg;

            while (true)
            {
                var received = @in.TryDequeue(out msg);
                if (received)
                {
                    count++;
                    @out.Enqueue(msg);

                    if (msg.Terminate)
                        break;
                }
            }

            Console.WriteLine("Consumer {0} received {1:#,#;;0} messages.", name, count);
        }

        protected override void ConsumerOne()
        {
            ConsumeAndPublish(_consumerOneIn, _consumerOneOut, "One");
        }

        protected override void ConsumerTwo()
        {
            ConsumeAndPublish(_consumerTwoIn, _consumerTwoOut, "Two");
        }

        protected override void ConsumerThree()
        {
            int count = 0;
            Message msgOne;
            Message msgTwo;

            while (true)
            {
                while (!_consumerOneOut.TryDequeue(out msgOne)) { }
                while (!_consumerTwoOut.TryDequeue(out msgTwo)) { }

                count++;
                if (msgTwo.Terminate)
                    break;
            }

            Console.WriteLine("Consumer three received {0:#,#;;0} joined messages.", count);
        }
    }
}

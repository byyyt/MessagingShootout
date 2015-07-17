using System;
using System.Collections.Concurrent;

namespace MessagingShootout.Scenarios.BlockingCollection
{
    [Scenario("BlockingCollection (backed by ConcurrentQueue) with 3 Fork/Join Consumers")]
    public class BlockingCollectionForkJoinScenario : ThreeConsumerForkJoinScenario<Message>
    {
        private readonly BlockingCollection<Message> _consumerOneIn = new BlockingCollection<Message>();
        private readonly BlockingCollection<Message> _consumerTwoIn = new BlockingCollection<Message>();

        private readonly BlockingCollection<Message> _consumerOneOut = new BlockingCollection<Message>();
        private readonly BlockingCollection<Message> _consumerTwoOut = new BlockingCollection<Message>();

        public override void Publish(Message message)
        {
            _consumerOneIn.Add(message);
            _consumerTwoIn.Add(message);
        }

        private void ConsumeAndPublish(BlockingCollection<Message> @in, BlockingCollection<Message> @out, string name)
        {
            int count = 0;
            Message msg;

            while (true)
            {
                var received = @in.TryTake(out msg);
                if (received)
                {
                    count++;
                    @out.Add(msg);

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
                while (!_consumerOneOut.TryTake(out msgOne)) { }
                while (!_consumerTwoOut.TryTake(out msgTwo)) { }

                count++;
                if (msgTwo.Terminate)
                    break;
            }

            Console.WriteLine("Consumer three received {0:#,#;;0} joined messages.", count);
        }
    }
}

using System;
using System.Threading.Tasks.Dataflow;

namespace MessagingShootout.Scenarios
{
    [Scenario("Dataflow BufferBlock with 3 Fork/Join Consumers")]
    public class DataflowBufferBlockForkJoinScenario : ThreeConsumerForkJoinScenario<Message>
    {
        private readonly BufferBlock<Message> _consumerOneIn = new BufferBlock<Message>();
        private readonly BufferBlock<Message> _consumerTwoIn = new BufferBlock<Message>();

        private readonly BufferBlock<Message> _consumerOneOut = new BufferBlock<Message>();
        private readonly BufferBlock<Message> _consumerTwoOut = new BufferBlock<Message>();

        public override void Publish(Message message)
        {
            _consumerOneIn.Post(message);
            _consumerTwoIn.Post(message);
        }

        private void ConsumeAndPublish(BufferBlock<Message> @in, BufferBlock<Message> @out, string name)
        {
            int count = 0;
            Message msg;

            while (true)
            {
                var received = @in.TryReceive(out msg);
                if (received)
                {
                    count++;
                    @out.Post(msg);

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
                while (!_consumerOneOut.TryReceive(out msgOne)) { }
                while (!_consumerTwoOut.TryReceive(out msgTwo)) { }

                count++;
                if (msgTwo.Terminate)
                    break;
            }

            Console.WriteLine("Consumer three received {0:#,#;;0} joined messages.", count);
        }

    }
}
using System;
using System.Threading.Tasks.Dataflow;

namespace MessagingShootout.Scenarios
{
    [Scenario("Dataflow JoinBlock 3 Fork/Join Consumers")]
    public class DataflowJoinBlockForkJoinScenario : ThreeConsumerForkJoinScenario<Message>
    {
        private readonly BufferBlock<Message> _consumerOneIn = new BufferBlock<Message>();
        private readonly BufferBlock<Message> _consumerTwoIn = new BufferBlock<Message>();

        private readonly JoinBlock<Message, Message> _join = new JoinBlock<Message, Message>();

        public override void Publish(Message message)
        {
            _consumerOneIn.Post(message);
            _consumerTwoIn.Post(message);
        }

        private void ConsumeAndPublish(BufferBlock<Message> @in, ITargetBlock<Message> @out, string name)
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
            ConsumeAndPublish(_consumerOneIn, _join.Target1, "One");
        }

        protected override void ConsumerTwo()
        {
            ConsumeAndPublish(_consumerTwoIn, _join.Target2, "Two");
        }

        protected override void ConsumerThree()
        {
            int count = 0;
            Tuple<Message, Message> msgs;

            while (true)
            {
                if (_join.TryReceive(out msgs))
                {
                    count++;
                    if (msgs.Item2.Terminate)
                        break;
                }
            }

            Console.WriteLine("Consumer three received {1:#,#;;0} joined messages.", count);
        }

    }
}
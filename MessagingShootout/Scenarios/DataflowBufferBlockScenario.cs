using System;
using System.Threading.Tasks.Dataflow;

namespace MessagingShootout.Scenarios
{
    [Scenario("Dataflow BufferBlock with 1 Consumer")]
    public class DataflowBufferBlockScenario : SingleConsumerScenario<Message>
    {
        private readonly BufferBlock<Message> _block = new BufferBlock<Message>();

        public override void Publish(Message message)
        {
            _block.Post(message);
        }

        protected override void ConsumerOne()
        {
            int count = 0;
            Message msg;

            while (true)
            {
                var received = _block.TryReceive(out msg);
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
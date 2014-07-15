using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MessagingShootout.Scenarios.Dataflow
{
    [Scenario("Dataflow ActionBlock with 1 Consumer")]
    public class DataflowActionBlockScenario : Scenario<Message>
    {
        private readonly ActionBlock<Message> _block;
        private TaskCompletionSource<bool>  _tcs = new TaskCompletionSource<bool>();

        private int _count = 0;

        public DataflowActionBlockScenario()
        {
            _block = new ActionBlock<Message>((Action<Message>)OnMessage);
        }

        public override void Publish(Message message)
        {
            _block.Post(message);
        }

        protected override Task StartScenarioTask()
        {
            return _tcs.Task;
        }

        private void OnMessage(Message msg)
        {
            _count++;
            if (msg.Terminate)
            {
                Console.WriteLine("Consumer received {0:#,#;;0} messages.", _count);
                _tcs.SetResult(true);
            }
        }
    }

    [Scenario("Dataflow Patterns with 3 Fork/Join Consumers")]
    public class DataflowPatternsForkJoinScenario : Scenario<Message>
    {
        private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        private readonly BroadcastBlock<Message> _in;
        private readonly ActionBlock<Message> _consumerOne;
        private readonly ActionBlock<Message> _consumerTwo;
        private readonly JoinBlock<Message, Message> _join;
        private readonly ActionBlock<Tuple<Message, Message>> _consumerThree;

        private int _oneCount = 0;
        private int _twoCount = 0;
        private int _threeCount = 0;

        public DataflowPatternsForkJoinScenario()
        {
            _consumerOne = new ActionBlock<Message>((Action<Message>)ConsumerOne);
            _consumerTwo = new ActionBlock<Message>((Action<Message>)ConsumerTwo);
            _consumerThree = new ActionBlock<Tuple<Message, Message>>((Action<Tuple<Message, Message>>)ConsumerThree);

            _in = new BroadcastBlock<Message>(x => x);
            _in.LinkTo(_consumerOne);
            _in.LinkTo(_consumerTwo);

            _join = new JoinBlock<Message, Message>();
            _join.LinkTo(_consumerThree);
        }

        protected override Task StartScenarioTask()
        {
            return _tcs.Task;
        }

        public override void Publish(Message message)
        {
            _in.Post(message);
        }

        private void ConsumerOne(Message msg)
        {
            _oneCount++;
            _join.Target1.Post(msg);

            if(msg.Terminate)
                Console.WriteLine("Consumer One received {0:#,#;;0} messages.", _oneCount);
        }

        private void ConsumerTwo(Message msg)
        {
            _twoCount++;
            _join.Target2.Post(msg);

            if (msg.Terminate)
                Console.WriteLine("Consumer Two received {0:#,#;;0} messages.", _twoCount);
        }

        private void ConsumerThree(Tuple<Message, Message> msg)
        {
            _threeCount++;

            if (msg.Item2.Terminate)
            {
                Console.WriteLine("Consumer Three received {0:#,#;;0} joined messages.", _threeCount);
                _tcs.SetResult(true);
            }
        }
    }
}
using System;
using System.Threading.Tasks;

namespace MessagingShootout
{
    public abstract class SingleConsumerScenario<TMessage> : Scenario<TMessage>
    {
        protected override Task StartScenarioTask()
        {
            return Task.Run((Action)ConsumerOne);
        }

        protected abstract void ConsumerOne();
    }
}
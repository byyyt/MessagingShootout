using System;
using System.Threading.Tasks;

namespace MessagingShootout
{
    public abstract class ThreeConsumerForkJoinScenario<TMessage> : Scenario<TMessage>
    {
        protected override Task StartScenarioTask()
        {
            return Task.WhenAll(
                Task.Run((Action)(ConsumerOne)),
                Task.Run((Action)(ConsumerTwo)),
                Task.Run((Action)(ConsumerThree))
            );
        }

        protected abstract void ConsumerOne();
        protected abstract void ConsumerTwo();
        protected abstract void ConsumerThree();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingShootout
{
    public abstract class Scenario<TMessage>
    {
        private Task _scenarioCompletedTask;

        public void StartScenario()
        {
            if(_scenarioCompletedTask != null)
                throw new InvalidOperationException("Scenario has already been started.");

            _scenarioCompletedTask = StartScenarioTask();
        }

        protected abstract Task StartScenarioTask();

        public virtual Task ScenarioCompletedTask
        {
            get
            {
                if(_scenarioCompletedTask == null)
                    throw new InvalidOperationException("You must start the scenario!");

                return _scenarioCompletedTask;
            }
        }

        public abstract void Publish(TMessage message);

    }
}

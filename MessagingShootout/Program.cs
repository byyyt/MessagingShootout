using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MessagingShootout
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=====================================");
            Console.WriteLine("  Messaging Shootout");
            Console.WriteLine("=====================================");
            Console.WriteLine();

            var scenarioFactory = PickScenario();
            if (scenarioFactory == null)
                return;

            for(int i = 1; i <= 5; i++)
                RunScenario(scenarioFactory, i);

            Console.WriteLine();
            Console.WriteLine("Press ENTER to exit.");
            Console.ReadLine();
        }

        private static void RunScenario(Func<Scenario<Message>> scenarioFactory, int run)
        {
            var scenario = scenarioFactory();

            Console.WriteLine();
            Console.WriteLine("Run {0}...", run);

            scenario.StartScenario();

            var msg = new Message
            {
                Value = "test",
                Terminate = false
            };

            int count = 0;
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed.TotalSeconds < 10)
            {
                scenario.Publish(msg);
                count++;
            }

            scenario.Publish(new Message { Value = "test", Terminate = true });
            count++;

            Console.WriteLine("Producer sent {0:#,#;;0} messages.", count);

            scenario.ScenarioCompletedTask.Wait();
            sw.Stop();

            Console.WriteLine("Run {0}: {1:#,#;;0} msgs in {2} for {3:#,#} msgs/sec", run, count, sw.Elapsed, (count / sw.Elapsed.TotalSeconds));
        }

        private static Func<Scenario<Message>> PickScenario()
        {
            var scenarios = typeof (Program).Assembly
                .DefinedTypes
                .Where(x => x.IsDefined(typeof (ScenarioAttribute), false))
                .Where(x => !x.IsAbstract && !x.IsInterface)
                .Select(x => new
                {
                    ScenarioType = x,
                    x.GetCustomAttribute<ScenarioAttribute>().Description
                })
                .ToArray();

            if (scenarios.Length <= 0)
            {
                Console.WriteLine("There are no messaging shootout scenarios implemented.");
                return null;
            }


            foreach (var scenario in scenarios.Select((Details, Index) => new { Details, Index }))
                Console.WriteLine("  {0}. {1}", scenario.Index + 1, scenario.Details.Description);

            Console.WriteLine();
            Console.Write("What messaging scenario do you want to run: ");
            var selection = Console.ReadLine();

            int selectedScenario;
            if (int.TryParse(selection, out selectedScenario))
            {
                Console.WriteLine();
                Console.WriteLine("Picked {0} scenario...", scenarios[selectedScenario - 1].Description);

                return () => (Scenario<Message>) Activator.CreateInstance(scenarios[selectedScenario - 1].ScenarioType);
            }

            return null;
        }
    }
}

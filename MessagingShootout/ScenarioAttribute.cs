using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingShootout
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited =  false)]
    public class ScenarioAttribute : Attribute
    {
        public ScenarioAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; private set; }
    }
}

using DemoORM.Implementation.Entities;
using DemoORM.Lib;

namespace DemoORM.Implementation
{
    public class Context : BaseContext
    {
        public ITable<Person> People { get; set; }
    }
}
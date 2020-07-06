using DemoORM.Implementation;
using DemoORM.Lib.Sql;

namespace DemoORM.Console
{
    using Console = System.Console;

    class Program
    {
        static void Main(string[] args)
        {
            var ctx = new Context();

            var sql = ctx.CreateSql();
            Console.WriteLine(sql);
        }
    }
}

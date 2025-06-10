using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalBroker.Tests.Performance
{
    public abstract class AbstractPreformanceTest
    {
        private readonly string _testName;
        private readonly int _repeatCount;
        protected readonly double _correction = 0;

        protected AbstractPreformanceTest(string testName, int repeatCount = 10)
        {
            _testName = testName;
            _repeatCount = repeatCount;
        }

        public async Task PerformTest()
        {
            Console.WriteLine($"Тест: {_testName}");
            Console.WriteLine("| Имя теста | Номер попытки | Время в mc |");
            Console.WriteLine("|-----------|---------------|------------|");

            var total = new List<double>();
            for (int i = 0; i < _repeatCount; i++)
            {
                var testTime = await Test() + _correction;

                Console.WriteLine($"|{_testName}|{i}|{testTime}|");
                total.Add(testTime);

            }
            Console.WriteLine($"| Среднее время | {total.Average()} |");
        }

        protected abstract Task<double> Test();
    }
}

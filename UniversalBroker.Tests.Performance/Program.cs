


using UniversalBroker.Tests.Performance;


RabbitToTcpPerformanceTest rabbitMqTest = new()
{
    RabbitHostName = "192.168.254.121",
    TcpIp = "192.168.1.68",
    Port = 9999,
    Message = "BSM\r\n.V/1LMSQ\r\n.J/S///01OCT/124657Z/TX000239/TXS13\r\n.F/B2925/01OCT/AER\r\n.N/0628143615001\r\n.S/Y/7F/C/037\r\n.P/PRAKOFYEVA/VOLHA\r\n.R/EAT1251\r\n.W/K/1/17\r\nENDBSM\r\n",
    QueueName = "twoOut",

};

await rabbitMqTest.PerformTest();

TcpToRabbitMqPerformanceTest tcpTest = new()
{
    RabbitHostName = "192.168.254.121",
    TcpIp = "192.168.1.68",
    Port = 9999,
    Message = "BSM\r\n.V/1LMSQ\r\n.J/S///01OCT/124657Z/TX000239/TXS13\r\n.F/B2925/01OCT/AER\r\n.N/0628143615001\r\n.S/Y/7F/C/037\r\n.P/PRAKOFYEVA/VOLHA\r\n.R/EAT1251\r\n.W/K/1/17\r\nENDBSM\r\n",
    QueueName = "twoIn",

};

await tcpTest.PerformTest();

TcpAckPerformanceTest tcpAckPerformance = new TcpAckPerformanceTest()
{
    TcpIp = "192.168.1.68",
    Port = 9999,
    Message = "BSM\r\n.V/1LMSQ\r\n.J/S///01OCT/124657Z/TX000239/TXS13\r\n.F/B2925/01OCT/AER\r\n.N/0628143615001\r\n.S/Y/7F/C/037\r\n.P/PRAKOFYEVA/VOLHA\r\n.R/EAT1251\r\n.W/K/1/17\r\nENDBSM\r\n",
};

await tcpAckPerformance.PerformTest();


Console.ReadLine();
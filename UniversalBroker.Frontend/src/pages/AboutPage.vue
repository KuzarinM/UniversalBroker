<script>
</script>

<template>
    <h2 class="text-start">О программе</h2>
     <link rel="stylesheet" href="https://stackedit.io/style.css" />
    <div class="stackedit">
        <div class="d-flex flex-column" style="width: fit-content;">
         <h5 id="основная-конфигурация">Основная конфигурация</h5>
         <table>
            <thead>
               <tr>
                  <th>Имя</th>
                  <th>Тип данных</th>
                  <th>значение по умолчанию</th>
               </tr>
            </thead>
            <tbody>
               <tr>
                  <td>AdapterConfiguration.TimeToLiveSeconds</td>
                  <td>double</td>
                  <td>120</td>
               </tr>
            </tbody>
         </table>
         <h5 id="планировщик">Планировщик</h5>
         <h4 id="конфигурация-планировщика">Конфигурация планировщика</h4>
         <table>
            <thead>
               <tr>
                  <th>Имя</th>
                  <th>Тип данных</th>
                  <th>значение по умолчанию</th>
               </tr>
            </thead>
            <tbody>
               <tr>
                  <td>SchedulerConfiguration.IntervalMs</td>
                  <td>int</td>
                  <td>25000</td>
               </tr>
               <tr>
                  <td>SchedulerConfiguration.MessageText</td>
                  <td>string?</td>
                  <td>null</td>
               </tr>
               <tr>
                  <td>SchedulerConfiguration.AutoReset</td>
                  <td>bool</td>
                  <td>true</td>
               </tr>
            </tbody>
         </table>
         <h5 id="rabbitmq">RabbitMq</h5>
         <h4 id="конфигурация-обменника">Конфигурация обменника</h4>
         <table>
            <thead>
               <tr>
                  <th>Имя</th>
                  <th>Тип данных</th>
                  <th>значение по умолчанию</th>
               </tr>
            </thead>
            <tbody>
               <tr>
                  <td>ExchangeConfig.NeedDeclare</td>
                  <td>bool</td>
                  <td>true</td>
               </tr>
               <tr>
                  <td>ExchangeConfig.Type</td>
                  <td>string</td>
                  <td>“fanout”</td>
               </tr>
               <tr>
                  <td>ExchangeConfig.Durable</td>
                  <td>bool</td>
                  <td>false</td>
               </tr>
               <tr>
                  <td>ExchangeConfig.Exclusive</td>
                  <td>bool</td>
                  <td>false</td>
               </tr>
               <tr>
                  <td>ExchangeConfig.AutoDelete</td>
                  <td>bool</td>
                  <td>false</td>
               </tr>
               <tr>
                  <td>ExchangeConfig.Passive</td>
                  <td>bool</td>
                  <td>false</td>
               </tr>
               <tr>
                  <td>ExchangeConfig.NoWait</td>
                  <td>bool</td>
                  <td>false</td>
               </tr>
            </tbody>
         </table>
         <h4 id="конфигурация-публикатора">Конфигурация публикатора</h4>
         <table>
            <thead>
               <tr>
                  <th>Имя</th>
                  <th>Тип данных</th>
                  <th>значение по умолчанию</th>
               </tr>
            </thead>
            <tbody>
               <tr>
                  <td>PublishConfiguration.UseExchenge</td>
                  <td>bool</td>
                  <td>false</td>
               </tr>
               <tr>
                  <td>PublishConfiguration.Mandatory</td>
                  <td>bool</td>
                  <td>false</td>
               </tr>
            </tbody>
         </table>
         <h4 id="конфигурация-очереди">Конфигурация очереди</h4>
         <table>
            <thead>
               <tr>
                  <th>Имя</th>
                  <th>Тип данных</th>
                  <th>значение по умолчанию</th>
               </tr>
            </thead>
            <tbody>
               <tr>
                  <td>QueueConfiguration.NeedDeclare</td>
                  <td>bool</td>
                  <td>true</td>
               </tr>
               <tr>
                  <td>QueueConfiguration.Durable</td>
                  <td>bool</td>
                  <td>false</td>
               </tr>
               <tr>
                  <td>QueueConfiguration.Exclusive</td>
                  <td>bool</td>
                  <td>false</td>
               </tr>
               <tr>
                  <td>QueueConfiguration.AutoDelete</td>
                  <td>bool</td>
                  <td>false</td>
               </tr>
               <tr>
                  <td>QueueConfiguration.Passive</td>
                  <td>bool</td>
                  <td>false</td>
               </tr>
               <tr>
                  <td>QueueConfiguration.NoWait</td>
                  <td>bool</td>
                  <td>false</td>
               </tr>
            </tbody>
         </table>
         <h5 id="tcp-сервер">Tcp сервер</h5>
         <h4 id="конфигурация">Конфигурация</h4>
         <table>
            <thead>
               <tr>
                  <th>Имя</th>
                  <th>Тип данных</th>
                  <th>значение по умолчанию</th>
               </tr>
            </thead>
            <tbody>
               <tr>
                  <td>TcpConfiguration.IsClient</td>
                  <td>bool</td>
                  <td>true</td>
               </tr>
               <tr>
                  <td>TcpConfiguration.MessageDeviderRegex</td>
                  <td>string?</td>
                  <td>“\n”</td>
               </tr>
               <tr>
                  <td>TcpConfiguration.MessageFixSize</td>
                  <td>int?</td>
                  <td>null</td>
               </tr>
               <tr>
                  <td>TcpConfiguration.MessageDevicerByte</td>
                  <td>byte?</td>
                  <td>null</td>
               </tr>
            </tbody>
         </table>
      </div>
    </div>
</template>
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Models.Internals;

namespace UniversalBroker.Core.Logic.Interfaces
{
    /// <summary>
    /// Сервис работы с V8 и скриптами. В целом можно и в Handler потом вынести, тут не так много
    /// </summary>
    public interface IChanelJsInterpretatorService
    {
        Task ExecuteScript(Chanel chanel, InternalMessage message);
    }
}

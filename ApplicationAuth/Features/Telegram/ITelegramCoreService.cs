using System;
using System.Threading.Tasks;

namespace ApplicationAuth.Features.Telegram
{
    public interface ITelegramCoreService
    {
        public Task SendMessage(int chatId, string text);

    }
}

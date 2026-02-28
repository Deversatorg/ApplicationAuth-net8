using ApplicationAuth.Features.Telegram.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Features.Telegram
{
    public interface ITelegramService
    {
        public Task<TelegramMessageResponseModel> TestInsert(string text);

        public Task<TelegramMessageResponseModel> SaveMessage(TelegramMessageRequestModel model);

        public Task<IEnumerable<TelegramMessageResponseModel>> GetMessagesByUserToken(string userToken);

        public Task<IEnumerable<TelegramStickerResponseModel>> GetTopStickers();

        public Task<TelegramStickerResponseModel> SaveStickerRate(string fileUniqueId, string stickerId);

    }
}

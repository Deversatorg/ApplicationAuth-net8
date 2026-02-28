using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationAuth.Domain.Entities.Telegram
{
    public class TelegramMessage : IEntity, IAuditableEntity
    {
        #region Properties

        public int Id { get; set; }

        public string UserToken { get; set; }

        public string Message { get; set; }

        public DateTime DateTime { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        #endregion
    }
}

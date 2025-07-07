using GestaoEscolarWeb.Data.Entities;
using System.Collections;
using System.Collections.Generic;

namespace GestaoEscolarWeb.Models
{
    public class UserMessagesViewModel
    {
        public IEnumerable<Alert> UserMessages { get; set; }

        public UserMessagesViewModel()
        {
            UserMessages = new List<Alert>();
        }
    }
}

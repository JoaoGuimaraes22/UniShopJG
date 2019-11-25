using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UStoreBot.Models
{
    public class StoreRoomItem
    {
        public int StoreRoomItemId { get; set; }

        public Ingredient Ingredient { get; set; }

        public int Quantity { get; set; }
    }
}

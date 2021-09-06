using System.Collections.Generic;
using System.Linq;
using Jubi.VKontakte.Enums.Widget;

namespace Jubi.VKontakte.Widget.Table
{
    public class Row
    {
        public Column[] Items { get; }
        
        public Row(params Column[] _items)
        {
            Items = _items;
        }
    }
}
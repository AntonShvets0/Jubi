using Jubi.VKontakte.Enums.Widget;

namespace Jubi.VKontakte.Widget.Table
{
    public class HeadColumn
    {
        public string Name { get; }
        
        public RowAlignType Align { get; }

        public HeadColumn(string name, RowAlignType align)
        {
            Name = name;
            Align = align;
        }
    }
}
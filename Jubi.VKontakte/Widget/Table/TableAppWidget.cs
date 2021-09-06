using System.Collections.Generic;
using System.Linq;
using Jubi.VKontakte.Enums.Widget;
using Newtonsoft.Json.Linq;

namespace Jubi.VKontakte.Widget.Table
{
    public class TableAppWidget : AppWidget
    {
        public override string Type { get; } = "table";
        
        public WidgetTitle Title { get; }
        
        public WidgetFooter Footer { get; }

        private List<Row> _rows { get; } = new List<Row>();

        private HeadColumn[] _head;
        
        public TableAppWidget(
            WidgetTitle title,
            WidgetFooter footer,
            params HeadColumn[] columns
        )
        {
            Title = title;
            Footer = footer;
            _head = columns;
        }

        
        public TableAppWidget(
            WidgetTitle title,
            params HeadColumn[] columns
        )
        {
            Title = title;
            _head = columns;
        }

        public void Add(Row row)
        {
            _rows.Add(row);
        }

        public void Add(params string[] items) => Add(new Row(items.Select(i => (Column)i).ToArray()));

        public JObject ToJson()
        {
            var table = new JObject();
            table.Merge(Title.ToJson());
            if (Footer != null) 
                table.Merge(Footer.ToJson());

            var head = new JArray();
            var body = new JArray();

            foreach (var headColumn in _head)
            {
                head.Add(new JObject
                {
                    {"text", headColumn.Name},
                    {"align", headColumn.Align switch
                    {
                        RowAlignType.Center => "center",
                        RowAlignType.Right => "right",
                        _ => "left"
                    }}
                });
            }
            
            foreach (var row in _rows)
            {
                var array = new JArray();
                foreach (var rowItem in row.Items)
                {
                    var column = new JObject
                    {
                        {"text", rowItem.Name}
                    };
                    
                    if (rowItem.Icon != null) column.Add("icon_id", rowItem.Icon);
                    if (rowItem.Icon != null) column.Add("url", rowItem.Url);
                    
                    array.Add(column);
                }
                
                body.Add(array);
            }
            
            table.Add("head", head);
            table.Add("body", body);
            return table;
        }

        public override string ToString() => $"return {ToJson()};";
    }
}
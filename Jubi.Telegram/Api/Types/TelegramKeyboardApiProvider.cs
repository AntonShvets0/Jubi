using Jubi.Api;
using Jubi.Api.Types;
using Jubi.Response.Attachments.Keyboard;
using Jubi.Response.Attachments.Keyboard.Models;
using Newtonsoft.Json.Linq;
using Jubi.Response.Attachments.Keyboard.Parameters;

namespace Jubi.Telegram.Api.Types
{
    public class TelegramKeyboardApiProvider : IKeyboardApiProvider
    {
        public IApiProvider Provider { get; set; }

        public JObject BuildReplyMarkupKeyboard(KeyboardButton menu, KeyboardPage keyboard, bool isOneTime = false)
        {
            var buttons = new JArray();
            foreach (var row in keyboard.Rows)
            {
                buttons.Add(new JArray());
                
                foreach (var button in row.Buttons)
                {
                    (buttons[buttons.Count - 1] as JArray).Add(button.Name);
                }
            }

            if (menu != null)
            {
                buttons.Add(new JArray
                {
                    {menu.Name}
                });
            }
            
            return new JObject
            {
                {"one_time_keyboard", isOneTime},
                {"resize_keyboard", true},
                {"keyboard", buttons}
            };
        }

        private JObject GetButton(KeyboardButton button)
        {
            var obj = new JObject
            {
                {"text", button.Name}
            };

            if (button.Action is DefaultButtonAction defaultButton)
            {
                if (defaultButton.Executor != null) obj.Add("callback_data", defaultButton.Executor); 
                else obj.Add("callback_data", button.Name);
            }
            else if (button.Action is LinkButtonAction linkButtonAction)
            {
                obj.Add("url", linkButtonAction.Url);
            }

            return obj;
        }

        public JObject BuildInlineMarkupKeyboard(KeyboardPage keyboard)
        {
            var buttons = new JArray();
            foreach (var row in keyboard.Rows)
            {
                buttons.Add(new JArray());
                
                foreach (var button in row.Buttons)
                {
                    (buttons[buttons.Count - 1] as JArray).Add(GetButton(button));
                }
            }
            
            return new JObject
            {
                {"inline_keyboard", buttons}
            };
        }

        public void OnInit() { }
    }
}
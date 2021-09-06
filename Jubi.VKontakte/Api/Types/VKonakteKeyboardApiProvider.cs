using Jubi.Api;
using Jubi.Api.Types;
using Jubi.Response.Attachments.Keyboard;
using Jubi.Response.Attachments.Keyboard.Models;
using Jubi.Response.Attachments.Keyboard.Parameters;
using Newtonsoft.Json.Linq;

namespace Jubi.VKontakte.Api.Types
{
    public class VKonakteKeyboardApiProvider : IKeyboardApiProvider
    {
        public IApiProvider Provider { get; set; }

        public void OnInit() { }

        public JObject BuildInlineMarkupKeyboard(KeyboardPage keyboard)
        {
            var buttons = new JArray();
            foreach (var row in keyboard.Rows)
            {
                if (row.Buttons.Count < 1) continue;
                
                buttons.Add(new JArray());
                
                foreach (var button in row.Buttons)
                {
                    (buttons[buttons.Count - 1] as JArray).Add(GetButton(button));
                }
            }
            
            return new JObject
            {
                {"buttons", buttons},
                {"inline", true}
            };
        }

        public JObject BuildReplyMarkupKeyboard(KeyboardButton menu, KeyboardPage keyboard, bool isOneTime = false)
        {
            var buttons = new JArray();
            foreach (var row in keyboard.Rows)
            {
                if (row.Buttons.Count < 1) continue;
                
                buttons.Add(new JArray());
                
                foreach (var button in row.Buttons)
                {
                    (buttons[buttons.Count - 1] as JArray).Add(GetButton(button));
                }
            }

            if (menu != null)
            {
                buttons.Add(new JArray
                {
                    GetButton(menu)
                });
            }
            
            return new JObject
            {
                {"one_time", isOneTime},
                {"buttons", buttons},
                {"inline", false}
            };
        }

        private JObject GetButton(KeyboardButton button)
        {
            var obj = new JObject
            {
                {
                    "action", GetAction(button)
                }
            };

            if (button.Action is DefaultButtonAction)
            {
                obj.Add("color", button.Color switch
                {
                    KeyboardColor.Green => "positive",
                    KeyboardColor.Red => "negative",
                    KeyboardColor.Primary => "primary",
                    _ => "secondary"
                });
            }

            return obj;
        }

        private JObject GetAction(KeyboardButton button)
        {
            if (button.Action is DefaultButtonAction)
                return new JObject
                {
                    {"type", "text"},
                    {"label", button.Name},
                    {
                        "payload", new JObject
                        {
                            {"command", (button.Action as DefaultButtonAction).Executor}
                        }.ToString()
                    }
                };
            
            if (button.Action is LinkButtonAction linkButtonAction)
            {
                return new JObject
                {
                    {"type", "open_link"},
                    {"label", button.Name},
                    {"link", linkButtonAction.Url}
                };
            }

            return null;
        }
    }
}
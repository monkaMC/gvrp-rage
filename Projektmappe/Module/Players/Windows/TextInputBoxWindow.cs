using System;
using GVRP.Module.Players.Db;
using GTANetworkAPI;
using Newtonsoft.Json;
using GVRP.Module.ClientUI.Windows;

namespace GVRP.Module.Players.Windows
{
    public class TextInputBoxWindow : Window<Func<DbPlayer, TextInputBoxWindowObject, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "textBoxObject")] private TextInputBoxWindowObject TextBoxObject { get; }

            public ShowEvent(DbPlayer dbPlayer, TextInputBoxWindowObject textBoxObject) : base(dbPlayer)
            {
                TextBoxObject = textBoxObject;
            }
        }

        public TextInputBoxWindow() : base("TextInputBox")
        {
        }

        public override Func<DbPlayer, TextInputBoxWindowObject, bool> Show()
        {
            return (player, textBoxObject) => OnShow(new ShowEvent(player, textBoxObject));
        }
    }

    public class TextInputBoxWindowObject
    {
        public string Title { get; set; }
        public string Message  { get; set; }
        public string Callback  { get; set; }
        public object CustomData { get; set; }
    }
}
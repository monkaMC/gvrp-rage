using System;
using System.Security.Principal;
using GVRP.Module.Players.Db;
using GTANetworkAPI;
using Newtonsoft.Json;
using GVRP.Module.ClientUI.Windows;

namespace GVRP.Module.Players.Windows
{
    public class ConfirmationWindow : Window<Func<DbPlayer, ConfirmationObject, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "confirmationObject")] private ConfirmationObject ConfirmationObject { get; }

            public ShowEvent(DbPlayer dbPlayer, ConfirmationObject confirmationObject) : base(dbPlayer)
            {
                ConfirmationObject = confirmationObject;
            }
        }

        public ConfirmationWindow() : base("Confirmation")
        {
        }

        public override Func<DbPlayer, ConfirmationObject, bool> Show()
        {
            return (player, confirmationObject) => OnShow(new ShowEvent(player, confirmationObject));
        }
    }

    public class ConfirmationObject
    {
        public string Title { get; set; }
        public string Message  { get; set; }
        public string Callback  { get; set; }
        public string Arg1  { get; set; }
        public string Arg2  { get; set; }

        public ConfirmationObject(string title, string message, string callback, string arg1 = "", string arg2 = "")
        {
            Title = title;
            Message = message;
            Callback = callback;
            Arg1 = arg1;
            Arg2 = arg2;
        }
    }
}
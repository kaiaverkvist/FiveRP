using System;
using System.Collections.Generic;
using System.Linq;
using FiveRP.Gamemode.Features.Weapons;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;
using Newtonsoft.Json;
using FiveRP.Gamemode.Features.Customization;
using FiveRP.Gamemode.Database.Tables;

namespace FiveRP.Gamemode.Features
{
    public class TattooParlors : Script
    {

        private static List<TattooParlor> _tattooParlors;

        public TattooParlors()
        {
            _tattooParlors = new List<TattooParlor>();

            API.onResourceStart += OnResourceStart;
        }

        private void OnResourceStart()
        {
            InitializeTattooParlors();
        }

        private void InitializeTattooParlors()
        {
            _tattooParlors.Add(new TattooParlor(new Vector3(321.0592, 178.1415, 103.528)));
            _tattooParlors.Add(new TattooParlor(new Vector3(-1155.2, -1422.9, 4.77)));
            _tattooParlors.Add(new TattooParlor(new Vector3(1858.99, 3748.5, 33.07)));
            foreach (var store in _tattooParlors)
            {
                API.createTextLabel("~b~Tattoo Parlor\n~y~/buytattoo\n~g~$250", store.Position, 45f, 0.3f);
                var blip = API.createBlip(store.Position);
                API.setBlipSprite(blip, 75);
                API.setBlipColor(blip, 2);
                API.setBlipScale(blip, 1.3f);
                API.setBlipShortRange(blip, true);
            }
        }


        [Command("buytattoo")]
        public void command_BuyTattoo(Client sender)
        {
            foreach (var store in _tattooParlors)
            {
                if (DistanceLibrary.DistanceBetween(API.getEntityPosition(sender), store.Position) <= 10)
                {
                    Character characterData = Account.GetPlayerCharacterData(sender);
                    if (characterData.CharacterData.Money < 250)
                    {
                        API.sendChatMessageToPlayer(sender, "~r~ERROR:~w~ You cannot afford this.");
                        return;
                    }

                    List<string> relevantVariants = new List<string>();
                    bool noVariant = true;
                    foreach (VariantData variant in PedVariants.pedVariants)
                    {
                        if (variant.Skin.ToLower() == Enum.GetName(typeof(PedHash), sender.model).ToLower() && variant.PublicName.StartsWith("tattoo_"))
                        {
                            relevantVariants.Add(variant.PublicName);
                            noVariant = false;
                        }
                    }
                    if (noVariant)
                    {
                        API.sendChatMessageToPlayer(sender, "~r~There is nothing for you here.");
                        return;
                    }
                    API.setEntityData(sender, "clothes_price", 250);
                    API.triggerClientEvent(sender, "display_variantopt_menu", relevantVariants.ToArray());
                    return;
                }
            }
        }
    }

    class TattooParlor
    {
        public Vector3 Position { get; set; }

        public TattooParlor(Vector3 position)
        {
            Position = position;
        }
    }
}
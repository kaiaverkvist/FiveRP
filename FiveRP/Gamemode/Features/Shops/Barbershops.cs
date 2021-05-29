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
    public class Barbershops : Script
    {
        private static List<Barbershop> _barbershopStores;

        public Barbershops()
        {
            _barbershopStores = new List<Barbershop>();

            API.onResourceStart += OnResourceStart;
        }

        private void OnResourceStart()
        {
            InitializeBarbershopStores();
        }

        private void InitializeBarbershopStores()
        {
            _barbershopStores.Add(new Barbershop(new Vector3(-823.12, -187.94, 37.56)));
            _barbershopStores.Add(new Barbershop(new Vector3(132.6296, -1712.11, 29.29169)));
            _barbershopStores.Add(new Barbershop(new Vector3(-30.31236, -147.472, 57.07739)));
            _barbershopStores.Add(new Barbershop(new Vector3(1207.301, -470.622, 66.15)));
            _barbershopStores.Add(new Barbershop(new Vector3(-1289, -1116.6, 6.98)));
            _barbershopStores.Add(new Barbershop(new Vector3(1933.6, 3725, 32.79)));
            foreach (var store in _barbershopStores)
            {
                API.createTextLabel("~b~Barber Shop\n~y~/buyhaircut\n~g~$150", store.Position, 45f, 0.3f);
                var blip = API.createBlip(store.Position);
                API.setBlipSprite(blip, 71);
                API.setBlipColor(blip, 2);
                API.setBlipScale(blip, 1.3f);
                API.setBlipShortRange(blip, true);
            }
        }

        [Command("buyhaircut")]
        public void command_BuyHaircut(Client sender)
        {
            foreach (var store in _barbershopStores)
            {
                if (DistanceLibrary.DistanceBetween(API.getEntityPosition(sender), store.Position) <= 10)
                {

                    Character characterData = Account.GetPlayerCharacterData(sender);
                    if (characterData.CharacterData.Money < 150)
                    {
                        API.sendChatMessageToPlayer(sender, "~r~ERROR:~w~ You cannot afford this.");
                        return;
                    }

                    List<string> relevantVariants = new List<string>();
                    bool noVariant = true;
                    foreach (VariantData variant in PedVariants.pedVariants)
                    {
                        if (variant.Skin.ToLower() == Enum.GetName(typeof(PedHash), sender.model).ToLower() && variant.PublicName.StartsWith("hair_"))
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
                    API.setEntityData(sender, "clothes_price", 150);
                    API.triggerClientEvent(sender, "display_variantopt_menu", relevantVariants.ToArray());
                    return;
                }
            }
        }
    }

    class Barbershop
    {
        public Vector3 Position { get; set; }

        public Barbershop(Vector3 position)
        {
            Position = position;
        }
    }
}
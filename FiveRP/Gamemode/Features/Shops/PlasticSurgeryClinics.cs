using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Features.Customization;
using FiveRP.Gamemode.Library.FunctionLibraries;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using GTANetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveRP.Gamemode.Features.Shops
{
    class PlasticSurgeryClinics : Script
    {
        private static List<PlasticSurgeryClinic> _plasticSurgeryClinics;

        public PlasticSurgeryClinics()
        {
            _plasticSurgeryClinics = new List<PlasticSurgeryClinic>();

            API.onResourceStart += OnResourceStart;
        }

        private void OnResourceStart()
        {
            InitializePlasticSurgeryClinics();
        }

        private void InitializePlasticSurgeryClinics()
        {
            _plasticSurgeryClinics.Add(new PlasticSurgeryClinic(new Vector3(-1882.842, -578.6257, 11.82038)));
            _plasticSurgeryClinics.Add(new PlasticSurgeryClinic(new Vector3(1157.14, -453.87, 66.98)));
            _plasticSurgeryClinics.Add(new PlasticSurgeryClinic(new Vector3(1854.26, 3701.29, 34.26)));
            foreach (var store in _plasticSurgeryClinics)
            {
                API.createTextLabel("~b~Plastic Surgeon\n~y~/changeface\n~g~$350", store.Position, 45f, 0.3f);
                var blip = API.createBlip(store.Position);
                API.setBlipSprite(blip, 61);
                API.setBlipColor(blip, 2);
                API.setBlipScale(blip, 1.3f);
                API.setBlipShortRange(blip, true);
            }
        }

        [Command("changeface")]
        public void command_BuyFace(Client sender)
        {
            foreach (var store in _plasticSurgeryClinics)
            {
                if (DistanceLibrary.DistanceBetween(API.getEntityPosition(sender), store.Position) <= 10)
                {
                    Character characterData = Account.GetPlayerCharacterData(sender);
                    if (characterData.CharacterData.Money <= 350)
                    {
                        API.sendChatMessageToPlayer(sender, "~r~ERROR:~w~ You cannot afford this.");
                        return;
                    }
                    bool noVariant = true;
                    List<string> relevantVariants = new List<string>();
                    foreach (VariantData variant in PedVariants.pedVariants)
                    {
                        if (variant.Skin.ToLower() == Enum.GetName(typeof(PedHash), sender.model).ToLower() && variant.PublicName.StartsWith("skin_"))
                        {
                            noVariant = false;
                            relevantVariants.Add(variant.PublicName);
                        }
                    }
                    if (noVariant)
                    {
                        API.sendChatMessageToPlayer(sender, "~r~There is nothing for you here.");
                        return;
                    }
                    API.setEntityData(sender, "clothes_price", 350);
                    API.triggerClientEvent(sender, "display_variantopt_menu", relevantVariants.ToArray());
                }
            }
        }
    }

    class PlasticSurgeryClinic
    {
        public Vector3 Position { get; set; }

        public PlasticSurgeryClinic(Vector3 position)
        {
            Position = position;
        }
    }
}

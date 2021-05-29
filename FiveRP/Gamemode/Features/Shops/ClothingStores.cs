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
    public class ClothingStores : Script
    {
        private static PedHash[] Blacklist =
        {
            PedHash.AbigailCutscene, PedHash.Agent14Cutscene, PedHash.AmandaTownleyCutscene, PedHash.AndreasCutscene,
            PedHash.AntonCutscene, PedHash.AshleyCutscene, PedHash.AviSchwartzmanCutscene, PedHash.BallasogCutscene,
            PedHash.BankmanCutscene, PedHash.BarryCutscene, PedHash.BeverlyCutscene, PedHash.BradCadaverCutscene,
            PedHash.BradCutscene, PedHash.BrideCutscene, PedHash.BurgerDrugCutscene, PedHash.Car3Guy1Cutscene,
            PedHash.Car3Guy2Cutscene, PedHash.CaseyCutscene, PedHash.Chef2Cutscene, PedHash.Claude01,
            PedHash.ClayCutscene, PedHash.CletusCutscene, PedHash.Corpse01, PedHash.Corpse02,
            PedHash.CrisFormageCutscene, PedHash.DaleCutscene, PedHash.DaveNortonCutscene, PedHash.DeniseCutscene,
            PedHash.DevinCutscene, PedHash.DomCutscene, PedHash.DreyfussCutscene, PedHash.DrFriedlanderCutscene,
            PedHash.FabienCutscene, PedHash.FbiSuit01Cutscene, PedHash.FilmNoir, PedHash.FloydCutscene,
            PedHash.GCutscene, PedHash.GroomCutscene, PedHash.HaoCutscene, PedHash.HunterCutscene, PedHash.Imporage,
            PedHash.JanetCutscene, PedHash.JanitorCutscene, PedHash.JewelassCutscene, PedHash.JimmyBostonCutscene,
            PedHash.JimmyDisantoCutscene, PedHash.JoeMinutemanCutscene, PedHash.JohnnyKlebitz,
            PedHash.JohnnyKlebitzCutscene, PedHash.JosefCutscene, PedHash.JoshCutscene, PedHash.KarenDanielsCutscene,
            PedHash.LamarDavisCutscene, PedHash.LazlowCutscene, PedHash.LesterCrestCutscene, PedHash.Lifeinvad01Cutscene,
            PedHash.MagentaCutscene, PedHash.ManuelCutscene, PedHash.MarnieCutscene, PedHash.Marston01,
            PedHash.MartinMadrazoCutscene, PedHash.MaryannCutscene, PedHash.MaudeCutscene, PedHash.MichelleCutscene,
            PedHash.MiltonCutscene, PedHash.Misty01, PedHash.MollyCutscene, PedHash.MoviePremFemaleCutscene,
            PedHash.MoviePremMaleCutscene, PedHash.MovAlien01, PedHash.Movspace01SMM, PedHash.MrKCutscene,
            PedHash.MrsPhillipsCutscene, PedHash.MrsThornhillCutscene, PedHash.NataliaCutscene,
            PedHash.NervousRonCutscene, PedHash.NigelCutscene, PedHash.Niko01, PedHash.OldMan1aCutscene,
            PedHash.OldMan2Cutscene, PedHash.OmegaCutscene, PedHash.Orleans, PedHash.OrleansCutscene,
            PedHash.OrtegaCutscene, PedHash.PaigeCutscene, PedHash.PaperCutscene, PedHash.PatriciaCutscene,
            PedHash.Pogo01, PedHash.PopovCutscene, PedHash.PriestCutscene, PedHash.PrologueDriverCutscene,
            PedHash.PrologueSec02Cutscene, PedHash.RampGangCutscene, PedHash.RampHicCutscene,
            PedHash.RampHipsterCutscene, PedHash.RampMarineCutscene, PedHash.RampMexCutscene, PedHash.RashkovskyCutscene,
            PedHash.RoccoPelosiCutscene, PedHash.RsRanger01AMO, PedHash.RussianDrunkCutscene,
            PedHash.ScreenWriterCutscene, PedHash.SiemonYetarianCutscene, PedHash.SolomonCutscene,
            PedHash.SteveHainsCutscene, PedHash.StretchCutscene, PedHash.TanishaCutscene, PedHash.TaoChengCutscene,
            PedHash.TaosTranslatorCutscene, PedHash.TennisCoachCutscene, PedHash.TerryCutscene,
            PedHash.TomEpsilonCutscene, PedHash.TonyaCutscene, PedHash.TracyDisantoCutscene,
            PedHash.TrafficWardenCutscene, PedHash.VagosSpeakCutscene, PedHash.WadeCutscene, PedHash.WeiChengCutscene,
            PedHash.ZimborCutscene, PedHash.Zombie01, PedHash.Armymech01SMY, PedHash.Blackops01SMY,
            PedHash.Blackops02SMY, PedHash.Blackops03SMY, PedHash.Marine01SMM, PedHash.Marine01SMY, PedHash.Marine02SMM,
            PedHash.Marine02SMY, PedHash.Marine03SMY, PedHash.FbiSuit01Cutscene, PedHash.FibArchitect,
            PedHash.FibOffice01SMM, PedHash.FibSec01, PedHash.FibSec01SMM, PedHash.Autopsy01SMY,
            PedHash.Fireman01SMY, PedHash.Paramedic01SMM, PedHash.Scrubs01SFY, PedHash.Cop01SFY, PedHash.Cop01SMY,
            PedHash.Hwaycop01SMY, PedHash.Swat01SMY, PedHash.CopCutscene, PedHash.Sheriff01SFY, PedHash.Sheriff01SMY,
            PedHash.Snowcop01SMM, PedHash.Ranger01SFY, PedHash.Ranger01SMY, PedHash.Boar, PedHash.Cat, PedHash.ChickenHawk, PedHash.Chimp, PedHash.Chop, PedHash.Cormorant, PedHash.Cow, PedHash.Coyote, PedHash.Crow, PedHash.Deer, PedHash.Dolphin, PedHash.Fish, PedHash.HammerShark, PedHash.Hen, PedHash.Humpback, PedHash.Husky, PedHash.KillerWhale, PedHash.MountainLion, PedHash.Pig, PedHash.Pigeon, PedHash.Poodle, PedHash.Pug, PedHash.Rabbit, PedHash.Rat, PedHash.Retriever, PedHash.Rhesus, PedHash.Rottweiler, PedHash.Seagull, PedHash.Shepherd, PedHash.Stingray, PedHash.TigerShark, PedHash.Westy
        };

        private static List<ClothingStore> _clothingStores;

        public ClothingStores()
        {
            _clothingStores = new List<ClothingStore>();

            API.onResourceStart += OnResourceStart;
        }

        private void OnResourceStart()
        {
            InitializeClothingStores();
        }

        private void InitializeClothingStores()
        {
            _clothingStores.Add(new ClothingStore(new Vector3(-710.1662, -153.1423, 37.41514)));
            _clothingStores.Add(new ClothingStore(new Vector3(75.67654, -1393.035, 29.37612)));
            _clothingStores.Add(new ClothingStore(new Vector3(-1192.344, -768.1927, 17.31975)));
            _clothingStores.Add(new ClothingStore(new Vector3(615.0091, 2762.964, 42.08809)));
            _clothingStores.Add(new ClothingStore(new Vector3(125.65, -223.7578, 54.55783)));
            _clothingStores.Add(new ClothingStore(new Vector3(-3170.988, 1044.218, 20.86321)));
            _clothingStores.Add(new ClothingStore(new Vector3(1693.887, 4822.513, 42.06308)));
            _clothingStores.Add(new ClothingStore(new Vector3(425.2208, -806.1973, 29.49113)));
            _clothingStores.Add(new ClothingStore(new Vector3(-1101.441, 2710.484, 19.10785)));
            _clothingStores.Add(new ClothingStore(new Vector3(1196.796, 2710.163, 38.2226)));
            _clothingStores.Add(new ClothingStore(new Vector3(-822.442, -1073.539, 11.3281)));
            _clothingStores.Add(new ClothingStore(new Vector3(4.643052, 6512.255, 31.87783)));
            _clothingStores.Add(new ClothingStore(new Vector3(-1450.509, -237.5589, 49.81047)));

            foreach (var store in _clothingStores)
            {
                API.createTextLabel("~b~Clothing store\n~y~/buyclothes ~g~$100~w~\n~y~/buyskin ~g~$100", store.Position, 45f, 0.3f);
                var blip = API.createBlip(store.Position);
                API.setBlipSprite(blip, 73);
                API.setBlipColor(blip, 2);
                API.setBlipScale(blip, 1.3f);
                API.setBlipShortRange(blip, true);
            }
        }

        [Command("buyclothes")]
        public void command_BuyClothes(Client sender)
        {
            foreach (var store in _clothingStores)
            {
                if (DistanceLibrary.DistanceBetween(API.getEntityPosition(sender), store.Position) <= 20)
                {
                    Character characterData = Account.GetPlayerCharacterData(sender);
                    if (characterData.CharacterData.Money < 100)
                    {
                        API.sendChatMessageToPlayer(sender, "~r~You cannot afford this.");
                        return;
                    }
                    List<string> relevantVariants = new List<string>();
                    bool noVariant = true;
                    foreach (VariantData variant in PedVariants.pedVariants)
                    {
                        if (variant.Skin.ToLower() == Enum.GetName(typeof(PedHash), sender.model).ToLower() && !variant.PublicName.StartsWith("skin_") && !variant.PublicName.StartsWith("hair_") && !variant.PublicName.StartsWith("tattoo_"))
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
                    API.setEntityData(sender, "clothes_price", 100);
                    API.triggerClientEvent(sender, "display_variantopt_menu", relevantVariants.ToArray());
                }
            }
        }

        [Command("buyskin", Alias = "bclothing")]
        public void BuyClothingCommand(Client sender, string model)
        {
            var senderData = Account.GetPlayerCharacterData(sender);
            int value = 0;
            if (int.TryParse(model.ToString(), out value))
                API.sendChatMessageToPlayer(sender, "~r~Error:~w~ Use the skin name.");
            foreach (var store in _clothingStores)
            {
                if (DistanceLibrary.DistanceBetween(API.getEntityPosition(sender), store.Position) <= 20)
                {
                    if (senderData.CharacterData.Money >= 100)
                    {
                        PedHash pedModel = API.pedNameToModel(model);
                        if (Blacklist.Contains(pedModel))
                        {
                            API.sendChatMessageToPlayer(sender,
                                "Try something else. This skin is currently not available.");
                            return;
                        }

                        if(Enum.IsDefined(typeof(PedHash), model))
                        {
                            API.setPlayerSkin(sender, pedModel);
                            API.sendNativeToPlayer(sender, 0x45EEE61580806D63, sender);

                            var charData = Account.GetPlayerCharacterData(sender);
                            charData.CharacterData.Money -= 100;

                            // Set the skin stored in the database.
                            charData.CharacterData.Skin = model;
                            charData.CharacterData.WardRobe.EquipVariants();
                            // Return their weapons
                            if (charData.CharacterData.Weapons.Length > 0)
                            {
                                var weaponHashes = JsonConvert.DeserializeObject<WeaponsJson[]>(charData.CharacterData.Weapons);

                                foreach (var weapon in weaponHashes)
                                {
                                    API.givePlayerWeapon(sender, API.weaponNameToModel(weapon.WeaponHash), weapon.Ammo, false, true);
                                }
                            }
                        } else sender.sendChatMessage("You will have to find another skin. This one isn't valid. slice.wikidot.com has a list of skins.");
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(sender, "You can't afford this.");
                    }
                }
            }
        }
    }

    class ClothingStore
    {
        public Vector3 Position { get; set; }

        public ClothingStore(Vector3 position)
        {
            Position = position;
        }
    }
}
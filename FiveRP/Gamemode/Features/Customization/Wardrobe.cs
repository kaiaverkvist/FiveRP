using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Managers;
using GTANetworkServer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveRP.Gamemode.Features.Customization
{
    public class Wardrobe : Script
    {
        private Dictionary<string, List<VariantData>> _skinVariantsList;
        private Client _player;

        public Wardrobe(Client player)
        {
            _skinVariantsList = new Dictionary<string, List<VariantData>>();
            _player = player;
            Character characterData = Account.GetPlayerCharacterData(_player);
            if (characterData.CharacterData.Variants.Length > 0)
            {
                VariantsJson[] variantsJson = JsonConvert.DeserializeObject<VariantsJson[]>(characterData.CharacterData.Variants);
                foreach (VariantsJson variantJson in variantsJson)
                {
                    string skin = variantJson.Skin;
                    string variants = variantJson.Variants;
                    List<string> variantNames = variants.Split(';').ToList();
                    variantNames = variantNames.Distinct().ToList();
                    List<VariantData> variantList = new List<VariantData>();
                    foreach (VariantData varData in PedVariants.pedVariants)
                    {
                        foreach (string variantName in variantNames)
                        {
                            if (variantName.ToLower() == varData.PublicName.ToLower() && Enum.GetName(typeof(PedHash), _player.model).ToLower() == varData.Skin.ToLower())
                            {
                                variantList.Add(varData);
                                break;
                            }
                        }
                    }
                    _skinVariantsList.Add(skin, variantList);
                }
            }
            EquipVariants();
        }

        public Wardrobe()
        {
            _skinVariantsList = new Dictionary<string, List<VariantData>>();
        }

        public void AddSkin(string skinName)
        {
            _skinVariantsList.Add(skinName, new List<VariantData>());
        }

        public void AddVariant(string variant)
        {
            Character characterData = Account.GetPlayerCharacterData(_player);
            string skinName = "";
            if (characterData != null)
                skinName = Enum.GetName(typeof(PedHash), _player.model);
            if (!_skinVariantsList.ContainsKey(skinName))
                AddSkin(skinName);
            List<VariantData> currentVariantList = _skinVariantsList[skinName];
            VariantData newVariant = null;
            foreach (VariantData v in PedVariants.pedVariants)
            {
                if (v.Skin.ToLower() != skinName.ToLower())
                    continue;
                if (v.PublicName == variant)
                    newVariant = v;
            }
            if (newVariant == null)
                return;
            bool foundSlot = false;
            List<VariantData> newVariantList = new List<VariantData>();
            foreach (VariantData curVar in currentVariantList)
            {
                if (newVariant.PublicName.ToLower() == curVar.PublicName.ToLower())
                    return;
            }
            List<string> added = new List<string>();
            foreach (VariantData currentVariant in currentVariantList)
            {
                string[] currentSplit = currentVariant.Parse.Split('|');
                int currentSlot = Convert.ToInt32(currentSplit[0]);
                string[] newSplit = newVariant.Parse.Split('|');
                int newSlot = Convert.ToInt32(newSplit[0]);
                if (currentSlot == newSlot && !foundSlot && !added.Contains(newVariant.PublicName))
                {
                    foundSlot = true;
                    newVariantList.Add(newVariant);
                    added.Add(newVariant.PublicName);
                }
                else if (foundSlot && currentSlot == newSlot)
                    continue;
                else if (!added.Contains(currentVariant.PublicName))
                {
                    newVariantList.Add(currentVariant);
                    added.Add(currentVariant.PublicName);
                }
            }
            if (newVariantList.Count == 0 || !foundSlot)
                newVariantList.Add(newVariant);
            _skinVariantsList[skinName] = newVariantList;
            SaveVariants();
            EquipVariants();
        }

        public void EquipVariants()
        {
            Character characterData = Account.GetPlayerCharacterData(_player);
            if (characterData == null)
                return;
            if (!_skinVariantsList.ContainsKey(Enum.GetName(typeof(PedHash), _player.model)))
            {
                API.setPlayerDefaultClothes(_player);
                return;
            }
            List<VariantData> currentVariantList = _skinVariantsList[Enum.GetName(typeof(PedHash), _player.model)];
            foreach (VariantData v in currentVariantList)
            {
                string[] split = v.Parse.Split('|');
                int slot = Convert.ToInt32(split[0]);
                int drawable = Convert.ToInt32(split[1]);
                int texture = Convert.ToInt32(split[2]);
                API.setPlayerClothes(_player, slot, drawable, texture);
            }
        }

        public void SaveVariants()
        {
            Character characterData = Account.GetPlayerCharacterData(_player);
            Dictionary<string, string> dicToSerialize = new Dictionary<string, string>();
            foreach (string skin in _skinVariantsList.Keys)
            {
                string variantsString = "";
                foreach (VariantData variant in _skinVariantsList[skin])
                {
                    if (variantsString.Length == 0)
                        variantsString = variant.PublicName;
                    else
                        variantsString += ";" + variant.PublicName;
                }
                dicToSerialize.Add(skin, variantsString);
            }
            if (characterData != null)
            {
                if (!_player.IsNull)
                {
                    var jsonVariants= JsonConvert.SerializeObject(dicToSerialize.Select(skin => new { Skin = skin.Key, Variants = skin.Value }));
                    characterData.CharacterData.Variants = jsonVariants;
                }
            }
        }
    }
}

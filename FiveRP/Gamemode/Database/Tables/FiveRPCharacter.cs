using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GTANetworkServer;
using FiveRP.Gamemode.Features.Inventories;
using FiveRP.Gamemode.Features.Customization;

namespace FiveRP.Gamemode.Database.Tables
{
    public class CharacterMoneyScript : Script
    {
        public void SetMoney(Client player, int amount)
        {
            if (player != null)
            {
                API.setEntitySyncedData(player, "moneyDisplay", amount);
            }
        }
        public void SetBank(Client player, int amount)
        {
            if (player != null)
            {
                API.setEntitySyncedData(player, "bankDisplay", amount);
            }
        }
    }

    [Table("fiverp_characters")]
    public class FiveRpCharacter
    {
        [Key]
        [Column("id")]
        public int Cid { get; set; }

        [Column("character_id")]
        public int CharacterUcpId { get; set; }

        [Column("banned_until", TypeName = "DateTime")]
        public DateTime? BannedUntil { get; set; }

        [NotMapped]
        private int _money;

        [Column("money")]
        public int Money
        {
            get
            {
                return this._money;
            }
            set
            {
                this._money = value;
                var moneyScript = new CharacterMoneyScript();
                moneyScript.SetMoney(Client, value);
            }
        }

        [NotMapped]
        private int _bank;

        [Column("bank")]
        public int Bank
        {
            get
            {
                return this._bank;
            }
            set
            {
                this._bank = value;
                var moneyScript = new CharacterMoneyScript();
                moneyScript.SetBank(Client, value);
            }
        }

        [Column("skin")]
        public string Skin { get; set; }

        [Column("variant")]
        public string Variants { get; set; }

        [Column("level")]
        public int Level { get; set; }

        [Column("minutes")]
        public int Minutes { get; set; }

        [Column("faction")]
        public int Organization { get; set; }

        [Column("factionrank")]
        public int OrganizationRank { get; set; }

        [Column("spawn")]
        public int Spawn { get; set; }

        [Column("jail_time")]
        public int JailTime { get; set; }

        [Column("radio_channel")]
        public int RadioChannel { get; set; }

        [Column("phone_number")]
        public int PhoneNumber { get; set; }

        [Column("saved_x")]
        public float SavedX { get; set; }

        [Column("saved_y")]
        public float SavedY { get; set; }

        [Column("saved_z")]
        public float SavedZ { get; set; }

        [Column("saved_dimension")]
        public int SavedDimension { get; set; }

        [Column("items")]
        public string Items { get; set; }

        [Column("weapons")]
        public string Weapons { get; set; }

        [Column("activity_streak")]
        public int ActivityStreak { get; set; }

        [Column("last_login")]
        public DateTime LastLogin { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [NotMapped]
        public List<string> AdminRoles { get; set; }

        [NotMapped]
        public List<string> AdminRoleShortcodes { get; set; }

        [NotMapped]
        public List<DisciplinaryAction> DisciplinaryActions { get; set; }

        [NotMapped]
        public List<FiveRPFine> Fines { get; set; }

        [NotMapped]
        public PlayerInventory Inventory { get; set; }

        [NotMapped]
        public Wardrobe WardRobe { get; set; }

        //Phone Storage
        [NotMapped]
        public List<PhoneMessage> PhoneMessages { get; set; }

        [NotMapped]
        public List<PhoneContact> PhoneContacts { get; set; }

        [NotMapped]
        public Client Client { private get; set; }

    }
}
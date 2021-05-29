using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FiveRP.Gamemode.Database.Tables
{
    [Table("fiverp_money_transfers")]
    public class MoneyTransfer
    {
        public enum TransferType
        {
            Bank,
            Cash,
            Withdraw,
            Deposit,
            AdminBank,
            AdminCash,
            Purchase
        }

        public MoneyTransfer()
        {

        }

        public MoneyTransfer(int fromCharacterId, int toCharacterId, TransferType type, int amount)
        {
            FromCharacterId = fromCharacterId;
            ToCharacterId = toCharacterId;
            Type = type;
            Amount = amount;
            CreatedAt = DateTime.Now;
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("from_character_id")]
        public int FromCharacterId { get; set; }

        [Column("to_character_id")]
        public int ToCharacterId { get; set; }

        [Column("type")]
        public TransferType Type { get; set; }

        [Column("amount")]
        public int Amount { get; set; }

        [Column("created_at", TypeName = "DateTime")]
        public DateTime CreatedAt { get; set; }
    }
}
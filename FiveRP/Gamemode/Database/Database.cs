using System.Data.Entity;
using FiveRP.Gamemode.Database.Tables;
using FiveRP.Gamemode.Library;
using MySql.Data.Entity;

namespace FiveRP.Gamemode.Database
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class Database : DbContext
    {
        // Connect to the database using stored credentials (server.cfg)
        public Database() : base($"data source={Config.GetKeyString("#server")};database={Config.GetKeyString("#database")};uid={Config.GetKeyString("#user")};pwd={Config.GetKeyString("#password")};Convert Zero Datetime=True;Allow Zero Datetime=True")
        {
            Configuration.LazyLoadingEnabled = false;
        }

        // Character and character fields.
        public DbSet<FiveRpCharacter> FiveRpCharacters { get; set; }
        public DbSet<Character> Character { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<UserHasRoles> UserHasRoles { get; set; }

        // Roles
        public DbSet<Role> Roles { get; set; }

        // Disciplinary action log
        public DbSet<DisciplinaryAction> DisciplinaryActions { get; set; }

        // Money transfer log
        public DbSet<MoneyTransfer> MoneyTransfers { get; set; }

        // Vehicles
        public DbSet<FiveRPVehicle> Vehicles { get; set; }

        // Teleports
        public DbSet<Teleport> Teleports { get; set; }

        // Factions
        public DbSet<Organization> Organizations { get; set; }

        // Alerts
        public DbSet<Alert> Alerts { get; set; }

        // Properties
        public DbSet<Property> Properties { get; set; }

        // Fines
        public DbSet<FiveRPFine> Fines { get; set; }

        // PhoneMessages
        public DbSet<PhoneMessage> PhoneMessages { get; set; }

        // Phone Contacts
        public DbSet<PhoneContact> PhoneContacts { get; set; }
    }
}

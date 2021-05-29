using GTANetworkServer;

namespace FiveRP.Gamemode.Features.BaseRoleplay
{
    public class HelpCommands : Script
    {
        [Command("help", GreedyArg = true)]
        public void BaseHelpCommand(Client sender)
        {
            API.sendChatMessageToPlayer(sender, "Chat   | /o(oc) /b /me(low) /do /my /s(hout) /l(ow) /w(hisper) /pm");
            API.sendChatMessageToPlayer(sender, "Player | ~b~/helpme~w~, /stats, /pay, /fines, /withdraw, /deposit, /sellvehicle, /anim, /report, /time");
            API.sendChatMessageToPlayer(sender, "Vehicle  | /vget, /vpark, /vlock (or /lock), /engine, /sellvehicle, /abortsale, /vehmenu, /vtitem(s), /pitem(s), /trunk, /hood, /door");
            API.sendChatMessageToPlayer(sender, "Inventory  | /items, /giveitem, /useitem, /deleteitem, /showitems, /showlicenses, /charity");
            API.sendChatMessageToPlayer(sender, "Organization | /online, /orgchat (/f), /policehelp");
            API.sendChatMessageToPlayer(sender, "Jobs   | /trucking, /fishing, /garbage");
            API.sendChatMessageToPlayer(sender, "Radio  | /r(adio), /setfrequency");
            API.sendChatMessageToPlayer(sender, "Phone  | /sms, /call, /contacts, /messages, (temp: /911)");
            API.sendChatMessageToPlayer(sender, "Weapons  | /weapons, /dropweapon, /equip, /unequip, /unload, /reload");
            API.sendChatMessageToPlayer(sender, "Properties  | /ppitem(s) /ptitems(s) /pinv /setrentable /evictall /kicktenant /tenants /stoprent /plock /pinfo /pwithdraw(all) /pdeposit /pmenu /sellproperty /playersellproperty");
        }

        [Command("policehelp", GreedyArg = true)]
        public void PoliceHelpCommand(Client sender)
        {
            API.sendChatMessageToPlayer(sender, "Chat   | /dep, /m");
            API.sendChatMessageToPlayer(sender, "General | /duty, /uniform");
            API.sendChatMessageToPlayer(sender, "Inventory  | /frisk, /seizeitem, /seizeweapons");
            API.sendChatMessageToPlayer(sender, "Arrest | /arrest, /cuff, /uncuff");
            API.sendChatMessageToPlayer(sender, "Licenses | /givelicense, /takelicense");
            API.sendChatMessageToPlayer(sender, "Fines | /fine, /checkfines");
        }

        [Command("animhelp", GreedyArg = true)]
        public void AnimationHelpCommand(Client sender)
        {
            API.sendChatMessageToPlayer(sender, "/anim help, /sit (more to come), /getup, /situps, /liedown, /smoke, /crossarms, /yoga, /checkbody, /walktime, /walkeat, /walkdrink, /halt, /hide ");
        }
    }
}
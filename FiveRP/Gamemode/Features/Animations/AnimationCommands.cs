using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkServer;

namespace FiveRP.Gamemode.Features.Animations
{
    public class AnimationCommands : Script
    {
        public Dictionary<string, string> AnimationList = new Dictionary<string, string>
        {
            {"finger", "mp_player_intfinger mp_player_int_finger"},
            {"guitar", "anim@mp_player_intcelebrationmale@air_guitar air_guitar"},
            {"shagging", "anim@mp_player_intcelebrationmale@air_shagging air_shagging"},
            {"synth", "anim@mp_player_intcelebrationmale@air_synth air_synth"},
            {"kiss", "anim@mp_player_intcelebrationmale@blow_kiss blow_kiss"},
            {"bro", "anim@mp_player_intcelebrationmale@bro_love bro_love"},
            {"chicken", "anim@mp_player_intcelebrationmale@chicken_taunt chicken_taunt"},
            {"chin", "anim@mp_player_intcelebrationmale@chin_brush chin_brush"},
            {"dj", "anim@mp_player_intcelebrationmale@dj dj"},
            {"dock", "anim@mp_player_intcelebrationmale@dock dock"},
            {"facepalm", "anim@mp_player_intcelebrationmale@face_palm face_palm"},
            {"fingerkiss", "anim@mp_player_intcelebrationmale@finger_kiss finger_kiss"},
            {"freakout", "anim@mp_player_intcelebrationmale@freakout freakout"},
            {"jazzhands", "anim@mp_player_intcelebrationmale@jazz_hands jazz_hands"},
            {"knuckle", "anim@mp_player_intcelebrationmale@knuckle_crunch knuckle_crunch"},
            {"nose", "anim@mp_player_intcelebrationmale@nose_pick nose_pick"},
            {"no", "anim@mp_player_intcelebrationmale@no_way no_way"},
            {"peace", "anim@mp_player_intcelebrationmale@peace peace"},
            {"photo", "anim@mp_player_intcelebrationmale@photography photography"},
            {"rock", "anim@mp_player_intcelebrationmale@rock rock"},
            {"salute", "anim@mp_player_intcelebrationmale@salute salute"},
            {"shush", "anim@mp_player_intcelebrationmale@shush shush"},
            {"slowclap", "anim@mp_player_intcelebrationmale@slow_clap slow_clap"},
            {"surrender", "anim@mp_player_intcelebrationmale@surrender surrender"},
            {"thumbs", "anim@mp_player_intcelebrationmale@thumbs_up thumbs_up"},
            {"taunt", "anim@mp_player_intcelebrationmale@thumb_on_ears thumb_on_ears"},
            {"vsign", "anim@mp_player_intcelebrationmale@v_sign v_sign"},
            {"wank", "anim@mp_player_intcelebrationmale@wank wank"},
            {"wave", "anim@mp_player_intcelebrationmale@wave wave"},
            {"loco", "anim@mp_player_intcelebrationmale@you_loco you_loco"},
            {"handsup", "missminuteman_1ig_2 handsup_base"},
            {"halt", "amb@code_human_police_crowd_control@idle_a idle_a" },
            {"walkdrink", "amb@code_human_wander_drinking@male@idle_a idle_c" },
            {"walkeat1", "amb@code_human_wander_eating_donut@male@idle_a idle_c" },
            {"walkeat2", "amb@code_human_wander_eating_donut@male@idle_a idle_b" },
            {"walktime", "amb@medic@standing@tendtodead@idle_a idle_f_checkwatch" },
            {"checkbody", "amb@medic@standing@tendtodead@idle_a idle_a" },
            {"liedown1", "amb@world_human_sit_ups@male@enter enter" },
            {"liedown2", "amb@world_human_sunbathe@female@back@enter enter" },
            {"liedown3", "amb@world_human_sunbathe@female@front@enter enter" },
            {"liedown4", "amb@world_human_sunbathe@female@back@enter enter" },
            {"liedown5", "amb@world_human_sunbathe@male@back@enter enter" },
            {"yoga1", "amb@world_human_yoga@female@base base_b" },
            {"yoga2", "amb@world_human_yoga@male@base base_a" },
            {"situps", "amb@world_human_sit_ups@male@base base" },
            {"smoke1", "amb@world_human_smoking@male@male_a@enter enter" },
            {"smoke2", "amb@world_human_smoking@male@male_a@base base" },
            {"crossarms1", "amb@world_human_stand_guard@male@enter enter" },
            {"crossarms2", "amb@world_human_stand_impatient@female@no_sign@base base" },
            {"getup", "amb@world_human_stupor@male@exit exit_flee" },
            {"sit", "amb@world_human_stupor@male@base base" },
            {"thinking", "amb@code_human_police_investigate@idle_a idle_a"}, // Flag 0
            {"idle1", "amb@code_human_cross_road@male@idle_a idle_e"}, // Flag 1
            {"idle2", "amb@code_human_cross_road@male@base base"}, // Flag 1
            {"ok", "anim@mp_player_intselfiedock idle_a"}, // Flag 1
            {"thumbsup", "anim@mp_player_intincarthumbs_upstd@ds@ idle_a"}, // Flag Flag 0
            {"poledance1", "mini@strip_club@pole_dance@pole_a_2_stage pole_a_2_stage"}, // Flag 0
            {"poledance2", "mini@strip_club@pole_dance@pole_b_2_stage pole_b_2_stage"}, // Flag 0
            {"poledance3", "mini@strip_club@pole_dance@pole_c_2_prvd_a pole_c_2_prvd_a"}, // Flag 0
            {"poledance5", "mini@strip_club@pole_dance@pole_dance3 pd_dance_03"}, // Flag 0
            {"takepic", "cellphone@self selfie_in"}, // Flag 2
            {"lazy", "mp_safehouse lap_dance_player"}, // Flag 1
            {"lapdance", "oddjobs@assassinate@multi@yachttarget@lapdance yacht_ld_f"}, // Flag 0
            {"twerk", "switch@trevor@mocks_lapdance 001443_01_trvs_28_idle_stripper"}, // Flag 1
        };

        [Command("stopanim")]
        public void animationCMD_StopAnim(Client sender)
        {
            if (API.hasEntityData(sender, "cuffed"))
            {
                if (API.getEntityData(sender, "cuffed") == true)
                {
                    API.sendChatMessageToPlayer(sender, "You can't do this while you're cuffed.");
                    return;
                }
            }
            else if (sender.isInVehicle)
            {
                API.sendChatMessageToPlayer(sender, "You can't do this while being in a vehicle.");
                return;
            }
            API.stopPlayerAnimation(sender);
        }

        [Command("anim", "~y~USAGE: ~w~/anim [animation]\n" +
                         "~y~USAGE: ~w~/anim help for animation list.\n" +
                         "~y~USAGE: ~w~/anim stop to stop current animation.")]
        public void SetPlayerAnim(Client sender, string animation)
        {
            if (animation == "help")
            {
                var helpText = AnimationList.Aggregate(new StringBuilder(),
                    (sb, kvp) => sb.Append(kvp.Key + " "), sb => sb.ToString());
                API.sendChatMessageToPlayer(sender, "~b~Available animations:");
                var split = helpText.Split();
                for (var i = 0; i < split.Length; i += 5)
                {
                    var output = "";
                    if (split.Length > i)
                        output += split[i] + " ";
                    if (split.Length > i + 1)
                        output += split[i + 1] + " ";
                    if (split.Length > i + 2)
                        output += split[i + 2] + " ";
                    if (split.Length > i + 3)
                        output += split[i + 3] + " ";
                    if (split.Length > i + 4)
                        output += split[i + 4] + " ";
                    if (!string.IsNullOrWhiteSpace(output))
                        API.sendChatMessageToPlayer(sender, "~b~>> ~w~" + output);
                }
            }
            else
            {
                if (API.hasEntityData(sender, "cuffed"))
                {
                    if (API.getEntityData(sender, "cuffed") == true)
                    {
                        API.sendChatMessageToPlayer(sender, "You can't do this while you're cuffed.");
                        return;
                    }
                }
                else if (sender.isInVehicle)
                {
                    API.sendChatMessageToPlayer(sender, "You can't do this while being in a vehicle.");
                    return;
                }

                if (animation == "stop")
                    API.stopPlayerAnimation(sender);
                else if (!AnimationList.ContainsKey(animation))
                {
                    API.sendChatMessageToPlayer(sender, "~r~ERROR: ~w~Animation not found!");
                }
                else
                {
                    var flag = 0;
                    if (animation == "handsup" || animation == "smoke2" || animation == "situps" || animation == "crossarms2" || animation == "idle1" || animation == "idle2" || animation == "thumbsup" || animation == "ok" || animation == "lazy" || animation == "twerk")
                        flag = 1;
                    else if (animation.Contains("liedown") || animation == "takepic" || animation == "sit")
                        flag = 2;
                    else if (animation.Contains("crossarms1"))
                        flag = 3;

                    API.playPlayerAnimation(sender, flag, AnimationList[animation].Split()[0], AnimationList[animation].Split()[1]);
                }
            }
        }
    }
}

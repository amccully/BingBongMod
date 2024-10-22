using GameNetcodeStuff;
using HarmonyLib;
using LethalNetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace BingBongMod.Patches
{
    class CustomPlayerNotes
    {
        // tried for unity network message api
        //public static GlobalNetMessage<(int, int)> fallDamageClientMessage;
        //public static GlobalNetMessage<(int, int)> fallDamageServerMessage;

        // tried for lethal network api with network vars
        // public network var allows any client to modify
        // KeyValuePair is (client id, fall damage taken) 
        //[PublicNetworkVariable]
        //public static LethalNetworkVariable<List<KeyValuePair<int, int>>> fallDamageDictNetwork = new LethalNetworkVariable<List<KeyValuePair<int, int>>>(identifier: "fallDamageDict");

        public static Dictionary<int, int> fallDamageDict = new Dictionary<int, int>();
        public static Dictionary<int, float> emoteTimeDict = new Dictionary<int, float>();
        public static Dictionary<int, int> bullyHits = new Dictionary<int, int>();
        public static Dictionary<(int, int), int> bullyRelation = new Dictionary<(int, int), int>();

        public static LethalServerMessage<(int, int)> fallDamageServerMessage = new LethalServerMessage<(int, int)>(identifier: "fallDamageId");
        public static LethalClientMessage<(int, int)> fallDamageClientMessage = new LethalClientMessage<(int, int)>(identifier: "fallDamageId");

        public static LethalServerMessage<(int, float)> emoteTimeServerMessage = new LethalServerMessage<(int, float)>(identifier: "emoteTimeId");
        public static LethalClientMessage<(int, float)> emoteTimeClientMessage = new LethalClientMessage<(int, float)>(identifier: "emoteTimeId");

        public static LethalServerMessage<(int, int)> bullyDamageServerMessage = new LethalServerMessage<(int, int)>(identifier: "bullyDamageId");
        public static LethalClientMessage<(int, int)> bullyDamageClientMessage = new LethalClientMessage<(int, int)>(identifier: "bullyDamageId");

        public static void Init()
        {
            BingBongModBase.MLS.LogInfo("CUSTOM PLAYER NOTES INIT CALLED, SUBSCRIBING TO EVENTS");
            fallDamageClientMessage.OnReceived += ReceiveFromServerFallDamage;
            fallDamageServerMessage.OnReceived += ReceiveFromClientFallDamage;
            emoteTimeClientMessage.OnReceived += ReceiveFromServerEmoteTime;
            emoteTimeServerMessage.OnReceived += ReceiveFromClientEmoteTime;
            bullyDamageClientMessage.OnReceived += ReceiveFromServerBullyDamage;
            bullyDamageServerMessage.OnReceived += ReceiveFromClientBullyDamage;

            // do we need to unsubscribe from events? client id keeps increasing when player rejoins
        }

        public static void ResetStats()
        {
            BingBongModBase.MLS.LogInfo("CUSTOM PLAYER NOTES RESET STATS CALLED, CLEARING LISTS");
            fallDamageDict.Clear();
            emoteTimeDict.Clear();
            bullyHits.Clear();
            bullyRelation.Clear();
        }

        // client subscription for BULLY DAMAGE
        private static void ReceiveFromServerBullyDamage((int playerWhoHit, int playerGotHit) data)
        {
            BingBongModBase.MLS.LogInfo("Received request from server to update bullyDamage for this client!");

            bullyHits[data.playerWhoHit] = bullyHits.GetValueOrDefault(data.playerWhoHit) + 1;
            BingBongModBase.MLS.LogInfo("Updating bully hits for " + data.playerWhoHit + " to " + bullyHits[data.playerWhoHit]);

            bullyRelation[(data.playerWhoHit, data.playerGotHit)] = bullyRelation.GetValueOrDefault((data.playerWhoHit, data.playerGotHit)) + 1;
            BingBongModBase.MLS.LogInfo("Updating bully relation between " + data.playerWhoHit + " and " 
                                        + data.playerGotHit + " to " + bullyRelation[(data.playerWhoHit, data.playerGotHit)]);

        }

        // server subscription for BULLY DAMAGE
        private static void ReceiveFromClientBullyDamage((int playerWhoHit, int playerGotHit) data, ulong clientId)
        {
            BingBongModBase.MLS.LogInfo("Received request from client to update bullyDamage for player: " + data.playerGotHit + ". CURRENTLY RUNNING ON SERVER");
            bullyDamageServerMessage.SendAllClients((data.playerWhoHit, data.playerGotHit));
        }

        // client subscription for FALL DAMAGE
        private static void ReceiveFromServerFallDamage((int playerId, int fallDamage) data)
        {
            BingBongModBase.MLS.LogDebug("Received request from server to update fallDamage for this client!");
            if(fallDamageDict.ContainsKey(data.playerId))
            {
                fallDamageDict[data.playerId] += data.fallDamage;
                BingBongModBase.MLS.LogDebug("Data exists already in fallDamageDict, updating playerId " 
                                            + data.playerId + " to " + fallDamageDict[data.playerId]);
            }
            else
            {
                fallDamageDict[data.playerId] = data.fallDamage;
                BingBongModBase.MLS.LogDebug("No data in fallDamageDict, setting playerId "
                                            + data.playerId + " to " + fallDamageDict[data.playerId]);
            }
        }

        // server subscription for FALL DAMAGE
        private static void ReceiveFromClientFallDamage((int playerId, int fallDamage) data, ulong clientId)
        {
            BingBongModBase.MLS.LogDebug("Received request from client to update fallDamage for player: " + data.playerId + ". CURRENTLY RUNNING ON SERVER");
            fallDamageServerMessage.SendAllClients(data);
        }

        // client subscription for EMOTE TIME
        private static void ReceiveFromServerEmoteTime((int playerId, float emoteTime) data)
        {
            BingBongModBase.MLS.LogDebug("Received request from server to update emoteTime for this client!");
            if (emoteTimeDict.ContainsKey(data.playerId))
            {
                emoteTimeDict[data.playerId] += data.emoteTime;
                BingBongModBase.MLS.LogDebug("Data exists already in emoteTimeDict, updating playerId "
                                            + data.playerId + " to " + emoteTimeDict[data.playerId]);
            }
            else
            {
                emoteTimeDict[data.playerId] = data.emoteTime;
                BingBongModBase.MLS.LogDebug("No data in emoteTimeDict, setting playerId "
                                            + data.playerId + " to " + emoteTimeDict[data.playerId]);
            }
        }

        // server subscription for EMOTE TIME
        private static void ReceiveFromClientEmoteTime((int playerId, float emoteTime) data, ulong clientId)
        {
            BingBongModBase.MLS.LogDebug("Received request from client to update emoteTime for player: " + data.playerId + ". CURRENTLY RUNNING ON SERVER");
            emoteTimeServerMessage.SendAllClients(data);
        }


        // called by patch to update fall damage dict values through server
        public static void addFallDamage(PlayerControllerB playerDamaged, int fallDamage) {
            int playerId = (int)playerDamaged.playerClientId;
            BingBongModBase.MLS.LogDebug("Received call to addFallDamage for player: " + playerId);
            fallDamageClientMessage.SendServer((playerId, fallDamage));

        }
        // called by patch to update emote time dict values through server
        public static void addEmoteTime(PlayerControllerB playerDancing, float emoteTime)
        {
            int playerId = (int)playerDancing.playerClientId;
            BingBongModBase.MLS.LogInfo("Received call to addEmoteTime for player: " + playerId);
            emoteTimeClientMessage.SendServer((playerId, emoteTime));

        }

        public static void addBullyDamage(int playerWhoHitId)
        {
            BingBongModBase.MLS.LogInfo("Received call to addBullyDamage for player");
            bullyDamageClientMessage.SendServer((playerWhoHitId, (int)GameNetworkManager.Instance.localPlayerController.playerClientId));
            BingBongModBase.MLS.LogInfo("PLAYER WHO HIT: " + playerWhoHitId + " AND PLAYER GOT HIT: " + (int)GameNetworkManager.Instance.localPlayerController.playerClientId);
        }
    }
}

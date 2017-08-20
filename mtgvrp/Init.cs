﻿/*
 *  File: Init.cs
 *  Author: Chenko
 *  Date: 12/24/2016
 * 
 * 
 *  Purpose: Initalizes the server
 * 
 * 
 * */


using System;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using mtgvrp.core;
using mtgvrp.core.Discord;
using mtgvrp.database_manager;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.vehicle_manager;

namespace mtgvrp
{
    public class Init : Script
    {
        public static string SERVER_NAME = "[EN] Moving Target Gaming Roleplay";
        public static string SERVER_VERSION = "v0.0.1682";
        public static string SERVER_WEBSITE = "www.mt-gaming.com";
        public static Random Random = new Random();

        public Init()
        {

            DebugManager.DebugMessage("[INIT] Initalizing script...");

            API.setServerName(SERVER_NAME + " ~b~| ~g~" + SERVER_WEBSITE);
            API.setGamemodeName("MTG V-RP " + SERVER_VERSION);

            API.onResourceStart += OnResourceStartHandler;
            API.onResourceStop += API_onResourceStop;
            API.onClientEventTrigger += API_onClientEventTrigger;
            InventoryManager.OnStorageItemUpdateAmount += InventoryManager_OnStorageItemUpdateAmount;

            SettingsManager.Load();

            DebugManager.DebugManagerInit();
            DatabaseManager.DatabaseManagerInit();
        }

        public delegate void OnPlayerEnterVehicleExHandler(Client player, NetHandle vehicle, int seat);
        public static event OnPlayerEnterVehicleExHandler OnPlayerEnterVehicleEx;

        private void API_onClientEventTrigger(GrandTheftMultiplayer.Server.Elements.Client sender, string eventName, params object[] arguments)
        {
            if (eventName == "OnPlayerEnterVehicleEx")
            {
                NetHandle veh = (NetHandle) arguments[0];
                int seat = (int) arguments[1];

                OnPlayerEnterVehicleEx?.Invoke(sender, veh, seat);
            }

            else if (eventName == "OBJECT_PLACED_PROPERLY")
            {
                NetHandle obj = (NetHandle) arguments[0];
                Vector3 pos = (Vector3) arguments[1];
                Vector3 rot = (Vector3) arguments[2];
                API.setEntityPosition(obj,pos);
                API.setEntityRotation(obj,rot);
            }

        }

        private void InventoryManager_OnStorageItemUpdateAmount(IStorage sender,
            InventoryManager.OnItemAmountUpdatedEventArgs args)
        {
            if (sender.GetType() == typeof(Character) && args.Item == typeof(Money))
            {
                Character c = (Character) sender;
                API.shared.triggerClientEvent(c.Client, "update_money_display", args.Amount);
            }
        }

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        [Command]
        public void getclass(Client player)
        {
            API.sendChatMessageToPlayer(player, "Class: " + API.shared.getVehicleClass(player.vehicle.handle.GetVehicle().VehModel));
        }

        public void OnResourceStartHandler()
        {
            //For Dealership.
            API.removeIpl("fakeint"); // remove the IPL "fakeint"
            API.requestIpl("shr_int"); // Request the IPL "shr_int"

            API.consoleOutput("[INIT] Unloaded fakeint IPL and loaded shr_int IPL.!");

            VehicleManager.load_all_unowned_vehicles();
            API.consoleOutput("[INIT] Script initalized!");

            LogManager.StartLogArchiveTimer();

            //Must be last to be called.
            if (IsRunningOnMono())
            {
                API.consoleOutput("[INIT] Starting Discord Bot!");
                DiscordManager.StartBot();
            }
        }

        private void API_onResourceStop()
        {
            SettingsManager.Save();
        }
    }
}

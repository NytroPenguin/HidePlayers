using System;
using System.Xml.Linq;
using System.Collections.Generic;
using Lib_K_Relay;
using Lib_K_Relay.Utilities;
using Lib_K_Relay.Interface;
using Lib_K_Relay.Networking;
using Lib_K_Relay.Networking.Packets;
using Lib_K_Relay.Networking.Packets.Client;
using Lib_K_Relay.Networking.Packets.Server;
using Lib_K_Relay.Networking.Packets.DataObjects;
using Lib_K_Relay.GameData;
namespace HidePlayers
{
    public class HidePlayers : IPlugin
    {
        private bool _hidePlayers = false;
        private List<int> pets = new List<int>();
        public string GetAuthor()
        {
            return "NytroPenguin";
        }

        public string[] GetCommands()
        {
            return new string[] { "/hideplayers on:off" }; ;
        }

        public string GetDescription()
        {
            return "Hides other players. You have to change maps in order for the change to take effect.";
        }

        public string GetName()
        {
            return "HidePlayers";
        }

        public void Initialize(Proxy proxy)
        {
            proxy.HookCommand("hideplayers", onCommand);
            proxy.HookPacket(PacketType.UPDATE, onUpdate);
            proxy.HookPacket(PacketType.NEWTICK, onTick);


            foreach (var obj in GameData.Objects.Map)
            {
                if (obj.Value.Pet)
                {
                    pets.Add(obj.Value.ID);
                    PluginUtils.Log("PetID", obj.Value.ID.ToString());
                }
            }
            PluginUtils.Log("test:", pets.Count.ToString() + " pets loaded");
        }

        private void onTick(Client client, Packet packet)
        {
            NewTickPacket tick = (NewTickPacket)packet;
            foreach (Status status in tick.Statuses)
            {
                if (status.ObjectId != client.ObjectId)
                    foreach (StatData data in status.Data)
                    {
                        if ((data.Id == StatsType.Effects || data.Id == StatsType.Effects2 || data.Id == StatsType.Size || data.Id == StatsType.Stars) && _hidePlayers)
                            data.IntValue = 0;
                        else if (data.Id == StatsType.PetType && _hidePlayers)
                            data.StringValue = "";
                    }
            }
        }

        private void onUpdate(Client client, Packet packet)
        {
            UpdatePacket update = (UpdatePacket)packet;
            List<Entity> betterObjects = new List<Entity>();
            foreach (Entity obj in update.NewObjs)
            {
                if (((Enum.IsDefined(typeof(Classes), (short)obj.ObjectType) || obj.ObjectType == 785) && obj.Status.ObjectId != client.ObjectId || (pets.Contains(obj.ObjectType))))
                {
                    foreach (var data in obj.Status.Data)
                    {
                        if ((data.Id == StatsType.Size || data.Id == StatsType.Effects || data.Id == StatsType.Effects2) && _hidePlayers)
                            data.IntValue = 0;
                        else if ((data.Id == StatsType.Name || data.Id == StatsType.PetType )&& _hidePlayers)
                            data.StringValue = "";
                    }
                }// else
                //{
                //    betterObjects.Add(obj);
                //}

            }
            //update.Send = false;
            //UpdatePacket betterUpdate = (UpdatePacket)Packet.Create(PacketType.UPDATE);
            //betterUpdate.Tiles = update.Tiles;
            //betterUpdate.NewObjs = betterObjects.ToArray();
            //betterUpdate.Drops = update.Drops;
            //client.SendToClient(betterUpdate);
        }

        private void onCommand(Client client, string command, string[] args)
        {
            if (args[0] == "on") _hidePlayers = true;
            else if (args[0] == "off") _hidePlayers = false;
            else return;
            client.SendToClient(PluginUtils.CreateNotification(client.ObjectId, "HidePlayers: Size = " + args[0]));
        }
    }
}

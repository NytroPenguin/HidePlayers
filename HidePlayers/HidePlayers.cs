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
            //add new tick back in to remove pets
            //proxy.HookPacket(PacketType.NEWTICK, onTick);


            foreach (var obj in GameData.Objects.Map)
            {
                if (obj.Value.Pet)
                {
                    pets.Add(obj.Value.ID);
                }
            }
        }
        /*
        private void onTick(Client client, Packet packet)
        {
            NewTickPacket tick = (NewTickPacket)packet;
            foreach (Status status in tick.Statuses)
            {
                if (status.ObjectId != client.ObjectId)
                    foreach (StatData data in status.Data)
                    {
                        if ((data.Id == StatsType.Effects || data.Id == StatsType.Effects2 ||  data.Id == StatsType.Size || data.Id == StatsType.Stars) && _hidePlayers)
                            data.IntValue = 0;
                    }
            }
        }
        */
        private void onUpdate(Client client, Packet packet)
        {
            UpdatePacket update = (UpdatePacket)packet;
            foreach (Entity obj in update.NewObjs)
            {
                //|| (pets.Contains(obj.ObjectType)) add this to the if for pet removal
                if (((Enum.IsDefined(typeof(Classes), (short)obj.ObjectType) || obj.ObjectType == 785) && obj.Status.ObjectId != client.ObjectId))
                {
                    foreach (var data in obj.Status.Data)
                    {
                        if ((data.Id == StatsType.Size || data.Id == StatsType.Effects || data.Id == StatsType.Effects2) && _hidePlayers)
                            data.IntValue = 0;  
                    }
                    
                }
            }
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

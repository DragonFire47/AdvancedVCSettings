namespace AdvancedVCSettings
{
    class PlayerVCSettings
    {
        public PlayerVCSettings(PLPlayer player) 
        {
            Player = player;
            SteamID = player.GetPhotonPlayer().SteamID.m_SteamID;
            if (SteamID != 0)
            {
                if (Global.SteamPlayerVolumes.Value.TryGetValue(SteamID, out float value))
                {
                    m_PlayerVolume = value;
                }
                player.TS_IsMuted = Global.MutedSteamPlayers.Value.Contains(SteamID);
            }
        }

        public ulong SteamID = 0;
        private float m_PlayerVolume = 1f;
        public bool IsPrioritySpeaker = false;
        public readonly PLPlayer Player = null;


        public float PlayerVolume
        {
            get 
            {
                return m_PlayerVolume; 
            }
            set 
            {
                if (SteamID != 0)
                {
                    Global.SteamPlayerVolumes.Value[SteamID] = value;
                }
                m_PlayerVolume = value; 
            }
        }
        
        public bool Muted
        {
            get
            {
                return Player.TS_IsMuted;
            }
            set
            {
                if (SteamID != 0)
                {
                    if(value)
                    {
                        if (!Global.MutedSteamPlayers.Value.Contains(SteamID))
                        {
                            Global.MutedSteamPlayers.Value.Add(SteamID);
                        }
                    }
                    else
                    {
                        Global.MutedSteamPlayers.Value.Remove(SteamID);
                    }
                }
                Player.TS_IsMuted = value;
            }
        }
    }
}

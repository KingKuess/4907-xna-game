using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuessaria
{
    public enum Protocol
    {
        Disconnected = 0,
        Connected = 1,
        MapJoined = 2,
        Load = 3,
        PlayerMoved = 4,
        weaponCreated = 5,
        enemyActionChanged = 6,
        enemyHit = 7,
        enemyDied = 8,
        playerHit = 9,
        enemyLoad = 10,
        getNPCs = 11,
        enemySync = 12,
        npcLoad = 13
    }
}

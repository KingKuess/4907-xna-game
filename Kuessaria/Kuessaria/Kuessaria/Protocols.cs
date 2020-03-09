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
        MapSwitch = 2,
        Load = 3,
        PlayerMoved = 4,
        weaponCreated = 5
    }
}

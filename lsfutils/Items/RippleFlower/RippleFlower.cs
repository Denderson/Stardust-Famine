using lsfUtils.Creatures;
using lsfUtils.CWTs;
using MoreSlugcats;
using Noise;
using RWCustom;
using Smoke;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using static lsfUtils.Plugin;
using Menu.Remix.MixedUI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.Remoting.Contexts;
using System.Security.Permissions;
using Watcher;


namespace lsfUtils.Items.RippleFlower
{
    public class RippleFlower : KarmaFlower
    {

        public RippleFlower(RippleFlowerAbstract abstr) : base(abstr)
        {
            Log.LogMessage("Spawning ripple flower!!");
            if (CWTs.KarmaFlowerCWT.TryGetData(this, out var data))
            {
                data.rippleFlower = true;
            }
            else
            {
                Log.LogMessage("Couldnt get karma flower CWT from ctor!");
            }
        }
    }
}

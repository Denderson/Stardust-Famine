using MoreSlugcats;
using RWCustom;
using UnityEngine;
using static lsfUtils.Plugin;

namespace lsfUtils.Items.CustomCloak;

public class CustomCloak : MoonCloak
{
    public CustomCloakAbstract CustomCloakAbstract => abstractPhysicalObject as CustomCloakAbstract;

    public CustomCloak(CustomCloakAbstract abstr, World world) : base(abstr)
    {
        abstractPhysicalObject = abstr;
        abstr.realizedObject = this;
        Log.LogMessage("Spawning custom cloak!");
    }
}
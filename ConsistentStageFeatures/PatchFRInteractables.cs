using FRCSharp;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ConsistentStageFeatures
{
    [HarmonyPatch(typeof(FRHooks), nameof(FRHooks.SceneDirector_onPostPopulateSceneServer))]
    public class PatchFRInteractables
    {
        public static void ILManipulator(ILContext il) 
        {
            ILCursor c = new(il);
            if (Main.RemoveRandomFHTeleporter.Value && c.TryGotoNext(MoveType.After, x => x.MatchLdsfld<VF2ContentPackProvider>(nameof(VF2ContentPackProvider.iscShatteredTeleporter)), x => x.MatchCallOrCallvirt<UnityEngine.Object>("op_Implicit")))
            {
                c.Emit(OpCodes.Pop);
                c.Emit(OpCodes.Ldc_I4_0);
                Main.Log.LogDebug("Removed Shattered Teleporters");
            }
            if (Main.RemoveRandomSageShrine.Value && c.TryGotoNext(MoveType.After, x => x.MatchLdsfld<VF2ContentPackProvider>(nameof(VF2ContentPackProvider.iscSagesShrine)), x => x.MatchCallOrCallvirt<UnityEngine.Object>("op_Implicit")))
            {
                c.Emit(OpCodes.Pop);
                c.Emit(OpCodes.Ldc_I4_0);
                Main.Log.LogDebug("Removed Sage's Shrines");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace NightmareIncarnation
{
    public class CompProperties_AbilitySuccubusEnlarge : CompProperties_AbilityEffect
    {
        public float manaCostPerPart = 2f;
        public float severityIncrease = 0.05f;

        public CompProperties_AbilitySuccubusEnlarge()
        {
            compClass = typeof(CompAbilityEffect_SuccubusEnlarge);
        }
    }

    public class CompAbilityEffect_SuccubusEnlarge : CompAbilityEffect
    {
        public new CompProperties_AbilitySuccubusEnlarge Props => (CompProperties_AbilitySuccubusEnlarge)props;

        public override bool GizmoDisabled(out string reason)
        {
            if (!RJWParts.Available)
            {
                reason = "NI_EnlargeRequiresRJW".Translate();
                return true;
            }

            Need_Mana mana = parent.pawn.needs?.TryGetNeed<Need_Mana>();
            if (mana == null || mana.CurLevel < Props.manaCostPerPart)
            {
                reason = "NI_EnlargeRequiresMana".Translate(Props.manaCostPerPart.ToString("0.##"));
                return true;
            }

            reason = null;
            return false;
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn targetPawn = target.Pawn;
            List<RJWParts.Part> parts = RJWParts.GetPenisParts(targetPawn);
            bool validPawn = targetPawn != null
                             && targetPawn != parent.pawn
                             && targetPawn.Spawned
                             && !targetPawn.Downed
                             && !targetPawn.Dead
                             && targetPawn.Map == parent.pawn.Map;

            if (!validPawn || parts.Count == 0)
            {
                if (throwMessages)
                    Messages.Message("NI_EnlargeInvalidTarget".Translate(), targetPawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            float manaCost = Props.manaCostPerPart * parts.Count;
            Need_Mana mana = parent.pawn.needs?.TryGetNeed<Need_Mana>();
            if (mana == null || mana.CurLevel < manaCost)
            {
                if (throwMessages)
                    Messages.Message("NI_EnlargeRequiresMana".Translate(manaCost.ToString("0.##")), parent.pawn, MessageTypeDefOf.RejectInput);
                return false;
            }

            return true;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn targetPawn = target.Pawn;
            List<RJWParts.Part> parts = RJWParts.GetPenisParts(targetPawn);
            Need_Mana mana = parent.pawn.needs?.TryGetNeed<Need_Mana>();
            float manaCost = Props.manaCostPerPart * parts.Count;
            if (targetPawn == null || parts.Count == 0 || mana == null || mana.CurLevel < manaCost)
                return;

            foreach (RJWParts.Part part in parts)
                RJWParts.SetSeverity(part, RJWParts.GetSeverity(part) + Props.severityIncrease);

            mana.CurLevel -= manaCost;
        }
    }

    /// <summary>
    /// Runtime-only RJW bridge. Reflection keeps this mod loadable when RJW is absent
    /// and avoids binding the project to an RJW assembly for a different game version.
    /// </summary>
    internal static class RJWParts
    {
        private const string RJWAssemblyName = "RJW";
        private const string SexPartInterfaceName = "rjw.ISexPartHediff";
        private const string SexPartCompName = "rjw.HediffComp_SexPart";

        private static readonly Type sexPartInterface;
        private static readonly Type sexPartCompType;
        private static readonly MethodInfo getPartComp;
        private static readonly MethodInfo getSeverity;
        private static readonly MethodInfo setSeverity;
        private static readonly MethodInfo getSize;
        private static readonly MethodInfo getBodySize;
        private static readonly MethodInfo forceSize;
        private static readonly PropertyInfo hasForcedSize;
        private static readonly PropertyInfo partDef;
        private static readonly FieldInfo genitalFamilyField;
        private static readonly PropertyInfo genitalFamilyProperty;
        private static bool loggedIntegrationError;

        public static bool Available => sexPartInterface != null
                                        && sexPartCompType != null
                                        && getPartComp != null
                                        && getSeverity != null
                                        && setSeverity != null
                                        && getSize != null
                                        && getBodySize != null
                                        && forceSize != null
                                        && hasForcedSize != null
                                        && partDef != null
                                        && (genitalFamilyField != null || genitalFamilyProperty != null);

        static RJWParts()
        {
            try
            {
                Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(candidate => candidate.GetName().Name == RJWAssemblyName);
                if (assembly == null)
                    return;

                sexPartInterface = assembly.GetType(SexPartInterfaceName);
                sexPartCompType = assembly.GetType(SexPartCompName);
                partDef = sexPartInterface?.GetProperty("Def");
                getPartComp = sexPartInterface?.GetMethods()
                    .FirstOrDefault(method => method.Name == "GetComp" && method.IsGenericMethodDefinition)
                    ?.MakeGenericMethod(sexPartCompType);
                getSeverity = sexPartCompType?.GetMethod("GetSeverity", Type.EmptyTypes);
                setSeverity = sexPartCompType?.GetMethod("SetSeverity", new[] { typeof(float), typeof(bool) });
                getSize = sexPartCompType?.GetMethod("GetSize", Type.EmptyTypes);
                getBodySize = sexPartCompType?.GetMethod("GetBodySize", Type.EmptyTypes);
                forceSize = sexPartCompType?.GetMethod("ForceSize", new[] { typeof(float) });
                hasForcedSize = sexPartCompType?.GetProperty("HasForcedSize");

                Type partDefType = partDef?.PropertyType;
                genitalFamilyField = partDefType?.GetField("genitalFamily");
                genitalFamilyProperty = partDefType?.GetProperty("genitalFamily");
            }
            catch (Exception exception)
            {
                LogIntegrationError(exception);
            }
        }

        public sealed class Part
        {
            internal readonly object comp;

            internal Part(object comp)
            {
                this.comp = comp;
            }
        }

        public static List<Part> GetPenisParts(Pawn pawn)
        {
            List<Part> parts = new List<Part>();
            if (!Available || pawn?.health?.hediffSet?.hediffs == null)
                return parts;

            try
            {
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (!sexPartInterface.IsInstanceOfType(hediff))
                        continue;

                    object def = partDef.GetValue(hediff);
                    object family = genitalFamilyField != null
                        ? genitalFamilyField.GetValue(def)
                        : genitalFamilyProperty.GetValue(def);
                    if (!string.Equals(family?.ToString(), "Penis", StringComparison.OrdinalIgnoreCase))
                        continue;

                    object comp = getPartComp.Invoke(hediff, null);
                    if (comp != null)
                        parts.Add(new Part(comp));
                }
            }
            catch (Exception exception)
            {
                LogIntegrationError(exception);
                parts.Clear();
            }

            return parts;
        }

        public static float GetSeverity(Part part)
        {
            return (float)getSeverity.Invoke(part.comp, null);
        }

        public static void SetSeverity(Part part, float severity)
        {
            bool isForced = (bool)hasForcedSize.GetValue(part.comp);
            if (isForced)
            {
                float bodySize = (float)getBodySize.Invoke(part.comp, null);
                float currentSize = (float)getSize.Invoke(part.comp, null);
                forceSize.Invoke(part.comp, new object[] { currentSize + (severity - GetSeverity(part)) * bodySize });
                return;
            }

            setSeverity.Invoke(part.comp, new object[] { severity, true });
        }

        private static void LogIntegrationError(Exception exception)
        {
            if (loggedIntegrationError)
                return;
            loggedIntegrationError = true;
            Log.Error("Nightmare Incarnation could not read RJW penis part data: " + exception);
        }
    }
}

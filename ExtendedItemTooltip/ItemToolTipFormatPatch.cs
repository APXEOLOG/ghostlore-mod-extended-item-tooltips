using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace ExtendedItemTooltip
{
    [HarmonyPatch(typeof(ItemToolTipFormat))]
    public class ItemToolTipFormatPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("BindTooltip")]
        static void BindTooltipPostfix(Item item, ItemInstance itemInstance, ItemToolTipAttributes attributes,
            ItemToolTipFormat __instance)
        {
            if (itemInstance != null && item != null)
            {
                var allMods = itemInstance.Mods ?? new List<ModifierInstance>();
                var coreMods = (from c in allMods
                    where (c.Attributes & ModifierInstanceAttributes.CoreMod) > ModifierInstanceAttributes.None
                    select c).ToList();

                var coreModsFormatted = FormatMods(itemInstance, coreMods);
                SetTextMesh(__instance, "coreMods", coreModsFormatted);
                
                var nonCoreMods = (from c in allMods
                    where (c.Attributes & ModifierInstanceAttributes.CoreMod) == ModifierInstanceAttributes.None
                    select c).ToList();
                
                var multiplier = 1f;
                if ((itemInstance.Attributes & ItemAttributes.OneAffixLimit) != ItemAttributes.None)
                {
                    multiplier = 0.5f;
                }
                var nonCoreFormatted = FormatMods(itemInstance, nonCoreMods, multiplier);
                SetTextMesh(__instance, "mods", nonCoreFormatted);
            }
        }

        static void SetTextMesh(ItemToolTipFormat __instance, string fieldName, string value)
        {
            var field = AccessTools.Field(typeof(ItemToolTipFormat), fieldName);
            var textMesh = (TextMeshProUGUI)field.GetValue(__instance);
            textMesh.text = value;
        }

        static string FormatMods(ItemInstance itemInstance, List<ModifierInstance> mods, float multiplier = 1.0f)
        {
            if (!mods.Any())
            {
                return null;
            }
            var text = string.Join(Environment.NewLine, mods.Select(it =>
            {
                var defaultText = ModifierFormatter.FormatMod(it);
                if (string.IsNullOrEmpty(defaultText))
                {
                    return defaultText;
                }
                
                // Calculate min and max rolls and add it to the mod description
                var lower = GetStat(it.Modifier.LowerMin, Mathf.Max(it.Modifier.LowerMin, it.Modifier.LowerMax), 
                    itemInstance.Level, it.Modifier.LowerPerLevel) * multiplier;
                var upper = GetStat(it.Modifier.UpperMin, Mathf.Max(it.Modifier.UpperMin, it.Modifier.UpperMax), 
                    itemInstance.Level, it.Modifier.UpperPerLevel) * multiplier;
                var modTemplate = it.Modifier.Stat.ModifierDescription.Split(' ')
                    .FirstOrDefault(str => str.Contains('{') && str.Contains('}'));
                modTemplate = modTemplate == null 
                    ? "{0:0%}" 
                    : modTemplate.Substring(modTemplate.IndexOf('{'), modTemplate.IndexOf('}') - modTemplate.IndexOf('{') + 1);
                if (upper <= 0.00001f)
                {
                    defaultText += string.Format(" ({0})", string.Format(modTemplate, lower));
                }
                else
                {
                    defaultText += string.Format(" ({0} - {1})", 
                        string.Format(modTemplate, lower), string.Format(modTemplate, upper));
                }
                return defaultText;
            }));
            if (!string.IsNullOrEmpty(text))
            {
                text += " ";
            }
            return text;
        }

        static float GetStat(float min, float max, int level, float perLevel)
        {
            float num = 1f;
            if (max != 0f)
            {
                num = min / max;
            }
            min += (float)level * perLevel * num;
            max += (float)level * perLevel;
            return min + (max - min);
        }
    }
}
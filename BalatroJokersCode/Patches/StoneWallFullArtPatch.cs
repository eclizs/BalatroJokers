using System.Reflection;
using BalatroJokers.BalatroJokersCode.Cards;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace BalatroJokers.BalatroJokersCode.Patches;

[HarmonyPatch(typeof(NCard))]
static class MakeFullArt
{
    [HarmonyPatch(nameof(NCard.UpdateVisuals))]
    static void Postfix(
        NCard? __instance,
        MegaLabel? ____typeLabel,
        NinePatchRect? ____typePlaque
    )
    {
        var model = __instance?.Model;
        if (model == null) return;
        try
        {
            if (____typePlaque != null) ____typePlaque.Visible = false;
            if (____typeLabel != null) ____typeLabel.Visible = false;
        }
        catch (Exception err)
        {
            MainFile.Logger.Error($"Something broke 2 {err}");
        }
    }

    [HarmonyPatch]
    class UseFullArt
    {
        [HarmonyTargetMethods]
        internal static IEnumerable<MethodInfo> Methods()
        {
            var type = typeof(NCard);
            if (type.Method("UpdatePortrait") is { } updatePortrait) yield return updatePortrait;
            else if (type.Method("Reload") is { } reload) yield return reload;
        }

        [HarmonyPostfix]
        static void Postfix(
            NCard? __instance,
            TextureRect? ____frame,
            TextureRect? ____portraitBorder,
            TextureRect? ____ancientBorder,
            TextureRect? ____portrait,
            TextureRect? ____ancientPortrait,
            TextureRect? ____banner,
            Control? ____ancientBanner,
            TextureRect? ____ancientTextBg,
            Material? ____canvasGroupMaskMaterial,
            CanvasGroup? ____portraitCanvasGroup
        )
        {
            CardModel? model = __instance?.Model;
            if (____frame == null || ____ancientBorder == null || ____portrait == null ||
                ____ancientPortrait == null || ____portraitBorder == null || ____banner == null ||
                ____portraitCanvasGroup == null || ____ancientTextBg == null || ____ancientBanner == null
                || model == null || model.CanonicalInstance != ModelDb.Card<StoneWall>()) return;

            try
            {
                if (model.Rarity != CardRarity.Ancient) ____ancientPortrait.Texture = ____portrait.Texture;

                ____portrait.Visible = false;
                ____portraitBorder.Visible = false;
                
                CardModel referenceCard = ancientSkill;

                ____frame.Visible = false;

                ____ancientPortrait.Visible = true;
                
                ____ancientBorder.Visible = true;
                ____ancientBorder.Texture = referenceCard.AncientBorder;

                // Title
                ____ancientBanner.Material = referenceCard.BannerMaterial;

                // reset first
                ____banner.Visible = false;
                ____ancientBanner.Visible = false;

                const float BannerTransparency = .7f;

                if (model.Rarity == CardRarity.Ancient)
                {
                    ____ancientBanner.Visible = false;
                    ____ancientBanner.Modulate = ____ancientBanner.Modulate with { A = BannerTransparency };
                }
                else
                {
                    ____banner.Visible = false;
                    ____banner.Modulate = ____banner.Modulate with { A = BannerTransparency };
                }

                // Description
                ____ancientTextBg.Texture = referenceCard.AncientTextBg;
                ____ancientTextBg.Visible = false;

                // Card Shape Mask
                ____canvasGroupMaskMaterial ??=
                    PreloadManager.Cache.GetMaterial("res://scenes/cards/card_canvas_group_mask_material.tres");
                ____portraitCanvasGroup.Material = ____canvasGroupMaskMaterial;
            }
            catch (Exception err)
            {
                MainFile.Logger.Error($"UseFullArt: {err}");
            }
        }
        private static readonly CardModel ancientSkill = ModelDb.Card<Apotheosis>();
    }
}
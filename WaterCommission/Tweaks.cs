using UnboundLib; // requires UnboundLib.dll
using UnboundLib.Cards; // " "
using UnboundLib.Utils;
using UnityEngine; // requires UnityEngine.dll, UnityEngine.CoreModule.dll, and UnityEngine.AssetBundleModule.dll
using System.Collections;
using Jotunn.Utils;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnboundLib.Utils.UI;
using UnboundLib.GameModes;


namespace WaterMod
{
    internal static class TweakCards
    {
        private static List<CardInfo> activeCards
        {
            get
            {
                return ((ObservableCollection<CardInfo>)typeof(CardManager).GetField("activeCards", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)).ToList();

            }
            set { }
        }
        private static List<CardInfo> inactiveCards
        {
            get
            {
                return (List<CardInfo>)typeof(CardManager).GetField("inactiveCards", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            }
            set { }
        }
        private static List<CardInfo> allCards
        {
            get
            {
                return activeCards.Concat(inactiveCards).ToList();
            }
            set { }
        }
        internal static IEnumerator TweakEnum(IGameModeHandler gm)
        {
            Tweak();
            yield break;
        }
        internal static void Tweak()
        {
            foreach (CardInfo card in allCards)
            {
                switch (card.cardName.ToLower())
                {
                    case "quick shot":
                        if (WaterMod.QuickShot)
                        {
                            // remove bullet speed stat, add attack speed stat, and add change reload stat at the end
                            card.cardStats = new CardInfoStat[]
                            {
                                // attack speed stat
                                new CardInfoStat() { stat = "ATKSPD", amount = "+100%", positive = true, simepleAmount = CardInfoStat.SimpleAmount.aLotOf },
                                // reload time stat
                                new CardInfoStat(){stat = "Reload time", amount = "+0.5s", positive = false, simepleAmount=CardInfoStat.SimpleAmount.notAssigned}
                            };

                            card.GetComponent<Gun>().projectileSpeed = 1f;
                            card.GetComponent<Gun>().attackSpeed = 0.5f;
                            card.GetComponent<Gun>().reloadTimeAdd = 0.5f;
                        }
                        else
                        {
                            // reset to default stats
                            card.cardStats = new CardInfoStat[]
                            {
                                // bullet speed stat
                                new CardInfoStat(){stat = "Bullet speed", amount = "+150%", positive = true, simepleAmount=CardInfoStat.SimpleAmount.aLotOf},
                                // reload time stat
                                new CardInfoStat(){stat = "Reload time", amount = "+0.25s", positive = false, simepleAmount=CardInfoStat.SimpleAmount.notAssigned}
                            };
                            card.GetComponent<Gun>().projectileSpeed = 2.5f;
                            card.GetComponent<Gun>().attackSpeed = 1f;
                            card.GetComponent<Gun>().reloadTimeAdd = 0.25f;
                        }
                        break;
                    case "grow":
                        if (WaterMod.Grow)
                        {
                            // increase rarity of Grow from uncommon to rare
                            card.rarity = CardInfo.Rarity.Rare;
                        }
                        else
                        {
                            // reset to default stats
                            card.rarity = CardInfo.Rarity.Uncommon;
                        }
                        break;
                    case "glass cannon":
                        if (WaterMod.GlassCannon)
                        {
                            // change health debuff from -100% (game actually does -50%) to -50% (actually -25%)
                            // remove the reload time penalty
                            card.cardStats = new CardInfoStat[]
                            {
                            new CardInfoStat() { stat = "DMG", amount = "+100%", positive = true, simepleAmount = CardInfoStat.SimpleAmount.aLotOf },
                            new CardInfoStat() { stat = "HP", amount = "-50%", positive = false, simepleAmount = CardInfoStat.SimpleAmount.lower }
                            };

                            card.GetComponent<CharacterStatModifiers>().health = 0.75f;
                            card.GetComponent<Gun>().reloadTimeAdd = 0f;
                        }
                        else
                        {
                            // reset to default stats
                            card.cardStats = new CardInfoStat[]
                            {
                            new CardInfoStat() { stat = "DMG", amount = "+100%", positive = true, simepleAmount = CardInfoStat.SimpleAmount.aLotOf },
                            new CardInfoStat() { stat = "HP", amount = "-100%", positive = false, simepleAmount = CardInfoStat.SimpleAmount.aLotLower },
                            new CardInfoStat() { stat = "Reload time", amount = "+0.25s", positive = false, simepleAmount = CardInfoStat.SimpleAmount.notAssigned }
                            };
                            card.GetComponent<Gun>().damage = 2f;
                            card.GetComponent<Gun>().reloadTimeAdd = 0.25f;
                            card.GetComponent<CharacterStatModifiers>().health = 0.5f;

                        }
                        break;
                }
            }
        }
    }
}

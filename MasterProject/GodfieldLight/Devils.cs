using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterProject.GodfieldLight {

    public static class Devils {

        public const float DEVIL_CHANCE = 0.25f;
        public const int SMALL_DEVIL_OCC = 7;
        public const int SMALL_DEVIL_DMG = 10;
        public const int MEDIUM_DEVIL_OCC = 5;
        public const int MEDIUM_DEVIL_DMG = 20;
        public const int LARGE_DEVIL_OCC = 3;
        public const int LARGE_DEVIL_DMG = 30;
        public const int FAIRY_OCC = 5;
        public const int FAIRY_HEAL = 10;

        public static bool CheckIfDevilAppears (System.Random rng, out string id, out int damage) {
            if (rng.NextDouble() < DEVIL_CHANCE) {
                var i = rng.Next(SMALL_DEVIL_OCC + MEDIUM_DEVIL_OCC + LARGE_DEVIL_OCC + FAIRY_OCC);
                i -= SMALL_DEVIL_OCC;
                if (i < 0) {
                    id = "SmallDevil";
                    damage = SMALL_DEVIL_DMG;
                    return true;
                }
                i -= MEDIUM_DEVIL_OCC;
                if (i < 0) {
                    id = "MediumDevil";
                    damage = MEDIUM_DEVIL_DMG;
                    return true;
                }
                i -= LARGE_DEVIL_OCC;
                if (i < 0) {
                    id = "LargeDevil";
                    damage = LARGE_DEVIL_DMG;
                    return true;
                }
                i -= FAIRY_OCC;
                if (i < 0) {
                    id = "CharityFairy";
                    damage = -FAIRY_HEAL;
                    return true;
                }
                throw new System.Exception("???");
            }
            id = string.Empty;
            damage = 0;
            return false;
        }

    }

}

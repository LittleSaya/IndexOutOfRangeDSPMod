using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPAddPlanet
{
    struct AdditionalPlanetConfig
    {
        /// <summary>
        /// 行星在星系中的索引，创建行星时使用
        /// </summary>
        public int Index;

        /// <summary>
        /// 行星的公转中心，创建行星时使用
        /// </summary>
        public int OrbitAround;

        /// <summary>
        /// 行星的公转轨道，创建行星时使用
        /// </summary>
        public int OrbitIndex;

        /// <summary>
        /// 在所有围绕该行星公转中心旋转的天体中，该行星的唯一编号，创建行星时使用
        /// </summary>
        public int Number;

        /// <summary>
        /// 是否是气态巨星，创建行星时使用
        /// </summary>
        public bool GasGiant;

        /// <summary>
        /// 种子一，创建行星时使用
        /// </summary>
        public int InfoSeed;

        /// <summary>
        /// 种子二，创建行星时使用
        /// </summary>
        public int GenSeed;

        /// <summary>
        /// 行星半径，后处理阶段使用
        /// </summary>
        public float Radius;

        /// <summary>
        /// 公转周期，后处理阶段使用
        /// </summary>
        public float OrbitalPeriod;

        /// <summary>
        /// 自转周期，后处理阶段使用
        /// </summary>
        public float RotationPeriod;

        /// <summary>
        /// 是否潮汐锁定，后处理阶段使用
        /// </summary>
        public bool IsTidalLocked;

        /// <summary>
        /// 轨道倾角，后处理阶段使用
        /// </summary>
        public float OrbitInclination;

        /// <summary>
        /// 地轴倾角，后处理阶段使用
        /// </summary>
        public float Obliquity;

        /// <summary>
        /// 是否生成矿脉，矿脉生成Prefix阶段使用
        /// </summary>
        public bool DontGenerateVein;

        /// <summary>
        /// 是否有行星主题参数，后处理阶段使用
        /// </summary>
        public bool _HasThemeId;

        /// <summary>
        /// 行星主题，后处理阶段使用
        /// </summary>
        public int ThemeId;

        /// <summary>
        /// 是否有升交点经度参数，后处理阶段使用
        /// </summary>
        public bool _HasOrbitLongitude;

        /// <summary>
        /// 升交点经度，后处理阶段使用
        /// </summary>
        public float OrbitLongitude;

        public bool _HasVeinCustom;

        public Dictionary<EVeinType, VeinConfig> VeinCustom;

        /// <summary>
        /// 是否有替换所有矿脉参数，矿脉生成Postfix阶段使用
        /// </summary>
        public bool _HasReplaceAllVeinsTo;

        /// <summary>
        /// 替换所有矿脉，矿脉生成Postfix阶段使用
        /// </summary>
        public EVeinType ReplaceAllVeinsTo;

        override public string ToString ()
        {
            StringBuilder str = new StringBuilder("AdditionalPlanetConfig:\r\n");
            str.Append("    ").Append("Index: ").Append(Index).Append("\r\n");
            str.Append("    ").Append("OrbitAround: ").Append(OrbitAround).Append("\r\n");
            str.Append("    ").Append("OrbitIndex: ").Append(OrbitIndex).Append("\r\n");
            str.Append("    ").Append("Number: ").Append(Number).Append("\r\n");
            str.Append("    ").Append("GasGiant: ").Append(GasGiant).Append("\r\n");
            str.Append("    ").Append("InfoSeed: ").Append(InfoSeed).Append("\r\n");
            str.Append("    ").Append("GenSeed: ").Append(GenSeed).Append("\r\n");
            str.Append("    ").Append("Radius: ").Append(Radius).Append("\r\n");
            str.Append("    ").Append("OrbitalPeriod: ").Append(OrbitalPeriod).Append("\r\n");
            str.Append("    ").Append("RotationPeriod: ").Append(RotationPeriod).Append("\r\n");
            str.Append("    ").Append("IsTidalLocked: ").Append(IsTidalLocked).Append("\r\n");
            str.Append("    ").Append("OrbitInclination: ").Append(OrbitInclination).Append("\r\n");
            str.Append("    ").Append("Obliquity: ").Append(Obliquity).Append("\r\n");
            str.Append("    ").Append("DontGenerateVein: ").Append(DontGenerateVein).Append("\r\n");
            str.Append("    ").Append("_HasThemeId: ").Append(_HasThemeId).Append("\r\n");
            str.Append("    ").Append("ThemeId: ").Append(ThemeId).Append("\r\n");
            str.Append("    ").Append("_HasOrbitLongitude: ").Append(_HasOrbitLongitude).Append("\r\n");
            str.Append("    ").Append("OrbitLongitude: ").Append(OrbitLongitude).Append("\r\n");
            str.Append("    ").Append("_HasVeinCustom: ").Append(_HasVeinCustom).Append("\r\n");
            str.Append("    ").Append("VeinCustom: ").Append("\r\n");
            if (_HasVeinCustom)
            {
                foreach (var pair in VeinCustom)
                {
                    str.Append("        ").Append("VeinType: ").Append(pair.Key).Append("\r\n");
                    str.Append("            ").Append("VeinGroupCount:").Append("\r\n");
                    str.Append("                ").Append("Type: ").Append(pair.Value.VeinGroupCount.Type).Append("\r\n");
                    str.Append("                ").Append("AccurateValue: ").Append(pair.Value.VeinGroupCount.AccurateValue).Append("\r\n");
                    str.Append("                ").Append("RandomBaseValue: ").Append(pair.Value.VeinGroupCount.RandomBaseValue).Append("\r\n");
                    str.Append("                ").Append("RandomCoef: ").Append(pair.Value.VeinGroupCount.RandomCoef).Append("\r\n");
                    str.Append("                ").Append("RandomMulOffset: ").Append(pair.Value.VeinGroupCount.RandomMulOffset).Append("\r\n");
                    str.Append("                ").Append("RandomAddOffset: ").Append(pair.Value.VeinGroupCount.RandomAddOffset).Append("\r\n");
                    str.Append("            ").Append("VeinSpotCount:").Append("\r\n");
                    str.Append("                ").Append("Type: ").Append(pair.Value.VeinSpotCount.Type).Append("\r\n");
                    str.Append("                ").Append("AccurateValue: ").Append(pair.Value.VeinSpotCount.AccurateValue).Append("\r\n");
                    str.Append("                ").Append("RandomBaseValue: ").Append(pair.Value.VeinSpotCount.RandomBaseValue).Append("\r\n");
                    str.Append("                ").Append("RandomCoef: ").Append(pair.Value.VeinSpotCount.RandomCoef).Append("\r\n");
                    str.Append("                ").Append("RandomMulOffset: ").Append(pair.Value.VeinSpotCount.RandomMulOffset).Append("\r\n");
                    str.Append("                ").Append("RandomAddOffset: ").Append(pair.Value.VeinSpotCount.RandomAddOffset).Append("\r\n");
                    str.Append("            ").Append("VeinAmount:").Append("\r\n");
                    str.Append("                ").Append("Type: ").Append(pair.Value.VeinAmount.Type).Append("\r\n");
                    str.Append("                ").Append("AccurateValue: ").Append(pair.Value.VeinAmount.AccurateValue).Append("\r\n");
                    str.Append("                ").Append("RandomBaseValue: ").Append(pair.Value.VeinAmount.RandomBaseValue).Append("\r\n");
                    str.Append("                ").Append("RandomCoef: ").Append(pair.Value.VeinAmount.RandomCoef).Append("\r\n");
                    str.Append("                ").Append("RandomMulOffset: ").Append(pair.Value.VeinAmount.RandomMulOffset).Append("\r\n");
                    str.Append("                ").Append("RandomAddOffset: ").Append(pair.Value.VeinAmount.RandomAddOffset).Append("\r\n");
                }
            }
            str.Append("    ").Append("_HasReplaceAllVeinsTo: ").Append(_HasReplaceAllVeinsTo).Append("\r\n");
            str.Append("    ").Append("ReplaceAllVeinsTo: ").Append(ReplaceAllVeinsTo).Append("\r\n");
            return str.ToString();
        }

        public struct VeinConfig
        {
            public CustomValue VeinGroupCount;

            public CustomValue VeinSpotCount;

            public CustomValue VeinAmount;

            public struct CustomValue
            {
                public CustomType Type;

                public int AccurateValue;

                public int RandomBaseValue;

                public float RandomCoef;

                public float RandomMulOffset;

                public int RandomAddOffset;

                public int GetRandomResult (DotNet35Random random)
                {
                    float baseValue = RandomBaseValue * RandomCoef;
                    float mulOffset = baseValue * RandomMulOffset;
                    float mulOffsetMin = baseValue - mulOffset;
                    if (mulOffsetMin < 0)
                    {
                        mulOffsetMin = 0;
                    }
                    float mulOffsetMax = baseValue + mulOffset;
                    int mulOffsetResult = random.Next((int)mulOffsetMin, (int)mulOffsetMax);
                    int addOffsetMin = mulOffsetResult - RandomAddOffset;
                    if (addOffsetMin < 0)
                    {
                        addOffsetMin = 0;
                    }
                    int addOffsetMax = mulOffsetResult + RandomAddOffset;
                    int addOffsetResult = random.Next(addOffsetMin, addOffsetMax);
                    return addOffsetResult;
                }

                public enum CustomType
                {
                    Default,
                    Random,
                    Accurate
                }
            }

            public const int DEFAULT_VEIN_GROUP_COUNT_RANDOM_BASE_VALUE = 8;

            public const float DEFAULT_VEIN_GROUP_COUNT_RANDOM_COEF = 1;

            public const float DEFAULT_VEIN_GROUP_COUNT_RANDOM_MUL_OFFSET = 0;

            public const int DEFAULT_VEIN_GROUP_COUNT_RANDOM_ADD_OFFSET = 2;

            public const int DEFAULT_VEIN_SPOT_COUNT_RANDOM_BASE_VALUE = 15;

            public const float DEFAULT_VEIN_SPOT_COUNT_RANDOM_COEF = 1;

            public const float DEFAULT_VEIN_SPOT_COUNT_RANDOM_MUL_OFFSET = 0;

            public const int DEFAULT_VEIN_SPOT_COUNT_RANDOM_ADD_OFFSET = 5;

            public const int DEFAULT_VEIN_AMOUNT_RANDOM_BASE_VALUE = 100000;

            public const float DEFAULT_VEIN_AMOUNT_RANDOM_COEF = 1;

            public const float DEFAULT_VEIN_AMOUNT_RANDOM_MUL_OFFSET = 0;

            public const int DEFAULT_VEIN_AMOUNT_RANDOM_ADD_OFFSET = 50000;
        }
    }
}

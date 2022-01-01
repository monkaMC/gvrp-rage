using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Customization
{
    public class ParentData
    {
        public byte FatherShape;
        public byte MotherShape;
        public byte FatherSkin;
        public byte MotherSkin;
        public float Similarity;
        public float SkinSimilarity;

        public ParentData(byte fatherShape, byte motherShape, byte fatherSkin, byte motherSkin, float similarity, float skinsimilarity)
        {
            FatherShape = fatherShape;
            MotherShape = motherShape;
            FatherSkin = fatherSkin;
            MotherSkin = motherSkin;
            Similarity = similarity;
            SkinSimilarity = skinsimilarity;
        }
    }
}

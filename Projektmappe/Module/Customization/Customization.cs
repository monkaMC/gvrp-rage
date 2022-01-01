using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Assets.Tattoo;

namespace GVRP.Module.Customization
{
    public class CharacterCustomization
    {
        // Player
        public int Gender;

        // Parents
        public ParentData Parents;

        // Features
        public float[] Features = new float[20];

        // Appearance
        public AppearanceItem[] Appearance = new AppearanceItem[11];

        // Hair & Colors
        public HairData Hair;

        public int EyebrowColor;
        public int BeardColor;
        public byte EyeColor;
        public int BlushColor;
        public int LipstickColor;
        public int ChestHairColor;

        public List<uint> Tattoos;

        public CharacterCustomization()
        {
            Gender = 0;
            Parents = new ParentData(0, 0, 0, 0, 1.0f, 1.0f);
            for (var i = 0; i < Features.Length; i++) Features[i] = 0f;
            for (var i = 0; i < Appearance.Length; i++) Appearance[i] = new AppearanceItem(255, 1.0f);
            Hair = new HairData(0, 0, 0);
            Tattoos = new List<uint>();
        }
    }
}

using System;
using UnityEngine;

namespace NeoFPS.Hub
{
    [Serializable]
    public class ReadmeHeader
    {
        public Texture2D image = null;
        public Texture2D darkSkinImage = null;
        public string title = string.Empty;
        
        public ReadmeHeader()
        {
            title = string.Empty;
            image = null;
            darkSkinImage = null;
        }

        public ReadmeHeader(string t)
        {
            title = t;
            image = null;
            darkSkinImage = null;
        }

        public ReadmeHeader(Texture2D img, string t)
        {
            title = t;
            image = img;
            darkSkinImage = null;
        }

        public ReadmeHeader(Texture2D ls, Texture2D ds, string t)
        {
            title = t;
            image = ls;
            darkSkinImage = ds;
        }
    }
}
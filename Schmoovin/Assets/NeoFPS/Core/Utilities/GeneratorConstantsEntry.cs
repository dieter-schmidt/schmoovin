using System;

namespace NeoFPS.ScriptGeneration
{
    [Serializable]
    public class GeneratorConstantsEntry
    {
        public string name = string.Empty;
        public bool nameInvalid = true;
        public bool nameNotUnique = false;
        public bool nameReserved = false;
    }
}
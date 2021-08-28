using System;
using UnityEditor;

namespace NeoFPSEditor.ScriptGeneration
{
    [Flags]
    public enum GeneratorConstantsState
    {
        Valid = 0,
        RequiresRebuild = 1,
        NameValidErrors = 2,
        NameDuplicateErrors = 4,
        NameReservedErrors = 8
    }
}

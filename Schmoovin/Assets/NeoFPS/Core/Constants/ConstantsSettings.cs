using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace NeoFPS
{
    [HelpURL("http://docs.neofps.com/manual/neofpsref-so-constantssettings.html")]
    [CreateAssetMenu (fileName="ConstantsSettings", menuName="NeoFPS/Constants Settings", order = NeoFpsMenuPriorities.ungrouped_constants)]
	public class ConstantsSettings : ScriptableObject
    {
        #pragma warning disable 0414

        [SerializeField, Tooltip("Where the generated constant script should be output.")]
        private string m_TargetDirectory = string.Empty;

		[SerializeField, Tooltip("Where the generated constant editor script should be output.")]
        private string m_EditorDirectory = string.Empty;

		[SerializeField, Tooltip("The text file to use when generating byte constants.")]
        private TextAsset m_TemplateByteConstant = null;

		[SerializeField, Tooltip("The text file to use when generating a byte constant editor script.")]
        private TextAsset m_TemplateByteConstantDrawer = null;

		[SerializeField, Tooltip("The text file to use when generating int constants.")]
        private TextAsset m_TemplateIntConstant = null;

		[SerializeField, Tooltip("The text file to use when generating an int constant editor script.")]
        private TextAsset m_TemplateIntConstantDrawer = null;

		[SerializeField, Tooltip("The text file to use when generating unsigned int constants.")]
        private TextAsset m_TemplateUIntConstant = null;

		[SerializeField, Tooltip("The text file to use when generating an unsigned int constant editor script.")]
        private TextAsset m_TemplateUIntConstantDrawer = null;

		[SerializeField, Tooltip("The text file to use when generating short constants.")]
        private TextAsset m_TemplateShortConstant = null;

		[SerializeField, Tooltip("The text file to use when generating a short constant editor script.")]
        private TextAsset m_TemplateShortConstantDrawer = null;

		[SerializeField, Tooltip("The text file to use when generating unsigned short constants.")]
        private TextAsset m_TemplateUShortConstant = null;

		[SerializeField, Tooltip("The text file to use when generating an unsigned short constant editor script.")]
        private TextAsset m_TemplateUShortConstantDrawer = null;

        [SerializeField]
        private ConstantsGroup[] m_Constants = new ConstantsGroup[0];

        #pragma warning restore 0414

        public enum BaseType
		{
			Byte,
			Int,
			UnsignedInt,
			Short,
			UnsignedShort
		}

		public ConstantsGroup[] constants
		{
			get { return m_Constants; }
		}

		[Serializable]
		public class ConstantsGroup
		{
            [Tooltip("The name for the output constant. This will also be the output script file name, while the output drawer script will be named Drawer.")]
            public string className = string.Empty;

            [Tooltip("The namespace for the output scripts.")]
            public string classNamespace = string.Empty;

            [Tooltip("This value specifies which source templates should be used to generate the constant.")]
            public BaseType baseType = BaseType.Int;

            [Tooltip("A sequential array of constant value names. These must be valid names, and not duplicated. Use the + and - buttons to add or remove values, or reorder by dragging the handle on the left of the array entry.")]
            public string[] constants = new string[0];
		}
	}
}
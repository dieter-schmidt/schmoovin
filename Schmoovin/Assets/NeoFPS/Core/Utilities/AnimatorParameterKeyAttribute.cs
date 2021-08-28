using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public class AnimatorParameterKeyAttribute : PropertyAttribute
    {
        public enum AnimatorSource
        {
            GameObject,
            ChildObject,
            Parent,
            FullHeirarchy,
            Property
        }

        public AnimatorSource animatorSource
        {
            get;
            private set;
        }

        public string animatorProperty
        {
            get;
            private set;
        }

        public AnimatorControllerParameterType parameterType
        {
            get;
            private set;
        }

        public AnimatorParameterKeyAttribute(AnimatorControllerParameterType parameterType, bool checkChildObjects = false, bool checkParent = false)
        {
            this.animatorProperty = string.Empty;
            this.parameterType = parameterType;
            if (checkChildObjects)
            {
                if (checkParent)
                    animatorSource = AnimatorSource.FullHeirarchy;
                else
                    animatorSource = AnimatorSource.ChildObject;
            }
            else
            {
                if (checkParent)
                    animatorSource = AnimatorSource.Parent;
                else
                    animatorSource = AnimatorSource.GameObject;
            }
        }

        public AnimatorParameterKeyAttribute(string animatorProperty, AnimatorControllerParameterType parameterType)
        {
            this.animatorProperty = animatorProperty;
            this.parameterType = parameterType;
            this.animatorSource = AnimatorSource.Property;
        }
    }
}
using System;
using UnityEngine;

//  ScreenFx © NullTale - https://x.com/NullTale
namespace ScreenFx
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class InplaceFieldAttribute : PropertyAttribute
    {
        public string[] PropertyPath;

        public InplaceFieldAttribute(params string[] propertyPath)
        {
            PropertyPath = propertyPath;
        }
    }
}
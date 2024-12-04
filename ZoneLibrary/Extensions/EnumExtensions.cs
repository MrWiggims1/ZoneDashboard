using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ZoneProductionLibrary.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        ///     A generic extension method that aids in reflecting 
        ///     and retrieving any attribute that is applied to an `Enum`.
        ///     source: https://stackoverflow.com/questions/13099834/how-to-get-the-display-name-attribute-of-an-enum-member-via-mvc-razor-code
        /// </summary>
        public static TAttribute? GetAttribute<TAttribute>(this Enum enumValue) where TAttribute : Attribute
        {
            return enumValue.GetType()
                                     .GetMember(enumValue.ToString())
                                     .First()
                                     .GetCustomAttribute<TAttribute>();
        }

        public static string CleanName(this Enum enumValue)
        {
            DisplayAttribute? attribute = enumValue.GetAttribute<DisplayAttribute>();

            if (attribute is null || attribute.Name is null)
            {
                Log.Warning("{enumType} does not contain a display attribute name using default ToString method.", nameof(enumValue));
                return enumValue.ToString();
            }
            else
            {
                return attribute.Name;
            }
        }        
    }
}
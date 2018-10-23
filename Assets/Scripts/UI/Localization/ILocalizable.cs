using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crops.Localization
{
    /// <summary>
    /// Implements the requirements for any game object with localizable text.
    /// </summary>
    public interface ILocalized
    {
        /// <summary>
        /// English name of this Object. Intended to provided a basis for localization.
        /// </summary>        
        string NameEnglish
        {
            get;
        }

        /// <summary>
        /// Object name's localization key.
        /// </summary>
        string NameLocalizationKey
        {
            get;
        }

        /// <summary>
        /// Object description in English. Intended to provided a basis for localization.
        /// </summary>
        string DescriptionEnglish
        {
            get;
        }

        /// <summary>
        /// Localization key for the Object's in-game description.
        /// </summary>        
        string DescriptionLocalizationKey
        {
            get;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using Crops.Localization;

namespace Crops
{
    /// <summary>
    /// ScriptableObjects that contain localized text should be inherited from this class.
    /// </summary>
    public abstract class LocalizedObject : ScriptableObject, ILocalized
    {
        /// <summary>
        /// English name of this item. Intended to provided a basis for localization.
        /// </summary>        
        public string NameEnglish
        {
            get { return _nameEnglish; }
        }

        /// <summary>
        /// Item name's localization key.
        /// </summary>
        public string NameLocalizationKey
        {
            get { return _nameLocalizationKey; }
        }

        /// <summary>
        /// Item description in English. Intended to provided a basis for localization.
        /// </summary>
        public string DescriptionEnglish
        {
            get { return _descriptionEnglish; }
        }

        /// <summary>
        /// Localization key for the Item's in-game description.
        /// </summary>        
        public string DescriptionLocalizationKey
        {
            get { return _descriptionLocalizationKey; }
        }
        
        [SerializeField]
        string _nameEnglish;

        [SerializeField]
        string _nameLocalizationKey;

        [SerializeField]
        string _descriptionEnglish;

        [SerializeField]
        string _descriptionLocalizationKey;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Crops.World
{
    /// <summary>
    /// Type of Item an item is. This dictates the logical class used to represent it (Crop = Crop.cs, etc).
    /// </summary>
    public enum ItemType { Crop };

    /// <summary>
    /// Base class of all items, such as machinery, chemicals, and crops.
    /// </summary>
    public class Item : LocalizedObject, IPurchaseable
    {

        ItemData data;

        /// <summary>
        /// Used to display item in menus.
        /// </summary>
        public Sprite iconSprite;

        /// <summary>
        /// Used to display item on tilemap.
        /// </summary>
        public Sprite tilemapSprite;

        /// <summary>
        /// ID of this item type -- this is a reference to all items of this type.
        /// </summary>
        public string ID
        {
            get { return data.ID; }
            set { data.ID = value; }
        }


        public float Quality
        {
            get { return data.quality; }
            set { data.quality = value; }
        }

        public float BaseValue
        {
            get
            {
                return _baseValue;
            }
            set
            {
                _baseValue = value;
            }
        }

        public ItemType Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }

        [SerializeField]
        private float _baseValue;

        [SerializeField]
        private ItemType type;

    }
}

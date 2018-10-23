using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crops.World
{
    /// <summary>
    /// Type of BuildableObject an object is. This dictates the logical class used to represent it (Building = Building.cs, etc).
    /// </summary>
    public enum BuildableObjectType { Building, Scenery, Road, CityBuilding, Fence };

    /// <summary>
    /// Base class of all objects that can be built, ranging from trees to buildings to fields.
    /// </summary>
    [CreateAssetMenu]
    public class BuildableObject : LocalizedObject, IBuildable
    {
        BuildableObjectData data;

        /// <summary>
        /// Base value of object before adjustments.
        /// </summary>
        public float BuildCost
        {
            get
            {
                return _buildCost;
            }
            set
            {
                _buildCost = value;
            }
        }

        [SerializeField]
        private float _buildCost;

        /// <summary>
        /// Cost to destroy this object.
        /// </summary>
        public float DestructionCost
        {
            get
            {
                return _destructionCost;
            }
            set
            {
                _destructionCost = value;
            }
        }

        [SerializeField]
        private float _destructionCost = 0;


        /// <summary>
        /// If true, fences can be built around the edges of this building.
        /// </summary>
        public bool AllowFence
        {
            get
            {
                return _allowFence;
            }
            set
            {
                _allowFence = value;
            }
        }

        [SerializeField]
        private bool _allowFence;

        #region Properties
        /// <summary>
        /// Footprint of the object.
        /// </summary>
        public Vector2Int Footprint
        {
            get { return data.footprint.ToVector2Int(); }
            set { data.footprint = new Vector2IntJSON(value); }
        }

        /// <summary>
        /// Lower left tile coordinate of the object, (0,0) if the object is more than 1 tile in size.
        /// </summary>
        public Vector3Int OriginTileCoordinates
        {
            get { return data.originCoords.ToVector3Int(); }
            set { data.originCoords = new Vector2IntJSON(value); }
        }

        /// <summary>
        /// Unique ID of this object.
        /// </summary>       
        public int ID
        {
            get { return data.ID; }
            set { data.ID = value; }
        }

        /// <summary>
        /// ID of this building's owner. -1 is the city; 0 is none.
        /// </summary>
        public int OwnerID
        {
            get { return data.ownerID; }
            set { data.ownerID = value; }
        }

        /// <summary>
        /// Object dictionary key for this object.
        /// </summary>
        public string ObjectDictionaryKey
        {
            get { return data.tileDictionaryKey; }
            set { data.tileDictionaryKey = value; }
        }

        /// <summary>
        /// Tile sprite key for this object.
        /// </summary>
        public string TileSpriteKey
        {
            get { return $"{data.footprint.x}x{data.footprint.y}"; }
        }

        /// <summary>
        /// Variant of this object's type that is used.
        /// </summary>
        public int Variant
        {
            get { return data.variant; }
            set { data.variant = value; }
        }
        [SerializeField]
        public BuildableObjectType BuildableObjectType;
        #endregion

        #region Visual Data        
        /// <summary>
        /// Holds a Key/Value pair of a string & a Texture2D Array. Exposes the pair to the inspector.
        /// </summary>       
        [System.Serializable]
        public struct TilesetTextureIndex
        {
            public string Key;
            public Texture2D[] Value;
        }

        [SerializeField]
        /// <summary>
        /// Base textures of this object, where the key is the footprint ("2x3", "1x1"), and each array index is a variant of that footprint.
        /// At runtime, this array is converted into the tileSprites dictionary.
        /// </summary>
        internal TilesetTextureIndex[] tileTextures;

        /// <summary>
        /// Pixels per inch for this texture. Defaults to 20.
        /// </summary>
        public int pixelsPerUnit = 20;

        /// <summary>
        /// Holds arrays of this objects' sprites, where the two-dimensional array is the sprite position in the footprint.
        /// The index is the variant. The key corresponds to the footprint ("2x3", "1x1").
        /// </summary>
        [SerializeField]
        public Dictionary<string, Sprite[][,]> tileSprites = new Dictionary<string, Sprite[][,]>();

        internal bool arrayInitialized = false;



        #endregion

        /// <summary>
        /// Initializes the buildable object.
        /// </summary>
        /// <param name="originTile"></param>
        /// <param name="objectVersion"></param>
        /// <param name="dictionaryKey"></param>
        /// <param name="uniqueID"></param>
        public void InitializeObject(Vector2Int footprint, Vector3Int originTile, int objectVersion, string dictionaryKey, int uniqueID, int ownerID = 0)
        {
            data = new BuildableObjectData();
            Footprint = footprint;
            OriginTileCoordinates = originTile;
            ObjectDictionaryKey = dictionaryKey;
            Variant = objectVersion;
            ID = uniqueID;
            OwnerID = ownerID;
            //InitTileSpritesArray();
        }

        /// <summary>
        /// Initializes the buildable object.
        /// </summary>
        public virtual void InitializeObject<DataType>(DataType objectData, ref Dictionary<string, Sprite[][,]> tileSprites) where DataType : BuildableObjectData
        {
            data = objectData as DataType;
            this.tileSprites = tileSprites;
            arrayInitialized = true;
        }

        public virtual void InitTileSpritesArray()
        {
            if (!arrayInitialized)
            {
                /// Convert each entry in baseTextures to the corresponding tileSprites entry.
                foreach (TilesetTextureIndex entry in tileTextures)
                {
                    Sprite[][,] tileSpriteIndex = new Sprite[entry.Value.Length][,];
                    for (int i = 0; i < entry.Value.Length; i++)
                    {
                        tileSpriteIndex[i] = Utilities.SpriteHelper.GetSpriteAtlas(entry.Value[i], pixelsPerUnit);
                    }
                    tileSprites.Add(entry.Key, tileSpriteIndex);
                }
                arrayInitialized = true;
            }
            else
            {
            }
        }

        /// <summary>
        /// Returns an array coordinates reflecting this object's footprint.
        /// </summary>
        /// <returns></returns>
        public Vector3Int[,] GetObjectFootprintTiles()
        {
            Vector3Int[,] footprintTiles = new Vector3Int[Footprint.x, Footprint.y];

            for (int x = 0; x < Footprint.x; x++)
            {
                for (int y = 0; y < Footprint.y; y++)
                {
                    footprintTiles[x, y] = new Vector3Int(OriginTileCoordinates.x + x, OriginTileCoordinates.y + y, 0);
                }
            }

            return footprintTiles;
        }

    }
}
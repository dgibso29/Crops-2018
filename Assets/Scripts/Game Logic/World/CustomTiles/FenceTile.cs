using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Crops.World
{
    public class FenceTile : DynamicTile
    {

        public Sprite NoAssignment => AssetReference.NoAssignment;

        /// <summary>
        /// Full-tile fence sprites (land owned on all sides).
        /// </summary>
        public Sprite[] FullSprites => AssetReference.fullSprites;

        /// <summary>
        /// Inner corner-tile fence sprites (land owned on all tiles save 1 corner (NE/SE/SW/NW).
        /// </summary>
        public Sprite[] SingleInnerCornerSprites => AssetReference.singleInnerCornerSprites;

        /// <summary>
        /// Land owned on all tiles save two corners on the same side (NE/NW, NW/SW, SW/SE, SE/NE).
        /// </summary>
        public Sprite[] DoubleInnerCornerSprites => AssetReference.doubleInnerCornerSprites;

        /// <summary>
        /// Land owned on all tiles save two corners on opposite sides.
        /// </summary>
        public Sprite[] DoubleInnerCornerDiagonalSprites => AssetReference.doubleInnerCornerDiagonalSprites;

        /// <summary>
        /// Land owned on only one corner.
        /// </summary>
        public Sprite[] TripleInnerCornerSprites => AssetReference.tripleInnerCornerSprites;

        /// <summary>
        /// No corner tiles owned.
        /// </summary>
        public Sprite[] QuadrupleInnerCornerSprites => AssetReference.quadrupleInnerCornerSprites;

        /// <summary>
        /// Outer corner-tile fence sprites (land owned on all tiles save 2 adjacent sides (N/E, E/S, S/W, W/N).
        /// </summary>
        public Sprite[] OuterCornerSprites => AssetReference.outerCornerSprites;

        /// <summary>
        /// Outer corner-tile fence sprites with the opposing corner unowned.
        /// </summary>
        public Sprite[] OuterCornerWithInnerSprites => AssetReference.outerCornerWithInnerSprites;

        /// <summary>
        /// Edge-tile (land owned on 1 side) fence sprites.
        /// </summary>
        public Sprite[] EdgeSprites => AssetReference.edgeSprites;

        public Sprite[] EdgeSingleInnerLeftSprites => AssetReference.edgeSingleInnerLeftSprites;

        public Sprite[] EdgeSingleInnerRightSprites => AssetReference.edgeSingleInnerRightSprites;

        public Sprite[] EdgeDoubleInnerSprites => AssetReference.edgeDoubleInnerSprites;

        /// <summary>
        /// Isthmus-tile (land owned on 2 opposing sides) fence sprites.
        /// </summary>
        public Sprite[] IsthmusSprites => AssetReference.isthmusSprites;

        /// <summary>
        /// Peninsula-tile (land owned on 3 sides) fence sprites.
        /// </summary>
        public Sprite[] PeninsulaSprites => AssetReference.peninsulaSprites;

        /// <summary>
        /// Pond-tile (land owned on all 4 sides) fence sprites.
        /// </summary>
        public Sprite[] PondSprites => AssetReference.pondSprites;

        ///// <summary>
        ///// Reference to the asset from which this tile was created. Used to pull sprites.
        ///// </summary>
        //public new FenceType AssetReference
        //{
        //    get { return base.AssetReference as FenceType; }
        //    private set { AssetReference = value as FenceType; }
        //}

        /// <summary>
        /// Reference to the asset from which this tile was created. Used to pull sprites.
        /// </summary>
        public FenceType AssetReference
        {
            get;
            private set;
        }

        public override void SetAssetReference(ScriptableObject asset)
        {
            AssetReference = asset as FenceType;
        }


        // This refreshes itself and other FenceTiles that are orthogonally and diagonally adjacent
        public override void RefreshTile(Vector3Int location, ITilemap tilemap)
        {
            for (int yd = -1; yd <= 1; yd++)
                for (int xd = -1; xd <= 1; xd++)
                {
                    Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
                    if (HasFenceTile(tilemap, position))
                        tilemap.RefreshTile(position);
                }
        }
        // This determines which sprite is used based on the FenceTiles that are adjacent to it and rotates it to fit the other tiles.
        // As the rotation is determined by the FenceTile, the TileFlags.OverrideTransform is set for the tile.
        public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
        {
            // Add each 8 neighbor tiles clockwise, starting at N.
            int mask = HasFenceTile(tilemap, location + new Vector3Int(0, 1, 0)) ? 1 : 0;   // N  = 1
            mask += HasFenceTile(tilemap, location + new Vector3Int(1, 1, 0)) ? 2 : 0;      // NE = 2
            mask += HasFenceTile(tilemap, location + new Vector3Int(1, 0, 0)) ? 4 : 0;      // E  = 4
            mask += HasFenceTile(tilemap, location + new Vector3Int(1, -1, 0)) ? 8 : 0;     // SE = 8
            mask += HasFenceTile(tilemap, location + new Vector3Int(0, -1, 0)) ? 16 : 0;    // S  = 16
            mask += HasFenceTile(tilemap, location + new Vector3Int(-1, -1, 0)) ? 32 : 0;   // SW = 32
            mask += HasFenceTile(tilemap, location + new Vector3Int(-1, 0, 0)) ? 64 : 0;    // W  = 64
            mask += HasFenceTile(tilemap, location + new Vector3Int(-1, 1, 0)) ? 128 : 0;   // NW = 128

            Sprite newSprite = GetSprite((byte)mask);
            //tileData.flags = 0;
            tileData.sprite = newSprite;

            tileData.transform = GetNewMatrixWithRotation(GetRotation((byte)mask));

            tileData.color = Color.white;
            tileData.flags = TileFlags.LockTransform;


        }
        // This determines if the Tile at the position is the same FenceTile.
        private bool HasFenceTile(ITilemap tilemap, Vector3Int position)
        {
            if (position.x < 0 || position.y < 0 || position.x > Map.StaticMapSize - 1 || position.y > Map.StaticMapSize - 1)
            {
                return true;
            }
            return tilemap.GetTile<FenceTile>(position);
        }
        // The following determines which sprite to use based on the number of adjacent FenceTiles
        protected override Sprite GetSprite(byte mask)
        {
            switch (mask)
            {
                case 0:
                case 8:
                case 2:
                case 32:
                case 128:
                case 10:
                case 40:
                case 160:
                case 130:
                case 42:
                case 168:
                case 162:
                case 138:
                case 34:
                case 136:
                case 170: return PondSprites[Random.Range(0, PondSprites.Length)];
                case 1:
                case 4:
                case 6:
                case 12:
                case 16:
                case 14:
                case 131:
                case 224:
                case 56:
                case 96:
                case 48:
                case 192:
                case 3:
                case 129:
                case 24:
                case 41:
                case 74:
                case 164:
                case 146:
                case 104:
                case 176:
                case 26:
                case 44:
                case 134:
                case 11:
                case 161:
                case 194:
                case 72:
                case 36:
                case 66:
                case 132:
                case 33:
                case 9:
                case 18:
                case 144:
                case 38:
                case 140:
                case 98:
                case 200:
                case 184:
                case 58:
                case 163:
                case 139:
                case 186:
                case 171:
                case 174:
                case 46:
                case 142:
                case 234:
                case 232:
                case 226:
                case 166:
                case 172:
                case 202:
                case 106:
                case 35:
                case 137:
                case 43:
                case 169:
                case 152:
                case 50:
                case 154:
                case 178:
                case 64: return PeninsulaSprites[Random.Range(0, PeninsulaSprites.Length)];
                case 108:
                case 198:
                case 27:
                case 177:
                case 17:
                case 100:
                case 76:
                case 196:
                case 70:
                case 49:
                case 19:
                case 25:
                case 187:
                case 238:
                case 145:
                case 228:
                case 78:
                case 57:
                case 147:
                case 236:
                case 110:
                case 185:
                case 155:
                case 59:
                case 206:
                case 86:
                case 204:
                case 179:
                case 230:
                case 102:
                case 51:
                case 153:
                case 68: return IsthmusSprites[Random.Range(0, IsthmusSprites.Length)];
                case 254:
                case 251:
                case 243:
                case 126:
                case 207:
                case 159:
                case 252:
                case 231:
                case 249:
                case 63:
                case 239:
                case 191:
                case 241:
                case 31:
                case 124:
                case 199: return EdgeSprites[Random.Range(0, EdgeSprites.Length)];
                case 29:
                case 219:
                case 111:
                case 209:
                case 244:
                case 211:
                case 79:
                case 61:
                case 71:
                case 116:
                case 246:
                case 189:
                case 118:
                case 157:
                case 217:
                case 103: return EdgeSingleInnerLeftSprites[Random.Range(0, EdgeSingleInnerLeftSprites.Length)];
                case 222:
                case 183:
                case 92:
                case 123:
                case 101:
                case 237:
                case 23:
                case 94:
                case 121:
                case 229:
                case 151:
                case 113:
                case 205:
                case 220:
                case 115:
                case 55:
                case 197: return EdgeSingleInnerRightSprites[Random.Range(0, EdgeSingleInnerRightSprites.Length)];
                case 214:
                case 91:
                case 109:
                case 84:
                case 81:
                case 69:
                case 21:
                case 149:
                case 212:
                case 89:
                case 77:
                case 83:
                case 53:
                case 181: return EdgeDoubleInnerSprites[Random.Range(0, EdgeSingleInnerLeftSprites.Length)];
                case 225:
                case 30:
                case 15:
                case 240:
                case 143:
                case 227:
                case 62:
                case 248:
                case 7:
                case 112:
                case 135:
                case 195:
                case 60:
                case 120:
                case 193:
                case 175:
                case 190:
                case 250:
                case 235:
                case 242:
                case 47:
                case 156:
                case 114:
                case 201:
                case 39:
                case 158:
                case 167:
                case 203:
                case 188:
                case 122:
                case 233:
                case 28: return OuterCornerSprites[Random.Range(0, OuterCornerSprites.Length)];
                case 5:
                case 20:
                case 80:
                case 88:
                case 67:
                case 13:
                case 97:
                case 52:
                case 22:
                case 208:
                case 133:
                case 99:
                case 141:
                case 54:
                case 216:
                case 90:
                case 45:
                case 150:
                case 210:
                case 73:
                case 37:
                case 148:
                case 82:
                case 107:
                case 173:
                case 182:
                case 218:
                case 105:
                case 75:
                case 165:
                case 180:
                case 65: return OuterCornerWithInnerSprites[Random.Range(0, OuterCornerWithInnerSprites.Length)];
                case 223:
                case 127:
                case 253:
                case 247: return SingleInnerCornerSprites[Random.Range(0, SingleInnerCornerSprites.Length)];
                case 95:
                case 125:
                case 215:
                case 245: return DoubleInnerCornerSprites[Random.Range(0, DoubleInnerCornerSprites.Length)];
                case 119:
                case 221: return DoubleInnerCornerDiagonalSprites[Random.Range(0, DoubleInnerCornerDiagonalSprites.Length)];
                case 117:
                case 213:
                case 87:
                case 93: return TripleInnerCornerSprites[Random.Range(0, TripleInnerCornerSprites.Length)];
                case 85: return QuadrupleInnerCornerSprites[Random.Range(0, QuadrupleInnerCornerSprites.Length)];
                case 255: return FullSprites[Random.Range(0, FullSprites.Length)];

            }
            //return pondSprites[Random.Range(0, pondSprites.Length)];
            return NoAssignment;
        }
        // The following determines which rotation to use based on the positions of adjacent FenceTiles
        protected override Quaternion GetRotation(byte mask)
        {
            switch (mask)
            {
                // WEST (9 o'clock)
                case 95:
                case 4:
                case 12:
                case 14:
                case 63:
                case 7:
                case 159:
                case 191:
                case 143:
                case 181:
                case 183:
                case 189:
                case 15:
                case 6:
                case 5:
                case 223:
                case 135:
                case 29:
                case 23:
                case 151:
                case 61:
                case 13:
                case 133:
                case 21:
                case 164:
                case 49:
                case 19:
                case 25:
                case 187:
                case 145:
                case 57:
                case 147:
                case 185:
                case 155:
                case 87:
                case 59:
                case 44:
                case 134:
                case 36:
                case 132:
                case 179:
                case 221:
                case 141:
                case 149:
                case 45:
                case 37:
                case 173:
                case 165:
                case 38:
                case 140:
                case 174:
                case 46:
                case 142:
                case 166:
                case 172:
                case 51:
                case 153:
                case 53:
                case 47:
                case 39:
                case 157:
                case 167:
                case 55:
                case 175:
                case 31: return Quaternion.Euler(0f, 0f, 270f); // West

                // NORTH (12 o'clock)
                case 16:
                case 60:
                case 30:
                case 20:
                case 62:
                case 252:
                case 254:
                case 126:
                case 125:
                case 127:
                case 56:
                case 28:
                case 48:
                case 24:
                case 92:
                case 214:
                case 222:
                case 246:
                case 52:
                case 180:
                case 22:
                case 94:
                case 244:
                case 84:
                case 146:
                case 116:
                case 190:
                case 93:
                case 176:
                case 26:
                case 18:
                case 144:
                case 54:
                case 118:
                case 220:
                case 212:
                case 150:
                case 148:
                case 182:
                case 184:
                case 58:
                case 186:
                case 152:
                case 50:
                case 154:
                case 178:
                case 156:
                case 158:
                case 188:
                case 124: return Quaternion.Euler(0f, 0f, 180f); // North

                // EAST (3 o'clock)         
                case 64:
                case 85:
                case 80:
                case 112:
                case 96:
                case 192:
                case 120:
                case 224:
                case 177:
                case 27:
                case 243:
                case 249:
                case 248:
                case 240:
                case 245:
                case 251:
                case 253:
                case 91:
                case 123:
                case 219:
                case 208:
                case 209:
                case 121:
                case 211:
                case 88:
                case 81:
                case 17:
                case 160:
                case 74:
                case 113:
                case 117:
                case 104:
                case 194:
                case 72:
                case 66:
                case 216:
                case 89:
                case 90:
                case 210:
                case 82:
                case 218:
                case 98:
                case 200:
                case 234:
                case 232:
                case 226:
                case 202:
                case 106:
                case 83:
                case 250:
                case 242:
                case 114:
                case 115:
                case 217:
                case 122:
                case 241: return Quaternion.Euler(0f, 0f, 90f); // East
            }
            return Quaternion.Euler(0f, 0f, 0f);
        }

    }
}

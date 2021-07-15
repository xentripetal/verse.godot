using System.Runtime.Serialization;
using Godot;
using Newtonsoft.Json;

namespace Verse.ldtk {
	public partial class TileInstance {
		public enum Flip {
			X = 1,
			Y = 2
		}
	}

	public partial class TilesetDefinition {
		public Rect2 getTileRegion(int tileId) {
			var atlasGridWidth = PxWid / TileGridSize;
			var pixelTile = getTilePxCoords(tileId);

			return new Rect2(pixelTile, new Vector2(TileGridSize, TileGridSize));
		}

		public Vector2 getTileGridCoords(int tileId) {
			var y = tileId / TileGridSize;
			var x = tileId - TileGridSize * y;
			return new Vector2(x, y);
		}

		public Vector2 getTilePxCoords(int tileId) {
			var gridCoords = getTileGridCoords(tileId);
			var x = Padding + gridCoords.x * (TileGridSize + Spacing);
			var y = Padding + gridCoords.y * (TileGridSize + Spacing);
			return new Vector2(x, y);
		}
	}

	public partial class LayerInstance {
		/// <summary>
		/// Layer type (possible values: IntGrid, Entities, Tiles or AutoLayer)
		/// </summary>
		[JsonProperty("__type")]
		public LayerType Type { get; set; }

		public enum LayerType {
			Entities,
			Tiles,
			IntGrid,
			AutoLayer
		}
	}
}
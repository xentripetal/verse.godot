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
			var pixelTile = getTilePxCoords(tileId);

			return new Rect2(pixelTile, new Vector2(TileGridSize, TileGridSize));
		}

		public Vector2 getTileGridCoords(int tileId) {
			var atlasGridWidth = PxWid / TileGridSize;
			var y = tileId / atlasGridWidth;
			var x = tileId - atlasGridWidth * y;
			return new Vector2(x, y);
		}

		public Vector2 getTilePxCoords(int tileId) {
			var gridCoords = getTileGridCoords(tileId);
			var x = Padding + gridCoords.x * (TileGridSize + Spacing);
			var y = Padding + gridCoords.y * (TileGridSize + Spacing);
			return new Vector2(x, y);
		}
	}

	public partial class Level {
		public bool Contains(Vector2 pixelPos) {
			var bounds = new Rect2(new Vector2(WorldX, WorldY), PxWid, PxHei);
			return bounds.HasPoint(pixelPos);
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
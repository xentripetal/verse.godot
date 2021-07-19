using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Verse.ldtk;
using Verse.Utilities;
using World = Godot.World;

namespace Verse {
	public class Level : Node2D {
		public World World { get; }
		public Verse.ldtk.Level LdtkLevel { get; }

		public static List<StackableTileMap> TileMaps = new List<StackableTileMap>();

		public Level(World world, Verse.ldtk.Level json) {
			World = world;
			LdtkLevel = json;
			Initialize();
		}

		/**
		 * Gets the neighbor level connected to this position.
		 * Must be a border position
		 */
		public long GetNeighbor(Vector2 borderPos) {
			foreach (var neighbor in LdtkLevel.Neighbours) {
				switch (neighbor.Dir) {
					case "n": borderPos = borderPos + Vector2.Up;
						break;
					case "e": borderPos = borderPos + Vector2.Right;
						break;
					case "s": borderPos = borderPos + Vector2.Down;
						break;
					case "w": borderPos = borderPos + Vector2.Left;
						break;
				}
				if (World.Json.Levels.First(x => x.Uid == neighbor.LevelUid).Contains(borderPos)) {
					return neighbor.LevelUid;
				}
			}

			return -1;
		}

		void Initialize() {
			Position = new Vector2(LdtkLevel.WorldX, LdtkLevel.WorldY);
			for (var i = LdtkLevel.LayerInstances.Length - 1; i >= 0; i--) {
				var layer = LdtkLevel.LayerInstances[i];
				switch (layer.Type) {
					case LayerInstance.LayerType.Entities:
						InitializeEntitiesLayer(layer);
						break;
					case LayerInstance.LayerType.Tiles:
					case LayerInstance.LayerType.IntGrid:
					case LayerInstance.LayerType.AutoLayer:
						InitializeTileLayer(layer);
						break;
				}
			}
		}

		void InitializeEntitiesLayer(LayerInstance layer) {
			GD.Print("Entites layer currently not supported");
		}

		void InitializeTileLayer(LayerInstance layer) {
			var size = layer.GridSize;
			var alpha = (byte) (layer.Opacity * 255);
			Debug.Assert(layer.TilesetDefUid != null, "layer.TilesetDefUid != null");
			var map = new StackableTileMap {
				ZAsRelative = true,
				TileSet = World.TileSets[(long) layer.TilesetDefUid],
				Name = LdtkLevel.Identifier + "_" + layer.Identifier,
				Visible = layer.Visible,
				Modulate = Color.Color8(255, 255, 255, alpha),
				Position = new Vector2(layer.PxTotalOffsetX, layer.PxTotalOffsetY),
				CellSize = new Vector2(layer.GridSize, layer.GridSize)
			};

			TileMaps.Add(map);

			if (layer.AutoLayerTiles.Length > 0 || layer.GridTiles.Length > 0) {
				var tiles = layer.AutoLayerTiles.Length > 0 ? layer.AutoLayerTiles : layer.GridTiles;
				foreach (var tile in tiles) {
					map.AppendCell((int) (tile.Px[0] / size), (int) (tile.Px[1] / size), (int) tile.T,
						(tile.F & 1) == 1,
						(tile.F & 2) == 2);
				}
			}
			else if (layer.IntGridCsv.Length > 0) {
				throw new NotImplementedException("Plain int grids are not currently supported.");
			}

			AddChild(map);
		}
	}
}
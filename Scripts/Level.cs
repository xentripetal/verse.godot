using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Verse.ldtk;
using Verse.Scripts;
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

		void Initialize() {
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
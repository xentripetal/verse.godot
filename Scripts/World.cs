using System.Collections.Generic;
using System.Linq;
using Godot;
using Newtonsoft.Json.Linq;
using Verse.ldtk;
using Verse.Utilities;

namespace Verse {
	public class World : Node2D {
		[Signal]
		public delegate void LevelChanged(Level newLevel, Level previousLevel);
		
		[Export] public string LevelsRoot = "res://Resources/Levels/";
		public long DefaultLevel = 0;
		public LdtkJson Json;
		public Dictionary<long, TileSet> TileSets = new Dictionary<long, TileSet>();
		public Dictionary<long, Level> InitializedLevels = new Dictionary<long, Level>();
		public Level CurrentLevel;

		public World(LdtkJson json) {
			Json = json;
			DefaultLevel = json.Levels.Select(x => x.Uid)
				.FirstOr(-1);
		}
		

		public World(LdtkJson json, long defaultLevel) {
			Json = json;
			DefaultLevel = defaultLevel;
		}

		public World(LdtkJson json, string defaultLevelIdentifier) {
			Json = json;
			DefaultLevel = json.Levels.Where(x => x.Identifier == defaultLevelIdentifier)
				.Select(x => x.Uid)
				.FirstOr(-1);
		}

		public override void _Ready() {
			LoadDefinitions(Json.Defs);
			if (DefaultLevel >= 0) {
				SetLevel(DefaultLevel);
			}
		}

		public void SetLevel(string name) {
			SetLevel(Json.Levels.First(x => x.Identifier == name));
		}
		
		public void SetLevel(long uid) {
			SetLevel(Json.Levels.First(x => x.Uid== uid));
		}

		public void SetLevel(ldtk.Level ldtkLevel) {
			if (!InitializedLevels.TryGetValue(ldtkLevel.Uid, out var level)) {
				level = InitializeLevel(ldtkLevel);
			}
			

			if (CurrentLevel != null) {
				RemoveChild(CurrentLevel);
			}
			AddChild(level);
			CurrentLevel = level;
			EmitSignal(nameof(LevelChanged), CurrentLevel);
		}


		void LoadDefinitions(Definitions defs) {
			LoadTilesetDefinitions(defs.Tilesets);
		}

		void LoadTilesetDefinitions(TilesetDefinition[] defs) {
			foreach (var def in defs) {
				var set = new TileSet();
				var texture = GD.Load<Texture>(LevelsRoot + def.RelPath);
				var w = (def.PxWid - def.Padding) / (def.TileGridSize + def.Spacing);
				var h = (def.PxHei - def.Padding) / (def.TileGridSize + def.Spacing);
				var size = w * h;
				//def.EnumTags[0]["enumValueId"]
				List<int> collisionTiles = new List<int>();
				foreach (var enumTag in def.EnumTags) {
					if ((string) enumTag["enumValueId"] != "COLLISION") {
						continue;
					}

					var test = (JArray) enumTag["tileIds"];
					foreach (var token in test) {
						collisionTiles.Add(token.ToObject<int>());
					}
				}
				
				for (int i = 0; i < size; i++) {
					set.CreateTile(i);
					set.TileSetTileMode(i, TileSet.TileMode.SingleTile);
					set.TileSetTexture(i, texture);
					var region = def.getTileRegion(i);
					set.TileSetRegion(i, region);
				}

				var shape = new RectangleShape2D();
				shape.Extents = new Vector2(def.TileGridSize / 2, def.TileGridSize / 2);
				set.TileAddShape(0, shape, Transform2D.Identity);
				foreach (var tile in collisionTiles) { 
					set.TileSetShape(tile, 0, shape);
					set.TileSetShapeOffset(tile, 0, new Vector2(def.TileGridSize / 2, def.TileGridSize / 2));
				}
				TileSets.Add(def.Uid, set);
			}
		}

		Level InitializeLevel(ldtk.Level ldtkLevel) {
			var level = new Level(this, ldtkLevel);
			InitializedLevels.Add(ldtkLevel.Uid, level);
			return level;
		}
	}
}
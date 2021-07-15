using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Verse.ldtk;

public class ldtkLoader : Node {
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	[Export] public string LevelsRoot = "res://Resources/Levels/";
	public string LevelName = "test.ldtk";
	public static Dictionary<long, TileSet> TileSets = new Dictionary<long, TileSet>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		var world = LoadCompleteMap(LevelName);
		ImportLevel(world, "Dead_end_east");
	}

	public void ImportLevel(LdtkJson world, String identifier) {
		var uid = world.Levels.First(x => x.Identifier == identifier).Uid;
		foreach (var def in world.Defs.Tilesets) {
			var set = new TileSet();
			var texture = GD.Load<Texture>(LevelsRoot + def.RelPath);
			var image = texture.GetData();
			var w = (def.PxWid - def.Padding) / (def.TileGridSize + def.Spacing);
			var h = (def.PxHei - def.Padding) / (def.TileGridSize + def.Spacing);
			var size = w * h;
			for (int i = 0; i < size; i++) {
				set.CreateTile(i);
				set.TileSetTileMode(i, TileSet.TileMode.SingleTile);
				set.TileSetTexture(i, texture);
				set.TileSetRegion(i, def.getTileRegion(i));
			}

			TileSets.Add(def.Uid, set);
		}

		ImportLevel(world, uid);
	}

	public void ImportLevel(LdtkJson world, long uid) {
		var level = world.Levels.First(x => x.Uid == uid);
		foreach (var layer in level.LayerInstances) {
			switch (layer.Type) {
				case LayerInstance.LayerType.Entities:
					ImportEntitiesLayer(world, level, layer);
					break;
				case LayerInstance.LayerType.Tiles:
				case LayerInstance.LayerType.IntGrid:
				case LayerInstance.LayerType.AutoLayer:
					ImportTileLayer(world, level, layer);
					break;
			}
		}
	}

	private void ImportTileLayer(LdtkJson world, Level level, LayerInstance layer) {
		var def = world.Defs.Layers.First(x => x.Uid == layer.LayerDefUid);
		var size = layer.GridSize;

		var map = new TileMap();
		Debug.Assert(layer.TilesetDefUid != null, "layer.TilesetDefUid != null");
		map.TileSet = TileSets[(long) layer.TilesetDefUid];
		map.Name = level.Identifier + "_" + layer.Identifier;
		map.Visible = layer.Visible;
		map.Position = new Vector2(layer.PxTotalOffsetX, layer.PxTotalOffsetY);
		map.CellSize = new Vector2(layer.GridSize, layer.GridSize);
		foreach (var tile in layer.AutoLayerTiles) {
			map.SetCell((int) (tile.Px[0] / size), (int) (tile.Px[1] / size), (int) tile.T, (tile.F & 1) == 1,
				(tile.F & 2) == 2, false);
		}

		//def.TilePivotY
		//layer.Opacity
		//layer.Type
		AddChild(map);
		GD.Print(layer.Identifier + ": " + layer.Type);
	}

	void ImportEntitiesLayer(LdtkJson world, Level level, LayerInstance layer) {
		GD.Print("Entity Layer not supported");
	}

	//Todo handle modding
	LdtkJson LoadCompleteMap(String levelName) {
		var files = new File();
		files.Open(LevelsRoot + LevelName, File.ModeFlags.Read);
		var text = files.GetAsText();
		var json = LdtkJson.FromJson(text);
		var externalLevels = new List<Level>();
		foreach (var level in json.Levels) {
			if (level.ExternalRelPath != null && level.ExternalRelPath != "") {
				files.Open(LevelsRoot + level.ExternalRelPath, File.ModeFlags.Read);
				var levelText = files.GetAsText();
				externalLevels.Add(Level.FromJson(levelText));
			}
		}

		foreach (var level in externalLevels) {
			var match = json.Levels.Select((v, i) => new {v, i}).FirstOrDefault(x => x.v.Uid == level.Uid);
			if (match == null) {
				json.Levels = json.Levels.Concat(new Level[] {level}).ToArray();
			}
			else {
				json.Levels.SetValue(level, match.i);
			}
		}

		files.Close();

		return json;
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
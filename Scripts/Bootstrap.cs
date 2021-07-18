using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Verse.ldtk;

namespace Verse {
	public class Bootstrap : Node {
		[Export] public string LevelsRoot = "res://Resources/Levels/";

		private World _world;
		private LdtkJson json;

		public override void _Ready() {
			json = LoadCompleteMap("test.ldtk");
			_world = new World(json, 29);
			AddChild(_world);
		}
		
		LdtkJson LoadCompleteMap(String worldName) {
			var files = new File();
			files.Open(LevelsRoot + worldName, File.ModeFlags.Read);
			var text = files.GetAsText();
			var json = LdtkJson.FromJson(text);
			var externalLevels = new List<ldtk.Level>();
			foreach (var level in json.Levels) {
				if (level.ExternalRelPath != null && level.ExternalRelPath != "") {
					files.Open(LevelsRoot + level.ExternalRelPath, File.ModeFlags.Read);
					var levelText = files.GetAsText();
					externalLevels.Add(ldtk.Level.FromJson(levelText));
				}
			}

			foreach (var level in externalLevels) {
				var match = json.Levels.Select((v, i) => new {v, i}).FirstOrDefault(x => x.v.Uid == level.Uid);
				if (match == null) {
					json.Levels = json.Levels.Concat(new[] {level}).ToArray();
				}
				else {
					json.Levels.SetValue(level, match.i);
				}
			}

			files.Close();

			return json;
		}

		public override void _UnhandledInput(InputEvent @event) {
			if (@event is InputEventKey eventKey)
				if (eventKey.Pressed && eventKey.Scancode == (int) KeyList.Space) {
					var rand = new Random();
					int i = rand.Next(0, json.Levels.Length);
					var randLevel = json.Levels[i];
					_world.SetLevel(randLevel);
				}

		}
	}
}
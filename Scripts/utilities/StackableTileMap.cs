using System;
using System.Collections.Generic;
using Godot;

namespace Verse.Scripts {
	public class StackableTileMap : Node2D {
		private List<TileMap> _tileMaps = new List<TileMap>();


		/// <summary>
		/// <para>The assigned <see cref="T:Godot.TileSet" />.</para>
		/// </summary>
		public TileSet TileSet {
			get => _tileSet;
			set {
				_tileSet = value;
				foreach (var tileMap in _tileMaps) {
					tileMap.TileSet = value;
				}
			}
		}

		private TileSet _tileSet;

		/// <summary>
		/// <para>The TileMap's cell size.</para>
		/// </summary>
		public Vector2 CellSize {
			get => _cellSize;
			set {
				_cellSize = value;
				foreach (var tileMap in _tileMaps) {
					tileMap.CellSize = _cellSize;
				}
			}
		}

		private Vector2 _cellSize = new Vector2(32, 32);


		private string _name = "stackable_tilemap";

		public StackableTileMap() { }

		TileMap addLayer() {
			if (_tileMaps.Count > byte.MaxValue) {
				throw new OverflowException("There cannot be more than " + byte.MaxValue +
				                            " internal layers in a stackable tilemap");
			}
			var tm = new TileMap();
			tm.TileSet = _tileSet;
			tm.ZAsRelative = true;
			//tm.ZIndex = _tileMaps.Count;
			tm.CellSize = _cellSize;
			AddChild(tm);
			_tileMaps.Add(tm);
			return tm;
		}

		public void SetCell(int x, int y, byte layer, int tile, bool flipX = false, bool flipY = false,
			bool transpose = false) {
			for (int i = layer; i >  _tileMaps.Count - 1; i--) {
				addLayer();
			}

			_tileMaps[layer].SetCell(x, y, tile, flipX, flipY, transpose);
		}

		public void AppendCell(int x, int y, int tile, bool flipX = false, bool flipY = false,
			bool transpose = false) {

			foreach (var tilemap in _tileMaps) {
				if (tilemap.GetCell(x, y) == -1) {
					tilemap.SetCell(x, y, tile, flipX, flipY, transpose);
					return;
				}
			}

			addLayer().SetCell(x, y, tile, flipX, flipY, transpose);
		}

		public int GetBottomCell(int x, int y) {
			return GetCell(x, y, 0);
		}
		
		public int GetCell(int x, int y, int layer) {
			if (layer > _tileMaps.Count - 1) {
				return -1;
			}

			return _tileMaps[layer].GetCell(x, y);
		}

		public void ClearCell(int x, int y) {
			foreach (var tileMap in _tileMaps) {
				tileMap.SetCell(x, y, -1);
			}
		}
	}
}
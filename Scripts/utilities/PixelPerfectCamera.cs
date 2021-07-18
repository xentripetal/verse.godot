using System;
using Godot;

namespace Verse.Scripts {
	public class PixelPerfectCamera : Camera2D {
		[Export] public Vector2 DesiredResolution = new Vector2(320, 180);

		[Export] public float scalingFactor = 0;
		private Viewport vp;

		public override void _Ready() {
			vp = GetViewport();
			vp.Connect("size_changed", this, "OnSizeChanged");
			OnSizeChanged();
		}

		private void OnSizeChanged() {
			var scale = vp.Size / DesiredResolution;
			var newScaleFactor = (float) Math.Max(Math.Floor(Math.Min(scale.x, scale.y)), 1);
			if (Math.Abs(newScaleFactor - scalingFactor) > float.Epsilon) {
				scalingFactor = newScaleFactor;
				Zoom = new Vector2(1 / scalingFactor, 1 / scalingFactor);
			}
		}
	}
}
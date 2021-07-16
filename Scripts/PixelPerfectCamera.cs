using System;
using Godot;

namespace Verse.Scripts {
	public class PixelPerfectCamera : ViewportContainer {
		[Export] public Vector2 DesiredResolution = new Vector2(320, 180);

		private int scalingFactor = 0;
		private Viewport vp;

		public override void _Ready() {
			vp = GetViewport();
			vp.Connect("size_changed", this, "OnSizeChanged");
		}

		private void OnSizeChanged() {
			var scale = vp.Size / DesiredResolution;
			var newScaleFactor = (int) Math.Max(Math.Floor(Math.Min(scale.x, scale.y)), 1);
			if (Math.Abs(newScaleFactor - scalingFactor) > float.Epsilon) {
				scalingFactor = newScaleFactor;
				var vpSize = DesiredResolution * scalingFactor;
				MarginLeft = -vpSize[0] / 2;
				MarginRight = vpSize[0] / 2;
				MarginTop = -vpSize[1] / 2;
				MarginBottom = vpSize[1] / 2;
			}
		}
	}
}
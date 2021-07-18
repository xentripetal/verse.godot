using Godot;

namespace Verse {
	public class Player : KinematicBody2D {
		public int Speed = 250;

		public Vector2 GetInput() {
			// Detect up/down/left/right keystate and only move when pressed
			var _velocity = new Vector2();

			if (Input.IsActionPressed("move_right"))
				_velocity.x += 1;

			if (Input.IsActionPressed("move_left"))
				_velocity.x -= 1;

			if (Input.IsActionPressed("move_down"))
				_velocity.y += 1;

			if (Input.IsActionPressed("move_up")) {
				_velocity.y -= 1;
			}

			return _velocity;
		}

		public override void _PhysicsProcess(float delta) {
			var v = GetInput();
			//Position += v * delta * Speed;
			MoveAndSlide(v * Speed);
		}
	}
}
using System;
using Godot;

namespace Verse {
	
	public enum PlayerAnimationState {
		WalkLeft,
		WalkRight,
		WalkUp,
		WalkDown,
		IdleLeft,
		IdleRight,
		IdleUp,
		IdleDown,
	}

	public static class PlayerAnimationStateExtensions {
		public static PlayerAnimationState ToIdleState(this PlayerAnimationState state) {
			if (state > PlayerAnimationState.WalkDown) {
				return state;
			}

			return state + 4;
		}
	}

	public class Player : KinematicBody2D {
		public int Speed = 250;


		public Vector2 PlayerInput;
		public PlayerAnimationState AnimationState;
		public World CurrentWorld;
		public Level CurrentLevel;
		

		private AnimatedSprite _sprite;
		private bool _inBorderState;
		private bool _levelChangeAcked = true;

		public override void _Ready() {
			_sprite = GetNode<AnimatedSprite>("AnimatedSprite");
			CurrentWorld = GetNode<World>("/root/Verse/World");
			CurrentLevel = CurrentWorld.CurrentLevel;
			CurrentWorld.Connect(nameof(World.LevelChanged), this, nameof(OnLevelChanged));
		}
		
		public void OnLevelChanged(Level newLevel, Level previousLevel) {
			CurrentLevel = newLevel;
			_levelChangeAcked = true;
		}

		public override void _Process(float delta) {
			// TODO probably should just use local coordinates for each level. Could have float issues for massive worlds
			var localPos = Position - new Vector2(CurrentLevel.LdtkLevel.WorldX, CurrentLevel.LdtkLevel.WorldY);
			if (localPos.x == 0 || Math.Abs(localPos.x - CurrentLevel.LdtkLevel.PxWid) < float.Epsilon|| localPos.y == 0 ||
			    Math.Abs(localPos.y - CurrentLevel.LdtkLevel.PxHei) < float.Epsilon) {
				if (_inBorderState) {
					return;
				}
				var targetLevel = CurrentLevel.GetNeighbor(Position);
				if (targetLevel >= 0) {
					_levelChangeAcked = false;
					CurrentWorld.SetLevel(targetLevel);
				}
			}
			else if (_inBorderState && _levelChangeAcked) {
				_inBorderState = false;
			}
		}


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
		
		PlayerAnimationState GetAnimationState(Vector2 v) {
			if (v.x == 0 && v.y == 0) {
				return AnimationState.ToIdleState();
			}

			if (v.x > 0) {
				return PlayerAnimationState.WalkRight;
			} else if (v.x < 0) {
				return PlayerAnimationState.WalkLeft;
			} else if (v.y > 0) {
				return PlayerAnimationState.WalkDown;
			}

			return PlayerAnimationState.WalkUp;
		}

		public override void _PhysicsProcess(float delta) {
			var v = GetInput();
			var animState = GetAnimationState(v);
			if (animState != AnimationState) {
				_sprite.Animation = animState.ToString();
				AnimationState = animState;
			}
			PlayerInput = v;
			//Position += v * delta * Speed;
			MoveAndSlide(v * Speed);
		}
	}
}
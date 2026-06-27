using System.Numerics;
using fennecs;

namespace Core;
public enum Shapes {
    Circle,
    Box,
}
public struct ChildOf;

public struct Destroy;
public record struct Position(float X, float Y);
public record struct Velocity(float X, float Y);
public record struct Scale(float Value);
public record struct Radius(float Value);
public record struct Size(float X, float Y);


public static class CoreLib {
    private static float DRAG = 0.9f;
    
    public static World World = new World();
    private static Stream<Position, Velocity> _stream_move =
        World.Stream<Position, Velocity>();
    private static Stream<Destroy> _stream_destroy = 
        World.Stream<Destroy>();

    private static Stream<Position> _stream_enforce_position =
        World.Stream<Position>();

    private static Stream<Velocity> _stream_velocity_to_drag =
        World.Stream<Velocity>();
    
    private static Size _boundary;
    private static void _ApplyDrag(float dt) {
        _stream_velocity_to_drag.For(
            uniform: MathF.Pow(DRAG, dt),
            static (float drag, ref Velocity vel) => {
                vel.X *= drag;
                vel.Y *= drag;
            });
    }
    
    private static void _SystemMove(float dt) {
        _stream_move.For(
            uniform: dt,
            static (float dt, ref Position pos, ref Velocity vel) => {
                pos.X += vel.X * dt;
                pos.Y += vel.Y * dt;
            });
    }

    private static void _SystemEnforcePosition() {
        _stream_enforce_position.For(
            uniform: _boundary,
            static (Size boundary, ref Position pos) => {
                pos.X = Math.Clamp(pos.X, -boundary.X, boundary.X);
                pos.Y = Math.Clamp(pos.Y, -boundary.Y, boundary.Y);
            });
    }

    private static void _SystemDestroy() {
        _stream_destroy.For(
            static (in Entity entity, ref Destroy _) => {
                entity.Despawn();
            });
    }
    
    public static void Update(float dt) {
        _ApplyDrag(dt);
        _SystemMove(dt);
        _SystemEnforcePosition();
    }
    
    public static void UpdateLast(float dt) {
        _SystemDestroy();
    }

    public static void InitMap(float width, float height) {
        _boundary = new Size(width, height);
    }
}

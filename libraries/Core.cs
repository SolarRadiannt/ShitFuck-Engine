using System.Runtime.InteropServices.Swift;
using Raylib_cs;
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
    public static World World = new World();
    
    private static Stream<Position, Velocity> _stream_move =
        World.Stream<Position, Velocity>();
    
    private static Stream<Destroy> _stream_destroy = 
        World.Stream<Destroy>();
    
    private static void _SystemMove(float dt) {
        _stream_move.For(
            uniform: dt,
            static (float dt , ref Position pos, ref Velocity vel) => {
                pos.X += vel.X * dt;
                pos.Y += vel.Y * dt;
            }); 
    }

    private static void _SystemDestroy() {
        _stream_destroy.For(
            static (in Entity entity, ref Destroy _) => {
                entity.Despawn();
            });
    }
    
    public static void Update(float dt) {
        _SystemMove(dt);
    }

    public static void UpdateLast(float dt) {
        _SystemDestroy();
    }
}

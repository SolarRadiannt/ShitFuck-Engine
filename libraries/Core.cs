using Raylib_cs;

namespace Core;

public enum Shapes {
    Circle,
    Box,
}

public struct ChildOf;

public record struct Position(float X, float Y);
public record struct Velocity(float X, float Y);
public record struct Scale(float Value);
public record struct Radius(float Value);

public record struct RenderShape(Shapes Value);
public record struct RenderColor(Color Value);

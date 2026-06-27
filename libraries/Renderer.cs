using System.Numerics;
using Raylib_cs;
using fennecs;
using Core;

namespace Renderer;

public record struct RenderShape(Shapes Value);
public record struct RenderColor(Color Value);

static class RenderLib {
    private static World _world = CoreLib.World;
    private static Camera2D _camera;
    private static Stream<Position, Scale, RenderShape, RenderColor> _stream_render =
        _world.Stream<Position, Scale, RenderShape, RenderColor>();
    private static Stream<RenderShape> _stream_no_scale =
        _world.Query<RenderShape>().Not<Scale>().Stream();
    private static Stream<RenderShape> _stream_no_color =
        _world.Query<RenderShape>().Not<RenderColor>().Stream();
    private static Stream<RenderShape> _stream_no_size =
        _world.Query<RenderShape>().Not<Size>().Stream();
    private static Stream<RenderShape> _stream_no_radius =
        _world.Query<RenderShape>().Not<Radius>().Stream();

    private static void _SystemApplyDefaults() {
        _stream_no_scale.For(
            static (in Entity entity, ref RenderShape _) => {
                entity.Add(new Scale(1));
            });
        _stream_no_color.For(
            static (in Entity entity, ref RenderShape _) => {
                entity.Add(new RenderColor(Color.Red));
            });
        _stream_no_size.For(
            static (in Entity entity, ref RenderShape shape) => {
                if (shape.Value == Shapes.Box) {
                    entity.Add(new Size(5, 5));
                }
            });
        _stream_no_radius.For(
            static (in Entity entity, ref RenderShape shape) => {
                if (shape.Value == Shapes.Circle) {
                    entity.Add(new Radius(5));
                }
            });
    }
    private static void _SystemRenderEntities() {
        _stream_render.For(
            static (in Entity entity, ref Position pos, ref Scale cscale, ref RenderShape shape, ref RenderColor color) => {
                (float x, float y) = (pos.X, pos.Y);
                float scale = cscale.Value;

                switch (shape.Value)
                {
                    case Shapes.Circle:
                        float radius = entity.Ref<Radius>().Value * scale;
                        Raylib.DrawCircleV(new Vector2(x, y), radius, color.Value);
                        break;
                    case Shapes.Box:
                    {
                        var size = entity.Ref<Size>();

                        float scaledWidth = size.X * scale;
                        float scaledHeight = size.Y * scale;

                        float halfX = scaledWidth * 0.5f;
                        float halfY = scaledHeight * 0.5f;

                        Raylib.DrawRectangleV(
                            new Vector2(x - halfX, y - halfY),
                            new Vector2(scaledWidth, scaledHeight),
                            color.Value
                        );
                        break;
                    }
                }
            });
    }
    
    public static void Update(float dt) {
        _SystemApplyDefaults();
        _SystemRenderEntities();
    }

    public static void InitCamera(int screenWidth, int screenHeight) {
        _camera = new Camera2D {
            Target = Vector2.Zero,
            Offset = new Vector2(screenWidth * 0.5f, screenHeight * 0.5f),
            Rotation = 0f,
            Zoom = 1f,
        };
    }

    public static void BeginCamera() => Raylib.BeginMode2D(_camera);
    public static void EndCamera() => Raylib.EndMode2D();
    
    
}
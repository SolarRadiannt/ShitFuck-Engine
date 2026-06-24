using System.Numerics;
using Raylib_cs;
using fennecs;
using Core;

namespace Renderer;

public record struct RenderShape(Shapes Value);
public record struct RenderColor(Color Value);

static class RenderLib {
    private static World _world = CoreLib.World;
    
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

    private static void _SystemRenderDefaults() {
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
                    entity.Add(new Size(10, 10));
                }
            });
        _stream_no_radius.For(
            static (in Entity entity, ref RenderShape shape) => {
                if (shape.Value == Shapes.Circle) {
                    entity.Add(new Radius(10));
                }
            });
    }
    private static void _SystemRenderEntities() {
        _stream_render.For(
            static (in Entity entity, ref Position pos, ref Scale cscale, ref RenderShape shape, ref RenderColor color) => {
                (float x, float y) = (pos.X, pos.Y);
                float scale = cscale.Value;
        
                switch (shape.Value) {
                    case Shapes.Circle:
                        float radius = entity.Ref<Radius>().Value * scale;
                        Raylib.DrawCircleV(new Vector2(x, y), radius, color.Value);
                        break;
                    case Shapes.Box: {
                        var size = entity.Ref<Size>();
                        Raylib.DrawRectangleV(new Vector2(x, y), new Vector2(size.X, size.Y) * scale, color.Value);
                        break;
                    }
                }
            });
    }
    public static void Update(float dt) {
        _SystemRenderDefaults();
        _SystemRenderEntities();
    }
}
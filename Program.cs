using System.Diagnostics;
using fennecs;
using Raylib_cs;
using Core;
using System.Text;

const double SPAWN_DURATION = 0.1;
const int WINDOWS_WIDTH = 800;
const int WINDOWS_HEIGHT = 400;
const string TITLE = "ShitFuck Engine";

var world = new World();

float spawnAccum = 0;

void SystemSpawn(float dt) {
    spawnAccum += dt;
	while (spawnAccum > SPAWN_DURATION) {
        world.Spawn()
            .Add(new Position(100, 100))
            .Add(new Velocity(200, 90))
            .Add(new RenderColor(Color.Red))
            .Add(new RenderShape(Shapes.Circle));

		spawnAccum -= (float)SPAWN_DURATION;
	}
}

var stream_move = world.Stream<Position, Velocity>();
void SystemMove(float dt) {
	stream_move.For(
	uniform: dt,
	static (float dt, ref Position pos, ref Velocity vel) => {
		pos.X += vel.X * dt;
		pos.Y += vel.Y * dt;
	});
}

var stream_bounce = world.Stream<Position, Velocity>();
void SystemBounce()
{
    stream_bounce.For(
    static (ref Position pos, ref Velocity vel) =>
    {
        if (pos.X >= WINDOWS_WIDTH || pos.X <= 0)
        {
            vel.X *= -1;
        }
        if (pos.Y >= WINDOWS_HEIGHT || pos.Y <= 0)
        {
            vel.Y *= -1;
        }
    });
}

var stream_no_scale = world.Query<RenderShape>().Not<Scale>().Stream();
var stream_no_color = world.Query<RenderShape>().Not<RenderColor>().Stream();

var stream_no_size = world.Query<RenderShape>().Not<Size>().Stream();
var stream_no_radius = world.Query<RenderShape>().Not<Radius>().Stream();
void SystemRenderDefaults() {
    stream_no_scale.For(
        static (in Entity, entity, ref RenderShape shape) => {
            entity.Add(new Scale(1));
        });
    stream_no_color.For(
        static (in Entity entity, ref RenderShape shape) => {
            entity.Add(new RenderColor(Color.White));
        });
    
    stream_no_size.For(
        static (in Entity, entity, ref RenderShape shape) => {
            if (shape.Value == Shapes.Box) {
                entity.Add(new Size(50, 50));
            }
        });
    stream_no_radius.For(
        static (in Entity entity, ref RenderShape shape) => {
            if (shape.Value == Shapes.Circle) {
                entity.Add(new Radius(10));
            }
        });
}

var stream_render = world.Stream<Position, Scale, RenderShape, RenderColor>();
void SystemRenderEntities() {
	stream_render.For(
	static (in Entity entity, ref Position pos, ref Scale cscale, ref RenderShape shape, ref RenderColor, color) => {
        (float x, float y) = (pos.X, pos.Y);
        float scale = cscale.Value;
        
        switch (shape.Value) {
            case Shapes.Circle:
                float radius = entity.Get<Radius>().Value * scale;
                Raylib.DrawCircleV(x, y, radius, color.Value);
                break;
            case Shapes.Box: {
                var size = entity.Get<Size>();
                Raylib.DrawRectangleV(x, y, size.X * scale, size.Y * scale, color.Value);
                break;
            }
        }
	});
}

void MainLoop(float dt) {
	SystemSpawn(dt);
	SystemBounce();
	SystemMove(dt);

	Raylib.ClearBackground(Color.White); // Do this ONCE at the start
    Raylib.DrawText("Hello, world!", 12, 12, 20, Color.Black);
    SystemRenderDefaults();
	SystemRenderEntities();
}

void Main() {
	Raylib.InitWindow(WINDOWS_WIDTH	, WINDOWS_HEIGHT, TITLE);

	var stopwatch = new Stopwatch();
	stopwatch.Start();
	double previousTime = 0;

	while (!Raylib.WindowShouldClose()) {
		double currentTime = stopwatch.Elapsed.TotalSeconds;

		float dt = (float)(currentTime - previousTime);
		previousTime = currentTime;

		Raylib.BeginDrawing();
		MainLoop(dt);
		Raylib.EndDrawing();
	}

	Raylib.CloseWindow();
}

Main();

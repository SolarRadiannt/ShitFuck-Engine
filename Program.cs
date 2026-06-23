using System.Diagnostics;
using fennecs;
using Raylib_cs;
using Core;

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
			.Add(new RenderShape(Shapes.Box))
			.Add(new Radius(10));

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
//                               MAKE COLOR OPTIONAL AS WELL!!!  MAKE RADIUS OPTIONAL LATER!!!!
var stream_render = world.Stream<Position, RenderColor, RenderShape, Radius>();
void SystemRenderEntities() {
	stream_render.For(//                               REMOVE THIS LATER                         REMOVE THIS LATER
	static (in Entity entity, ref Position pos, ref RenderColor color, ref RenderShape shape, ref Radius cradius) => {
        (int x, int y) = ((int)pos.X, (int)pos.Y);
        int radius = (int)cradius.Value;
        
        switch (shape.Value) {
            case Shapes.Circle:
                Raylib.DrawCircle(x, y, radius, color.Value);
                break;
            case Shapes.Box: {
                Raylib.DrawRectangle(x, y, radius, radius, color.Value);
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

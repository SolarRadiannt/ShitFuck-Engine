using System.Diagnostics;
using Raylib_cs;
using Core;
using Renderer;

const int WINDOWS_WIDTH = 800;
const int WINDOWS_HEIGHT = 400;

const double SPAWN_DURATION = 0.1;
const string TITLE = "ShitFuck Engine";

var world = CoreLib.World;
var rng = new Random();

float spawnAccum = 0;

void SystemSpawn(float dt) {
    spawnAccum += dt;
	while (spawnAccum > SPAWN_DURATION) {
        world.Spawn()
            .Add(new Position(100, 100))
            .Add(new Velocity(rng.Next(-100, 100), rng.Next(-100, 100)))
            .Add(new RenderShape(Shapes.Box))
            .Add(new Scale(4));

		spawnAccum -= (float)SPAWN_DURATION;
	}
}

var stream_bounce = world.Stream<Position, Velocity>();
void SystemBounce() {
    stream_bounce.For(
    static (ref pos, ref vel) => {
        if (pos.X >= WINDOWS_WIDTH || pos.X <= 0) {
            vel.X *= -1;
        }
        if (pos.Y >= WINDOWS_HEIGHT || pos.Y <= 0) {
            vel.Y *= -1;
        }
    });
}

void DrawText() {
	Raylib.ClearBackground(Color.White);
	Raylib.DrawText("Hello, world!", 12, 12, 20, Color.Black);
}

void MainLoop(float dt) {
	SystemSpawn(dt);
	SystemBounce();
	
	DrawText();

	CoreLib.Update(dt);
	
	RenderLib.Update(dt);
	
    CoreLib.UpdateLast(dt);
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

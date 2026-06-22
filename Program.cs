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
	if (spawnAccum > SPAWN_DURATION) {
		var entity = world.Spawn()
			.Add(new Position(100, 100))
			.Add(new Velocity(200, 90));
		
		spawnAccum -= (float)SPAWN_DURATION;
	} else {
		spawnAccum += dt;
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
void SystemBounce() {
	stream_bounce.For(
	static (ref Position pos, ref Velocity vel) => {
		if (pos.X >= WINDOWS_WIDTH || pos.X <= 0) {
			vel.X *= -1;
		} else if (pos.Y >= WINDOWS_HEIGHT || pos.Y <= 0) {
			vel.Y *= -1;
		}
	});
}

var stream_render = world.Stream<Position>();
void SystemRenderEntities() {
	stream_render.For(
	static (ref Position pos) => {
		Raylib.DrawCircle((int)pos.X, (int)pos.Y, 10, Color.Red);
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
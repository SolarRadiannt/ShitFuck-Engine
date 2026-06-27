using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using Core;
using Renderer;
using fennecs;
using System.Collections.Generic;
using System.Xml;
using System.Runtime.InteropServices;

const string TITLE = "ShitFuck Engine";

const int WINDOWS_WIDTH = 1200;
const int WINDOWS_HEIGHT = 650;

const int HALF_W_WIDTH = WINDOWS_WIDTH / 2;
const int HALF_W_HEIGHT = WINDOWS_HEIGHT / 2;

var rng = new Random();
var world = CoreLib.World;

var stream_bounce = world.Stream<Position, Velocity>();
void SystemBounce() {
	stream_bounce.For(
		static (ref Position pos, ref Velocity vel) => {
			if (pos.X >= HALF_W_WIDTH || pos.X <= -HALF_W_WIDTH) {
				vel.X *= -1;
			}
			if (pos.Y >= HALF_W_HEIGHT  || pos.Y <= -HALF_W_HEIGHT) {
				vel.Y *= -1;
			}
		});
}


void MainLoop(float dt) {
	SystemBounce();
	CoreLib.Update(dt);
    CoreLib.UpdateLast(dt);
}

static void RenderLoop(float dt) {
	Raylib.BeginDrawing();
	Raylib.ClearBackground(Color.Black);
	RenderLib.BeginCamera();
	RenderLib.Update(dt);
	RenderLib.EndCamera();
	Raylib.EndDrawing();
	Raylib.EndDrawing();
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
		
		MainLoop(dt);
		RenderLoop(dt);
	}

	Raylib.CloseWindow();
}

RenderLib.InitCamera(WINDOWS_WIDTH, WINDOWS_HEIGHT);
CoreLib.InitMap(HALF_W_WIDTH, HALF_W_HEIGHT);
Main();
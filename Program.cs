using System.Diagnostics;
using Raylib_cs;
using Core;
using Renderer;

const int WINDOWS_WIDTH = 800;
const int WINDOWS_HEIGHT = 400;

const int HALF_W_WIDTH = WINDOWS_WIDTH / 2;
const int HALF_W_HEIGHT = WINDOWS_HEIGHT / 2;

const string TITLE = "ShitFuck Engine";

void MainLoop(float dt) {
	CoreLib.Update(dt);
	
	RenderLib.BeginCamera();
	RenderLib.Update(dt);
	RenderLib.EndCamera();
	
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

RenderLib.InitCamera(WINDOWS_WIDTH, WINDOWS_HEIGHT);
CoreLib.InitMap(HALF_W_WIDTH, HALF_W_HEIGHT);
Main();

using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using Core;
using Renderer;
using fennecs;

const string TITLE = "ShitFuck Engine";

const int WINDOWS_WIDTH = 800;
const int WINDOWS_HEIGHT = 400;

const int HALF_W_WIDTH = WINDOWS_WIDTH / 2;
const int HALF_W_HEIGHT = WINDOWS_HEIGHT / 2;

const int MAX_CELLS = 400;

var rng = new Random();
var world = CoreLib.World;

var redSpawner = world.Entity()
	.Add(new Position(0, 0))
	.Add(new Velocity(0, 0))
	.Add(new RenderShape(Shapes.Circle))
	.Add<RedCell>();

void SetupCells() {
	redSpawner.Spawn(MAX_CELLS);
	
	world.Stream<Position>().For(
		uniform: rng,
		static (Random rng, ref Position pos) => {
			pos.X = rng.Next(-HALF_W_WIDTH, HALF_W_WIDTH);
			pos.Y = rng.Next(-HALF_W_HEIGHT, HALF_W_HEIGHT);
		});
}

var stream_red_cells = world.Query<Position>()
	.Has<RedCell>()
	.Stream();
var stream_repels_other = world.Stream<Position, Velocity, RedCell>();

Entity getClosestEntity(Entity ownEntity, Vector2 origin) {
	Entity closestEntity = ownEntity;
	float closestDist = float.PositiveInfinity;
	
	foreach (var v in stream_red_cells)
	{
		var entity = v.Item1;
		if (entity == ownEntity) { continue; }
		
		var cpos = v.Item2;
		var pos = new Vector2(cpos.X, cpos.Y);

		var resultant = pos - origin;
		if (resultant.Length() < closestDist) {
			closestEntity = entity;
		}
	}
	
	return closestEntity;
}


void SystemRepelsOther()
{
	stream_repels_other.For(
		(in Entity entity, ref Position pos, ref Velocity vel, ref RedCell _) => {
			var closestEntity = getClosestEntity(entity, new Vector2(pos.X, pos.Y));
			
		});
}

static void MainLoop(float dt) {
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
	
	SetupCells();
	
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


struct RedCell;
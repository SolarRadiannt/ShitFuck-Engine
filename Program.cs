using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using Core;
using Renderer;
using fennecs;

const string TITLE = "ShitFuck Engine";

const int WINDOWS_WIDTH = 1200;
const int WINDOWS_HEIGHT = 650;

const int HALF_W_WIDTH = WINDOWS_WIDTH / 2;
const int HALF_W_HEIGHT = WINDOWS_HEIGHT / 2;

const int MAX_CELLS = 400;
const float EFFECT_DIST = 60;
const float PUSH_FORCE = 60;

/*
 * NEXT STEP:
 * MAKE MORE CELL TYPES THAT
 * MAY ATTRACT MORE/LESS OR REPEL MORE/LESS
 *
 * MAKE A CELL 
 */

var rng = new Random();
var world = CoreLib.World;

void SetupCells() {
	world.Entity()
		.Add(new Position(0, 0))
		.Add(new Velocity(0, 0))
		.Add(new RenderShape(Shapes.Circle))
		.Add<Cell>()
		.Spawn(MAX_CELLS)
		.Dispose();
	
	world.Query<Position>().Has<Cell>().Stream().For(
		uniform: rng,
		static (Random rng, ref Position pos) => {
			pos.X = rng.Next(-HALF_W_WIDTH, HALF_W_WIDTH);
			pos.Y = rng.Next(-HALF_W_HEIGHT, HALF_W_HEIGHT);
		});
}

var stream_cells = world.Query<Position>()
	.Has<Cell>()
	.Stream();
var stream_repels_other = world.Stream<Position, Velocity, Cell>();
ClosestData getClosestEntity(Entity ownEntity, Vector2 origin) {
	var closestEntity = ownEntity;
	float closestDist = float.PositiveInfinity;
	var closestPos = origin;
	
	foreach (var v in stream_cells)
	{
		var entity = v.Item1;
		if (entity == ownEntity) { continue; }
		
		var cpos = v.Item2;
		var pos = new Vector2(cpos.X, cpos.Y);

		var resultant = pos - origin;
		var distance = resultant.Length();
		if (distance < closestDist) {
			closestEntity = entity;
			closestPos = pos;
			closestDist = distance;
		}
	}

	return new ClosestData {
		Entity = closestEntity,
		Position = closestPos,
		Distance = closestDist,
	};
}


void SystemRepelsOther()
{
	stream_repels_other.For(
		(in Entity entity, ref Position pos, ref Velocity vel, ref Cell _) => {
			var mathPos = new Vector2(pos.X, pos.Y);
			var closestData = getClosestEntity(entity, mathPos);
			if (closestData.Distance > EFFECT_DIST) { return; }

			float t = 1 - (closestData.Distance / EFFECT_DIST);
			float force = t * t * PUSH_FORCE;
			
			var dir = Vector2.Normalize(mathPos - closestData.Position);
			vel.X += dir.X * force;
			vel.Y += dir.Y * force;
		});
}

void MainLoop(float dt) {
	SystemRepelsOther();
	
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
		Raylib.ClearBackground(Color.Black);
		MainLoop(dt);
		Raylib.EndDrawing();
	}

	Raylib.CloseWindow();
}

RenderLib.InitCamera(WINDOWS_WIDTH, WINDOWS_HEIGHT);
CoreLib.InitMap(HALF_W_WIDTH, HALF_W_HEIGHT);
Main();


struct Cell;

struct ClosestData {
	public Entity Entity;
	public Vector2 Position;
	public float Distance;
}
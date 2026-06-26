using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using Core;
using Renderer;
using fennecs;
using System.Collections.Generic;

const string TITLE = "ShitFuck Engine";

const int WINDOWS_WIDTH = 1200;
const int WINDOWS_HEIGHT = 650;

const int HALF_W_WIDTH = WINDOWS_WIDTH / 2;
const int HALF_W_HEIGHT = WINDOWS_HEIGHT / 2;

const int MAX_CELLS = 400;
const float EFFECT_DIST = 100;
const float PUSH_FORCE = 200;

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

List<FoundData> getCellsFromRadius(Vector2 origin, float radius, Entity ownEntity) {
	var found = new List<FoundData>();

	foreach (var v in stream_cells) {
		var entity = v.Item1;
		if (entity  == ownEntity) { continue; }

		var cpos = v.Item2;
		var pos = new Vector2(cpos.X, cpos.Y);

		var resultant = origin - pos;
		var distance = resultant.Length();
		if (distance <= radius) {
			found.Add(new FoundData {
				Entity = entity,
				Position = pos,
				Distance = distance,
				Resultant = resultant,
			});
		}
	}

	return found;
} 


var stream_repels_other = world.Stream<Position, Velocity, Cell>();
void SystemRepelsOther(float dt) {
	stream_repels_other.For(
		uniform: dt,
		(float dt, in Entity entity, ref Position pos, ref Velocity vel, ref Cell _) => {
			var mathPos = new Vector2(pos.X, pos.Y);

			var neighbors = getCellsFromRadius(mathPos, EFFECT_DIST, entity);
			foreach (var data in neighbors) {
				float t = 1 - (data.Distance / EFFECT_DIST);
				float force = t * t * PUSH_FORCE;
				
				var dir = Vector2.Normalize(data.Position - mathPos);
				ref var otherVel = ref data.Entity.Ref<Velocity>();
				
				otherVel.X += dir.X * force;
				otherVel.Y += dir.Y * force;
			}
		});
}

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
	SystemRepelsOther(dt);
	
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

struct FoundData {
	public Entity Entity;
	public Vector2 Position;
	public float Distance;
	public Vector2 Resultant;
}
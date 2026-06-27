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

const int RED_CELLS = 200;
const int YELLOW_CELLS = 200;
const int BLUE_CELLS = 200;

var rng = new Random();
var world = CoreLib.World;

void spawnCells(CellType type, int amount) {
	var color = type switch {
		CellType.Red => Color.Red,
		CellType.Yellow => Color.Yellow,
		CellType.Blue => Color.Blue,
	};
	
	world.Entity()
		.Add(new Position(0, 0))
		.Add(new Velocity(0, 0))
		.Add(new RenderShape(Shapes.Circle))
		.Add(new RenderColor(color))
		.Add(new Cell(type))
		.Spawn(amount)
		.Dispose();
}

void SetupCells() {
	spawnCells(CellType.Red, RED_CELLS);
	spawnCells(CellType.Yellow, YELLOW_CELLS);
	spawnCells(CellType.Blue, BLUE_CELLS);
	
	world.Query<Position>().Has<Cell>().Stream().For(
		uniform: rng,
		static (Random rng, ref Position pos) => {
			pos.X = rng.Next(-HALF_W_WIDTH, HALF_W_WIDTH);
			pos.Y = rng.Next(-HALF_W_HEIGHT, HALF_W_HEIGHT);
		});
}

static float getForce(float distance, float maxDistance, float force) {
	float t = 1 - (distance / maxDistance);
	return t * t * force;
}

static void applyForce(Entity entity, float force, Vector2 dir) {
	ref var vel = ref entity.Ref<Velocity>();
	vel.X += dir.X * force;
	vel.Y += dir.Y * force;
}

var stream_sim_query = world.Stream<Position, Cell>();
void SystemApplyForces(float dt) {
	stream_sim_query.For(
		uniform: dt,
		static (float dt, in Entity entity, ref Position pos, ref Cell _) => {
			var mathPos = new Vector2(pos.X, pos.Y);
			
			foreach (var v in CoreLib.World.Query<Position, Velocity>().Has<Cell>().Stream()) {
				var otherEntity = v.Item1;
				if (entity == otherEntity) continue;
				
				var otherPos = new Vector2(v.Item2.X, v.Item2.Y);
				var otherVel = v.Item3;
				
				var behavior = CellsInteractions.GetBehavior(entity, otherEntity);
				var resultant = mathPos - otherPos;
				var distance = resultant.Length();
				
				if (distance <= behavior.Distance) {
					var repelResult = resultant;
					var dir = Vector2.Normalize(repelResult);
					
					var force = getForce(distance, behavior.Distance, behavior.Force) * dt;
					applyForce(otherEntity, force, dir);
				}
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
	SystemApplyForces(dt);
	SystemBounce();
	CoreLib.Update(dt);
    CoreLib.UpdateLast(dt);
}

void RenderLoop(float dt) {
	Raylib.BeginDrawing();
	Raylib.ClearBackground(Color.Black);
	RenderLib.BeginCamera();
	RenderLib.Update(dt);
	RenderLib.EndCamera();
	Raylib.EndDrawing();
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
		
		MainLoop(dt);
		RenderLoop(dt);
	}

	Raylib.CloseWindow();
}

RenderLib.InitCamera(WINDOWS_WIDTH, WINDOWS_HEIGHT);
CoreLib.InitMap(HALF_W_WIDTH, HALF_W_HEIGHT);
Main();

enum CellType {
	Red,
	Yellow,
	Blue,
};
record struct Cell(CellType Type);
public record struct CellBehavior(
    float Force,
	float Distance
);
public static class CellsInteractions {
	private static readonly Random _rng = new();
	private static float _rval(){
		return _rng.Next(-100, 100);
	}
	private static readonly Dictionary<(CellType, CellType), CellBehavior> _behaviors = new() {
		[(CellType.Red, CellType.Red)] = new(_rval(), _rval()),
		[(CellType.Red, CellType.Yellow)] = new(_rval(), _rval()),
		[(CellType.Red, CellType.Blue)] = new(_rval(), _rval()),
		
		[(CellType.Blue, CellType.Blue)] = new(_rval(), _rval()),
		[(CellType.Blue, CellType.Red)] = new(_rval(), _rval()),
		[(CellType.Blue, CellType.Yellow)] = new(_rval(), _rval()),
		
		[(CellType.Yellow, CellType.Yellow)] = new(_rval(), _rval()),
		[(CellType.Yellow, CellType.Blue)] = new(_rval(), _rval()),
		[(CellType.Yellow, CellType.Red)] = new(_rval(), _rval()),
	};
	
	public static CellBehavior GetBehavior(Entity entity, Entity otherEntity) {
		return _behaviors[(
			entity.Ref<Cell>().Type,
			otherEntity.Ref<Cell>().Type
		)];
	}
}
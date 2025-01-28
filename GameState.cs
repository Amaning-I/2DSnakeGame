using System;
using System.Collections.Generic;
using System.Linq;

namespace Clone
{
    public class GameState
    {
        public GameState(GridValue[,] grid)
        {
            Grid = grid;
        }

        public int Rows { get; }
        public int Cols { get; }
        public GridValue[,] Grid { get; }
        public Direction Dir { get; private set; }
        public int Score { get; private set; }
        public bool GameOver { get; private set; }

        private bool IsSpedUp { get; set; } = false; // Tracks whether the snake is sped up --> default is false
        public bool IsGodMode { get; set; } = false; // Tracks if god mode is active 

        public void ToggleGodMode()
        {
            IsGodMode = !IsGodMode;
        }
        public bool IsDashing { get; private set; }
        private int dashRemainingSteps;
        private int growthPending;

        private readonly LinkedList<Direction> dirChanges = new LinkedList<Direction>();
        private readonly LinkedList<Position> snakePositions = new LinkedList<Position>();
        private readonly Random random = new Random();

        // Enum to represent different food types
        public enum FoodType { Regular, Special, Rare }
        public FoodType CurrentFoodType { get; private set; } = FoodType.Regular;



        public GameState(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Grid = new GridValue[rows, cols];
            Dir = Direction.Right;

            AddSnake();
            AddFood();

            // Spawn monsters
            AddMonsters(GridValue.Monster, 4); // 4 generic monsters
            AddMonsters(GridValue.Dragonfly, 4); // 4 vampires
            AddMonsters(GridValue.Knight, 4); // 4 knights
        }

        private void AddSnake()
        {
            int r = Rows / 2;

            for (int c = 1; c <= 3; c++)
            {
                Grid[r, c] = GridValue.Snake;
                snakePositions.AddFirst(new Position(r, c));
            }
        }

        private IEnumerable<Position> EmptyPositions()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (Grid[r, c] == GridValue.Empty)
                    {
                        yield return new Position(r, c);
                    }
                }
            }
        }

        private void AddFood()
        {
            UpdateFoodType();

            List<Position> empty = new List<Position>(EmptyPositions());
            if (empty.Count == 0)
            {
                return;
            }

            Position pos = empty[random.Next(empty.Count)];
            switch (CurrentFoodType)
            {
                case FoodType.Regular:
                    Grid[pos.Row, pos.Column] = GridValue.Food;
                    break;
                case FoodType.Special:
                    Grid[pos.Row, pos.Column] = GridValue.SpecialFood;
                    break;
                case FoodType.Rare:
                    Grid[pos.Row, pos.Column] = GridValue.RareFood;
                    break;
            }
        }

        private void UpdateFoodType()
        {
            FoodType previousType = CurrentFoodType;
            if (Score >= 5)
            {
                CurrentFoodType = FoodType.Rare;
            }
            else if (Score >= 3)
            {
                CurrentFoodType = FoodType.Special;
            }
            else if (Score >= 1)
            {
                CurrentFoodType = FoodType.Regular;
            }

            if (previousType != CurrentFoodType)
            {
                Console.WriteLine($"Food type changed from {previousType} to {CurrentFoodType}.");
            }
        }

        private void AddMonsters(GridValue monsterType, int count)
        {
            List<Position> emptyPositions = new List<Position>(EmptyPositions());

            for (int i = 0; i < count; i++)
            {
                if (emptyPositions.Count == 0)
                {
                    break;
                }

                Position pos = emptyPositions[random.Next(emptyPositions.Count)];
                emptyPositions.Remove(pos); // Avoid playing multiple monsters in the same spot

                Grid[pos.Row, pos.Column] = monsterType;
            }
        }

        public Position HeadPosition()
        {
            return snakePositions.First.Value;
        }

        public Position TailPosition()
        {
            return snakePositions.Last.Value;
        }

        public IEnumerable<Position> SnakePositions()
        {
            return snakePositions;
        }

        private void AddHead(Position pos)
        {
            snakePositions.AddFirst(pos);
            Grid[pos.Row, pos.Column] = GridValue.Snake;
        }

        private void RemoveTail()
        {
            Position tail = snakePositions.Last.Value;
            Grid[tail.Row, tail.Column] = GridValue.Empty;
            snakePositions.RemoveLast();
        }

        private Direction GetLastDirection()
        {
            return dirChanges.Count == 0 ? Dir : dirChanges.Last.Value;
        }

        private bool CanChangeDirection(Direction newDir)
        {
            if (dirChanges.Count == 2)
            {
                return false;
            }

            Direction lastDir = GetLastDirection();
            return newDir != lastDir && newDir != lastDir.Opposite();
        }

        public void ChangeDirection(Direction dir)
        {
            if (CanChangeDirection(dir))
            {
                dirChanges.AddLast(dir);
            }
        }

        private bool OutsideGrid(Position pos)
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Column < 0 || pos.Column >= Cols;
        }

        private GridValue WillHit(Position newHeadPos)
        {
            if (OutsideGrid(newHeadPos))
            {
                return GridValue.Outside;
            }

            if (newHeadPos == TailPosition())
            {
                return Grid[newHeadPos.Row, newHeadPos.Column];
            }

            return Grid[newHeadPos.Row, newHeadPos.Column];
        }

        public void Move()
        {
            if (dirChanges.Count > 0)
            {
                Dir = dirChanges.First.Value;
                dirChanges.RemoveFirst();
            }

            int steps = IsDashing ? 2 : 1; // Dash moves 2 steps at once
            bool hitSomething = false;

            for (int i = 0; i < steps && !hitSomething; i++)
            {
                Position newHeadPos = HeadPosition().Translate(Dir);
                GridValue hit = WillHit(newHeadPos);

                if (hit == GridValue.Outside || (!IsGodMode && hit == GridValue.Snake))
                {
                    GameOver = true;
                    return;
                }
                else if (IsEnemy(hit) && !IsGodMode && !IsDashing)
                {
                    HandleEnemyCollision(hit);
                    GameOver = true;
                    return;
                }
                else if (IsEnemy(hit) && (IsGodMode || IsDashing))
                {
                    HandleEnemyKill(newHeadPos, hit);
                }
                else if (hit == GridValue.Empty || IsEnemy(hit))
                {
                    RegularMovement(newHeadPos);
                }
                else if (IsFood(hit))
                {
                    HandleFoodConsumption(newHeadPos);
                }

                hitSomething = hit != GridValue.Empty && !IsFood(hit);
            }

            if (IsDashing)
            {
                dashRemainingSteps--;
                if (dashRemainingSteps <= 0)
                {
                    IsDashing = false;
                }
            }
        }

        private void HandleEnemyCollision(GridValue hit)
        {
            Console.WriteLine($"Game Over! Collided with {hit}.");
            GameOver = true; // Ends the game
        }

        // NEW: Helper methods
        private bool IsEnemy(GridValue value) =>
            value == GridValue.Monster || value == GridValue.Dragonfly || value == GridValue.Knight;

        private bool IsFood(GridValue value) =>
            value == GridValue.Food || value == GridValue.SpecialFood || value == GridValue.RareFood;

        private void HandleEnemyKill(Position position, GridValue enemyType)
        {
            Console.WriteLine($"Destroyed {enemyType}!");
            Grid[position.Row, position.Column] = GridValue.Empty;
            AddEnemy(enemyType); // Respawn enemy
            AddHead(position);
            Score += (int)enemyType; // Assign score values to enum if needed
        }

        private void HandleFoodConsumption(Position newHeadPos)
        {
            AddHead(newHeadPos);
            Score++;
            growthPending += IsSpedUp ? 2 : 1; // Grow more when sped up
            AddFood();
        }

        private void RegularMovement(Position newHeadPos)
        {
            if (growthPending > 0)
            {
                growthPending--;
            }
            else
            {
                RemoveTail();
            }
            AddHead(newHeadPos);
        }

        // NEW: Speed and dash controls
        public void ToggleSpeed()
        {
            IsSpedUp = !IsSpedUp;
            growthPending += IsSpedUp ? 1 : 0; // Immediate growth when speeding up
        }

        public void ActivateDash()
        {
            if (!IsDashing)
            {
                IsDashing = true;
                dashRemainingSteps = 5; // Dash lasts 5 moves
            }
        }

        // Modified AddEnemy for respawning
        private void AddEnemy(GridValue enemyType)
        {
            List<Position> empty = new List<Position>(EmptyPositions());
            if (empty.Count == 0) return;

            Position pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Column] = enemyType;
        }

        // ... (rest of existing methods)
    }
}



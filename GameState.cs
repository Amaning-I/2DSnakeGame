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
            AddMonsters(GridValue.Monster, 4); // 3 generic monsters
            AddMonsters(GridValue.Dragonfly, 4); // 3 vampires
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
                emptyPositions.Remove(pos);

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

            Position newHeadPos = HeadPosition().Translate(Dir);
            GridValue hit = WillHit(newHeadPos);

            if (hit == GridValue.Outside || hit == GridValue.Snake)
            {
                GameOver = true;
            }
            else if (hit == GridValue.Monster)
            {
                Console.WriteLine("You died to a Monster!");
                GameOver = true;
            }
            else if (hit == GridValue.Dragonfly)
            {
                Console.WriteLine("You got wrecked by a Vampire!");
                GameOver = true;
            }
            else if (hit == GridValue.Knight)
            {
                Console.WriteLine("You were slain by a Knight!");
                GameOver = true;
            }
            else if (hit == GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPos);
            }
            else if (hit == GridValue.Food || hit == GridValue.SpecialFood || hit == GridValue.RareFood)
            {
                AddHead(newHeadPos);
                Score++;
                AddFood();
            }
        }
    }
}


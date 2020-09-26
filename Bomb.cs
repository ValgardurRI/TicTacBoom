using System.Collections.Generic;

namespace TicTacBoom
{
    public interface IExplodable
    {
        IEnumerable<Position> Explode(Position bombPosition, int width, int height);
    }

    public class PlusBomb : IExplodable
    {
        public IEnumerable<Position> Explode(Position bombPosition, int width, int height)
        {
            var explodedSpots = new List<Position>();
            explodedSpots.Add(bombPosition);
            
            int step = bombPosition.x - 1;
            while(step >= 0)
            {
                explodedSpots.Add(new Position{x = step, y = bombPosition.y});
                step -= 1;
            }
            step = bombPosition.x + 1;
            while(step < width)
            {
                explodedSpots.Add(new Position{x = step, y = bombPosition.y});
                step += 1;
            }
            step = bombPosition.y - 1;
            while(step >= 0)
            {
                explodedSpots.Add(new Position{x = bombPosition.x, y = step});
                step -= 1;
            }
            step = bombPosition.y + 1;
            while(step < height)
            {
                explodedSpots.Add(new Position{x = bombPosition.x, y = step});
                step += 1;
            }
            return explodedSpots;
        }
    }

    public class BigBomb : IExplodable
    {
        public IEnumerable<Position> Explode(Position bombPosition, int width, int height)
        {
            var explodedSpots = new List<Position>();
            for(int y = -1; y <= 1; y++)
            {
                for(int x = -1; x <= 1; x++)
                {
                    int relativeX = bombPosition.x + x;
                    int relativeY = bombPosition.y + y;
                    if((relativeX < width && relativeX >= 0) && (relativeY < height && relativeY >= 0))
                    {
                        explodedSpots.Add(new Position{x = relativeX, y = relativeY});
                    }
                }
            }
            return explodedSpots;
        }
    }
}
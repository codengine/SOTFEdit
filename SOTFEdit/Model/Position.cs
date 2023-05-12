using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using SOTFEdit.ViewModel;

namespace SOTFEdit.Model;

public class Position : ObservableObject
{
    private readonly float _y;

    public Position(float x, float y, float z)
    {
        X = x;
        _y = y;
        Z = z;
    }

    [JsonIgnore]
    public Area Area { get; init; } = AreaMaskManager.Surface;

    private float YOffset { get; set; }

    [JsonProperty("x")]
    public float X { get; init; }

    [JsonProperty("y")]
    public float Y => _y + YOffset;

    [JsonProperty("z")]
    public float Z { get; init; }

    [JsonIgnore]
    public string PrintableShort => $"X: {X:F2}, Y: {Y:F2}, Z: {Z:F2}";

    [JsonIgnore]
    public string Printable => $"X: {X}, Y: {Y}, Z: {Z}";

    [JsonIgnore]
    public float Rotation { get; set; }


    public Position WithoutOffset()
    {
        return WithoutOffset(Area);
    }

    public Position WithoutOffset(Area area)
    {
        return new Position(X, _y, Z)
        {
            Area = area
        };
    }

    public Position WithYOffset(int yOffset)
    {
        var newPos = WithoutOffset();
        newPos.YOffset = yOffset;
        return newPos;
    }

    private bool Equals(Position other)
    {
        return _y.Equals(other._y) && X.Equals(other.X) && Z.Equals(other.Z) && Area.Equals(other.Area);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((Position)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_y, X, Z, Area);
    }

    public List<Tuple<float, float>> DistributeCoordinates(int count, int space, SpawnPattern spawnPattern)
    {
        switch (spawnPattern)
        {
            case SpawnPattern.Cross:
                return DistributeCoordinatesInCross(count, space);
            case SpawnPattern.VerticalLine:
                return DistributeCoordinatesInLine(count, space, LineOrientation.Vertical);
            case SpawnPattern.HorizontalLine:
                return DistributeCoordinatesInLine(count, space, LineOrientation.Horizontal);
            case SpawnPattern.Random:
                return DistributeCoordinatesRandomly(count, space);
            case SpawnPattern.Rectangle:
                return DistributeCoordinatesInRectangle(count, space);
            case SpawnPattern.Grid:
            default:
                return DistributeCoordinatesInGrid(count, space);
        }
    }

    private List<Tuple<float, float>> DistributeCoordinatesInGrid(int count, int space)
    {
        var newCoordinates = new List<Tuple<float, float>> { Tuple.Create(X, Z) };

        var currentCount = 1;
        var currentLevel = 0;
        var currentDirection = 0;

        float[] directionX = { 1, 0, -1, 0 };
        float[] directionY = { 0, -1, 0, 1 };

        var curX = X;
        var curZ = Z;

        while (currentCount < count)
        {
            currentLevel++;
            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < currentLevel && currentCount < count; j++)
                {
                    curX += directionX[currentDirection] * space;
                    curZ += directionY[currentDirection] * space;

                    newCoordinates.Add(Tuple.Create(curX, curZ));
                    currentCount++;
                }

                currentDirection = (currentDirection + 1) % 4;
            }
        }

        return newCoordinates;
    }

    private List<Tuple<float, float>> DistributeCoordinatesRandomly(int count, int space)
    {
        var newCoordinates = new List<Tuple<float, float>> { Tuple.Create(X, Z) };
        var random = new Random();

        for (var i = 1; i < count; i++)
        {
            var angle = (float)(random.NextDouble() * 2 * Math.PI);
            var radius = (float)(random.NextDouble() * space);

            var newX = X + radius * (float)Math.Cos(angle);
            var newY = Z + radius * (float)Math.Sin(angle);

            newCoordinates.Add(Tuple.Create(newX, newY));
        }

        return newCoordinates;
    }

    private List<Tuple<float, float>> DistributeCoordinatesInLine(int count, int space, LineOrientation orientation)
    {
        var newCoordinates = new List<Tuple<float, float>> { Tuple.Create(X, Z) };

        var halfCount = count / 2;

        for (var i = 1; i <= halfCount; i++)
        {
            float newX, newY;

            if (orientation == LineOrientation.Horizontal)
            {
                newX = X + i * space;
                newY = Z;
                newCoordinates.Add(Tuple.Create(newX, newY));

                newX = X - i * space;
                newCoordinates.Add(Tuple.Create(newX, newY));
            }
            else
            {
                newX = X;
                newY = Z + i * space;
                newCoordinates.Add(Tuple.Create(newX, newY));

                newY = Z - i * space;
                newCoordinates.Add(Tuple.Create(newX, newY));
            }
        }

        if (count % 2 != 0)
        {
            float newX, newY;

            if (orientation == LineOrientation.Horizontal)
            {
                newX = X + (halfCount + 1) * space;
                newY = Z;
            }
            else
            {
                newX = X;
                newY = Z + (halfCount + 1) * space;
            }

            newCoordinates.Add(Tuple.Create(newX, newY));
        }

        return newCoordinates;
    }

    private List<Tuple<float, float>> DistributeCoordinatesInCross(int count, int space)
    {
        var newCoordinates = new List<Tuple<float, float>> { Tuple.Create(X, Z) };
        var halfCount = count / 4;

        for (var i = 1; i <= halfCount; i++)
        {
            // Right
            var newX = X + i * space;
            var newY = Z;
            newCoordinates.Add(Tuple.Create(newX, newY));

            // Left
            newX = X - i * space;
            newCoordinates.Add(Tuple.Create(newX, newY));

            // Top
            newX = X;
            newY = Z + i * space;
            newCoordinates.Add(Tuple.Create(newX, newY));

            // Bottom
            newY = Z - i * space;
            newCoordinates.Add(Tuple.Create(newX, newY));
        }

        if (count % 4 != 0)
        {
            var extraCount = count % 4;

            for (var i = 0; i < extraCount; i++)
            {
                float newX, newY;

                switch (i)
                {
                    case 0:
                        newX = X + (halfCount + 1) * space;
                        newY = Z;
                        break;
                    case 1:
                        newX = X - (halfCount + 1) * space;
                        newY = Z;
                        break;
                    default:
                        newX = X;
                        newY = Z + (halfCount + 1) * space;
                        break;
                }

                newCoordinates.Add(Tuple.Create(newX, newY));
            }
        }

        return newCoordinates;
    }

    private List<Tuple<float, float>> DistributeCoordinatesInRectangle(int count, int space)
    {
        var newCoordinates = new List<Tuple<float, float>>();

        var side = (int)Math.Ceiling(Math.Sqrt(count / 4.0)) * 2;
        var totalPoints = side * 4 - 4;

        while (totalPoints < count)
        {
            side += 2;
            totalPoints = side * 4 - 4;
        }

        var excessPoints = totalPoints - count;
        var horizontalPoints = side - excessPoints / 2;
        var verticalPoints = side - 2 - (excessPoints - excessPoints / 2);

        var adjustedSpace = (count - 4) * space / (float)(4 * (side - 1) - 4);

        var startX = X - (horizontalPoints - 1) * adjustedSpace / 2f;
        var startZ = Z - (verticalPoints - 1) * adjustedSpace / 2f;

        // Top row
        for (var i = 0; i < horizontalPoints; i++)
        {
            var newX = startX + i * adjustedSpace;
            newCoordinates.Add(Tuple.Create(newX, startZ));
        }

        // Right column
        for (var i = 1; i < verticalPoints; i++)
        {
            var newX = startX + (horizontalPoints - 1) * adjustedSpace;
            var newY = startZ + i * adjustedSpace;
            newCoordinates.Add(Tuple.Create(newX, newY));
        }

        // Bottom row
        for (var i = 0; i < horizontalPoints; i++)
        {
            var newX = startX + (horizontalPoints - 1 - i) * adjustedSpace;
            var newY = startZ + (verticalPoints - 1) * adjustedSpace;
            newCoordinates.Add(Tuple.Create(newX, newY));
        }

        // Left column
        for (var i = 1; i < verticalPoints - 1; i++)
        {
            var newY = startZ + (verticalPoints - 1 - i) * adjustedSpace;
            newCoordinates.Add(Tuple.Create(startX, newY));
        }

        return newCoordinates;
    }

    private enum LineOrientation
    {
        Horizontal,
        Vertical
    }
}
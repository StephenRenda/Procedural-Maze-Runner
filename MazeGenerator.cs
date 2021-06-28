using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Flags]
public enum WallState
{   // Binary
    // 0000 -> no walls
    // 1111 -> left, right, up down
    LEFT = 1, // 0001
    RIGHT = 2,// 0010
    UP = 4,   // 0100
    DOWN = 8, // 1000

    VISITED = 128, // 1000 0000
}

public struct Position
{
    public int X;
    public int Y;
}

public struct Neighbour
{
    public Position Position;
    public WallState SharedWall;
}

public class MazeGenerator
{
    // WallState wallState = WallState.LEFT | WallState.RIGHT; -> 0011
    // wallState |= WallState.UP; -> 0111
    // wallState &= WallState.RIGHT; -> 0101

    private static WallState GetOppositeWall(WallState wall)
    {
        switch (wall)
        {
            case WallState.RIGHT: return WallState.LEFT;
            case WallState.LEFT: return WallState.RIGHT;
            case WallState.UP: return WallState.DOWN;
            case WallState.DOWN: return WallState.UP;
            default: return WallState.LEFT;
        }
    }
    private static WallState[,] ApplyRecursiveBacktracker(WallState[,] maze, int width, int height)
    {
        var rnd = new System.Random();
        var positionStack = new Stack<Position>();
        var position = new Position { X = rnd.Next(0,width), Y = rnd.Next(0,height)};

        maze[position.X, position.Y] |= WallState.VISITED; // 1000 1111
        positionStack.Push(position);

        while (positionStack.Count > 0)
        {
            // Start at current node
            var current = positionStack.Pop();

            // Find the neighbours
            var neighbours = GetUnvisitedNeighbours(current, maze, width, height);

            if(neighbours.Count > 0)
            {
                positionStack.Push(current);

                // Choose a random neighbour
                var randIndex = rnd.Next(0,neighbours.Count);
                var randomNeighbour = neighbours[randIndex];

                //  Remove walls
                var nPosition = randomNeighbour.Position;
                maze[current.X, current.Y] &= ~randomNeighbour.SharedWall;
                maze[nPosition.X, nPosition.Y] &= ~GetOppositeWall(randomNeighbour.SharedWall);

                maze[nPosition.X, nPosition.Y] |= WallState.VISITED;
                positionStack.Push(nPosition);
            }
        }
        return maze;
    }

    private static List<Neighbour> GetUnvisitedNeighbours(Position p, WallState[,] maze, int width, int height)
    {
        var list = new List<Neighbour>();
        if(p.X > 0 ) //LEFT
        {
            if(!maze[p.X -1,p.Y].HasFlag(WallState.VISITED))
            {
                list.Add(new Neighbour
                            {
                                Position = new Position
                                {
                                    X = p.X -1,
                                    Y = p.Y
                                },
                                SharedWall =WallState.LEFT
                            });
            }
        }
        if(p.Y > 0 ) //DOWN
        {
            if(!maze[p.X,p.Y -1].HasFlag(WallState.VISITED))
            {
                list.Add(new Neighbour
                            {
                                Position = new Position
                                {
                                    X = p.X,
                                    Y = p.Y - 1
                                },
                                SharedWall =WallState.DOWN
                            });
            }
        }
        if(p.Y < height -1 ) //UP
        {
            if(!maze[p.X,p.Y+1].HasFlag(WallState.VISITED))
            {
                list.Add(new Neighbour
                            {
                                Position = new Position
                                {
                                    X = p.X,
                                    Y = p.Y + 1
                                },
                                SharedWall =WallState.UP
                            });
            }
        }
        if(p.X < width -1 ) //RIGHT
        {
            if(!maze[p.X + 1,p.Y].HasFlag(WallState.VISITED))
            {
                list.Add(new Neighbour
                            {
                                Position = new Position
                                {
                                    X = p.X + 1,
                                    Y = p.Y
                                },
                                SharedWall =WallState.RIGHT
                            });
            }
        }
        return list;
    }

    public static WallState[,] Generate(int width, int height)
    {
        WallState[,] maze = new WallState[width, height];
        WallState initial = WallState.RIGHT | WallState.LEFT | WallState.UP | WallState.DOWN; // 1111
        for (int i=0; i < width; i++)
        {
            for (int j=0; j < height; j++)
            {
                maze[i, j] = initial;
                // maze[i,j].HasFlag(WallState.RIGHT); returns a bool
            }
        }
        return ApplyRecursiveBacktracker(maze, width, height);
    }
}

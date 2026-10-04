using System;
using System.Collections.Generic;
using Puzzle.Core.Gameplay.Data;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using Puzzle.Core.Level;

namespace Puzzle.Variants.Match3.Logic
{
    /// <summary>
    /// Factory for generating LevelData configurations for Match-3 puzzle sessions.
    /// Provides deterministic test boards and clean randomized boards without pre-existing matches.
    /// </summary>
    public static class Match3LevelFactory
    {
        private static readonly PieceType[] Palette = new[]
        {
            PieceType.ColorA,
            PieceType.ColorB,
            PieceType.ColorC,
            PieceType.ColorD
        };

        public static LevelData CreateSampleLevel(
            LevelId id,
            int width = 6,
            int height = 6,
            int moves = 20,
            int targetScore = 1000,
            int seed = 777)
        {
            var data = new LevelData(id, width, height, moves, targetScore);
            var rng = new Random(seed);
            int pieceId = 1;

            var grid = new PieceType[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Filter out colors that would cause an immediate 3-in-a-row match
                    var forbidden = new HashSet<PieceType>();

                    if (x >= 2 && grid[x - 1, y] == grid[x - 2, y])
                    {
                        forbidden.Add(grid[x - 1, y]);
                    }

                    if (y >= 2 && grid[x, y - 1] == grid[x, y - 2])
                    {
                        forbidden.Add(grid[x, y - 1]);
                    }

                    var validTypes = new List<PieceType>();
                    foreach (var type in Palette)
                    {
                        if (!forbidden.Contains(type))
                        {
                            validTypes.Add(type);
                        }
                    }

                    var chosenType = validTypes.Count > 0
                        ? validTypes[rng.Next(validTypes.Count)]
                        : Palette[rng.Next(Palette.Length)];

                    grid[x, y] = chosenType;
                    data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(x, y), chosenType, pieceId++));
                }
            }

            return data;
        }

        public static LevelData CreateDeterministicLevel(
            LevelId id,
            int width = 5,
            int height = 5,
            int moves = 10,
            int targetScore = 500)
        {
            var data = new LevelData(id, width, height, moves, targetScore);
            int pieceId = 1;

            // Known layout:
            // Row 0: ColorA, ColorB, ColorA, ColorC, ColorD
            // Row 1: ColorB, ColorA, ColorD, ColorB, ColorC
            // Row 2: ColorC, ColorC, ColorD, ColorD, ColorA
            // Row 3: ColorD, ColorD, ColorC, ColorC, ColorB
            // Row 4: ColorA, ColorB, ColorC, ColorD, ColorA
            // Notice: Swapping (1,0)[ColorB] with (1,1)[ColorA] creates a horizontal match of ColorA at (0,0), (1,0), (2,0)!
            PieceType[,] layout = new PieceType[5, 5]
            {
                { PieceType.ColorA, PieceType.ColorB, PieceType.ColorA, PieceType.ColorC, PieceType.ColorD },
                { PieceType.ColorB, PieceType.ColorA, PieceType.ColorD, PieceType.ColorB, PieceType.ColorC },
                { PieceType.ColorC, PieceType.ColorC, PieceType.ColorD, PieceType.ColorD, PieceType.ColorA },
                { PieceType.ColorD, PieceType.ColorD, PieceType.ColorC, PieceType.ColorC, PieceType.ColorB },
                { PieceType.ColorA, PieceType.ColorB, PieceType.ColorC, PieceType.ColorD, PieceType.ColorA }
            };

            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(x, y), layout[y, x], pieceId++));
                }
            }

            return data;
        }
    }
}

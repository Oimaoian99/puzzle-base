using System;
using System.Collections.Generic;
using Puzzle.Core.Gameplay.Data;
using Puzzle.Core.Gameplay.Grid;
using Puzzle.Core.Gameplay.Pieces;
using Puzzle.Core.Level;

namespace Puzzle.Variants.Link.Logic
{
    /// <summary>
    /// Factory for generating pre-configured LevelData configurations for Link/Line puzzle sessions.
    /// Supports deterministic seed-based layouts and predefined test configurations.
    /// </summary>
    public static class LinkLevelFactory
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
            int moves = 15,
            int targetScore = 1000,
            int seed = 42)
        {
            var data = new LevelData(id, width, height, moves, targetScore);
            var rng = new Random(seed);
            int pieceId = 1;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var type = Palette[rng.Next(Palette.Length)];
                    data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(x, y), type, pieceId++));
                }
            }

            return data;
        }

        public static LevelData CreateDeterministicLevel(
            LevelId id,
            int width = 4,
            int height = 4,
            int moves = 10,
            int targetScore = 500)
        {
            var data = new LevelData(id, width, height, moves, targetScore);
            int pieceId = 1;

            // Row 0: ColorA, ColorA, ColorA, ColorB
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(0, 0), PieceType.ColorA, pieceId++));
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(1, 0), PieceType.ColorA, pieceId++));
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(2, 0), PieceType.ColorA, pieceId++));
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(3, 0), PieceType.ColorB, pieceId++));

            // Row 1: ColorB, ColorB, ColorB, ColorC
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(0, 1), PieceType.ColorB, pieceId++));
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(1, 1), PieceType.ColorB, pieceId++));
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(2, 1), PieceType.ColorB, pieceId++));
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(3, 1), PieceType.ColorC, pieceId++));

            // Row 2: ColorC, ColorC, ColorC, ColorD
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(0, 2), PieceType.ColorC, pieceId++));
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(1, 2), PieceType.ColorC, pieceId++));
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(2, 2), PieceType.ColorC, pieceId++));
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(3, 2), PieceType.ColorD, pieceId++));

            // Row 3: ColorD, ColorD, ColorD, ColorA
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(0, 3), PieceType.ColorD, pieceId++));
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(1, 3), PieceType.ColorD, pieceId++));
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(2, 3), PieceType.ColorD, pieceId++));
            data.InitialPieces.Add(new InitialPieceSetup(new GridPosition(3, 3), PieceType.ColorA, pieceId++));

            return data;
        }
    }
}

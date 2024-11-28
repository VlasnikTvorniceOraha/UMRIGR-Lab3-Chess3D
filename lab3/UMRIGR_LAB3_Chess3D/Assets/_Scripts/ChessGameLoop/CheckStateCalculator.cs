namespace ChessMainLoop
{
    public static class CheckStateCalculator
    {
        public static SideColor CalculateCheck(Piece[,] grid)
        {
            bool whiteCheck = false;
            bool blackCheck = false;
            int gridSize = grid.GetLength(0);

            for (int i = 0; i < gridSize; i++)
            {
                for(int j = 0; j < gridSize; j++)
                {
                    if (grid[i, j] == null)
                    {
                        continue;
                    }

                    if (grid[i, j].IsAttackingKing(i, j))
                    {
                        if (grid[i, j].PieceColor == SideColor.Black)
                        {
                            whiteCheck = true;
                        }
                        else
                        {
                            blackCheck = true;
                        }
                    }
                }            
            }

            return whiteCheck ? blackCheck ? SideColor.Both : SideColor.White : blackCheck ? SideColor.Black : SideColor.None;
        }

        #region Direction Lookup Tables
        private static readonly int[,] DiagonalLookup =
        {
           { 1, 1 },
           { 1, -1 },
           { -1, 1 },
           { -1, -1 }
        };

        private static readonly int[,] VerticalLookup =
        {
           { 1, 0 },
           { -1, 0 },
           { 0, 1 },
           { 0, -1 }
        };
        #endregion

        public static bool IsAttackingKingDiagonal(int row, int column, SideColor attackerColor)
        {
            return IsAttackingKingInDirection(row, column, DiagonalLookup, attackerColor);
        }

        public static bool IsAttackingKingVertical(int row, int column, SideColor attackerColor)
        {
            return IsAttackingKingInDirection(row, column, VerticalLookup, attackerColor);
        }

        private static bool IsAttackingKingInDirection(int row, int column, int[,] directionLookupTable, SideColor attackerColor)
        {
            /*
             * Potrebno je zamijeniti liniju return false; logikom za provjeru napadala li figura sa danog polja koordinatama row i column
             * neprijateljskog kralja ovisnom o danom smijeru napada figure koji je definiran directionLookupTable parametrom.
             */


            for (int i = 0; i < 4; i++)
            {
                int smjer1 = directionLookupTable[i, 0];
                int smjer2 = directionLookupTable[i, 1];
                

                //provjeri svaki tile redom
                while (BoardState.Instance.IsInBorders(row + smjer1, column + smjer2))
                {
                    //je li tamo kralj?
                    if (IsEnemyKingAtLocation(row, column, smjer1, smjer2, attackerColor))
                    {
                        return true;
                    }
                    //ako je tamo neka figura, prekini
                    Piece piece = BoardState.Instance.GetField(row + smjer1, column + smjer2);
                    if (piece != null)
                    {
                        break;
                    }
                    //povecaj smjerove
                    smjer1 += directionLookupTable[i, 0];
                    smjer2 += directionLookupTable[i, 1];
                }
            }
            
            return false;
        }

        public static bool IsEnemyKingAtLocation(int row, int column, int rowDirection, int columnDirection, SideColor attackerColor)
        {

            if (BoardState.Instance.IsInBorders(row + rowDirection, column + columnDirection))
            {
                Piece piece = BoardState.Instance.GetField(row + rowDirection, column + columnDirection);

                if (piece == null) return false;
                if (piece is King && piece.PieceColor != attackerColor) return true;
            }

            return false;
        }
    }
}
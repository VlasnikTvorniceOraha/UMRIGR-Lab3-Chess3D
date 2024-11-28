using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ChessMainLoop
{
    public class BoardState : Singleton<BoardState>
    {
        [SerializeField] private int _boardSize;
        [SerializeField] private Transform _blackPiecesTransform;
        [SerializeField] private List<Piece> _blackPieces;
        [SerializeField] private Transform _whitePiecesTransform;
        [SerializeField] private List<Piece> _whitePieces;
        [SerializeField] private Queue<Piece> _promotedPieces;
        private Piece[,] _gridState;

        [SerializeField] private List<GameObject> blackPiecePrefabs;
        [SerializeField] private List<GameObject> whitePiecePrefabs;

        public int BoardSize { get => _boardSize; }
        public static float Offset = 1.5f;

       


        private void Start()
        {
            _gridState = new Piece[_boardSize, _boardSize];
            InitializeGrid();
            _promotedPieces = new Queue<Piece>();
        }

        public void InitializeGrid()
        {
            for (int i = 0; i < _boardSize; i++)
            {
                for (int j = 0; j < _boardSize; j++)
                {
                    _gridState[i, j] = null;
                }
            }

            Vector3 position = new Vector3();

            for (int i = 0; i < _blackPieces.Count; i++)
            {
                Piece piece = _blackPieces[i];
                var location = piece.Location;
                _gridState[location.Row, location.Column] = piece;
                position.x = piece.Location.Row;
                position.z = piece.Location.Column;
                position *= Offset;
                position.y = piece.transform.localPosition.y;
                piece.transform.localPosition = position;
            }

            for (int i = 0; i < _whitePieces.Count; i++)
            {
                Piece piece = _whitePieces[i];
                var location = piece.Location;
                _gridState[location.Row, location.Column] = piece;
                position.x = piece.Location.Row;
                position.z = piece.Location.Column;
                position *= Offset;
                position.y = piece.transform.localPosition.y;
                piece.transform.localPosition = position;
            }
        }

        /// <summary>
        /// Retrieves current state of that fied on board
        /// </summary>
        /// <returns>Piece reference of the piece on the field, or null if not occupied</returns>
        public Piece GetField(int row, int column) => _gridState[row, column];

        public void SetField(Piece piece, int newRow, int newColumn)
        {
            _gridState[piece.Location.Row, piece.Location.Column] = null;
            _gridState[newRow, newColumn] = piece;
        }

        public void ClearField(int row, int column)
        {
            _gridState[row, column] = null;
        }

        /// <summary>
        /// Checks if cooridantes are inside board borders
        /// </summary>
        public bool IsInBorders(int row, int column)
        {
            bool check = (row >= 0 && row < _boardSize && column >= 0 && column < _boardSize);
            return check;
        }

        /// <summary>
        /// Mocks the translation of the piece to the target position and check if it would result in check.
        /// </summary>
        /// <returns>Weather translation performed on the piece would result in a check state</returns>
        public SideColor SimulateCheckState(int rowOld, int columnOld, int rowNew, int columnNew)
        {
            /*
             * Potrebno je zamijeniti liniju return SideColor.None logikom koja provjerava čijim stanjem šaha bi 
             * završilo stanje ploče prilikom izvođenja tog poteza.
             */
            if (!IsInBorders(rowNew, columnNew))
            {
                return SideColor.None;
            }
            Piece simulatedPiece = BoardState.Instance.GetField(rowOld, columnOld);
            Piece newPiece = BoardState.Instance.GetField(rowNew, columnNew);
            
            //Pomakni figuru
            ClearField(rowNew, columnNew);
            SetField(simulatedPiece, rowNew, columnNew);

            

            SideColor checkedSide = CheckStateCalculator.CalculateCheck(_gridState);

            //vrati figuru
            SetField(simulatedPiece, rowOld, columnOld);
            ClearField(rowNew, columnNew);
            //vrati staru figuru ako postoji
            if (newPiece != null)
            {
                SetField(newPiece, rowNew, columnNew);
            }

            return checkedSide;
        }

        public SideColor CalculateCheckAfterMove()
        {
            return CheckStateCalculator.CalculateCheck(_gridState);

        }

        public SideColor CheckIfGameOver()
        {
            return GameEndCalculator.CheckIfGameEnd(_gridState);
        }

        public void ResetPieces()
        {
            foreach (Piece piece in _blackPieces)
            {
                piece.ResetPiece();
            }
            foreach (Piece piece in _whitePieces)
            {
                piece.ResetPiece();
            }

            while (_promotedPieces.Count > 0)
            {
                Destroy(_promotedPieces.Dequeue());
            }

            InitializeGrid();
        }

        /// <summary>
        /// Replaces pawn being promoted with the selected piece.
        /// </summary>
        public void PromotePawn(Pawn promotingPawn, Piece piece, ChessPieceType pieceIndex)
        {
            MoveTracker.Instance.AddMove(promotingPawn.Location.Row, promotingPawn.Location.Column,
                (int)pieceIndex, (int)pieceIndex, GameManager.Instance.TurnCount - 1);
            _gridState[promotingPawn.Location.Row, promotingPawn.Location.Column] = piece;
            piece.PiecePromoted(promotingPawn);
            promotingPawn.gameObject.SetActive(false);
        }

        public void RandomizeBoard() 
        {
            //promijeni blackPieces i whitePieces liste i inicijaliziraj board ponovno
            List<GameObject> whitePieceCopy = new List<GameObject>(whitePiecePrefabs);
            List<GameObject> blackPieceCopy = new List<GameObject>(blackPiecePrefabs);


            //izbrisi one koji postoje
            foreach (Piece piece in _blackPieces)
            {
                Destroy(piece.gameObject);
            }

            foreach (Piece piece in _whitePieces)
            {
                Destroy(piece.gameObject);
            }

            _blackPieces.Clear();
            _whitePieces.Clear();

            int[] indexes = new int[16];
            
            //izaberi 16 random figura osim kralja koji se smije odabrati jednom
            King whiteKing = null;
            List<Rook> whiteRooks = new List<Rook>();
            for (int i = 0; i < 16; i++)
            {
                int nextPieceIndex = UnityEngine.Random.Range(0, whitePieceCopy.Count);
                

                if (i == 15 && !_whitePieces.Contains(whitePieceCopy[1].GetComponent<Piece>()))
                {
                    nextPieceIndex = 1;
                }
                indexes[i] = nextPieceIndex;
                Piece nextPiece = whitePieceCopy[nextPieceIndex].GetComponent<Piece>();
                

                if (i < 8)
                {
                    nextPiece.SetLocation(7, i);
                }
                else
                {
                    nextPiece.SetLocation(6, i - 8);
                }
                
                //spawnaj novi objekt i stavi u listu
                GameObject instancedPiece = Instantiate(whitePieceCopy[nextPieceIndex], _whitePiecesTransform);
                _whitePieces.Add(instancedPiece.GetComponent<Piece>());

                //ako je kralj onda makni kralja iz mogucnosti
                if (nextPiece is King)
                {
                    whitePieceCopy.Remove(nextPiece.gameObject);
                    whiteKing = instancedPiece.GetComponent<King>();

                }
                else if (nextPiece is Rook)
                {
                    whiteRooks.Add(instancedPiece.GetComponent<Rook>());
                }

            }
            whiteKing._rooks = whiteRooks;
            if (whiteRooks.Count > 0)
            {
                foreach (Rook rook in whiteRooks)
                {
                    rook._king = whiteKing;
                }
            }

            King blackKing = null;
            List<Rook> blackRooks = new List<Rook>();
            for (int i = 0; i < 16; i++)
            {
                int nextPieceIndex = indexes[i];

                Piece nextPiece = blackPieceCopy[nextPieceIndex].GetComponent<Piece>();
                

                if (i < 8)
                {
                    nextPiece.SetLocation(0, i);
                }
                else
                {
                    nextPiece.SetLocation(1, i - 8);
                }

                
                //spawnaj novi objekt i stavi u listu
                GameObject instancedPiece = Instantiate(blackPieceCopy[nextPieceIndex], _blackPiecesTransform);
                _blackPieces.Add(instancedPiece.GetComponent<Piece>());

                //ako je kralj onda makni kralja iz mogucnosti
                if (nextPiece is King)
                {
                    blackPieceCopy.Remove(nextPiece.gameObject);
                    blackKing = instancedPiece.GetComponent<King>();

                }
                else if (nextPiece is Rook)
                {
                    blackRooks.Add(instancedPiece.GetComponent<Rook>());
                }
                

            }

            blackKing._rooks = blackRooks;
            if (blackRooks.Count > 0)
            {
                foreach (Rook rook in blackRooks)
                {
                    rook._king = blackKing;
                }
            }

            InitializeGrid();
        }
    }
}

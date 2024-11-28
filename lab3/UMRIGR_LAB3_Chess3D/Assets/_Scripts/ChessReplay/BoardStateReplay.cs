using System.Collections.Generic;
using UnityEngine;

namespace ChessReplay
{
    public class BoardStateReplay : Singleton<BoardStateReplay>
    {
        [SerializeField] private int _boardSize;
        [SerializeField] private List<ReplayPiece> _blackPieces;
        [SerializeField] private List<ReplayPiece> _whitePieces;
        [Header("ReplayPiece prefabs")]
        [SerializeField] private ReplayPiece _whiteQueen;
        [SerializeField] private ReplayPiece _blackQueen;
        [SerializeField] private ReplayPiece _whiteBishop;
        [SerializeField] private ReplayPiece _blackBishop;
        [SerializeField] private ReplayPiece _whiteRook;
        [SerializeField] private ReplayPiece _blackRook;
        [SerializeField] private ReplayPiece _whiteKnight;
        [SerializeField] private ReplayPiece _blackKnight;

        [SerializeField] private Transform blackPiecesTransform;
        [SerializeField] private Transform whitePiecesTransform;
        private ReplayPiece[,] _gridState;
        private Dictionary<int, ReplayPiece> _killedDict;
        private Dictionary<int, ReplayPiece[]> _promotedOnesDict;

        public static float Offset = 1.5f;
        public int BoardSize { get => _boardSize; }
        

        /// <summary>
        /// Resets grid state and positions of all pieces.
        /// </summary>
        public void InitializeGrid()
        {
            _promotedOnesDict = new Dictionary<int, ReplayPiece[]>();
            _gridState = new ReplayPiece[_boardSize, _boardSize];
            _killedDict = new Dictionary<int, ReplayPiece>();

            for (int i = 0; i < _gridState.GetLength(0); i++)
            {
                for (int j = 0; j < _gridState.GetLength(1); j++)
                {
                    _gridState[i, j] = null;
                }
            }

            Vector3 position = new Vector3();

            for (int i = 0; i < _blackPieces.Count; i++)
            {
                ReplayPiece piece = _blackPieces[i];
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
                ReplayPiece piece = _whitePieces[i];
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
        /// Moves piece located on first paramter to location of second parameter. Stores pieces killed or promoted by turn counter parameter.
        /// </summary>
        /// If end position vector is negative, based on the value chooses a piece to replace pawn with for promotion or kills en passanted piece
        public void MovePiece(Vector2 startPosition, Vector2 endPosition, int turnCount)
        {
            if (_gridState == null)
            {
                InitializeGrid();
            }

            int startX = (int)startPosition.x;
            int startY = (int)startPosition.y;
            int endX = (int)endPosition.x;
            int endY = (int)endPosition.y;

            //Parameter -1 signifies piece got passanted, and from -2 to -9 resembles index
            //of piece mesh to replace pawn with
            if (endPosition.x == -1)
            {

                _killedDict.Add(turnCount, _gridState[startX, startY]);
                _gridState[startX, startY].gameObject.SetActive(false);
                _gridState[startX, startY] = null;
                return;
            }
            if(startPosition.x == -1)
            {
                return;
            }
            else if (endPosition.x < -1 || startPosition.x < -1)
            {
                
                PromotePawn(startPosition, _gridState[startX, startY],
                    (ChessPieceType)endPosition.x, turnCount);
                return;
            }
            else
            {
                if (_gridState[endX, endY] != null)
                {
                    _killedDict.Add(turnCount, _gridState[endX, endY]);
                    _gridState[endX, endY].gameObject.SetActive(false);
                }

                _gridState[endX, endY] = _gridState[startX, startY];
                _gridState[startX, startY] = null;
                _gridState[endX, endY].transform.localPosition = new Vector3(endPosition.x * Offset,
                    _gridState[endX, endY].transform.localPosition.y, endPosition.y * Offset);
            }
        }

        public void PromotePawn(Vector3 endPosition, ReplayPiece pawn, ChessPieceType pieceType, int turnCount)
        {
            /*
            * Nadopuniti metodu logikom koja izvodi akciju promocije pijuna u odrabranu figuru definiranu parametrom pieceType.
            */

            //zamijeni pawn sa primjerenim objektom

            

            switch (pieceType)
            {
                case ChessPieceType.BlackQueen:
                SpawnPiece(pawn, _blackQueen, blackPiecesTransform, turnCount);
                break;
                case ChessPieceType.BlackBishop:
                SpawnPiece(pawn, _blackBishop, blackPiecesTransform, turnCount);
                break;
                case ChessPieceType.BlackKnight:
                SpawnPiece(pawn, _blackKnight, blackPiecesTransform, turnCount);
                break;
                case ChessPieceType.BlackRook:
                SpawnPiece(pawn, _blackRook, blackPiecesTransform, turnCount);
                break;
                case ChessPieceType.WhiteKnight:
                SpawnPiece(pawn, _whiteKnight, whitePiecesTransform, turnCount);
                break;
                case ChessPieceType.WhiteRook:
                SpawnPiece(pawn, _whiteRook, whitePiecesTransform, turnCount);
                break;
                case ChessPieceType.WhiteQueen:
                SpawnPiece(pawn, _whiteQueen, whitePiecesTransform, turnCount);
                break;
                case ChessPieceType.WhiteBishop:
                SpawnPiece(pawn, _whiteBishop, whitePiecesTransform, turnCount);
                break;

            }

            

        }

        private void SpawnPiece(ReplayPiece pawn, ReplayPiece unit, Transform parent, int turnCount)
        {
            
            GameObject prefab = Instantiate(unit.gameObject, parent);
            _promotedOnesDict.Add(turnCount, new ReplayPiece[2] {pawn, prefab.GetComponent<ReplayPiece>()});

            //translatiraj na poziciju
            prefab.transform.localPosition = pawn.transform.localPosition;
            prefab.gameObject.SetActive(true);
            //dodaj u gridstate
            _gridState[pawn.Location.Row, pawn.Location.Column] = prefab.GetComponent<ReplayPiece>();
            //unisti pijuna
            pawn.gameObject.SetActive(false);
        }

        private void Unpromote(int turnCount)
        {
            //get promotions from dict
            ReplayPiece pawn = _promotedOnesDict[turnCount][0];
            ReplayPiece otherPiece = _promotedOnesDict[turnCount][1];

            _promotedOnesDict.Remove(turnCount);
            pawn.gameObject.SetActive(true);
            //translatiraj na poziciju
            pawn.transform.localPosition = otherPiece.transform.localPosition;
            //dodaj u gridstate
            _gridState[otherPiece.Location.Row, otherPiece.Location.Column] = pawn.GetComponent<ReplayPiece>();
            //unisti pijuna
            otherPiece.gameObject.SetActive(false);
        }

        /// <summary>
        /// Replays last move by playing it backwards.
        /// </summary>
        public void UndoMove(Vector2 startPosition, Vector2 endPosition, int turnCount)
        {
            /*
            * Nadopuniti metodu logikom za vraćanje poteza unazad. Također je potrebno vratiti figure koje su potencijalno bile
            * pojedene taj potez ili pijuna koji se pomovirao u novu figuru.
            */

            int startX = (int)startPosition.x;
            int startY = (int)startPosition.y;
            int endX = (int)endPosition.x;
            int endY = (int)endPosition.y;

            if (endPosition.x == -1)
            {

                
                return;
            }
            if(startPosition.x == -1)
            {
                return;
            }
            else if (endPosition.x < -1 || startPosition.x < -1)
            {
                
                Unpromote(turnCount);
                return;
            }
            else
            {
                //normalni move

                _gridState[startX, startY] = _gridState[endX, endY];
                //provjeri jel ima entry u kill dictu
                if (_killedDict.ContainsKey(turnCount))
                {
                    _gridState[endX, endY] = _killedDict[turnCount];
                    _gridState[endX, endY].gameObject.SetActive(true);
                    _killedDict.Remove(turnCount);
                }
                else
                {
                    _gridState[endX, endY] = null;
                }
                
                _gridState[startX, startY].transform.localPosition = new Vector3(startPosition.x * Offset,
                _gridState[startX, startY].transform.localPosition.y, startPosition.y * Offset);
            }

        }
    }
}
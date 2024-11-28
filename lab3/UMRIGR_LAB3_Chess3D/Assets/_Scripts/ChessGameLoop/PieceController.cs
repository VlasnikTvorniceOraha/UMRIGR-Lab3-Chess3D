using System.Collections;
using UnityEngine;

namespace ChessMainLoop
{
    public class PieceController : Singleton<PieceController>
    {
        private Piece _activePiece;
        public bool AnyActive { get => _activePiece != null; }
        [SerializeField] private Camera _camera;

        public static event PieceMoved PieceMoved;

        //If user presses anywhere thats not a piece and currently selected piece exists, piece gets deselcted
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) == false || AnyActive == false)
            {
                return;
            }

            RaycastHit hit;
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.TryGetComponent(out PathPiece path) || hit.transform.TryGetComponent(out Piece piece))
                {
                    return;
                }
            }

            PieceMoved?.Invoke();
            _activePiece.IsActive = false;
            _activePiece = null;
        }

        void OnEnable()
        {
            _activePiece = null;
            Piece.Selected += PieceSelected;
            PathPiece.PathSelect += PathSelected;
        }

        void OnDisable()
        {
            Piece.Selected -= PieceSelected;
            PathPiece.PathSelect -= PathSelected;
        }

        /// <summary>
        /// Upon selecting path to move selected piece to starts the moving coroutine and clears all active paths
        /// </summary>
        private void PathSelected(PathPiece path)
        {
            Piece assignedEnemy = path.AssignedPiece;
            Piece assignedCastle = path.AssignedCastle;
            GameManager.Instance.IsPieceMoving = true;
            if (assignedCastle != null)
            {
                path.AssignedCastle.AssignedAsCastle = null;
            }
            PieceMoved?.Invoke();

            int oldRow = _activePiece.Location.Row;
            int oldColumn = _activePiece.Location.Column;
            int newRow = path.Location.Row;
            int newColumn = path.Location.Column;

            if (assignedCastle)
            {
                StartCoroutine(PieceMoverCastle(oldRow, oldColumn, newRow, newColumn));
            }
            else
            {
                StartCoroutine(PieceMoverRegular(oldRow, oldColumn, newRow, newColumn, assignedEnemy));
            }
            _activePiece.IsActive = false;
        }

        private IEnumerator PieceMoverRegular(int oldRow, int oldColumn, int newRow, int newColumn, Piece assignedEnemy)
        {
            /*
             * Potrebno je nadopuniti metodu korutine koja pomiće figuru na odabrano polje.
             * Također je potrebno ažurirati podatak o tome je li neki od igrača u stanju šaha nakon izvršavanja poteza.
             * Figuru je potrebno pomaknuti pozivom metode AnimationManager instance. 
             * Nakon što završi pomicanje figure potrebno je zamijeniti koja je strana na potezu.
             */
            Piece pieceToMove = BoardState.Instance.GetField(oldRow, oldColumn);
            Vector3 position;
            position.x = newRow * BoardState.Offset;
            position.y = 0;
            position.z = newColumn * BoardState.Offset;
            pieceToMove.Move(newRow, newColumn);
            AnimationManager.Instance.MovePiece(pieceToMove, position, assignedEnemy);
            while (AnimationManager.Instance.IsActive == true)
            {
                yield return null;
            }
            SideColor checkedSide;
            checkedSide = BoardState.Instance.CalculateCheckAfterMove();
            GameManager.Instance.CheckedSide = checkedSide;
            if (pieceToMove is not Pawn)
            {
                GameManager.Instance.Passantable = null;
            }
            

            _activePiece = null;
            GameManager.Instance.IsPieceMoving = false;
            GameManager.Instance.ChangeTurn();
        }

        private IEnumerator PieceMoverCastle(int callerRow, int callerColumn, int castleRow, int castleColumn)
        {
            Vector3 targetPositionKing = new Vector3();
            Vector3 targetPositionRook = new Vector3();

            Piece firstPiece = BoardState.Instance.GetField(callerRow, callerColumn);
            Piece secondPiece = BoardState.Instance.GetField(castleRow, castleColumn);
            Piece king = firstPiece is King ? firstPiece : secondPiece;
            Piece rook = firstPiece is Rook ? firstPiece : secondPiece;

            //If target is a castling position performs special castling action. Position calculations are done differently if the target is a King or a Rook          
            int columnMedian = (int)Mathf.Ceil((king.Location.Column + rook.Location.Column) / 2f);
            int rookNewColumn = columnMedian > king.Location.Column ? columnMedian - 1 : columnMedian + 1;
            SideColor checkedSide;

            targetPositionKing.x = callerRow * BoardState.Offset;
            targetPositionKing.y = 0;
            targetPositionKing.z = columnMedian * BoardState.Offset;

            targetPositionRook.x = callerRow * BoardState.Offset;
            targetPositionRook.y = 0;
            targetPositionRook.z = rookNewColumn * BoardState.Offset;

            king.Move(callerRow, columnMedian);
            AnimationManager.Instance.MovePiece(king, targetPositionKing, null);
            while (AnimationManager.Instance.IsActive == true)
            {
                yield return null;
            }

            checkedSide = BoardState.Instance.CalculateCheckAfterMove();

            rook.Move(callerRow, rookNewColumn);
            AnimationManager.Instance.MovePiece(rook, targetPositionRook, null);
            while (AnimationManager.Instance.IsActive == true)
            {
                yield return null;
            }

            GameManager.Instance.CheckedSide = checkedSide;
            GameManager.Instance.Passantable = null;

            _activePiece = null;
            GameManager.Instance.IsPieceMoving = false;
            GameManager.Instance.ChangeTurn();
        }

        /// <summary>
        /// Replaces status of selected piece with newly selected piece.
        /// </summary>
        private void PieceSelected(Piece piece)
        {
            if (_activePiece)
            {
                _activePiece.IsActive = false;
                PieceMoved?.Invoke();
            }

            _activePiece = piece;
        }
    }
}
using System;

namespace ChessGameApp
{
    class ChessGameApplication
    {
        static void Main(string[] args){
            new GameManager().GameOn();
        }
    }
    class GameManager // Controls the flow of the game and IO.
    {
        ChessGame board; string inputPlayerMove; bool isWhiteTurn;
        public GameManager() { this.board = new ChessGame(); this.inputPlayerMove = ""; this.isWhiteTurn = true; }
        public void NextTurn() { this.isWhiteTurn = !this.isWhiteTurn; }

        public void GameOn() { // Starts a Chess game.
            Console.WriteLine(board.ToString());  welcomeMessage();
            while (true){
                if (this.board.IsCheck(isWhiteTurn) && this.board.IsDrawByStalemate(isWhiteTurn)) // Checkmate.
                { checkMateMessege(); return; }
                if (this.board.IsDrawByDeadPosition())  // Checks draw by dead position.
                { drawByDeadPosition(); return; }
                if (this.board.IsDrawByStalemate(isWhiteTurn)) // Checks draw by stalemate.
                { drawByStalemate(); return; }
                if (this.board.IsCheck(isWhiteTurn)) // Checks if there is check.
                    checkMessege(isWhiteTurn);
                inputPlayerMove = nextMoveMessage(isWhiteTurn);// Player input next move. ( In a loop until input is legit ).
                if (this.board.IsDrawByRepetition())
                { drawByRepetition(); return; }
                if (!IsLegalMove(inputPlayerMove)) // Checks if the input move is legal.
                { invalidMoveMessege(); continue; }
                if (this.board.IsDrawByFiftyMovesWithoutCapture()) // Checks draw by 50 moves without capture.
                { drawByFiftyMovesWithoutCapture(); return; }
                MovePiece(inputPlayerMove);
                this.board.PawnAtLastTile(isWhiteTurn);
                NextTurn(); // Changes turn.
                Console.WriteLine(board.ToString()); // Prints board.
            }
        }
        private void welcomeMessage() { Console.WriteLine("Welcome to Dean's chess app! Have fun!\n"); } // Welcome message upon game start.
        private void drawByDeadPosition() { Console.WriteLine("It's a draw!\nDraw by dead position.\n"); }
        private void drawByStalemate() { Console.WriteLine("It's a draw!\nDraw by stalemate.\n"); }
        private void checkMateMessege() { Console.WriteLine("Checkmate! Congratulations.\n"); }
        private void checkMessege(bool isWhite) { if (isWhite) { Console.WriteLine("Check on white!\n"); } else { Console.WriteLine("Check on black!\n"); } }
        private void drawByFiftyMovesWithoutCapture() { Console.WriteLine("It's a draw!\nDraw by fifty moves without capture.\n"); ; }
        private void drawByRepetition() { Console.WriteLine("It's a draw!\nDraw by repetition law.\n"); }
        private void invalidMoveMessege() { Console.WriteLine("Invalid move. Try again.\n"); }
        private string nextMoveMessage(bool isWhiteTurn) { // Next move messege which repeats after every turn. Calculates valid move input within boundaries.
            string input = "", move = "Please insert your next move.";
            while (true) { // Loops until a valid move acceppted.
                if (isWhiteTurn) Console.WriteLine("White turn\n" + move);
                else Console.WriteLine("Black turn\n" + move);

                input = Console.ReadLine().ToLower();
                if (input.Length != 4) { Console.WriteLine("Invalid input. Try again, Example for a valid move: a1a2\n"); continue; } // Short or long input.
                if (!"abcdefgh".Contains(input[0]) || !"12345678".Contains(input[1]) || !"abcdefgh".Contains(input[2]) || !"12345678".Contains(input[3])) // Invalid boundaries.
                { Console.WriteLine("Invalid input. Index out of boundary. Try again, Example for a valid move: a1a2\n"); continue; }
                if (input[0] == input[2] && input[1] == input[3]) { Console.WriteLine("Invalid input. You are trying to move a piece to the same position. Try again.\n"); continue; }
                break;
            }
            return input;
        }
        public bool IsLegalMove(string playerMove) { // Breaks string input to (x,y) coordinates and checks if the move is legal.
            int xStart = 0,yStart = 0, xEnd = 0, yEnd = 0; // Starting (x,y) position and end (x,y) position
            for (int i = 0; i < 8; i++) { // Breaks the string input into (x,y) coordinates.
                if (playerMove[0] == "abcdefgh"[i]) yStart = i;
                if (playerMove[1] == "12345678"[i]) xStart = i;
                if (playerMove[2] == "abcdefgh"[i]) yEnd = i;
                if (playerMove[3] == "12345678"[i]) xEnd = i;
            }
            if (this.board.getBoard()[xStart, yStart] is GhostPiece) return false;
            if ((xStart == xEnd) && (this.board.getBoard()[xStart, yStart] is King) && (Math.Abs(yStart - yEnd) == 2)) // Checks if player tries to castle.
                return board.IsCastlingPossible(xStart, yStart, xEnd, yEnd); // Checks if castle is possible.
            board.EnPasanUpdate(xStart, yStart, xEnd, yEnd); // Updates the state of EnPasan moves.
            if (this.board.getBoard()[xStart, yStart].LegalMove(xEnd, yEnd, this.isWhiteTurn, this.board)) { // Checks if the move is legal.
                if (this.board.IsCheckAfterMove(xStart, yStart, xEnd, yEnd, isWhiteTurn)) return false;
               return true;
            }
            return false;
        }
        public void MovePiece(string playerMove) {
            int xStart = 0, yStart = 0, xEnd = 0, yEnd = 0; // Starting (x,y) position and end (x,y) position
            for (int i = 0; i < 8; i++) { // Breaks the string input into (x,y) coordinates.
                if (playerMove[0] == "abcdefgh"[i]) yStart = i;
                if (playerMove[1] == "12345678"[i]) xStart = i;
                if (playerMove[2] == "abcdefgh"[i]) yEnd = i;
                if (playerMove[3] == "12345678"[i]) xEnd = i;
            }
            this.board.getBoard()[xStart, yStart].Move(xEnd, yEnd, isWhiteTurn, this.board); // Regular move.
        }
    }
    class ChessGame // Support the state of the game such as tie,checks and legal.
    {
        private ChessPiece[,] board; // Defines the chessboard as a matrix of 8*8.
        private int movesWithoutCapture; private int currentBoardPieces; string[] boardMemory = new string[100];
        int[,] enPasanPawnsState;

        public ChessGame() // Board constructor which sets all the chess pieces in the correct position.
        {
            this.movesWithoutCapture = 0; this.currentBoardPieces = 32; this.boardMemory = new string[100];
            this.enPasanPawnsState = new int[2, 8];
            for (int i = 0; i < 8; i++) { this.enPasanPawnsState[0, i] = 0; this.enPasanPawnsState[1, i] = 0; }
            this.board = new ChessPiece[8, 8];
            this.board[0, 0] = new Rook(0, 0, false, false); this.board[0, 7] = new Rook(0, 7, false, false); // Sets Black Rook
            this.board[7, 0] = new Rook(7, 0, true, false); this.board[7, 7] = new Rook(7, 7, true, false); // Sets White Rook
            this.board[0, 1] = new Knight(0, 1, false); this.board[0, 6] = new Knight(0, 6, false); // Sets Black Knight
            this.board[7, 1] = new Knight(7, 1, true); this.board[7, 6] = new Knight(7, 6, true); // Sets White Knight
            this.board[0, 2] = new Bishop(0, 2, false); this.board[0, 5] = new Bishop(0, 5, false); // Sets Black Bishop
            this.board[7, 2] = new Bishop(7, 2, true); this.board[7, 5] = new Bishop(7, 5, true); // Sets White Bishop
            this.board[0, 3] = new Queen(0, 3, false); this.board[7, 3] = new Queen(7, 3, true); // Sets Queens
            this.board[0, 4] = new King(0, 4, false, false); this.board[7, 4] = new King(7, 4, true, false); // Sets Kings
            for (int i = 0; i < 8; i++) // Sets Pawns
            {
                this.board[1, i] = new Pawn(1, i, false); // Black Pawns
                this.board[6, i] = new Pawn(6, i, true); ; // White Pawns
            }
            for (int i = 2; i < 6; i++) // Sets Empty Cells
                for (int j = 0; j < 8; j++)
                    this.board[i, j] = new GhostPiece(i, j, false);
        }
        public ChessPiece[,] getBoard() { return this.board; }
        public override string ToString() // Prints the board as a string to the screen.
        {
            string[,] outputBoard = new string[9, 9]; // String output of the current board state.
            outputBoard[0, 1] = "A"; outputBoard[0, 2] = "B"; outputBoard[0, 3] = "C"; outputBoard[0, 4] = "D"; outputBoard[0, 5] = "E";
            outputBoard[0, 6] = "F"; outputBoard[0, 7] = "G"; outputBoard[0, 8] = "H"; outputBoard[1, 0] = "1"; outputBoard[2, 0] = "2";
            outputBoard[3, 0] = "3"; outputBoard[4, 0] = "4"; outputBoard[5, 0] = "5"; outputBoard[6, 0] = "6"; outputBoard[7, 0] = "7";
            outputBoard[8, 0] = "8"; outputBoard[0, 0] = " ";
            for (int i = 1; i < 9; i++)
                for (int j = 1; j < 9; j++)
                {
                    if (this.board[i - 1, j - 1] is GhostPiece) { outputBoard[i, j] = "EE"; continue; } // EE for empty cells.
                    if (this.board[i - 1, j - 1].IsWhiteColor()) { outputBoard[i, j] += "W"; } // W for white pieces.
                    if (!this.board[i - 1, j - 1].IsWhiteColor()) { outputBoard[i, j] += "B"; } // B for black pieces.
                    outputBoard[i, j] += pieceKind(this.board[i - 1, j - 1]);
                }
            string finalBoard = ""; // Adds a basic graphical view as a string to the board.
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (i == 0)
                        finalBoard += outputBoard[i, j] + "| ";
                    else
                        finalBoard += outputBoard[i, j] + "|";
                }
                finalBoard += "\n" + "--------------------------" + "\n";
            }
            return finalBoard;
        }
        public void PawnAtLastTile(bool isWhite) // Checks if there is a pawn at the last tile and calls for a promotion function.
        {
            if (isWhite) // White pawns.
            {
                for (int i = 0; i < 8; i++)
                    if (this.board[0, i] is Pawn)
                        (this.board[0, i] as Pawn).PawnLastRow(0, i, isWhite, this);
            }
            else // Black pawns.
            {
                for (int i = 0; i < 8; i++)
                    if (this.board[7, i] is Pawn)
                        (this.board[7, i] as Pawn).PawnLastRow(7, i, isWhite, this);
            }
        }
        public bool CanPawnEnPasan(int xStart, int yStart, int xEnd, int yEnd) // Checks if a WHITE pawn at (x,y) can 'En Pasan'.
        {
            if (this.board[xStart, yStart] is Pawn && this.board[xStart, yStart].IsWhiteColor() && xStart == 3 && xEnd == 2 && Math.Abs(yStart - yEnd) == 1)
                if ((this.board[xStart, yEnd] is Pawn) && !this.board[xStart, yEnd].IsWhiteColor())
                    if (this.enPasanPawnsState[0, yStart] == 1)
                        return true;
            if (this.board[xStart, yStart] is Pawn && !this.board[xStart, yStart].IsWhiteColor() && xStart == 4 && xEnd == 5 && Math.Abs(yStart - yEnd) == 1) // Checks if a BLACK pawn at (x,y) can 'En Pasan'.
                if ((this.board[xStart, yEnd] is Pawn) && this.board[xStart, yEnd].IsWhiteColor())
                    if (this.enPasanPawnsState[1, yStart] == 1)
                        return true;
            return false;
        }
        public void EnPasanUpdate(int xStart, int yStart, int xEnd, int yEnd) // Updates the current 'En Pasan' pawns state in an array.
        {
            int saveEnPasanMoveOne = -1; int saveEnPasanMoveTwo = -1;
            if (this.board[xStart, yStart] is Pawn && !this.board[xStart, yStart].IsWhiteColor() && xStart == 1 && xEnd == 3) // Handles activation of WHITE 'en pasan' upon black movement.
            {
                if (yStart > 0 && yStart < 7) // Handles inner col 'En pasan'.
                {
                    if (this.board[xEnd, yEnd - 1] is Pawn && this.board[xEnd, yEnd - 1].IsWhiteColor())
                    {
                        this.enPasanPawnsState[0, yEnd - 1] = 1;
                        saveEnPasanMoveOne = yEnd - 1;
                    }
                    if (this.board[xEnd, yEnd + 1] is Pawn && this.board[xEnd, yEnd + 1].IsWhiteColor())
                    {
                        this.enPasanPawnsState[0, yEnd + 1] = 1;
                        saveEnPasanMoveTwo = yEnd + 1;
                    }
                }
                if (yStart == 0) // Handles first col.
                {
                    if (this.board[xEnd, yEnd + 1] is Pawn && this.board[xEnd, yEnd + 1].IsWhiteColor())
                    {
                        this.enPasanPawnsState[0, yEnd + 1] = 1;
                        saveEnPasanMoveOne = yEnd + 1;
                    }
                }
                if (yStart == 7) // Handles last col.
                {
                    if (this.board[xEnd, yEnd - 1] is Pawn && this.board[xEnd, yEnd - 1].IsWhiteColor())
                    {
                        this.enPasanPawnsState[0, yEnd - 1] = 1;
                        saveEnPasanMoveOne = yEnd - 1;
                    }
                }
                for (int i = 0; i < 8; i++) // Updates the board white pawns en pasan state.
                {
                    if (saveEnPasanMoveOne != -1 && i == saveEnPasanMoveOne) continue;
                    if (saveEnPasanMoveTwo != -1 && i == saveEnPasanMoveTwo) continue;
                    this.enPasanPawnsState[0, i] = 0;
                }
            }
            if (this.board[xStart, yStart] is Pawn && this.board[xStart, yStart].IsWhiteColor() && xStart == 6 && xEnd == 4) // Handles activation of BLACK 'en pasan' upon black movement.
            {
                if (yStart > 0 && yStart < 7) // Handles inner col 'En pasan'.
                {
                    if (this.board[xEnd, yEnd - 1] is Pawn && !this.board[xEnd, yEnd - 1].IsWhiteColor())
                    {
                        this.enPasanPawnsState[1, yEnd - 1] = 1;
                        saveEnPasanMoveOne = yEnd - 1;
                    }
                    if (this.board[xEnd, yEnd + 1] is Pawn && !this.board[xEnd, yEnd + 1].IsWhiteColor())
                    {
                        this.enPasanPawnsState[1, yEnd + 1] = 1;
                        saveEnPasanMoveTwo = yEnd + 1;
                    }
                }
                if (yStart == 0) // Handles first col.
                {
                    if (this.board[xEnd, yEnd + 1] is Pawn && !this.board[xEnd, yEnd + 1].IsWhiteColor())
                    {
                        this.enPasanPawnsState[1, yEnd + 1] = 1;
                        saveEnPasanMoveOne = yEnd + 1;
                    }
                }
                if (yStart == 7) // Handles last col.
                {
                    if (this.board[xEnd, yEnd - 1] is Pawn && !this.board[xEnd, yEnd - 1].IsWhiteColor())
                    {
                        this.enPasanPawnsState[1, yEnd - 1] = 1;
                        saveEnPasanMoveOne = yEnd - 1;
                    }
                }
                for (int i = 0; i < 8; i++) // Updates the board white pawns en pasan state.
                {
                    if (saveEnPasanMoveOne != -1 && i == saveEnPasanMoveOne) continue;
                    if (saveEnPasanMoveTwo != -1 && i == saveEnPasanMoveTwo) continue;
                    this.enPasanPawnsState[1, i] = 0;
                }
            }
        }
        private string pieceKind(ChessPiece chessPiece) // Supports toString in order to define chesspiece king as char: K,Q,R,B,N,P.
        {
            if (chessPiece is King) { return "K"; }
            if (chessPiece is Queen) { return "Q"; }
            if (chessPiece is Rook) { return "R"; }
            if (chessPiece is Knight) { return "N"; }
            if (chessPiece is Bishop) { return "B"; }
            if (chessPiece is Pawn) { return "P"; }
            return "";
        }
        public bool IsCellThreatened(int x, int y, bool isWhite) // Checks if cell (x,y) is threatend by player 'isWhite'.
        {
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    if (this.board[i, j] is GhostPiece) continue; // Skips empty cells.
                    if (this.board[i, j].IsWhiteColor() != isWhite) continue; // Skips same piece colors.
                    if (x == i && y == j) continue;
                    if (!(this.board[x, y] is GhostPiece)) // Checks if piece on (x,y) is protected.
                    { // Deletes the piece temporarly in order to check if a piece is threatened (x,y) cell.
                        ChessPiece tempPiece = this.board[x, y];
                        this.board[x, y] = new GhostPiece(x, y, false);
                        if (this.board[i, j].LegalMove(x, y, isWhite, this))
                        {
                            this.board[x, y] = tempPiece;
                            return true;
                        }
                        this.board[x, y] = tempPiece;
                    }
                    if (!(this.board[i, j] is Pawn) && this.board[i, j].LegalMove(x, y, isWhite, this)) // Checks if empty cell (x,y) is threatened.
                        return true;
                    if ((this.board[i, j] is Pawn) && (Math.Abs(j - y)) == 1 && (Math.Abs(x - i) == 1)) // Checks if pawn positioned correctly.
                    {
                        if (isWhite && i > x) return true;  // White pawns.
                        if (!isWhite && i < x) return true;  // Black pawns.
                    }
                }
            return false;
        }
        public bool IsCastlingPossible(int xStart, int yStart, int xEnd, int yEnd) // Checks if castling is possible.
        {
            bool currentKingColor = this.board[xStart, yStart].IsWhiteColor(); // Figures king color.
            if (IsCheck(currentKingColor)) return false;
            if ((this.board[xStart, yStart] as King).GetDidMove()) return false; // Checks if king didn't move.
            if (yStart < yEnd) // Castling to the right.
            {
                if ((this.board[xStart, yEnd + 1] is Rook) && !(this.board[xStart, yEnd + 1] as Rook).GetDidMove()) // Checks if Right Rook exist and didn't move.
                {
                    if (this.board[xStart, yStart + 1] is GhostPiece && this.board[xEnd, yEnd] is GhostPiece) // Checks the path is clear.
                    {
                        if (IsCellThreatened(xStart, yStart + 1, !currentKingColor) || IsCellThreatened(xEnd, yEnd, !currentKingColor)) // Checks if the path is threatened.
                            return false;
                        else // Moves the Rook since Castling is legal.
                        {
                            this.board[xStart, yEnd + 1].Move(xEnd, yEnd - 1, currentKingColor, this);
                            return true;
                        }
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else // Castling to the left.
            {
                if ((this.board[xStart, yEnd - 2] is Rook) && !(this.board[xStart, yEnd - 2] as Rook).GetDidMove()) // Checks if Left Rook exist and didn't move.
                {
                    if (this.board[xStart, yStart - 1] is GhostPiece && this.board[xEnd, yEnd] is GhostPiece && (this.board[xEnd, yEnd - 1] is GhostPiece)) // Checks the path is clear.
                    {
                        if (IsCellThreatened(xStart, yStart - 1, !currentKingColor) || IsCellThreatened(xStart, yEnd, !currentKingColor) || IsCellThreatened(xStart, yEnd - 1, !currentKingColor)) // Checks if the path is threatened.
                            return false;
                        else // Moves the Rook since Castling is legal.
                        {
                            this.board[xStart, yEnd - 2].Move(xEnd, yEnd + 1, currentKingColor, this);
                            return true;
                        }
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
        }
        public bool IsCheck(bool isWhite) // Checks if there is check on the current player king.
        {
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    if (this.board[i, j] is GhostPiece) continue; // Skips empty cells.
                    if ((this.board[i, j] is King) && (this.board[i, j].IsWhiteColor() == isWhite)) // Checks if piece is king of same color.
                        if (IsCellThreatened(i, j, !isWhite)) // Checks if king is threatened by enemy.
                            return true;
                }
            return false;
        }
        public bool IsCheckAfterMove(int xStart, int yStart, int xEnd, int yEnd, bool isWhite) // Checks if there is check after moving a piece.
        {
            if (this.board[xStart, yStart] is King) // Checks if king moves into a threatened tile.
            {
                if ((IsCellThreatened(xEnd, yEnd, !isWhite)))
                    return true;
                else
                    return false;
            }
            else // Checks if after moving a piece there is check.
            {
                if (this.board[xEnd, yEnd] is GhostPiece)  // Piece moves into empty cell.
                {
                    if (Math.Abs(yStart - yEnd) == 1 && this.board[xStart, yStart] is Pawn && (xStart == 4 || xStart == 3))
                        return false;
                    this.board[xStart, yStart].Move(xEnd, yEnd, isWhite, this);
                    if (IsCheck(isWhite))
                    {
                        this.board[xEnd, yEnd].Move(xStart, yStart, isWhite, this);
                        return true;
                    }
                    else
                    {
                        this.board[xEnd, yEnd].Move(xStart, yStart, isWhite, this);
                        return false;
                    }
                }
                else // Piece captures enemy piece.
                {
                    ChessPiece temp = null; // Checks which kind the eaten piece is in order to restore it later on.
                    if (this.board[xEnd, yEnd] is Pawn)
                        temp = new Pawn(xEnd, yEnd, !isWhite);
                    if (this.board[xEnd, yEnd] is Knight)
                        temp = new Knight(xEnd, yEnd, !isWhite);
                    if (this.board[xEnd, yEnd] is Bishop)
                        temp = new Bishop(xEnd, yEnd, !isWhite);
                    if (this.board[xEnd, yEnd] is Rook)
                        temp = new Rook(xEnd, yEnd, !isWhite, true);
                    if (this.board[xEnd, yEnd] is Queen)
                        temp = new Queen(xEnd, yEnd, !isWhite);
                    this.board[xStart, yStart].Move(xEnd, yEnd, isWhite, this);
                    if (IsCheck(isWhite)) // Moves the pieces and checks if there is check and then reverse the movement.
                    {
                        this.board[xEnd, yEnd].Move(xStart, yStart, isWhite, this);
                        if (temp is Pawn)
                            this.board[xEnd, yEnd] = new Pawn(xEnd, yEnd, !isWhite);
                        if (temp is Knight)
                            this.board[xEnd, yEnd] = new Knight(xEnd, yEnd, !isWhite);
                        if (temp is Bishop)
                            this.board[xEnd, yEnd] = new Bishop(xEnd, yEnd, !isWhite);
                        if (temp is Rook)
                            this.board[xEnd, yEnd] = new Rook(xEnd, yEnd, !isWhite, true);
                        if (temp is Queen)
                            this.board[xEnd, yEnd] = new Queen(xEnd, yEnd, !isWhite);
                        return true;
                    }
                    this.board[xEnd, yEnd].Move(xStart, yStart, isWhite, this);
                    if (temp is Pawn)
                        this.board[xEnd, yEnd] = new Pawn(xEnd, yEnd, !isWhite);
                    if (temp is Knight)
                        this.board[xEnd, yEnd] = new Knight(xEnd, yEnd, !isWhite);
                    if (temp is Bishop)
                        this.board[xEnd, yEnd] = new Bishop(xEnd, yEnd, !isWhite);
                    if (temp is Rook)
                        this.board[xEnd, yEnd] = new Rook(xEnd, yEnd, !isWhite, true);
                    if (temp is Queen)
                        this.board[xEnd, yEnd] = new Queen(xEnd, yEnd, !isWhite);
                }
                return false;
            }
        }
        public bool IsDrawByRepetition() // Checks if there were 3 same boards during the game and declares a draw if so.
        {
            string currentBoard = ""; // Current board.
            for (int i = 0; i < 8; i++) // Breaks the board to a string.
                for (int j = 0; j < 8; j++)
                {
                    currentBoard += board[i, j].toString();
                    if (board[i, j] is Rook)   // Rook / Kings movement matter to board state.
                        currentBoard += (board[i, j] as Rook).GetDidMove();
                    if (board[i, j] is King)
                        currentBoard += (board[i, j] as King).GetDidMove();
                }
            for (int i = 0; i < this.boardMemory.Length; i++) // Adding the board as string to the memory.
                if (boardMemory[i] == null)
                {
                    boardMemory[i] = currentBoard;
                    break;
                }
            int repetition;
            for (int i = 0; i < boardMemory.Length; i++) // Counts how many repetitive boards there were.
            {
                repetition = 0; if (boardMemory[i] == null) break;
                for (int j = 0; j < boardMemory.Length; j++)
                {
                    if (boardMemory[j] == null)
                        break;
                    if (boardMemory[i] == boardMemory[j] && i != j)
                        repetition++;
                    if (repetition == 3)
                        return true;
                }
            }
            return false;
        }
        public bool IsDrawByFiftyMovesWithoutCapture() // Checks if there were 50 moves without any capture.
        {
            if (this.movesWithoutCapture == 50) return true;
            int chessPieces = 0;
            for (int i = 0; i < 8; i++) // Counts how many pieces are on the board.
                for (int j = 0; j < 8; j++)
                {
                    if (this.board[i, j] is GhostPiece) continue;
                    else
                        chessPieces++;
                }
            if (this.currentBoardPieces != chessPieces) // Defines if a piece was eaten or not.
            {
                this.movesWithoutCapture = 0;
                this.currentBoardPieces = chessPieces;
            }
            else
                this.movesWithoutCapture++;
            return false;
        }
        public bool IsDrawByStalemate(bool isWhite) // Checks if there is any legal move the current player can do.
        {
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    if (this.board[i, j] is GhostPiece) continue;
                    if (this.board[i, j].IsWhiteColor() != isWhite) continue;
                    for (int row = 0; row < 8; row++)
                    {
                        for (int col = 0; col < 8; col++)
                        {
                            if (!(this.board[i, j] is King) && (this.board[i, j].LegalMove(row, col, isWhite, this)) && !(this.IsCheckAfterMove(i, j, row, col, isWhite)))
                                return false;
                            if ((this.board[i, j] is King) && (this.board[i, j].LegalMove(row, col, isWhite, this)) && !(this.IsCheckAfterMove(i, j, row, col, isWhite)))
                                return false;
                        }
                    }
                }
            return true;
        }
        public bool IsDrawByDeadPosition() // Defines if players have enough pieces to checkmate otherwise declares a draw.
        {
            int[] whitePieces = new int[5]; int[] blackPieces = new int[5]; // Array that holds the number of pieces in different indexs.
            for (int i = 0; i < 8; i++) // Updates the array with the amount of pieces on board.
            {
                for (int j = 0; j < 8; j++)
                {
                    if (this.board[i, j] is GhostPiece) // Skips empty cells.
                        continue;
                    if (this.board[i, j] is Pawn) // Counts Pawns.
                    {
                        if (this.board[i, j].IsWhiteColor())
                            whitePieces[0]++;
                        else
                            blackPieces[0]++;
                    }
                    if (this.board[i, j] is Rook) // Counts Rooks.
                    {
                        if (this.board[i, j].IsWhiteColor())
                            whitePieces[1]++;
                        else
                            blackPieces[1]++;
                    }
                    if (this.board[i, j] is Knight) // Counts Knights
                    {
                        if (this.board[i, j].IsWhiteColor())
                            whitePieces[2]++;
                        else
                            blackPieces[2]++;
                    }
                    if (this.board[i, j] is Bishop) // Counts Bishops
                    {
                        if (this.board[i, j].IsWhiteColor())
                            whitePieces[3]++;
                        else
                            blackPieces[3]++;
                    }
                    if (board[i, j] is Queen) // Counts Queens
                    {
                        if (this.board[i, j].IsWhiteColor())
                            whitePieces[4]++;
                        else
                            blackPieces[4]++;
                    }
                }
            } // Checks if either player has zero pieces that can mate ( Pawns,Rooks,Queens ).
            if (whitePieces[0] == 0 && whitePieces[1] == 0 && whitePieces[4] == 0 && blackPieces[0] == 0 && blackPieces[1] == 0 && blackPieces[4] == 0)
            {
                if (whitePieces[2] + whitePieces[3] >= 2 || blackPieces[2] + blackPieces[3] >= 2) return false;// Checks if either player has at least 2 pieces.
                else
                    return true;
            }
            else
                return false;
        }
    }
    class ChessPiece // Generic Chesspiece
    {
        private bool isWhite; // Color , True = white , False = black.
        private int x; private int y; // Position on board.

        public ChessPiece(int x, int y, bool color) { this.x = x; this.y = y; this.isWhite = color; } // Basic Constructor.
        public int GetX() { return this.x; }
        public int GetY() { return this.y; }
        public bool IsWhiteColor() { return this.isWhite; }
        public void SetColor() { this.isWhite = !isWhite; }
        public virtual bool LegalMove(int x, int y, bool isWhiteTurn, ChessGame board) { return false; } // Check if move (x,y) is legal.
        public virtual void Move(int x, int y, bool isWhiteTurn, ChessGame board) // Sets the starting position to empty cell.
        { board.getBoard()[this.GetX(), this.GetY()] = new GhostPiece(this.GetX(), this.GetY(), false); return; }
        public virtual string toString(){
            if (this.isWhite) return "white";
            else return "black";
        }
    }
    class Pawn : ChessPiece
    {
        public Pawn(int x, int y, bool isWhite) : base(x, y, isWhite) { } // Basic Constructor.
        public override string toString() { return base.toString() + "Pawn"; }
        public override void Move(int x, int y, bool isWhiteTurn, ChessGame board) { // Moves Pawn to (x,y) and updates 'En Passant' to false.
            base.Move(this.GetX(), this.GetY(), isWhiteTurn, board);
            if (this.GetX() == 3 && x == 2 && Math.Abs(y - this.GetY()) == 1 && board.getBoard()[x, y] is GhostPiece)
                board.getBoard()[3, y] = new GhostPiece(3, y, false);
            if (this.GetX() == 4 && x == 5 && Math.Abs(y - this.GetY()) == 1 && board.getBoard()[x, y] is GhostPiece)
                board.getBoard()[4, y] = new GhostPiece(4, y, false);
            board.getBoard()[x, y] = new Pawn(x, y, isWhiteTurn); return;
        }
        public override bool LegalMove(int x, int y, bool isWhiteTurn, ChessGame board){
            if (this.IsWhiteColor() != isWhiteTurn) return false; // Checks if moved piece belongs to the current player turn.

            if ( (Math.Abs( x - this.GetX()) == 1) || ( Math.Abs(x - this.GetX()) == 2 && (this.GetX() == 6 && isWhiteTurn || this.GetX() == 1 && !isWhiteTurn) )) return true;
            if (Math.Abs(x - this.GetX()) == 1 && Math.Abs(y - this.GetY()) == 1 && !(board.getBoard()[x, y] is GhostPiece)
                && isWhiteTurn != board.getBoard()[x, y].IsWhiteColor()) return true;
            if (isWhiteTurn)  { // White pawns.
                if (x == this.GetX() - 1 && (Math.Abs(y - this.GetY()) == 1) && board.getBoard()[this.GetX(), y] is Pawn
                && !board.getBoard()[this.GetX(), y].IsWhiteColor() && board.getBoard()[x, y] is GhostPiece && board.CanPawnEnPasan(this.GetX(), this.GetY(), x, y)) // Checks if Pawn can 'En Pasan'.
                    return true;
                return false;
            }
            else { // Black Pawns.
                if (x == this.GetX() + 1 && (Math.Abs(y - this.GetY()) == 1) && board.getBoard()[this.GetX(), y] is Pawn
                && board.getBoard()[this.GetX(), y].IsWhiteColor() && board.getBoard()[x, y] is GhostPiece && board.CanPawnEnPasan(this.GetX(), this.GetY(), x, y)) // Checks if Pawn can 'En Pasan'.
                    return true;
                return false;
            }
        }
        public void PawnLastRow(int x, int y, bool isWhiteTurn, ChessGame board) { // Handles pawn at last row and piece transformation with user select.
            string playerChoice = ""; int pieceKind = 0;
            Console.WriteLine("Please enter your pawn promotion choice:\n1 - Rook\t2 - Knight\t3 - Bishop\t4 - Queen");
            while (true) { // Infinite loop until player chooses correctly.
                playerChoice = Console.ReadLine();
                if (playerChoice.Length == 1 && (playerChoice[0].ToString() == "1" || playerChoice[0].ToString() == "2" || playerChoice[0].ToString() == "3"
                    || playerChoice[0].ToString() == "4"))
                    pieceKind = int.Parse(playerChoice);
                switch (pieceKind) { // Upon choice promoting the pawn to the selected piece.
                    case 1: board.getBoard()[x, y] = new Rook(x, y, isWhiteTurn, true); return;
                    case 2: board.getBoard()[x, y] = new Knight(x, y, isWhiteTurn); return;
                    case 3: board.getBoard()[x, y] = new Bishop(x, y, isWhiteTurn); return;
                    case 4: board.getBoard()[x, y] = new Queen(x, y, isWhiteTurn); return;
                    default: break;
                }
                Console.WriteLine("Invalid choice. Try again.");
            }
        }
    }
    class Rook : ChessPiece
    {
        private bool didMove; // Defines if Rook was moved ( Relevant to Castling ).

        public Rook(int x, int y, bool isWhite, bool moved) : base(x, y, isWhite) { this.didMove = moved; } // Basic Constructor.
        public bool GetDidMove() { return this.didMove; }
        public void SetDidMove(bool moved) { this.didMove = moved; }
        public override string toString() { return base.toString() + "Rook"; }
        public override void Move(int x, int y, bool isWhiteTurn, ChessGame board) { // Moves Rook to (x,y) and updates the move status for Castling.
            board.getBoard()[x, y] = new Rook(x, y, isWhiteTurn, true);
            base.Move(this.GetX(), this.GetY(), isWhiteTurn, board);
        }

        public override bool LegalMove(int x, int y, bool isWhiteTurn, ChessGame board) { // Checks if a Rook can legally move to ( x,y ).
            if (this.IsWhiteColor() != isWhiteTurn) return false; // Checks if moved piece belongs to the current player turn.
            if (this.GetX() != x && this.GetY() != y) return false; // Checks if the path is vertical or horizontical.

            if( (x - this.GetX() == 0) || (y - this.GetY() == 0)) { // Genreic 4 way check.
                int jumpX = 0, jumpY = 0;
                if( x == this.GetX()) {
                    if (this.GetY() > y) jumpY = -1; else jumpY = 1;
                }
                else {
                    if (this.GetX() > x) jumpX = -1; else jumpX = 1;
                }
                for (int i = this.GetX() + jumpX, j = this.GetY() + jumpY; i != x && j != y; i += jumpX, j += jumpY)
                    if (!(board.getBoard()[i, j] is GhostPiece)) return false;
            }
            return true;
        }
    }
    class Knight : ChessPiece
    {
        public Knight(int x, int y, bool isWhite) : base(x, y, isWhite) { } // Basic Constructor.
        public override string toString() { return base.toString() + "Knight"; }
        public override void Move(int x, int y, bool isWhiteTurn, ChessGame board) { // Moves Knight to (x,y).
            board.getBoard()[x, y] = new Knight(x, y, isWhiteTurn);
            base.Move(this.GetX(), this.GetY(), isWhiteTurn, board);
        }
        public override bool LegalMove(int x, int y, bool isWhiteTurn, ChessGame board){
            if (this.IsWhiteColor() != isWhiteTurn) return false; // Checks if moved piece belongs to the current player turn.
            if ((x == this.GetX()) || (y == this.GetY())) return false; // Handles obvious illegal move.
            if ((x == this.GetX() - 2 && y == this.GetY() - 1) || (x == this.GetX() - 1 && y == this.GetY() - 2) || (x == this.GetX() + 1 && y == this.GetY() - 2)
                || (x == this.GetX() + 2 && y == this.GetY() - 1) || (x == this.GetX() + 2 && y == this.GetY() + 1) || (x == this.GetX() + 1 && y == this.GetY() + 2)
                || (x == this.GetX() - 1 && y == this.GetY() + 2) || (x == this.GetX() - 2 && y == this.GetY() + 1)) {
                if ((board.getBoard()[x, y] is GhostPiece) || board.getBoard()[x, y].IsWhiteColor() != isWhiteTurn) return true; // Checks if (x,y) is empty or has enemy piece.
                else return false; // (x,y) has a piece of same color.
            }
            else
                return false;
        }
    }
    class Bishop : ChessPiece
    {
        public Bishop(int x, int y, bool isWhite) : base(x, y, isWhite) { } // Basic Constructor.
        public override string toString() { return base.toString() + "Bishop"; }
        public override void Move(int x, int y, bool isWhiteTurn, ChessGame board) { // Moves Bishop to (x,y).
            board.getBoard()[x, y] = new Bishop(x, y, isWhiteTurn);
            base.Move(this.GetX(), this.GetY(), isWhiteTurn, board);
        }
        public override bool LegalMove(int x, int y, bool isWhiteTurn, ChessGame board){
            if (this.IsWhiteColor() != isWhiteTurn) return false; // Checks if moved piece belongs to the current player turn.
            if (x == this.GetX() || y == this.GetY()) return false; // Checks if (x,y) in same row/col which is impossible.
                                                                    
            if (Math.Abs(x - this.GetX()) == Math.Abs(y - this.GetY())) { // Generic 4 way check.
                int jumpX, jumpY;
                if (x < this.GetX()) jumpX = -1; else jumpX = 1;
                if (y < this.GetY()) jumpY = -1; else jumpY = 1;
                for (int i = this.GetX() + jumpX, j = this.GetY() + jumpY; i != x; i += jumpX, j += jumpY)
                    if ( !(board.getBoard()[i, j] is GhostPiece) ) return false;
            }
            return true;
        }
    }
    class Queen : ChessPiece
    {
        public Queen(int x, int y, bool isWhite) : base(x, y, isWhite) { } // Basic Constructor.
        public override string toString() { return base.toString() + "Queen"; }
        public override void Move(int x, int y, bool isWhiteTurn, ChessGame board) // Moves Queen to (x,y).
        { board.getBoard()[x, y] = new Queen(x, y, isWhiteTurn); base.Move(this.GetX(), this.GetY(), false, board); }

        public override bool LegalMove(int x, int y, bool isWhiteTurn, ChessGame board){
            if (this.IsWhiteColor() != isWhiteTurn) return false; // Checks if moved piece belongs to the current player turn.
            if (new Rook(this.GetX(), this.GetY(), isWhiteTurn, false).LegalMove(x, y, isWhiteTurn, board) ||
                new Bishop(this.GetX(), this.GetY(), isWhiteTurn).LegalMove(x, y, isWhiteTurn, board))
                return true; // Queen movements similar to Rook/Bishop.
            else
                return false;
        }
    }
    class King : ChessPiece
    {
        bool didMove; // Defines if King was moved ( Relevant to Castling ).

        public King(int x, int y, bool isWhite, bool moved) : base(x, y, isWhite) { this.didMove = moved; } // Basic Constructor.
        public bool GetDidMove() { return this.didMove; }
        public override string toString() { return base.toString() + "King"; }
        public override void Move(int x, int y, bool isWhiteTurn, ChessGame board) { // Moves King to (x,y).
            board.getBoard()[x, y] = new King(x, y, isWhiteTurn, true);
            base.Move(this.GetX(), this.GetY(), false, board);
        }
        public override bool LegalMove(int x, int y, bool isWhiteTurn, ChessGame board){
            if (this.IsWhiteColor() != isWhiteTurn) return false; // Checks if moved piece belongs to the current player turn.
            if ((x == this.GetX() - 1 && y == this.GetY() - 1) || (x == this.GetX() - 1 && y == this.GetY()) || (x == this.GetX() - 1 && y == this.GetY() + 1)
                || (x == this.GetX() && y == this.GetY() - 1) || (x == this.GetX() && y == this.GetY() + 1) || (x == this.GetX() + 1 && y == this.GetY() - 1)
                || (x == this.GetX() + 1 && y == this.GetY()) || (x == this.GetX() + 1 && y == this.GetY() + 1)){
                if ((board.getBoard()[x, y] is GhostPiece) || board.getBoard()[x, y].IsWhiteColor() != isWhiteTurn) // Checks if (x,y) is empty or has enemy piece.
                    return true;
                else // (x,y) has a piece of same color.
                    return false;
            }
            else 
                return false;
        }
    }
    class GhostPiece : ChessPiece { public GhostPiece(int x, int y, bool color) : base(x, y, color) { } } // Defines empty cells.
}
using System;
using System.Collections;

namespace TicTacToe
{
    class Program
    {
        #region Nested Structures and Enumerations
        
        enum Player
        {
            Player1 = 1, 
            Player2 = 2
        }

        enum Cell
        {
            Empty = 0, 
            O = 1, 
            X = 2
        }

        enum Action
        {
            Play, 
            Quit
        }

        struct CellAddress
        {
            public byte Row;

            public byte Col;
        }

        struct Command
        {
            // User action (e.g. Play, Quit, etc.)
            public Action Action;

            // Contains cell address ONLY if Action = Action.Play
            public CellAddress Location;
        }

        #endregion

        #region Main Method

        static void Main(string[] args)
        {
            Cell[,] board = new Cell[3, 3];
            Player currentPlayer = Player.Player1;
            Command currentCommand;
            string errorMessage = string.Empty;

            while (true)
            {
                Console.Clear();

                DrawBoard(board);

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    PrintErrorMessage(errorMessage);
                    errorMessage = string.Empty;
                }
                PrintHelpMessage();

                if (!TryGetCommand(currentPlayer, out currentCommand))
                {
                    errorMessage = "Invalid command. Try again!";

                    // continue with the next iteration of the loop (same player)
                    continue;
                }
                else
                {
                    if (currentCommand.Action == Action.Play)
                    {
                        if (!TryPlay(currentPlayer, board, currentCommand.Location))
                        {
                            errorMessage = "Position already played. Please try again with different coordinates!";

                            // continue with the next iteration of the loop (same player)
                            continue;
                        }
                    }

                    if (currentCommand.Action == Action.Quit)
                    {
                        // exit the program with code 0 (e.g. Success)  
                        Environment.Exit(0);
                    }
                }

                // check if current player has one 
                if (GameOver(board))
                {
                    DrawBoard(board);

                    Console.WriteLine($"=> {currentPlayer} won!");

                    // exit the infinite loop
                    break;
                }

                if (!HasEmptyCells(board))
                {
                    Console.WriteLine($"=> Draw! Try again :)");

                    // exit the infinite loop
                    break;
                }


                // switch players for the next iteration
                currentPlayer = (currentPlayer == Player.Player1) ? Player.Player2 : Player.Player1;
            }

            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }

        #endregion

        #region Methods

        static void DrawBoard(Cell[,] board)
        {
            // represents a map / relation between the Cell enumeration 
            // and one character presentation when we print it on screen
            Hashtable cellMap = new Hashtable()
            {
                { Cell.Empty,    ' ' },
                { Cell.X,       'X' },
                { Cell.O,       'O' }
            };

            // header (coordinates)
            Console.WriteLine("        1   2   3   ");

            for (int row = 0; row < board.GetLength(0); row++)
            {
                // each row has top border
                Console.WriteLine("      #############");

                // left coordinates
                Console.Write($"    {row + 1} ");

                for (int col = 0; col < board.GetLength(1); col++)
                {
                    Console.Write("# {0} ", cellMap[board[row, col]]);
                }

                // right coordinates
                Console.WriteLine($"# {row + 1}");
            }
                     
            // footer (bottom boarder + coordinates)
            Console.WriteLine("      #############");
            Console.WriteLine("        1   2   3   ");
        }

        static void PrintErrorMessage(string msg)
        {
            // we need to preserve the original foreground color
            // so that we can return it after the error message in red is written
            var originalForegroundColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"=> {msg}");

            Console.ForegroundColor = originalForegroundColor;
        }

        static void PrintHelpMessage()
        {
            Console.WriteLine("=> Enter `<row><col>` as coordinates on the board OR `q` to quit");
        }

        static bool TryGetCommand(Player currentPlayer, out Command command)
        {
            // asking the user to provide a command
            Console.Write($"=> {currentPlayer}: ");

            // we are trimming and converting the input to lower case
            // so that later check are not accounting for lower / upper cases
            // e.g. trimming = removing spaces and tabulation in the 
            //                 beginning and at the end of the input string            
            string input = Console.ReadLine().Trim().ToLower();

            // checks if the user has provided only one character
            // and it is the letter 'q' (both need to be true)
            if (input.Length == 1 && input[0] == 'q')
            {
                command = new Command() { Action = Action.Quit };
                return true;
            }

            // checks if the user has provided only two characters
            // and both are digits (e.g. 0-9)
            if (input.Length == 2 && char.IsDigit(input[0]) && char.IsDigit(input[1]))
            {
                // parsing the first and second character as digits               
                byte row = byte.Parse(input[0].ToString());
                byte col = byte.Parse(input[1].ToString());

                // further check that x and y are valid coordinates,
                // which means their value is between 1 and 3 (e.g. values 1, 2, 3)
                if (row >= 1 && row <= 3 && col >= 1 && col <= 3)
                {
                    command = new Command()
                    {
                        Action = Action.Play,
                        Location = new CellAddress() { Row = row, Col = col }
                    };
                    return true;
                }
            }

            // in all other cases (e.g. the conditional states above are not passing)
            // we return false, which means the command is invalid and an empty 
            // command to the caller (e.g. the caller should not use it in this case)
            command = default(Command);
            return false;
        }

        static bool GameOver(Cell[,] board)
        {
            // check all combinations for winning the game (e.g. horizontal, vertical dialogonal with the same mark)
            // the first one that we find will return `true` to the caller
            
            // check if there is a vertical line of marks from the same kind
            for (byte x = 0; x < board.GetLength(0); x++)
            {
                if (board[x, 0] != Cell.Empty && board[x, 0] == board[x, 1] && board[x, 1] == board[x, 2])
                    return true;
            }

            // check if there is a horizontal line of marks from the same kind
            for (byte y = 0; y < board.GetLength(0); y++)
            {
                if (board[0, y] != Cell.Empty && board[0, y] == board[1, y] && board[1, y] == board[2, y])
                    return true;
            }

            // check both diagonals 
            if (board[0, 0] != Cell.Empty && board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2]) return true;
            if (board[0, 2] != Cell.Empty && board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0]) return true;


            // if we reach this point all combinations are invalid
            return false;
        }

        static bool HasEmptyCells(Cell[,] board)
        {
            // check that that there are still empty (e.g. Cell.Empty)
            for (byte row = 0; row < board.GetLength(0); row++)
            {
                for (byte col = 0; col < board.GetLength(1); col++)
                {
                    if (board[row, col] == Cell.Empty)
                        return true;
                }
            }

            return false;
        }

        static bool TryPlay(Player currentPlayer, Cell[,] board, CellAddress position)
        {
            // hint: we need to substract 1 from the coordinates so that they are 
            //       programmer oriented (e.g. count from 0) in contrast to normal 
            //       people (e.g. users) who count from 1 :)

            // check if the position is already played
            if (board[position.Row - 1,position.Col - 1] != Cell.Empty)
            {
                return false;
            }

            // otherwise place player's mark (e.g. X or O) at the given position
            board[position.Row - 1, position.Col - 1] = (currentPlayer == Player.Player1) ? Cell.O : Cell.X;

            return true;
        }

        #endregion
    }
}
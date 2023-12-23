using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client_TicTacToe
{
    internal class TicTacToe
    {
        private int[,] board = { { -1, -1, -1 }, { -1, -1, -1 }, { -1, -1, -1 } };
        private int turnCount = 1;
        private int whosTurn = 0; // 0 => X, 1 => O
        public bool isFinished = false;
        private void checkIfFinished() {
            //horizontal
            for(int i=0; i<3; i++) {
                if (board[i, 0] != -1 && board[i, 0] == board[i, 1] && board[i, 0] == board[i, 2]) {
                    isFinished = true; return;
                }
            }
            //vertical
            for (int i = 0; i < 3; i++) {
                if (board[0, i] != -1 && board[0, i] == board[1, i] && board[0, i] == board[2, i]){
                    isFinished = true; return;
                }
            }
            //diagonal
            if (board[1,1] == -1 && ((board[0,0] == board[1,1] && board[1,1] == board[2,2]) || (board[1,1] == board[0,2] && board[1,1] == board[2,0]))){
                isFinished = true;
            }
            
        }
        private void nextTurn() {
            whosTurn = whosTurn++%2;
        }
        private bool play(int x, int y) {
            if (board[x,y] == -1)
            {
                board[x,y] = whosTurn;
                checkIfFinished();
                if (!isFinished) {
                    nextTurn();
                    turnCount++;
                }
                return true;
            }
            return false;
        }
        public bool x_plays(int x, int y) {
            if (whosTurn != 0) return false;
            return play(x, y);
        }
        public bool y_plays(int x, int y) {
            if (whosTurn != 1) return false;
            return play(x, y);
        }
    }
}

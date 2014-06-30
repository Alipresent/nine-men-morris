using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NineMenMorris
{
    public partial class MainForm : Form
    {

        /// <summary>
        /// The PictureBox of each piece.
        /// </summary>
        public PictureBox[,,] pieces = new PictureBox[3,3,3];

        /// <summary>
        /// The location of the pieces.
        /// </summary>
        public Point[,,] locations = new Point[3, 3, 3];

        public MainForm()
        {
            InitializeComponent();
            UpdateToolStripStatus();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Pieces' locations.
            // First square.
            locations[0, 0, 0] = new Point(12, 12);
            locations[0, 0, 1] = new Point(210, 12);
            locations[0, 0, 2] = new Point(410, 12);
            locations[0, 1, 0] = new Point(12, 210);
            locations[0, 1, 2] = new Point(410, 210);
            locations[0, 2, 0] = new Point(12, 410);
            locations[0, 2, 1] = new Point(210, 410);
            locations[0, 2, 2] = new Point(410, 410);
            // Second square.
            locations[1, 0, 0] = new Point(75, 75);
            locations[1, 0, 1] = new Point(210, 75);
            locations[1, 0, 2] = new Point(340, 75);
            locations[1, 1, 0] = new Point(75, 210);
            locations[1, 1, 2] = new Point(340, 210);
            locations[1, 2, 0] = new Point(75, 340);
            locations[1, 2, 1] = new Point(210, 340);
            locations[1, 2, 2] = new Point(340, 340);
            // Third square.
            locations[2, 0, 0] = new Point(140, 140);
            locations[2, 0, 1] = new Point(210, 140);
            locations[2, 0, 2] = new Point(275, 140);
            locations[2, 1, 0] = new Point(140, 210);
            locations[2, 1, 2] = new Point(275, 210);
            locations[2, 2, 0] = new Point(140, 275);
            locations[2, 2, 1] = new Point(210, 275);
            locations[2, 2, 2] = new Point(275, 275);
            // Generating pieces.
            for (int i = 0; i != 3; ++i)
            {
                for (int j = 0; j != 3; ++j)
                {
                    for (int k = 0; k != 3; ++k)
                    {
                        if ((j == 1) && (k == 1))
                        {
                            continue;
                        }
                        pieces[i, j, k] = new PictureBox();
                        pieces[i, j, k].Parent = this;
                        pieces[i, j, k].BackColor = Color.Transparent;
                        pieces[i, j, k].Location = locations[i, j, k];
                        pieces[i, j, k].Size = new Size(32, 32);
                        pieces[i, j, k].Tag = new int[] { i, j, k, 0 };
                        pieces[i, j, k].Click += new EventHandler(OnPieceClick);
                    }
                }
            }
        }

        private void OnPieceClick(object o, EventArgs e)
        {
            PictureBox sender = (PictureBox)o;
            int[] tag = (int[])sender.Tag;
            int playerToMove = GetPlayerToMove();
            bool gameStateChanged = false;
            // One of the player has to remove a piece. 
            if (Program.hasToRemove != 0)
            {
                if ((GetPieceOwner(sender) != 0) && (GetPieceOwner(sender) != Program.hasToRemove))
                {
                    SetPieceStatus(sender, 0, false);
                    --Program.piecesLeft[GetOpponentPlayerID(Program.hasToRemove)];
                    Program.hasToRemove = 0;
                }
            } 
            // Players are still placing their pieces.
            else if ((Program.pieces[1] > 0) || (Program.pieces[2] > 0))
            {
                if (tag[3] == 0)
                {
                    SetPieceStatus(sender, playerToMove, false);
                    gameStateChanged = true;
                    --Program.pieces[playerToMove];
                    ++Program.piecesLeft[playerToMove];
                    ++Program.turnNo;
                }
            }
            // Players are still fighting.
            else if ((Program.piecesLeft[1] > 2) && (Program.piecesLeft[2] > 2))
            {
                // Player selects a piece.
                if (Program.selectedTag == null)
                {
                    if (tag[3] == playerToMove)
                    {
                        SetPieceStatus(sender, playerToMove, true);
                    }
                }
            	// Player chooses the destination.
                else
                {
                    PictureBox from = pieces[Program.selectedTag[0], Program.selectedTag[1], Program.selectedTag[2]];
                    int delta =
                        Math.Abs(tag[0] - Program.selectedTag[0]) +
                        Math.Abs(tag[1] - Program.selectedTag[1]) +
                        Math.Abs(tag[2] - Program.selectedTag[2]);
                    // Checking if the destination is empty and in range of this piece.
                    if ((tag[3] == 0) && (delta == 1))
                    {
                        SetPieceStatus(sender, Program.selectedTag[3], false);
                        SetPieceStatus(from, 0, false);
                        gameStateChanged = true;
                        ++Program.turnNo;
                    }
                    else
                    {
                        SetPieceStatus(from, Program.selectedTag[3], false);
                    }
                    // Deletes the selection.
                    Program.selectedTag = null;
                }
            }
            // If any changes were made...
            if (gameStateChanged)
            {
                // .. we are looking for mills.
                if (CheckForMills(tag[0], tag[1], tag[2]))
                {
                    Program.hasToRemove = playerToMove;
                }
            }
            UpdateToolStripStatus();
            DoVictoryCheck();
        }

        private void SetPieceStatus(PictureBox p, int player, bool isSelected)
        {
            int[] tag = (int[])p.Tag;
            if (player == 0)
            {
                p.Image = null;
                tag[3] = 0;
            }
            else
            {
                // Updating the image.
                if (isSelected)
                {
                    if (player == 1)
                    {
                        p.Image = global::NineMenMorris.Properties.Resources.selected1;
                    }
                    else
                    {
                        p.Image = global::NineMenMorris.Properties.Resources.selected2;
                    }
                    Program.selectedTag = tag;
                }
                else
                {
                    if (player == 1)
                    {
                        p.Image = global::NineMenMorris.Properties.Resources.player1;
                    }
                    else
                    {
                        p.Image = global::NineMenMorris.Properties.Resources.player2;
                    }
                    Program.selectedTag = null;
                }
                // Updating the owner.
                tag[3] = player;
            }
        }

        private bool CheckForMills(int x, int y, int z)
        {
            int player = GetPieceOwner(pieces[x, y, z]);
            if (player == 0)
            {
                throw new ArgumentException();
            }
            bool ok1 = true, ok2 = false, ok3 = false;
            // Between squares.
            for (int i = 0; i != 3; ++i)
            {
                if (player != GetPieceOwner(pieces[i, y, z]))
                {
                    ok1 = false;
                    break;
                }
            }
            // Horizontal.
            if (y != 1)
            {
                ok2 = true;
                for (int k = 0; k != 3; ++k)
                {
                    if (player != GetPieceOwner(pieces[x, y, k]))
                    {
                        ok2 = false;
                        break;
                    }
                }
            }
            // Vertical.
            if (z != 1)
            {
                ok3 = true;
                for (int j = 0; j != 3; ++j)
                {
                    if (player != GetPieceOwner(pieces[x, j, z]))
                    {
                        ok3 = false;
                        break;
                    }
                }
            }
            return ok1 || ok2 || ok3;
        }

        private int GetPieceOwner(PictureBox pb)
        {
            int[] tag = (int[]) pb.Tag;
            return tag[3];
        }

        private void DoVictoryCheck()
        {
            // There are still pieces to be put, the game couldn't have finished.
            if ((Program.pieces[1] > 0) || (Program.pieces[2] > 0))
            {
                return;
            }
            int winner = 0;
            if (Program.piecesLeft[1] < 3)
            {
                winner = 2;
            }
            else if (Program.piecesLeft[2] < 3)
            {
                winner = 1;
            }
            if (winner != 0)
            {
                toolStripStatusLabel1.Text = GetPlayerNameByID(winner) + " a castigat!";
                toolStripStatusLabel2.Text = "";
                MessageBox.Show(GetPlayerNameByID(winner) + " a castigat!");
            }
        }

        private int GetPlayerToMove()
        {
            return (Program.turnNo % 2) + 1;
        }

        private int GetOpponentPlayerID(int playerId)
        {
            if (playerId == 1)
            {
                return 2;
            }
            else if (playerId == 2)
            {
                return 1;
            }
            throw new ArgumentException();
        }

        private string GetPlayerNameByID(int playerId)
        {
            if (playerId == 1)
            {
                return "Red";
            }
            else if (playerId == 2)
            {
                return "Blue";
            }
            throw new ArgumentException();
        }

        private void UpdateToolStripStatus()
        {
            // Label 1.
            if (Program.hasToRemove != 0)
            {
                toolStripStatusLabel1.Text = "Player " + Program.hasToRemove + " has to remove a piece of the opponent.";
            }
            else
            {
                int playerToMove = GetPlayerToMove();
                if (Program.pieces[playerToMove] > 0)
                {
                    toolStripStatusLabel1.Text = GetPlayerNameByID(playerToMove) + " has to place a piece.";
                }
                else
                {
                    toolStripStatusLabel1.Text = GetPlayerNameByID(playerToMove) + " has to move.";
                }
            }
            // Label 2.
            toolStripStatusLabel2.Text = 
                GetPlayerNameByID(1) + ": " + Program.piecesLeft[1].ToString() + " (" + Program.pieces[1].ToString() + ")" + 
                " // " + 
                GetPlayerNameByID(2) + ": " + Program.piecesLeft[2].ToString() + " (" + Program.pieces[2].ToString() + ")";
        }
    }
}

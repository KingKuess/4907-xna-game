using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kuessaria
{
    class Map// this class is for creating the tiles of the background
    {
        private List<CollisionTiles> collisionTiles = new List<CollisionTiles>();// this holds all the tiles

        public List<CollisionTiles> CollisionTiles//this encapsulation allows public access to the list information
        {
            get { return collisionTiles; }
        }

        private int width, height;// these hold the map width and height

        public int Width//these makes those values publicly accesible
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public Map()
        {

        }
        public char[,] Read(string FileName)// this reads in the map from a file
        {
            string[] tempMap;// this holds the entire rows of the map
            tempMap = System.IO.File.ReadAllLines(FileName);// this fills the tempMap with the rows
            char[] TempX;//this will hold the characters in each row, o
            char[,] X = new char[tempMap[0].ToCharArray().Length, tempMap.Length];//this is the int array that will be returned
            for (int i = 0; i < tempMap.Length; i++)//for the length of y
            {
                TempX = tempMap[i].ToCharArray();//set TempX to the new row in a char array so that its got all the X
                for (int j = 0; j < TempX.Length; j++)// for the length of the X
                {
                    X[j, i] = (TempX[j]);//set int[] X at position j,i to hold whatever value is at the position x(j) in row TempX(i)
                }
            }
            return X;//returns the finished array
        }

        public void Generate(char[,] map, int size)//this is what generates the map, using int size for the size of the block and map is an integer value equivalent to the block
        {
            //1 = grassblock
            //2=dirtblock
            //3 = stone block
            for (int x = 0; x < map.GetLength(0); x++)//this runs for the length of the x
            {
                for (int y = 0; y < map.GetLength(1); y++)//this runs for the length of the y
                {
                    char tile = map[x, y];// sets number to the value of map at position X Y

                    if (tile != '0')// if the number isnt 0
                        CollisionTiles.Add(new CollisionTiles(tile, new Rectangle(x * size, y * size, size, size)));// it adds a new tile of type number at position x any y * size
                    //if (tile == 'D')
                    //{
                    //    System.Windows.Forms.MessageBox.Show(collisionTiles.Count.ToString());
                    //}
                    width = (x + 1) * size;//sets the width value that holds the size of the map based on how many blocks are loaded
                    height = (y + 1) * size;// same but for y
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Color color)//this just draws the tiles
        {
            foreach (CollisionTiles tile in collisionTiles)
            {
                if (tile.tileChar == '1' || tile.tileChar == '2' || tile.tileChar == '3')
                {
                    tile.Draw(spriteBatch, color);
                }
                else
                {
                    tile.Draw(spriteBatch, Color.White);
                }
            }
        }
    }
}

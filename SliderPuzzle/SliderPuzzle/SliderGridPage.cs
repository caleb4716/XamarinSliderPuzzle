using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

using Xamarin.Forms;

namespace SliderPuzzle
{
    public class SliderGridPage : ContentPage
    {

        private const int SIZE = 4;
        private Label wonLabel;
        private int shuffleTimes = 20;
        private bool easySolveShuffle = true;


        private AbsoluteLayout _absoluteLayout;
        private Dictionary<GridPosition, GridItem> _gridItems;

        public SliderGridPage()
        {
            wonLabel = new Label
            {
                Text = "Game Won: False"
            };
            _gridItems = new Dictionary<GridPosition, GridItem>();
            _absoluteLayout = new AbsoluteLayout
            {
                BackgroundColor = Color.Blue,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            var counter = 1;
            for (var row = 0; row < 4; row++)
            {

                for (var col = 0; col < 4; col++)
                {

                    GridItem item;

                    if(counter == 16)
                    {
                        item = new GridItem(new GridPosition(row, col), counter, true);
                    } else
                    {
                        item = new GridItem(new GridPosition(row, col), counter, false);
                    }
                    

                    var tapRecognizer = new TapGestureRecognizer();
                    tapRecognizer.Tapped += OnLabelTapped;
                    item.GestureRecognizers.Add(tapRecognizer);

                    _gridItems.Add(item.Position, item);
                    _absoluteLayout.Children.Add(item);

                    counter++;

                }

            }
            //Test
            _absoluteLayout.Children.Add(wonLabel);
            //Endtest
            ContentView contentView = new ContentView
            {
                Content = _absoluteLayout
            };
            contentView.SizeChanged += OnContentViewSizeChanged;
            this.Padding = new Thickness(5, Device.OnPlatform(25, 5, 5), 5, 5);
            this.Content = contentView;

            shufflePieces();

        } //End SliderGridPage constructor

        void OnContentViewSizeChanged(object sender, EventArgs args)
        {

            ContentView contentView = sender as ContentView;
            double squareSize = Math.Min(contentView.Width, contentView.Height) / 4;

            for (var row = 0; row < 4; row++)
            {

                for (var col = 0; col < 4; col++)
                {

                    GridItem item = _gridItems[new GridPosition(row, col)];
                    Rectangle rect = new Rectangle(col * squareSize, row * squareSize, squareSize, squareSize);
                    AbsoluteLayout.SetLayoutBounds(item, rect);

                }

            }

        }

        void OnLabelTapped(object sender, EventArgs args)
        {

            GridItem item = sender as GridItem;
            int row = 0;
            int col = 0;

            
            if(emptyIsAdjacent(item.Position, out row, out col))
            {
                GridItem swapWith = _gridItems[new GridPosition(row, col)];
                Swap(item, swapWith);
                OnContentViewSizeChanged(this.Content, null);
                //win stuff
                wonLabel.Text = "Game Won: " + gameIsWon().ToString();
                if (gameIsWon())
                {
                    _gridItems[new GridPosition(3, 3)].Source = ImageSource.FromResource("SliderPuzzle.17.jpeg");
                }
            }

        } //End OnLabelTapped

        void Swap(GridItem item1, GridItem item2)
        {

            //First Swap Positions
            GridPosition temp = item1.Position;
            item1.Position = item2.Position;
            item2.Position = temp;

            //Update dictionary too
            _gridItems[item1.Position] = item1;
            _gridItems[item2.Position] = item2;

        }




        class GridItem : Image
        {
            public GridPosition Position
            {
                get;
                set;
            }

            public bool isEmpty
            {
                get;
                set;
            }
            public int checkWinId
            {
                get;
                set;
            }

            public GridItem(GridPosition position, int id, bool empty)
            {
                Position = position;
                checkWinId = id;
                isEmpty = empty;
                String path = "SliderPuzzle." + id.ToString() + ".jpeg";
                Source = ImageSource.FromResource(path);
                HorizontalOptions = LayoutOptions.Center;
                VerticalOptions = LayoutOptions.Center;
            }
        }

        class GridPosition
        {

            public int Row
            {
                get;
                set;
            }

            public int Column
            {
                get;
                set;
            }

            public GridPosition(int row, int col)
            {
                Row = row;
                Column = col;
            }

            public override bool Equals(object obj)
            {
                GridPosition other = obj as GridPosition;

                if (other != null && this.Row == other.Row && this.Column == other.Column)
                {
                    return true;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return 17 * 23 + this.Row.GetHashCode() * 23 + this.Column.GetHashCode();
            }

        }


        //Part 2 methods

        private bool emptyIsAdjacent(GridPosition pos, out int r, out int c)
        {
            GridItem emptyCheckItem;
            bool isAdjacent = false;
            r = 0;
            c = 0;

            if(_gridItems.TryGetValue(new GridPosition(pos.Row - 1, pos.Column), out emptyCheckItem)) //If north GridItem exists, assign to emptyCheckItem
            {
                if (emptyCheckItem.isEmpty)
                {
                    isAdjacent = true;
                    r = pos.Row - 1;
                    c = pos.Column;
                    return isAdjacent;
                }
            }

            if (_gridItems.TryGetValue(new GridPosition(pos.Row + 1, pos.Column), out emptyCheckItem)) //If south GridItem exists, assign to emptyCheckItem
            {
                if (emptyCheckItem.isEmpty)
                {
                    isAdjacent = true;
                    r = pos.Row + 1;
                    c = pos.Column;
                    return isAdjacent;
                }
            }

            if (_gridItems.TryGetValue(new GridPosition(pos.Row, pos.Column + 1), out emptyCheckItem)) //If east GridItem exists, assign to emptyCheckItem
            {
                if (emptyCheckItem.isEmpty)
                {
                    isAdjacent = true;
                    r = pos.Row;
                    c = pos.Column + 1;
                    return isAdjacent;
                }
            }

            if (_gridItems.TryGetValue(new GridPosition(pos.Row, pos.Column - 1), out emptyCheckItem)) //If west GridItem exists, assign to emptyCheckItem
            {
                if (emptyCheckItem.isEmpty)
                {
                    isAdjacent = true;
                    r = pos.Row;
                    c = pos.Column - 1;
                    return isAdjacent;
                }
            }
            return isAdjacent;
        }

        private bool gameIsWon()
        {

            int idCounter = 1;
            bool won = true;

            for(int r = 0; r < 4; r++)
            {
                for(int c = 0; c < 4; c++)
                {

                    if (_gridItems[new GridPosition(r, c)].checkWinId != idCounter) //Position contains GridItem with incorrect id
                    {
                        won = false;
                        return won;
                    } else //Position contains GridItem with correct id
                    {
                        idCounter++;
                    }

                }
            }
            return won;
        }

        private void shufflePieces()
        {
            Random rnd = new Random();
            if (easySolveShuffle)
            {
                Swap(_gridItems[new GridPosition(3, 3)], _gridItems[new GridPosition(2, 3)]);
                Swap(_gridItems[new GridPosition(2, 3)], _gridItems[new GridPosition(1, 3)]);
            } else
            {
                for (int i = 0; i < shuffleTimes; i++)
                {
                    GridPosition pos1 = new GridPosition(rnd.Next(0, 4), rnd.Next(0, 4));
                    GridPosition pos2 = new GridPosition(rnd.Next(0, 4), rnd.Next(0, 4));
                    if (!pos1.Equals(pos2))//make sure the positions are not the same
                    {
                        Swap(_gridItems[pos1], _gridItems[pos2]);
                    }
                }
            }
            
        }
        
    }
}

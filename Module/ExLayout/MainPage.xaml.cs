namespace ExLayout
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();

            for (int i = 0; i < 7; i++)
            {
                CreateGraySquare(0, i);
            }

            for (int i = 0; i < 7; i++)            {
                if (i != 3) { 
                    CreateGraySquare(1, i);
                }
            }


            Grid.SetColumn(tooth_brocken, 3);
            Grid.SetRow(tooth_brocken, 1);

            for (int i = 0; i < 2; i++) {
                Border border = new Border
                {
                    Stroke = Color.FromRgb(255, 255, 255),

                    Content = new Label
                    {
                        WidthRequest = 25,
                        HeightRequest = 50,
                        BackgroundColor = Color.FromRgb(230, 230, 230),

                    }
                };

                tooth_brocken.Add(border, i, 0);
            }

        }

        private void CreateGraySquare(int row, int colomn)
        {
            Border border = new Border
            {
                Stroke = Color.FromRgb(255, 255, 255),

                Content = new Label
                {
                    WidthRequest = 50,
                    HeightRequest = 50,
                    BackgroundColor = Color.FromRgb(211, 211, 211),
                    
                }
            };

            
            teeth_grid.Add(border, colomn, row);
        }

    }

}

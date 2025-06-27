using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using System;

namespace Kesmai.WorldForge.Windows
{
    public class TextInputWindow : Window
    {
        public string InputText => _input.Text;
        public bool IsConfirmed { get; private set; }

        private readonly TextBox _input;

        public TextInputWindow( string prompt, string title )
        {
            Title = title;
            IsConfirmed = false;

            Width = 300;
            Height = 150;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;

            var root = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Vector4F( 10 ),
                Style = "DarkCanvas"
            };

            root.Children.Add( new TextBlock
            {
                Text = prompt,
                Font = "Tahoma",
                FontSize = 12,
                Margin = new Vector4F( 5 ),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            } );

            _input = new TextBox
            {
                Margin = new Vector4F( 5 ),
                Width = 250,
                MaxLength = 50
            };
            root.Children.Add( _input );

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Vector4F( 5 )
            };

            var okButton = new Button
            {
                Content = new TextBlock
                {
                    Text = "OK",
                    Font = "Tahoma",
                    FontSize = 10,
                    Foreground = Color.LightGreen,
                    Stroke = Color.Black,
                    FontStyle = MSDFStyle.Outline,
                    Margin = new Vector4F( 5 )
                }
            };
            okButton.Click += ( _, _ ) =>
            {
                IsConfirmed = true;
                Close();
            };

            var cancelButton = new Button
            {
                Content = new TextBlock
                {
                    Text = "Cancel",
                    Font = "Tahoma",
                    FontSize = 10,
                    Foreground = Color.OrangeRed,
                    Stroke = Color.Black,
                    FontStyle = MSDFStyle.Outline,
                    Margin = new Vector4F( 5 )
                }
            };
            cancelButton.Click += ( _, _ ) =>
            {
                IsConfirmed = false;
                Close();
            };

            buttonPanel.Children.Add( okButton );
            buttonPanel.Children.Add( cancelButton );
            root.Children.Add( buttonPanel );

            Content = root;
        }
    }
}

using System;
using System.Windows;

namespace Kesmai.Prototype.VirtualFileTree
{
    public partial class MainWindow : Window
    {
        private readonly SegmentSolution _solution = new();

        public MainWindow()
        {
            InitializeComponent();
            _solution.Name = "Example";
            _solution.RootPath = @"C:\\Example";
            FileTree.Solution = _solution;
        }
    }
}

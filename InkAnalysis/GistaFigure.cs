using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace InkAnalysis
{
    class GistaFigure:FrameworkElement
    {
        FormattedText ft;
        Point cent;
        private GistaFigure() { }
        public GistaFigure(string name, Point center, double size, Brush color)
        {
            init(name, center, color, size);
        }
        public GistaFigure(string name, Point center, Brush color)
        {
            init(name,center,color, 12);

        }

        void init(string name, Point center, Brush color, double size)
        { 
            ft = new FormattedText(
                name, System.Globalization.CultureInfo.CurrentCulture,
               FlowDirection.LeftToRight,
               new Typeface(new FontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), size, color);

           
            cent = center;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            cent.X -= ft.Width / 2;
            cent.Y -= ft.Height / 2;
            drawingContext.DrawText(ft,cent);

        }
    }
}

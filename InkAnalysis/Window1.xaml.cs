using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Windows.Ink;
using System.ComponentModel;


namespace InkAnalysis
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>

    public partial class Window1 : System.Windows.Window
    {
        private AnalysisHintNode hint;
        

        public Window1()
        {
            InitializeComponent();
        }

        InkAnalyzer m_analyzer;
        void onLoaded(object sender, RoutedEventArgs e)
        {
            m_analyzer = new InkAnalyzer();
            m_analyzer.AnalysisModes = AnalysisModes.AutomaticReconciliationEnabled;
            m_analyzer.ResultsUpdated += new ResultsUpdatedEventHandler(m_analyzer_ResultsUpdated);

            string[] wordListArray = { "+", "-", "x", "/", "=", };
            //00D7 = Multiplication X
            //2217 = Multiplication *
            //00F7 = Divide Sign
            //00B2 = ^2


            hint = m_analyzer.CreateAnalysisHint();
            hint.Location.MakeInfinite();
            hint.Factoid = "(NUMBER)";
            hint.SetWordlist(wordListArray);
            hint.Name = "Wordlist";


        }

        void OnGesture(object sender, InkCanvasGestureEventArgs e)
        {
            ApplicationGesture topGesture = e.GetGestureRecognitionResults()[0].ApplicationGesture;
            if (topGesture == ApplicationGesture.ScratchOut)
            {
                StrokeCollection strokesToDelete = myInkCanvas.Strokes.HitTest(e.Strokes.GetBounds(), 1);
                myInkCanvas.Strokes.Remove(strokesToDelete);
            }
            else
            {
                e.Cancel = true;
            }
        }

        void onStrokeErasing(object sender, InkCanvasStrokeErasingEventArgs e)
        {
            m_analyzer.RemoveStroke(e.Stroke);
        }

       

        void onStrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            m_analyzer.AddStroke(e.Stroke);
            m_analyzer.BackgroundAnalyze();
        }

        GistaFigure temp;
        void m_analyzer_ResultsUpdated(object sender, ResultsUpdatedEventArgs e)
        {
            if (e.Status.Successful)
            {
                ContextNodeCollection nodes = ((InkAnalyzer)sender).FindLeafNodes();
                foreach (ContextNode node in nodes)
                {
                    if (node is InkWordNode)
                    {
                        InkWordNode t = node as InkWordNode;
                        String text = t.GetRecognizedString();
                        Rect l = t.Location.GetBounds();
                        Point a = new Point(l.Left + l.Width / 2, l.Top + l.Height / 2);
                        double de = l.Height;
                        Brush be = Brushes.Green;
                        

                        switch (t.InkRecognitionConfidence)
                        { 
                            case InkRecognitionConfidence.Intermediate:
                                be = Brushes.Yellow;
                                break;
                            case InkRecognitionConfidence.Poor:
                                be = Brushes.Red;
                                break;
                            case InkRecognitionConfidence.Unknown:
                                be = Brushes.Brown;
                                break;
                        }

                        if (text.IndexOf("=") != -1)
                        {
                            List<int> operatorLocations = new List<int>();
                            List<string> piece = new List<string>();
                            double result;

                            //grab all operation except =
                            for (int i = 0; i < text.Length; i++)
                            {
                                if (text.Substring(i, 1) == "+" || text.Substring(i, 1) == "-" || text.Substring(i, 1) == "x" || text.Substring(i, 1) == "*" || text.Substring(i, 1) == "/")
                                {
                                    operatorLocations.Add(i);
                                }
                            }

                            //Grab index of = sign in text, assuming there is only be 1
                            operatorLocations.Add(text.IndexOf("="));

                            //Grab first number
                            piece.Add( text.Substring(0, operatorLocations[0]));
                            //grab number after each operator
                            for (int i = 0; i < operatorLocations.Count - 1; i++)
                            {
                                piece.Add(text.Substring(operatorLocations[i] + 1, operatorLocations[i + 1] - (operatorLocations[i] + 1)));
                            }
                            for (int i = 0; i < operatorLocations.Count - 1; i++)
                            {
                                double tempResult = 0;
                                //operate
                                if (text.Substring(operatorLocations[i], 1) == "x" || text.Substring(operatorLocations[i], 1) == "*")
                                {
                                    tempResult = Convert.ToDouble(piece[i]) * Convert.ToDouble(piece[i+1]);
                                }
                                if (text.Substring(operatorLocations[i], 1) == "/")
                                {
                                    tempResult = Convert.ToDouble(piece[i]) / Convert.ToDouble(piece[i+1]);
                                }
                                //Rebuild string
                                if (text.Substring(operatorLocations[i], 1) == "x" || text.Substring(operatorLocations[i], 1) == "*" || text.Substring(operatorLocations[i], 1) == "/")
                                {
                                    int originalLength = text.Length;
                                    if ( i != 0 )
                                        text = text.Substring(0, operatorLocations[i - 1] + 1) + Convert.ToString(tempResult) + text.Substring(operatorLocations[i + 1]);
                                    else
                                        text = Convert.ToString(tempResult) + text.Substring(operatorLocations[i + 1]);
                                    piece.RemoveAt(i);
                                    piece.RemoveAt(i);
                                    operatorLocations.RemoveAt(i);
                                    for (int j = i; j < operatorLocations.Count; j++)
                                    {
                                        operatorLocations[j] -= originalLength - text.Length;
                                    }
                                    if (i != 0)
                                        if (piece.Count > i)
                                            piece.Insert(i, text.Substring(operatorLocations[i - 1] + 1, operatorLocations[i] - (operatorLocations[i - 1] + 1)));
                                        else
                                            piece.Add(text.Substring(operatorLocations[i - 1] + 1, operatorLocations[i] - (operatorLocations[i - 1] + 1)));
                                    else
                                        piece.Add(text.Substring(0, operatorLocations[0]));

                                            
                                    i--;
                                }
                                
                            }
                            //find result
                            result = Convert.ToDouble(piece[0]);
                            for (int i = 0; i < operatorLocations.Count - 1; i++)
                            {
                                if (text.Substring(operatorLocations[i], 1) == "+")
                                {
                                    result += Convert.ToDouble(piece[i + 1]);
                                }
                                if (text.Substring(operatorLocations[i], 1) == "-")
                                {
                                    result -= Convert.ToDouble(piece[i + 1]);
                                }

                            }
                            //text += " " + Convert.ToString(result);
                            result = Math.Round(result, 3);
                            a = new Point(l.Right + ((l.Width / text.Length)), l.Top + l.Height / 2);
                            text = Convert.ToString(result);
                        }

                            myInkCanvas.Children.Remove(temp);
                            GistaFigure figure = new GistaFigure(text, a,de , be);
                            temp = figure;
                            myInkCanvas.Children.Add(figure);

                        
                    }
                    
                }
            }
        }

        private void myInkCanvas_Gesture(object sender, InkCanvasGestureEventArgs e)
        {

        }
    }
}
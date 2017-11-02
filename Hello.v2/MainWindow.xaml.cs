using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;

namespace Hello.v2
{
    public partial class MainWindow : Window
    {
        MediaPlayer mplayer = new MediaPlayer();
        Random r = new Random();
        private List<int> used = new List<int> { };
        private List<string> RNGpool_A = new List<string> { "a1", "a2", "a3", "a4", "a5", "a6", "a7" };
        private List<string> RNGpool_B = new List<string> { "b1", "b2", "b3", "b4", "b5", "b6", "b7" };
        private List<string> RNGpool_C = new List<string> { "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8" };
        private List<string> RNGpool_D = new List<string> { "d1", "d2", "d3", "d4", "d5", "d6", "d7" };
        private List<string> RNGpool_E = new List<string> { "e1", "e2", "e3", "e4", "e5", "e6", "e7" };
        private List<string> RNGpool_F = new List<string> { "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8" };
        private List<string> RNGpool_G = new List<string> { "g1", "g2", "g3", "g4", "g5", "g6", "g7" };
        private List<string> RNGpool_H = new List<string> { "h1", "h2", "h3", "h4", "h5", "h6", "h7" };
        private List<string> RNGpool_I = new List<string> { "i1", "i2", "i3", "i4", "i5", "i6", "i7", "i8" };
        private KinectSensor sensor;
        private List<Span> recognitionSpans;
        private SpeechRecognitionEngine speechEngine;

        private const string MediumGreyBrushKey = "MediumGreyBrush";
        private int seed, score = 0;

        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                try
                {
                    // Start the sensor!
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    // Some other application is streaming from the same Kinect sensor
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                //this.statusBarText.Text = Properties.Resources.NoKinectReady;
                return;
            }

            RecognizerInfo ri = GetKinectRecognizer();

            if (null != ri)
            {
                recognitionSpans = new List<Span> { };

                this.speechEngine = new SpeechRecognitionEngine(ri.Id);

                using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(Properties.Resources.SpeechGrammar)))
                {
                    var g = new Grammar(memoryStream);
                    speechEngine.LoadGrammar(g);
                }

                speechEngine.SpeechRecognized += SpeechRecognized;
                speechEngine.SpeechRecognitionRejected += SpeechRejected;
                speechEngine.SetInputToAudioStream(
                    sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
        }

        public MainWindow()
        {
            mplayer.Open(new Uri("../../../Soundtrack/Casa Bossa Nova.mp3", UriKind.RelativeOrAbsolute));
            mplayer.MediaEnded += new EventHandler(Media_Ended);
            mplayer.Play();

            InitializeComponent();
        }

        private void ClearRecognitionHighlights()
        {
            foreach (Span span in recognitionSpans)
            {
                span.Foreground = (Brush)this.Resources[MediumGreyBrushKey];
                span.FontWeight = FontWeights.Normal;
            }
        }

        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            const double ConfidenceThreshold = 0;

            /*if (Mainmenu.Visibility == Visibility.Hidden || Level.Visibility == Visibility.Hidden ||
                star1.Visibility == Visibility.Hidden || star2.Visibility == Visibility.Hidden ||
                star3.Visibility == Visibility.Hidden)
                pause.Visibility = Visibility.Visible;
            else
                pause.Visibility = Visibility.Hidden;*/

            ClearRecognitionHighlights();
            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                switch (e.Result.Semantics.Value.ToString())
                {
                    //FUNCTIONS
                    case "START":
                        if (Mainmenu.Visibility == Visibility.Visible)
                        {
                            Mainmenu.Visibility = Visibility.Hidden;
                            Level.Visibility = Visibility.Visible;
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "CREDITS":
                        if (Mainmenu.Visibility == Visibility.Visible)
                        {
                            Mainmenu.Visibility = Visibility.Hidden;
                            Credits.Visibility = Visibility.Visible;
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "BACK":
                        if (Credits.Visibility == Visibility.Visible)
                        {
                            Credits.Visibility = Visibility.Hidden;
                            Mainmenu.Visibility = Visibility.Visible;
                        }
                        else if (Help.Visibility == Visibility.Visible)
                        {
                            Help.Visibility = Visibility.Hidden;
                            Mainmenu.Visibility = Visibility.Visible;
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "MENU":
                        if (star1.Visibility == Visibility.Visible || star2.Visibility == Visibility.Visible || star3.Visibility == Visibility.Visible || pausemenu.Visibility == Visibility.Visible)
                        {
                            foreach (Grid g in container.Children)
                                g.Visibility = Visibility.Hidden;
                            Mainmenu.Visibility = Visibility.Visible;
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "CLOSE":
                    case "EXIT":
                        if (star1.Visibility == Visibility.Visible || star2.Visibility == Visibility.Visible || star3.Visibility == Visibility.Visible || pausemenu.Visibility == Visibility.Visible)
                            exitprompt.Visibility = Visibility.Visible;
                        ClearRecognitionHighlights();
                        break;

                    case "YES":
                        if (exitprompt.Visibility == Visibility.Visible)
                            this.Close();
                        ClearRecognitionHighlights();
                        break;

                    case "NO":
                        if (exitprompt.Visibility == Visibility.Visible)
                            exitprompt.Visibility = Visibility.Hidden;
                        ClearRecognitionHighlights();
                        break;
                    case "HELP":
                        if (Mainmenu.Visibility == Visibility.Visible)
                        {
                            Help.Visibility = Visibility.Visible;
                            Mainmenu.Visibility = Visibility.Hidden;
                        }
                        break;

                    case "PAUSE":
                        if (Mainmenu.Visibility == Visibility.Hidden && Level.Visibility == Visibility.Hidden &&
                star1.Visibility == Visibility.Hidden && star2.Visibility == Visibility.Hidden &&
                star3.Visibility == Visibility.Hidden)
                        {
                            pausemenu.Visibility = Visibility.Visible;
                            pause.Visibility = Visibility.Hidden;
                        }
                        ClearRecognitionHighlights();
                        break;

                    //LEVEL 1,4,7
                    case "ONE":
                        if (Level.Visibility == Visibility.Visible)
                        {
                            Level.Visibility = Visibility.Hidden;

                            seed = r.Next(0, 7);
                            used.Add(seed);
                            getAnimal("A");
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "FOUR":
                        if (Level.Visibility == Visibility.Visible)
                        {
                            Level.Visibility = Visibility.Hidden;

                            seed = r.Next(0, 7);
                            used.Add(seed);
                            getAnimal("D");
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "SEVEN":
                        if (Level.Visibility == Visibility.Visible)
                        {
                            Level.Visibility = Visibility.Hidden;

                            seed = r.Next(0, 7);
                            used.Add(seed);
                            getAnimal("G");
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "CAT":
                        if (a1.Visibility == Visibility.Visible || d1.Visibility == Visibility.Visible || g1.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            if (a1.Visibility == Visibility.Visible)
                                getAnimal("A");
                            else if (d1.Visibility == Visibility.Visible)
                                getAnimal("D");
                            else if (g1.Visibility == Visibility.Visible)
                                getAnimal("G");
                        }
                        if (used.Count == 7)
                            getScore();
                        break;
                    case "DOG":
                        if (a2.Visibility == Visibility.Visible || d2.Visibility == Visibility.Visible || g2.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            if (a2.Visibility == Visibility.Visible)
                                getAnimal("A");
                            else if (d2.Visibility == Visibility.Visible)
                                getAnimal("D");
                            else if (g2.Visibility == Visibility.Visible)
                                getAnimal("G");
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "FOX":
                        if (a3.Visibility == Visibility.Visible || d3.Visibility == Visibility.Visible || g3.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            if (a3.Visibility == Visibility.Visible)
                                getAnimal("A");
                            else if (d3.Visibility == Visibility.Visible)
                                getAnimal("D");
                            else if (g3.Visibility == Visibility.Visible)
                                getAnimal("G");
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "HORSE":
                        if (a4.Visibility == Visibility.Visible || d4.Visibility == Visibility.Visible || g4.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            if (a4.Visibility == Visibility.Visible)
                                getAnimal("A");
                            else if (d4.Visibility == Visibility.Visible)
                                getAnimal("D");
                            else if (g4.Visibility == Visibility.Visible)
                                getAnimal("A");
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "LION":
                        if (a5.Visibility == Visibility.Visible || d5.Visibility == Visibility.Visible || g5.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            if (a5.Visibility == Visibility.Visible)
                                getAnimal("A");
                            else if (d5.Visibility == Visibility.Visible)
                                getAnimal("D");
                            else if (g5.Visibility == Visibility.Visible)
                                getAnimal("G");
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "MONKEY":
                        if (a6.Visibility == Visibility.Visible || d6.Visibility == Visibility.Visible || g6.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            if (a6.Visibility == Visibility.Visible)
                                getAnimal("A");
                            else if (d6.Visibility == Visibility.Visible)
                                getAnimal("A");
                            else if (g6.Visibility == Visibility.Visible)
                                getAnimal("A");
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "PANDA":
                        if (a7.Visibility == Visibility.Visible || d7.Visibility == Visibility.Visible || g7.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            if (a7.Visibility == Visibility.Visible)
                                getAnimal("A");
                            else if (d7.Visibility == Visibility.Visible)
                                getAnimal("D");
                            else if (g7.Visibility == Visibility.Visible)
                                getAnimal("G");
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;

                    //LEVEL 2,5,8
                    case "TWO":
                        if (Level.Visibility == Visibility.Visible)
                        {
                            Level.Visibility = Visibility.Hidden;

                            seed = r.Next(0, 7);
                            used.Add(seed);
                            getAnimal("B");
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "FIVE":
                        if (Level.Visibility == Visibility.Visible)
                        {
                            Level.Visibility = Visibility.Hidden;

                            seed = r.Next(0, 7);
                            used.Add(seed);
                            getAnimal("E");
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "EIGHT":
                        if (Level.Visibility == Visibility.Visible)
                        {
                            Level.Visibility = Visibility.Hidden;

                            seed = r.Next(0, 7);
                            used.Add(seed);
                            getAnimal("H");
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "CAMEL":
                        if (b1.Visibility == Visibility.Visible || e1.Visibility == Visibility.Visible || h1.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            if (b1.Visibility == Visibility.Visible)
                                getAnimal("B");
                            else if (e1.Visibility == Visibility.Visible)
                                getAnimal("E");
                            else if (h1.Visibility == Visibility.Visible)
                                getAnimal("H");
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "CROCODILE":
                        if (b2.Visibility == Visibility.Visible || e2.Visibility == Visibility.Visible || h2.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            if (b2.Visibility == Visibility.Visible)
                                getAnimal("B");
                            else if (e2.Visibility == Visibility.Visible)
                                getAnimal("E");
                            else if (h2.Visibility == Visibility.Visible)
                                getAnimal("H");
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "ELEPHANT":
                        if (b3.Visibility == Visibility.Visible || e3.Visibility == Visibility.Visible || h3.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            if (b3.Visibility == Visibility.Visible)
                                getAnimal("B");
                            else if (e3.Visibility == Visibility.Visible)
                                getAnimal("E");
                            else if (h3.Visibility == Visibility.Visible)
                                getAnimal("H");
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "GIRAFFE":
                        if (b4.Visibility == Visibility.Visible || e4.Visibility == Visibility.Visible || h4.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            if (b4.Visibility == Visibility.Visible)
                                getAnimal("B");
                            else if (e4.Visibility == Visibility.Visible)
                                getAnimal("E");
                            else if (h4.Visibility == Visibility.Visible)
                                getAnimal("H");
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "GORILLA":
                        if (b5.Visibility == Visibility.Visible || e5.Visibility == Visibility.Visible || h5.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            if (b5.Visibility == Visibility.Visible)
                                getAnimal("B");
                            else if (e5.Visibility == Visibility.Visible)
                                getAnimal("E");
                            else if (h5.Visibility == Visibility.Visible)
                                getAnimal("H");
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "SNAKE":
                        if (b6.Visibility == Visibility.Visible || e6.Visibility == Visibility.Visible || h6.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            used.Add(seed);
                            if (b6.Visibility == Visibility.Visible)
                                getAnimal("B");
                            else if (e6.Visibility == Visibility.Visible)
                                getAnimal("E");
                            else if (h6.Visibility == Visibility.Visible)
                                getAnimal("H");
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "ZEBRA":
                        if (b7.Visibility == Visibility.Visible || e7.Visibility == Visibility.Visible || h7.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            if (b7.Visibility == Visibility.Visible)
                                getAnimal("B");
                            else if (e7.Visibility == Visibility.Visible)
                                getAnimal("E");
                            else if (h7.Visibility == Visibility.Visible)
                                getAnimal("H");
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;

                    //LEVEL 3,6,9
                    case "THREE":
                        if (Level.Visibility == Visibility.Visible)
                        {
                            Level.Visibility = Visibility.Hidden;

                            seed = r.Next(0, 8);
                            used.Add(seed);
                            foreach (Grid g in container.Children)
                            {
                                if (RNGpool_C[seed] == g.Name)
                                    g.Visibility = Visibility.Visible;
                                else if (RNGpool_C[seed] != g.Name)
                                    g.Visibility = Visibility.Hidden;
                            }
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "SIX":
                        if (Level.Visibility == Visibility.Visible)
                        {
                            Level.Visibility = Visibility.Hidden;

                            seed = r.Next(0, 8);
                            used.Add(seed);
                            foreach (Grid g in container.Children)
                            {
                                if (RNGpool_F[seed] == g.Name)
                                    g.Visibility = Visibility.Visible;
                                else if (RNGpool_F[seed] != g.Name)
                                    g.Visibility = Visibility.Hidden;
                            }
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "NINE":
                        if (Level.Visibility == Visibility.Visible)
                        {
                            Level.Visibility = Visibility.Hidden;

                            seed = r.Next(0, 8);
                            used.Add(seed);
                            foreach (Grid g in container.Children)
                            {
                                if (RNGpool_I[seed] == g.Name)
                                    g.Visibility = Visibility.Visible;
                                else if (RNGpool_I[seed] != g.Name)
                                    g.Visibility = Visibility.Hidden;
                            }
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "CHIMPANZEE":
                        if (c1.Visibility == Visibility.Visible || f1.Visibility == Visibility.Visible || i1.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(8);
                            if (c1.Visibility == Visibility.Visible)
                                getAnimal("C");
                            else if (f1.Visibility == Visibility.Visible)
                                getAnimal("F");
                            else if (i1.Visibility == Visibility.Visible)
                                getAnimal("I");
                        }
                        if (used.Count == 8)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "EMU":
                        if (c2.Visibility == Visibility.Visible || f2.Visibility == Visibility.Visible || i2.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(8);
                            if (c2.Visibility == Visibility.Visible)
                                getAnimal("C");
                            else if (f2.Visibility == Visibility.Visible)
                                getAnimal("F");
                            else if (i2.Visibility == Visibility.Visible)
                                getAnimal("I");
                        }
                        if (used.Count == 8)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "HEDGEHOG":
                        if (c3.Visibility == Visibility.Visible || f3.Visibility == Visibility.Visible || i3.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(8);
                            if (c3.Visibility == Visibility.Visible)
                                getAnimal("C");
                            else if (f3.Visibility == Visibility.Visible)
                                getAnimal("F");
                            else if (i3.Visibility == Visibility.Visible)
                                getAnimal("I");
                        }
                        if (used.Count == 8)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "LEMUR":
                        if (c4.Visibility == Visibility.Visible || f4.Visibility == Visibility.Visible || i4.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(8);
                            if (c4.Visibility == Visibility.Visible)
                                getAnimal("C");
                            else if (f4.Visibility == Visibility.Visible)
                                getAnimal("F");
                            else if (i4.Visibility == Visibility.Visible)
                                getAnimal("I");
                        }
                        if (used.Count == 8)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "LEOPARD":
                        if (c5.Visibility == Visibility.Visible || f5.Visibility == Visibility.Visible || i5.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(8);
                            if (c5.Visibility == Visibility.Visible)
                                getAnimal("C");
                            else if (f5.Visibility == Visibility.Visible)
                                getAnimal("F");
                            else if (i5.Visibility == Visibility.Visible)
                                getAnimal("I");
                        }
                        if (used.Count == 8)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "OTTER":
                        if (c6.Visibility == Visibility.Visible || f6.Visibility == Visibility.Visible || i6.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(8);
                            if (c6.Visibility == Visibility.Visible)
                                getAnimal("C");
                            else if (f6.Visibility == Visibility.Visible)
                                getAnimal("F");
                            else if (i6.Visibility == Visibility.Visible)
                                getAnimal("I");
                        }
                        if (used.Count == 8)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "TOUCAN":
                        if (c7.Visibility == Visibility.Visible || f7.Visibility == Visibility.Visible || i7.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(8);
                            if (c7.Visibility == Visibility.Visible)
                                getAnimal("C");
                            else if (f7.Visibility == Visibility.Visible)
                                getAnimal("F");
                            else if (i7.Visibility == Visibility.Visible)
                                getAnimal("I");
                        }
                        if (used.Count == 8)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "TURTLE":
                        if (c8.Visibility == Visibility.Visible || f8.Visibility == Visibility.Visible || i8.Visibility == Visibility.Visible)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(8);
                            if (c8.Visibility == Visibility.Visible)
                            {
                                foreach (Grid g in container.Children)
                                {
                                    if (RNGpool_C[seed] == g.Name)
                                        g.Visibility = Visibility.Visible;
                                    else if (RNGpool_C[seed] != g.Name)
                                        g.Visibility = Visibility.Hidden;
                                }
                            }
                            else if (f8.Visibility == Visibility.Visible)
                            {
                                foreach (Grid g in container.Children)
                                {
                                    if (RNGpool_F[seed] == g.Name)
                                        g.Visibility = Visibility.Visible;
                                    else if (RNGpool_F[seed] != g.Name)
                                        g.Visibility = Visibility.Hidden;
                                }
                            }
                            else if (i8.Visibility == Visibility.Visible)
                            {
                                foreach (Grid g in container.Children)
                                {
                                    if (RNGpool_I[seed] == g.Name)
                                        g.Visibility = Visibility.Visible;
                                    else if (RNGpool_I[seed] != g.Name)
                                        g.Visibility = Visibility.Hidden;
                                }
                            }
                        }
                        if (used.Count == 8)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    default:
                        ClearRecognitionHighlights();
                        break;
                }

                if (star1.Visibility == Visibility.Visible)
                {
                    lblScore1.Content = "Score: "+score.ToString();
                    lblScore1.Visibility = Visibility.Visible;
                    used.Clear();
                }
                else if (star2.Visibility == Visibility.Visible)
                {
                    lblScore2.Content = "Score: " + score.ToString();
                    lblScore2.Visibility = Visibility.Visible;
                    used.Clear();
                }
                else if (star3.Visibility == Visibility.Visible)
                {
                    lblScore3.Content = "Score: " + score.ToString();
                    lblScore3.Visibility = Visibility.Visible;
                    used.Clear();
                }
                else
                {
                    lblScore1.Content = "Score: ";
                    lblScore2.Content = "Score: ";
                    lblScore3.Content = "Score: ";
                    lblScore1.Visibility = Visibility.Visible;
                    lblScore2.Visibility = Visibility.Visible;
                    lblScore3.Visibility = Visibility.Visible;
                }
            }
        }

        private int RNG(int elements)
        {
            seed = r.Next(0, elements);
            for (int j = 0; j < used.Count; j++)
            {
                if (seed == used[j])
                {
                    seed = r.Next(0, elements);
                    j = -1;
                }
            }
            used.Add(seed);
            return seed;
        }

        private void getAnimal(string level)
        {
            if (level == "A")
            {
                foreach (Grid g in container.Children)
                {
                    if (RNGpool_A[seed] == g.Name)
                        g.Visibility = Visibility.Visible;
                    else if (RNGpool_A[seed] != g.Name)
                        g.Visibility = Visibility.Hidden;
                }
            }
            else if (level == "B")
            {
                foreach (Grid g in container.Children)
                {
                    if (RNGpool_B[seed] == g.Name)
                        g.Visibility = Visibility.Visible;
                    else if (RNGpool_B[seed] != g.Name)
                        g.Visibility = Visibility.Hidden;
                }
            }
            else if (level == "C")
            {
                foreach (Grid g in container.Children)
                {
                    if (RNGpool_C[seed] == g.Name)
                        g.Visibility = Visibility.Visible;
                    else if (RNGpool_C[seed] != g.Name)
                        g.Visibility = Visibility.Hidden;
                }
            }
            else if (level == "D")
            {
                foreach (Grid g in container.Children)
                {
                    if (RNGpool_D[seed] == g.Name)
                        g.Visibility = Visibility.Visible;
                    else if (RNGpool_D[seed] != g.Name)
                        g.Visibility = Visibility.Hidden;
                }
            }
            else if (level == "E")
            {
                foreach (Grid g in container.Children)
                {
                    if (RNGpool_E[seed] == g.Name)
                        g.Visibility = Visibility.Visible;
                    else if (RNGpool_E[seed] != g.Name)
                        g.Visibility = Visibility.Hidden;
                }
            }
            else if (level == "F")
            {
                foreach (Grid g in container.Children)
                {
                    if (RNGpool_F[seed] == g.Name)
                        g.Visibility = Visibility.Visible;
                    else if (RNGpool_F[seed] != g.Name)
                        g.Visibility = Visibility.Hidden;
                }
            }
            else if (level == "G")
            {
                foreach (Grid g in container.Children)
                {
                    if (RNGpool_G[seed] == g.Name)
                        g.Visibility = Visibility.Visible;
                    else if (RNGpool_G[seed] != g.Name)
                        g.Visibility = Visibility.Hidden;
                }
            }
            else if (level == "H")
            {
                foreach (Grid g in container.Children)
                {
                    if (RNGpool_H[seed] == g.Name)
                        g.Visibility = Visibility.Visible;
                    else if (RNGpool_H[seed] != g.Name)
                        g.Visibility = Visibility.Hidden;
                }
            }
            else if (level == "I")
            {
                foreach (Grid g in container.Children)
                {
                    if (RNGpool_I[seed] == g.Name)
                        g.Visibility = Visibility.Visible;
                    else if (RNGpool_I[seed] != g.Name)
                        g.Visibility = Visibility.Hidden;
                }
            }
        }

        private void getScore()
        {
            foreach (Grid g in container.Children)
                g.Visibility = Visibility.Hidden;
            if (score < 1500)
                star1.Visibility = Visibility.Visible;
            else if (score >= 1500 && score < 3000)
                star2.Visibility = Visibility.Visible;
            else
                star3.Visibility = Visibility.Visible;
        }

        private void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            ClearRecognitionHighlights();
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.AudioSource.Stop();

                this.sensor.Stop();
                this.sensor = null;
            }

            if (null != this.speechEngine)
            {
                this.speechEngine.SpeechRecognized -= SpeechRecognized;
                this.speechEngine.SpeechRecognitionRejected -= SpeechRejected;
                this.speechEngine.RecognizeAsyncStop();
            }
        }

        private void Media_Ended(object sender, EventArgs e)
        {
            mplayer.Position = TimeSpan.Zero;
            mplayer.Play();
        }
    }
}

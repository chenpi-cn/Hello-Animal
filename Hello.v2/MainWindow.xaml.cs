using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using ApiAiSDK;
using ApiAiSDK.Model;

namespace Hello.v2
{
    public partial class MainWindow : Window
    {
        MediaPlayer mplayer = new MediaPlayer();
        Random r = new Random();
        private List<int> used = new List<int> { };
        /*private List<string> RNGpool_A = new List<string> { "a1", "a2", "a3", "a4", "a5", "a6", "a7" };
        private List<string> RNGpool_B = new List<string> { "b1", "b2", "b3", "b4", "b5", "b6", "b7" };
        private List<string> RNGpool_C = new List<string> { "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8" };
        private List<string> RNGpool_D = new List<string> { "d1", "d2", "d3", "d4", "d5", "d6", "d7" };
        private List<string> RNGpool_E = new List<string> { "e1", "e2", "e3", "e4", "e5", "e6", "e7" };
        private List<string> RNGpool_F = new List<string> { "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8" };
        private List<string> RNGpool_G = new List<string> { "g1", "g2", "g3", "g4", "g5", "g6", "g7" };
        private List<string> RNGpool_H = new List<string> { "h1", "h2", "h3", "h4", "h5", "h6", "h7" };
        private List<string> RNGpool_I = new List<string> { "i1", "i2", "i3", "i4", "i5", "i6", "i7", "i8" };*/
        private List<string> RNGpool_147 = new List<string> { "cat", "dogs", "fox", "horse", "lion", "monkey", "panda" };
        private List<string> RNGpool_258 = new List<string> { "camel", "crocodile", "elephant", "giraffe", "gorilla", "snake", "zebra" };
        private List<string> RNGpool_369 = new List<string> { "chimpanzee", "emu", "hedgehog", "lemur", "leopard", "otter", "toucan", "turtle" };
        private KinectSensor sensor;
        private List<Span> recognitionSpans;
        private SpeechRecognitionEngine speechEngine;
        private ApiAi apiAi;
        private string locator;
        private string locatorMem;
        private int level;

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
            var config = new AIConfiguration("49389ef632e541a2810c3dfd328db2c1", SupportedLanguage.English);
            apiAi = new ApiAi(config);
            locator = "Main";

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

            ClearRecognitionHighlights();
            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                switch (e.Result.Semantics.Value.ToString())
                {
                    //FUNCTIONS
                    case "START":
                        if (locator == "Main")
                        {
                            container.Background = new ImageBrush(new BitmapImage(new Uri(@"../../../Images/Mainmenu-01.png", UriKind.Relative)));
                            locator = "level";
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "CREDITS":
                        if (locator == "Main")
                        {
                            container.Background = new ImageBrush(new BitmapImage(new Uri(@"../../../Images/credit-01.png", UriKind.Relative)));
                            locator = "credits";
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "HELP":
                        if (locator == "Main")
                        {
                            container.Background = new ImageBrush(new BitmapImage(new Uri(@"../../../Images/help-01.png", UriKind.Relative)));
                            locator = "help";
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "BACK":
                        if (locator == "help" || locator == "credits")
                        {
                            container.Background = new ImageBrush(new BitmapImage(new Uri(@"../../../Images/Mainmenu-01.png", UriKind.Relative)));
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "MENU":
                        if (locator == "endgame" || locator == "pause")
                        {
                            container.Background = new ImageBrush(new BitmapImage(new Uri(@"../../../Images/Mainmenu-01.png", UriKind.Relative)));
                            score = 0;
                            used.Clear();
                        }
                        ClearRecognitionHighlights();
                        break;
                    //case "CLOSE":
                    case "EXIT":
                        if (locator == "endgame" || locator == "pause")
                        {
                            Frame_ExitPrompt.Visibility = Visibility.Visible;
                            locatorMem = locator;
                            locator = "exitprompt";
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "YES":
                        if (Frame_ExitPrompt.Visibility == Visibility.Visible)
                            this.Close();
                        ClearRecognitionHighlights();
                        break;

                    case "NO":
                        if (Frame_ExitPrompt.Visibility == Visibility.Visible)
                        {
                            Frame_ExitPrompt.Visibility = Visibility.Hidden;
                            locator = locatorMem;
                        }
                        ClearRecognitionHighlights();
                        break;

                    case "PAUSE":
                        if (Frame_Pause.Visibility == Visibility.Visible)
                        {
                            Frame_Pausemenu.Visibility = Visibility.Visible;
                            locatorMem = locator;
                            locator = "pause";
                        }
                        ClearRecognitionHighlights();
                        break;

                    case "RESUME":
                        if (Frame_Pause.Visibility == Visibility.Visible)
                        {
                            Frame_Pausemenu.Visibility = Visibility.Visible;
                            locatorMem = locator;
                            locator = locatorMem;
                        }
                        ClearRecognitionHighlights();
                        break;

                    //LEVEL 1,4,7
                    case "ONE":
                        if (locator == "level")
                        {
                            seed = r.Next(0, 7);
                            used.Add(seed);
                            level = 1;
                            getAnimal(level.ToString());
                        }                        
                        ClearRecognitionHighlights();
                        break;
                    case "FOUR":
                        if (locator == "level")
                        {
                            seed = r.Next(0, 7);
                            used.Add(seed);
                            level = 4;
                            getAnimal(level.ToString());
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "SEVEN":
                        if (locator == "level")
                        {
                            seed = r.Next(0, 7);
                            used.Add(seed);
                            level = 7;
                            getAnimal(level.ToString());
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "CAT":
                        if (locator == "cat" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "DOG":
                        if (locator == "dog" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "FOX":
                        if (locator == "fox" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "HORSE":
                        if (locator == "horse" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "LION":
                        if (locator == "lion" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "MONKEY":
                        if (locator == "monkey" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "PANDA":
                        if (locator == "panda" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;

                    //LEVEL 2,5,8
                    case "TWO":
                        if (locator == "level")
                        {
                            seed = r.Next(0, 7);
                            used.Add(seed);
                            level = 2;
                            getAnimal(level.ToString());
                        }

                        ClearRecognitionHighlights();
                        break;
                    case "FIVE":
                        if (locator == "level")
                        {
                            seed = r.Next(0, 7);
                            used.Add(seed);
                            level = 5;
                            getAnimal(level.ToString());
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "EIGHT":
                        if (locator == "level")
                        {
                            seed = r.Next(0, 7);
                            used.Add(seed);
                            level = 8;
                            getAnimal(level.ToString());
                        }

                        ClearRecognitionHighlights();
                        break;
                    case "CAMEL":
                        if (locator == "camel" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "CROCODILE":
                        if (locator == "crocodile" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "ELEPHANT":
                        if (locator == "elephant" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "GIRAFFE":
                        if (locator == "giraffe" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "GORILLA":
                        if (locator == "gorilla" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "SNAKE":
                        if (locator == "snake" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "ZEBRA":
                        if (locator == "zebra" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(7);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 7)
                            getScore();
                        ClearRecognitionHighlights();
                        break;

                    //LEVEL 3,6,9
                    case "THREE":
                        if (locator == "level")
                        {
                            seed = r.Next(0, 8);
                            used.Add(seed);
                            level = 3;
                            getAnimal(level.ToString());
                        }

                        ClearRecognitionHighlights();
                        break;
                    case "SIX":
                        if (locator == "level")
                        {
                            seed = r.Next(0, 8);
                            used.Add(seed);
                            level = 6;
                            getAnimal(level.ToString());
                        }

                        ClearRecognitionHighlights();
                        break;
                    case "NINE":
                        if (locator == "level")
                        {
                            seed = r.Next(0, 8);
                            used.Add(seed);
                            level = 9;
                            getAnimal(level.ToString());
                        }
                        ClearRecognitionHighlights();
                        break;
                    case "CHIMPANZEE":
                        if (locator == "chimpanzee" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(8);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 8)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "EMU":
                        if (locator == "emu" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(8);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 8)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "HEDGEHOG":
                        if (locator == "hedgehog" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(8);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 8)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "LEMUR":
                        if (locator == "lemur" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(8);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 8)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "LEOPARD":
                        if (locator == "leopard" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(8);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 8)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "OTTER":
                        if (locator == "otter" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(8);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 8)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "TOUCAN":
                        if (locator == "toucan" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(8);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 8)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    case "TURTLE":
                        if (locator == "turtle" && Frame_Pausemenu.Visibility == Visibility.Hidden)
                        {
                            score += (int)(e.Result.Confidence * 1000);
                            seed = RNG(8);
                            getAnimal(level.ToString());
                        }
                        if (used.Count == 8)
                            getScore();
                        ClearRecognitionHighlights();
                        break;
                    default:
                        ClearRecognitionHighlights();
                        break;
                }

                if (locator == "endgame")
                {
                    lblScore.Content = "Score: "+score.ToString();
                    lblScore.Visibility = Visibility.Visible;
                    used.Clear();
                }
                else
                {
                    lblScore.Content = "Score: ";
                    lblScore.Visibility = Visibility.Visible;
                    used.Clear();
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
            if (level == "1" || level == "4" || level == "7")
            {
                container.Background = new ImageBrush(new BitmapImage(new Uri(@"../../../Images/" + level + "/" + RNGpool_147[seed] + ".png", UriKind.Relative)));
                locator = RNGpool_147[seed];
            }
            else if (level == "2" || level == "5" || level == "8")
            {
                container.Background = new ImageBrush(new BitmapImage(new Uri(@"../../../Images/" + level + "/" + RNGpool_147[seed] + ".png", UriKind.Relative)));
                locator = RNGpool_258[seed];
            }
            else if (level == "3" || level == "6" || level == "9")
            {
                container.Background = new ImageBrush(new BitmapImage(new Uri(@"../../../Images/" + level + "/" + RNGpool_147[seed] + ".png", UriKind.Relative)));
                locator = RNGpool_369[seed];
            }
            ShowPause();
        }

        private void getScore()
        {
            HidePause();
            if (score < 1500)
                container.Background = new ImageBrush(new BitmapImage(new Uri(@"../../../Images/score1-01.png", UriKind.Relative)));
            else if (score >= 1500 && score < 3000)
                container.Background = new ImageBrush(new BitmapImage(new Uri(@"../../../Images/score2-01.png", UriKind.Relative)));
            else
                container.Background = new ImageBrush(new BitmapImage(new Uri(@"../../../Images/score-01.png", UriKind.Relative)));
        }

        private void ShowPause()
        {
            Frame_Pause.Visibility = Visibility.Visible;
        }

        private void HidePause()
        {
            Frame_Pause.Visibility = Visibility.Hidden;
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

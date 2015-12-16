using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Windows.Threading;

namespace RecipeHelperProxemics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            CenterWindowOnScreen();

            this.KeyUp += MainWindow_KeyUp;

            AppearFirstPage();

            InitializeKinect();

        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Application.Current.Shutdown();
            }
            if (e.Key == Key.Down)
            {
                // Make popup appear
                TogglePopup();
            }
            if (e.Key == Key.Up)
            {
                AlternateSteps();
            }
            if (e.Key == Key.Left)
            {
                AppearThirdPage();
            }
            if (e.Key == Key.Right)
            {
                AppearSecondPage();
            }
        }

        #region Kinect

        #region States
        // State variable and constant values
        private int state;
        private const int PUBLIC_STATE = 0;
        private const int SOCIAL_STATE = 1;
        private const int PERSONAL_STATE = 2;

        // Thresholds for states.
        private const double PUBLIC_THRESHOLD = 3;
        private const double SOCIAL_THRESHOLD = 1.5;    // Approx. 4'
        private const double PERSONAL_THRESHOLD = 0.1; // Approx. 2
        private Dictionary<ulong, int> bodyStates = new Dictionary<ulong, int>();

        private void setState(int newState)
        {
            if (state == newState)
                return;
            if (state == PUBLIC_STATE) // people are separated
            {
                AppearSecondPage();
            } if (state == SOCIAL_STATE) // people are close
            {
                AppearThirdPage();
            } if(state == PERSONAL_STATE) // person wants to move to next step
            {
                AppearSecondPage();
            }
            state = newState;
        }
        #endregion

        private KinectSensor kinectSensor = null;
        List<Body> bodies;

        public void InitializeKinect()
        {
            this.kinectSensor = KinectSensor.GetDefault();

            this.kinectSensor.Open();
            this.kinectSensor.BodyFrameSource.OpenReader().FrameArrived += MainWindow_FrameArrived;

            setState(PUBLIC_STATE);
        }

        private void MainWindow_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            BodyFrame frame = e.FrameReference.AcquireFrame();

            using (frame)
            {
                if (frame != null)
                {

                    this.bodies = new Body[frame.BodyFrameSource.BodyCount].ToList();

                    // get and refresh body data
                    frame.GetAndRefreshBodyData(this.bodies);

                    LinkedList<Body> socialBodies = new LinkedList<Body>();
                    LinkedList<Body> personalBodies = new LinkedList<Body>();
                    LinkedList<Body> publicBodies = new LinkedList<Body>();

                    foreach (var body in this.bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                float x = body.Joints[JointType.SpineShoulder].Position.X;
                                // Check when person changes step
                                if (x < PERSONAL_THRESHOLD)
                                {
                                    personalBodies.AddLast(body);
                                }

                                // Check when people are close
                                else if (x > PERSONAL_THRESHOLD && x < SOCIAL_THRESHOLD)
                                {
                                    socialBodies.AddLast(body);
                                }

                                // Check when people are far
                                else if (x > SOCIAL_THRESHOLD && x < PUBLIC_THRESHOLD)
                                {
                                    Console.WriteLine("PUBLIC");
                                    publicBodies.AddLast(body);
                                }
                            }
                        }
                    }

                    if (personalBodies.Count > 0 && state != PERSONAL_STATE)
                    {
                        Console.WriteLine("PERSONAL STATE");
                        setState(PERSONAL_STATE);
                    }
                    else if (personalBodies.Count == 0 && socialBodies.Count > 0 && state != SOCIAL_STATE) { 
                        Console.WriteLine("SOCIAL STATE");
                        setState(SOCIAL_STATE);
                    }

                    else if (socialBodies.Count == 0 && personalBodies.Count == 0 && publicBodies.Count > 0)  // Else, enter public state
                    {
                        if (state != PUBLIC_STATE)
                        {
                            Console.WriteLine("PUBLIC STATE");
                            setState(PUBLIC_STATE);
                        }
                    }

                    foreach (var body in personalBodies)
                    {
                        checkHandGestures(body);
                    }
                }
            }
            reactToGestures();
        }

        #region Multiple people
        
        #endregion

        #region Gestures
        // State variables for gestures
        private bool isHandRaised = false;
        private bool isLeftHandOut = false;
        private bool isRightHandOut = false;

        // Constants for gestures
        private const int NONE = 0;
        private const int HAND_RAISED = 1;
        private const int LEFT_HAND_OUT = 2;
        private const int RIGHT_HAND_OUT = 3;

        // Threshold of acceptable error for Kinect gesture detection
        private float allowance = 0;

        private void reactToGestures()
        {
            if (isHandRaised)
            {
                if (isSecondPage)
                {
                    TogglePopup();
                } else if(isThirdPage)
                {
                    AppearSecondPage();
                }
                isHandRaised = false;
            }
            else if (isLeftHandOut)
            {
                if(isFirstPage)
                {
                    AppearThirdPage();
                } else if (isSecondPage)
                {
                    AppearFirstPage();
                } else if (isThirdPage)
                {
                    AlternateSteps();
                }
                isLeftHandOut = false;
            }
            else if (isRightHandOut)
            {
                if (isFirstPage)
                {
                    AppearSecondPage();
                }
                else if (isSecondPage)
                {
                    AppearThirdPage();
                }
                else if (isThirdPage)
                {
                    AlternateSteps();
                }
                isRightHandOut = false;
            }
        }

        private float setAllowance(float value1, float value2)
        {
            allowance = Math.Abs(value1 - value2);
            return allowance;
        }
        private bool isApprox(float value1, float value2)
        {
            if (Math.Abs(value1 - value2) <= allowance)
                return true;
            else
                return false;
        }

        private void checkHandGestures(Body body)
        {
            // Track persistent gesture state of this body
            int bodyState = 0;
            if (bodyStates.ContainsKey(body.TrackingId))
                bodyState = bodyStates[body.TrackingId];
            else
                bodyStates.Add(body.TrackingId, state);

            // Set error allowance for this body: the largest value between the hand tip-to-elbow distance of the left and right arms. 
            if (setAllowance(body.Joints[JointType.HandTipRight].Position.Y, body.Joints[JointType.ElbowRight].Position.Y) > setAllowance(body.Joints[JointType.HandTipLeft].Position.Y, body.Joints[JointType.ElbowLeft].Position.Y))
                setAllowance(body.Joints[JointType.HandTipRight].Position.Y, body.Joints[JointType.ElbowRight].Position.Y);
            else
                setAllowance(body.Joints[JointType.HandTipLeft].Position.Y, body.Joints[JointType.ElbowLeft].Position.Y);

            // Check for right hand raised above head
            if (body.Joints[JointType.HandRight].Position.Y > body.Joints[JointType.Head].Position.Y
                && body.Joints[JointType.HandLeft].Position.Y < body.Joints[JointType.SpineShoulder].Position.Y
                && isApprox(body.Joints[JointType.ShoulderRight].Position.X, body.Joints[JointType.HandRight].Position.X)
                && isApprox(body.Joints[JointType.ElbowRight].Position.X, body.Joints[JointType.HandRight].Position.X))
            {
                if (!isHandRaised && bodyState != HAND_RAISED)	// Only set system gesture state if user wasn't already in this state
                {
                    Console.WriteLine("RIGHT HAND RAISED");
                    isHandRaised = true;
                }
                bodyState = HAND_RAISED;	// Set user gesture state
            }
            // Check for left hand raised above head
            else if (body.Joints[JointType.HandLeft].Position.Y > body.Joints[JointType.Head].Position.Y
                && body.Joints[JointType.HandRight].Position.Y < body.Joints[JointType.SpineShoulder].Position.Y
                && isApprox(body.Joints[JointType.ShoulderLeft].Position.X, body.Joints[JointType.HandLeft].Position.X)
                && isApprox(body.Joints[JointType.ElbowLeft].Position.X, body.Joints[JointType.HandLeft].Position.X))
            {
                if (!isHandRaised && bodyState != HAND_RAISED)	// Only set system gesture state if user wasn't already in this state
                {
                    Console.WriteLine("LEFT HAND RAISED");
                    isHandRaised = true;
                }
                bodyState = HAND_RAISED;	// Set user gesture state
            }
            // Check for left hand out
            else if (!(body.Joints[JointType.HandRight].Position.X > body.Joints[JointType.ElbowRight].Position.X
                && body.Joints[JointType.ElbowRight].Position.X > body.Joints[JointType.ShoulderRight].Position.X
                && isApprox(body.Joints[JointType.HandRight].Position.Y, body.Joints[JointType.ShoulderRight].Position.Y)
                && isApprox(body.Joints[JointType.HandRight].Position.Y, body.Joints[JointType.ElbowRight].Position.Y)
                && !isApprox(body.Joints[JointType.ShoulderRight].Position.X, body.Joints[JointType.HandRight].Position.X))

                && body.Joints[JointType.HandLeft].Position.X < body.Joints[JointType.ElbowLeft].Position.X
                && body.Joints[JointType.ElbowLeft].Position.X < body.Joints[JointType.ShoulderLeft].Position.X
                && isApprox(body.Joints[JointType.HandLeft].Position.Y, body.Joints[JointType.ShoulderLeft].Position.Y)
                && isApprox(body.Joints[JointType.HandLeft].Position.Y, body.Joints[JointType.ElbowLeft].Position.Y)
                && !isApprox(body.Joints[JointType.ShoulderLeft].Position.X, body.Joints[JointType.HandLeft].Position.X))
            {
                if (!isLeftHandOut && bodyState != LEFT_HAND_OUT)	// Only set system gesture state if user wasn't already in this state
                {
                    Console.WriteLine("LEFT HAND OUT");
                    isLeftHandOut = true;
                }
                bodyState = LEFT_HAND_OUT;	// Set user gesture state
            }
            // Check for right hand out
            else if (!(body.Joints[JointType.HandLeft].Position.X < body.Joints[JointType.ElbowLeft].Position.X
                && body.Joints[JointType.ElbowLeft].Position.X < body.Joints[JointType.ShoulderLeft].Position.X
                && isApprox(body.Joints[JointType.HandLeft].Position.Y, body.Joints[JointType.ShoulderLeft].Position.Y)
                && isApprox(body.Joints[JointType.HandLeft].Position.Y, body.Joints[JointType.ElbowLeft].Position.Y)
                && !isApprox(body.Joints[JointType.ShoulderLeft].Position.X, body.Joints[JointType.HandLeft].Position.X))

                && body.Joints[JointType.HandRight].Position.X > body.Joints[JointType.ElbowRight].Position.X
                && body.Joints[JointType.ElbowRight].Position.X > body.Joints[JointType.ShoulderRight].Position.X
                && isApprox(body.Joints[JointType.HandRight].Position.Y, body.Joints[JointType.ShoulderRight].Position.Y)
                && isApprox(body.Joints[JointType.HandRight].Position.Y, body.Joints[JointType.ElbowRight].Position.Y)
                && !isApprox(body.Joints[JointType.ShoulderRight].Position.X, body.Joints[JointType.HandRight].Position.X))
            {
                if (!isRightHandOut && bodyState != RIGHT_HAND_OUT)		// Only set system gesture state if user wasn't already in this state
                {
                    Console.WriteLine("RIGHT HAND OUT");
                    isRightHandOut = true;
                }
                bodyState = RIGHT_HAND_OUT;	// Set user gesture state
            }
            else
                bodyState = NONE;	// No significant gesture

            bodyStates[body.TrackingId] = bodyState;
        }
        #endregion
        #endregion

        #region Pages

        private bool isFirstPage = false;
        private bool isSecondPage = false;
        private bool isThirdPage = false;

        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }

        private void AppearFirstPage()
        {
            ((Storyboard)this.Resources["Section1_Enter"]).Begin();
            Section1.Visibility = Visibility.Visible;
            Section2.Visibility = Visibility.Collapsed;
            Section3_Saad1.Visibility = Visibility.Collapsed;
            Section3_Saad2.Visibility = Visibility.Collapsed;
            Section3_Jack.Visibility = Visibility.Collapsed;
            isFirstPage = true;
            isSecondPage = false;
            isThirdPage = false;
        }

        private void AppearSecondPage()
        {
            Section1.Visibility = Visibility.Collapsed;
            ((Storyboard)this.Resources["Section2_Enter"]).Begin();
            Section2.Visibility = Visibility.Visible;
            popup.Visibility = Visibility.Collapsed;
            popup_text.Visibility = Visibility.Collapsed;
            popup_text2.Visibility = Visibility.Collapsed;
            Section3_Saad1.Visibility = Visibility.Collapsed;
            Section3_Saad2.Visibility = Visibility.Collapsed;
            Section3_Jack.Visibility = Visibility.Collapsed;

            isFirstPage = false;
            isSecondPage = true;
            isThirdPage = false;
        }

        private void AppearThirdPage()
        {
            Section1.Visibility = Visibility.Collapsed;
            Section2.Visibility = Visibility.Collapsed;
            ((Storyboard)this.Resources["Section3_Enter"]).Begin();
            AlternateSteps();
            Section3_Jack.Visibility = Visibility.Visible;

            isFirstPage = false;
            isSecondPage = false;
            isThirdPage = true;
        }

        private void AlternateSteps()
        {
            if (Section3_Saad1.Visibility == Visibility.Visible)
            {
                ((Storyboard)this.Resources["Saad1_Exit"]).Begin();
                Section3_Saad1.Visibility = Visibility.Collapsed;
                ((Storyboard)this.Resources["Saad2_Enter"]).Begin();
                Section3_Saad2.Visibility = Visibility.Visible;
            } else
            {
                ((Storyboard)this.Resources["Saad2_Exit"]).Begin();
                Section3_Saad2.Visibility = Visibility.Collapsed;
                ((Storyboard)this.Resources["Saad1_Enter"]).Begin();
                Section3_Saad1.Visibility = Visibility.Visible;
            }
            
        }

        #endregion

        #region Effects

        private void TogglePopup()
        {
            if (popup.Visibility == Visibility.Collapsed)
            {
                ((Storyboard)this.Resources["Popup_Enter"]).Begin();
                popup.Visibility = Visibility.Visible;
                popup_text.Visibility = Visibility.Visible;
                popup_text2.Visibility = Visibility.Visible;
            }
            else
            {
                ((Storyboard)this.Resources["Popup_Exit"]).Begin();
                popup.Visibility = Visibility.Collapsed;
                popup_text.Visibility = Visibility.Collapsed;
                popup_text2.Visibility = Visibility.Collapsed;
            }
        }

        private void Btn_Role_OnClick(object sender, RoutedEventArgs e)
        {
            if (btn_role.Content.ToString() == "SAAD")
            {
                btn_role.Content = "JACK";
            }
            else
            {
                btn_role.Content = "SAAD";
            }
        }

        private void Btn_Role2_OnClick(object sender, RoutedEventArgs e)
        {
            if (btn_role2.Content.ToString() == "SAAD")
            {
                btn_role2.Content = "JACK";
            }
            else
            {
                btn_role2.Content = "SAAD";
            }
        }

        private void Btn_Role3_OnClick(object sender, RoutedEventArgs e)
        {
            if (btn_role3.Content.ToString() == "SAAD")
            {
                btn_role3.Content = "JACK";
            }
            else
            {
                btn_role3.Content = "SAAD";
            }
        }

        private void Btn_Role4_OnClick(object sender, RoutedEventArgs e)
        {
            if (btn_role4.Content.ToString() == "SAAD")
            {
                btn_role4.Content = "JACK";
            }
            else
            {
                btn_role4.Content = "SAAD";
            }
        }
        #endregion

        #region Testing

        #endregion
    }
}

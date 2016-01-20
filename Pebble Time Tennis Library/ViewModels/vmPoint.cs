using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using Windows.UI;
using Tennis_Statistics.Game_Logic;
using Windows.UI.Xaml.Media;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Tennis_Statistics.ViewModels
{
    public class vmPoint : INotifyPropertyChanged
    {
        public class ActionCondition
        {
            public PointAction Action;
            public String Condition;
            public TennisMatch.LogLevelEnum LogLevel;
        }

        #region Properties

        private ObservableCollection<PointAction> m_PossibleActions;

        /// <summary>
        /// The list of possible actions/choices for the current point
        /// </summary>
        public ObservableCollection<PointAction> PossibleActions
        {
            get
            {
                if (m_PossibleActions == null) m_PossibleActions = new ObservableCollection<PointAction>();

                return m_PossibleActions;
            }
        }

        private TennisPoint m_Point;

        /// <summary>
        /// The instance of the tennis point currently being played
        /// </summary>
        public TennisPoint Point
        {
            get
            {
                return m_Point;
            }

            set
            {
                m_Point = value;
                BuildActions();
            }
        }

        /// <summary>
        /// The loglevel of the current point / game of match in play
        /// </summary>
        public TennisMatch.LogLevelEnum LogLevel { get; set; }


        /// <summary>
        /// Describes the type op point; breakpoint, setpoint, matchpoint etc
        /// </summary>
        public string Description
        {
            get
            {
                if (Point == null) return "";
                if (Point.Type.Contains(TennisPoint.PointType.MatchPoint)) return "MATCHPOINT";
                if (Point.Type.Contains(TennisPoint.PointType.SetPoint)) return "SETPOINT";
                if (Point.Type.Contains(TennisPoint.PointType.BreakPoint) && Point.Type.Contains(TennisPoint.PointType.GamePoint)) return "GAMEPOINT & BREAKPOINT";
                if (Point.Type.Contains(TennisPoint.PointType.BreakPoint)) return "BREAKPOINT";

                return "";
            }
        }


        #endregion

        #region Choice structure

        /// <summary>
        /// This list describes all possible actions on a tennis point and the conditions when they are valid
        /// </summary>
        private List<ActionCondition> ActionConditions = new List<ActionCondition>() {
            new ActionCondition { Action = new PointAction { Command = "Ace", Method = "CommandAce", Color = Color.FromArgb(192, 80, 255, 0)}, Condition="Winner=0", LogLevel = TennisMatch.LogLevelEnum.Points},
            new ActionCondition { Action = new PointAction { Command = "Second Serve", Method = "CommandSecondServe", Color = Color.FromArgb(192, 255, 255, 0), /*Icon = new BitmapImage(new Uri("ms-appx:///Assets/shadow_secondserve.png"))*/},  Condition="Winner=0&Serve=FirstServe" , LogLevel = TennisMatch.LogLevelEnum.Points},
            new ActionCondition { Action = new PointAction { Command = "Double Fault", Method = "CommandDoubleFault", Color = Color.FromArgb(192, 255, 80, 0), /*Icon = new BitmapImage(new Uri("ms-appx:///Assets/shadow_doublefault.png"))*/}, Condition="Winner=0&Serve=SecondServe" , LogLevel = TennisMatch.LogLevelEnum.Points},
            new ActionCondition { Action = new PointAction { Command = "Win", Method = "CommandWin", Color = Color.FromArgb(192, 80, 255, 0), /*Icon = new BitmapImage(new Uri("ms-appx:///Assets/shadow_win.png"))*/},  Condition="Winner=0" , LogLevel = TennisMatch.LogLevelEnum.Points},
            new ActionCondition { Action = new PointAction { Command = "Lose", Method = "CommandLose", Color = Color.FromArgb(192, 255, 80, 0), /*Icon = new BitmapImage(new Uri("ms-appx:///Assets/shadow_lose.png"))*/},  Condition="Winner=0" , LogLevel = TennisMatch.LogLevelEnum.Points},
            new ActionCondition { Action = new PointAction { Command = "Winner", Method = "CommandWinner", Color = Color.FromArgb(192, 80, 255, 0)},  Condition="Winner=!0&ResultType=Unknown" , LogLevel = TennisMatch.LogLevelEnum.Errors},
            new ActionCondition { Action = new PointAction { Command = "Forced Error", Method = "CommandForcedError", Color = Color.FromArgb(192, 80, 255, 0)},  Condition="Winner=!0&ResultType=Unknown" , LogLevel = TennisMatch.LogLevelEnum.Errors},
            new ActionCondition { Action = new PointAction { Command = "Unforced Error", Method = "CommandUnforcedError", Color = Color.FromArgb(192, 80, 255, 0)},  Condition="Winner=!0&ResultType=Unknown" , LogLevel = TennisMatch.LogLevelEnum.Errors},
            new ActionCondition { Action = new PointAction { Command = "Forehand", Method = "CommandForehand", Color = Color.FromArgb(192, 80, 255, 0), /*Icon = new BitmapImage(new Uri("ms-appx:///Assets/shadow_forehand.png"))*/},  Condition="ResultType=!Unknown&GetShot=Unknown" , LogLevel = TennisMatch.LogLevelEnum.Shots},
            new ActionCondition { Action = new PointAction { Command = "Backhand", Method = "CommandBackhand", Color = Color.FromArgb(192, 80, 255, 0), /*Icon = new BitmapImage(new Uri("ms-appx:///Assets/shadow_backhand.png"))*/},  Condition="ResultType=!Unknown&GetShot=Unknown" , LogLevel = TennisMatch.LogLevelEnum.Shots},
            new ActionCondition { Action = new PointAction { Command = "Volley", Method = "CommandVolley", Color = Color.FromArgb(192, 80, 255, 0), /*Icon = new BitmapImage(new Uri("ms-appx:///Assets/shadow_volley.png"))*/}, Condition="ResultType=!Unknown&GetShot=Unknown" , LogLevel = TennisMatch.LogLevelEnum.Shots},
            new ActionCondition { Action = new PointAction { Command = "Smash", Method = "CommandSmash", Color = Color.FromArgb(192, 80, 255, 0)},  Condition="ResultType=!Unknown&GetShot=Unknown" , LogLevel = TennisMatch.LogLevelEnum.Shots},
            new ActionCondition { Action = new PointAction { Command = "Lob", Method = "CommandLob", Color = Color.FromArgb(192, 80, 255, 0), /*Icon = new BitmapImage(new Uri("ms-appx:///Assets/shadow_lob.png"))*/},  Condition="ResultType=!Unknown&GetShot=Unknown" , LogLevel = TennisMatch.LogLevelEnum.Shots},
            new ActionCondition { Action = new PointAction { Command = "Dropshot", Method = "CommandDropshot", Color = Color.FromArgb(192, 80, 255, 0)},  Condition="ResultType=!Unknown&GetShot=Unknown" , LogLevel = TennisMatch.LogLevelEnum.Shots},
            new ActionCondition { Action = new PointAction { Command = "Passing", Method = "CommandPassing", Color = Color.FromArgb(192, 80, 255, 0), /*Icon = new BitmapImage(new Uri("ms-appx:///Assets/shadow_passing.png"))*/},  Condition="ResultType=!Unknown&GetShot=Unknown" , LogLevel = TennisMatch.LogLevelEnum.Shots},
            new ActionCondition { Action = new PointAction { Command = "Wide", Method = "CommandAceWide", Color = Color.FromArgb(192, 80, 255, 0)},  Condition="GetShot=Ace&Ace=Unknown" , LogLevel = TennisMatch.LogLevelEnum.Placement},
            new ActionCondition { Action = new PointAction { Command = "Line", Method = "CommandAceLine", Color = Color.FromArgb(192, 80, 255, 0)},  Condition="GetShot=Ace&Ace=Unknown" , LogLevel = TennisMatch.LogLevelEnum.Placement},
            new ActionCondition { Action = new PointAction { Command = "Body", Method = "CommandAceBody", Color = Color.FromArgb(192, 80, 255, 0)},  Condition="GetShot=Ace&Ace=Unknown" , LogLevel = TennisMatch.LogLevelEnum.Placement},
            new ActionCondition { Action = new PointAction { Command = "Long", Method = "CommandErrorLong", Color = Color.FromArgb(192, 80, 255, 0)},  Condition="GetShot=!Unknown&ResultType=ForcedError&Error=Unknown|Shot=!Unknown&ResultType=UnforcedError&Error=Unknown" , LogLevel = TennisMatch.LogLevelEnum.Placement},
            new ActionCondition { Action = new PointAction { Command = "Net", Method = "CommandErrorNet", Color = Color.FromArgb(192, 80, 255, 0)},  Condition="GetShot=!Unknown&ResultType=ForcedError&Error=Unknown|Shot=!Unknown&ResultType=UnforcedError&Error=Unknown" , LogLevel = TennisMatch.LogLevelEnum.Placement},
            new ActionCondition { Action = new PointAction { Command = "Wide", Method = "CommandErrorWide", Color = Color.FromArgb(192, 80, 255, 0)},  Condition="GetShot=!Unknown&ResultType=ForcedError&Error=Unknown|Shot=!Unknown&ResultType=UnforcedError&Error=Unknown" , LogLevel = TennisMatch.LogLevelEnum.Placement},
            new ActionCondition { Action = new PointAction { Command = "Other", Method = "CommandErrorOther", Color = Color.FromArgb(192, 80, 255, 0)},  Condition="GetShot=!Unknown&ResultType=ForcedError&Error=Unknown|Shot=!Unknown&ResultType=UnforcedError&Error=Unknown" , LogLevel = TennisMatch.LogLevelEnum.Placement},
            //new ActionCondition { Action = new PointAction { Command = "Undo", Method = "CommandUndo", Color = Color.FromArgb(255, 128, 128, 128)},  Condition="Winner=0|Winner=1|Winner=2" , LogLevel = TennisMatch.LogLevelEnum.Points},
        };

        #endregion

        #region Methods

        /// <summary>
        /// Rebuild the list of possible actions, based on the changes made to the current point
        /// </summary>
        public void BuildActions()
        {
            PossibleActions.Clear();

            if (m_Point == null) return;

            foreach (ActionCondition Candidate in ActionConditions )
            {
                if (CheckCondition(Point, Candidate.Condition))
                {
                    if (Candidate.LogLevel <= LogLevel)
                    {
                        PossibleActions.Add(Candidate.Action);
                    }
                }
            }
        }

        /// <summary>
        /// Process the user action in the current point
        /// </summary>
        /// <param name="Methods"></param>
        public void ProcessAction(string Method)
        {
            if (Point!=null)
            {
                MethodInfo MI = Point.GetType().GetRuntimeMethod(Method, new Type[0]);
      
                if (MI != null)
                {
                    //If method is found, invoke it
                    MI.Invoke(Point, null);

                    //Rebuild possible actions
                    BuildActions();

                    //Notify
                    NotifyPropertyChanges();
                }
            }
        }

        /// <summary>
        /// Return true if the object instance complies to all conditions
        /// </summary>
        /// <param name="Instance"></param>
        /// <param name="Condition"></param>
        /// <returns></returns>
        private bool CheckCondition(object Instance, string Condition)
        {
            bool Result = false;

            string[] Conditions = Condition.Split("|".ToCharArray());

            foreach (string SingleCondition in Conditions)
            {
                if (CheckSingleCondition(Instance, SingleCondition))
                {
                    Result = true;
                    break;
                }            
            }

            return Result;
        }

        /// <summary>
        /// Notify changes to all properties of this vmPoint
        /// </summary>
        public void NotifyPropertyChanges()
        {
            NotifyPropertyChanged("Description");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Instance"></param>
        /// <param name="Condition"></param>
        /// <returns></returns>
        private bool CheckSingleCondition(object Instance, string Condition)
        {
            bool Result = true;

            string[] ConditionParts = Condition.Split("&".ToCharArray());

            foreach (string ConditionPart in ConditionParts)
            {
                string[] ConditionElements = ConditionPart.Split("=".ToCharArray());

                if (ConditionElements.Length == 2)
                {
                    object _value = GetValue(Instance, ConditionElements[0]);
                    String Value = _value.ToString();

                    if (ConditionElements[1].StartsWith("!"))
                    {
                        String ConditionElement = ConditionElements[1].Substring(1);

                        if (Value == ConditionElement)
                        {
                            Result = false;
                            break;
                        }
                    }
                    else
                    {
                        if (Value != ConditionElements[1])
                        {
                            Result = false;
                            break;
                        }
                    }
                }
            }

            return Result;
        }

        /// <summary>
        /// Get the value of the property or field of the instance
        /// </summary>
        /// <param name="Instance"></param>
        /// <param name="PropertyOrField"></param>
        /// <returns></returns>
        private object GetValue(object Instance, string PropertyOrField)
        {
            //if (Instance == null) return "";

            //Try Property
            PropertyInfo PI = System.Reflection.RuntimeReflectionExtensions.GetRuntimeProperty(Instance.GetType(), PropertyOrField);
            if (PI != null) return PI.GetValue(Instance);

            //Try Field
            FieldInfo FI = System.Reflection.RuntimeReflectionExtensions.GetRuntimeField(Instance.GetType(), PropertyOrField);
            if (FI != null) return FI.GetValue(Instance);

            //Try Method
            System.Type[] abc = new Type[0];
            MethodInfo MI = System.Reflection.RuntimeReflectionExtensions.GetRuntimeMethod(Instance.GetType(), PropertyOrField, abc);
            if (MI != null)
            {
                object[] def = new object[0];
                return MI.Invoke(Instance, def);
            }

            return "";
            
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the page that a data context property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }
}

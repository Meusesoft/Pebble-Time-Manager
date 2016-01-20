using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Tennis_Statistics.Game_Logic
{
    [DataContract(IsReference = true)]
    public class TennisSet : IXmlSerializable
    {
        #region Constructors

        public TennisSet()
        { }

        public TennisSet(TennisMatch parent)
        {
            PartOf = parent;
            Duration = new TennisDuration();
            Games = new List<TennisGame>();
            StartServer = PartOf.GetNextServer() == PartOf.Contestant1 ? 1 : 2;
            Statistics = new TennisStatistics(this);
            ScoreContestant1 = 0;
            ScoreContestant2 = 0;
        }

        #endregion

        #region Properties

        [DataMember]
        public List<TennisGame> Games;

        [DataMember]
        public long ID;
        
        [DataMember]
        public int Winner;

        [DataMember]
        public int StartServer;
        
        [DataMember]
        public int ScoreContestant1;
        
        [DataMember]
        public int ScoreContestant2;
        
        [DataMember]
        public TennisDuration Duration;
        
        [DataMember]
        public TennisStatistics Statistics;
        
        [DataMember]
        public TennisMatch PartOf;

        #endregion

        #region Methods

        /// <summary>
        /// Start this set
        /// </summary>
        public void Start()
	    {
            Duration.Start();
	    }

        /// <summary>
        /// Stop the current match
        /// </summary>
        public void End(int Winner)
        {
           /* this.Winner = Winner;
            this.EndTime = DateTime.Now;

            TennisGame _game = currentGame();
            if (_game != null)
            {
                _game.End(Winner);
            }*/
        }
	
        /// <summary>
        /// Return the instance of the match this set belongs to.
        /// </summary>
        /// <returns></returns>
	    public TennisMatch currentMatch()
	    {
		    return PartOf;
	    }

        /// <summary>
        /// Return the instance of the game currently in progress
        /// </summary>
        /// <returns></returns>
	    public TennisGame currentGame()
	    {
		    if (Games.Count==0) return null;
		
		    return Games.Last();		
	    }
	
        /// <summary>
        /// Start a new game or tiebreak if applicable
        /// </summary>
	    public void StartNewGame()
	    {
		    //Check if another game is in progress
		    if (this.currentGame()!=null)
		    {
			    if (currentGame().State != TennisGame.StateEnum.sFinished)
			    {
				    throw new Exception("Another game is already in progress");
			    }
		    }
		
		    TennisGame newGame;

            if (ScoreContestant1 == ScoreContestant2
                && ScoreContestant1 == PartOf.TieBreakAtSameScoreOf
                && (PartOf.TieBreakFinalSet || PartOf.Sets.Count != PartOf.BestOutOf)
                )
            {
                //The set has entered a tiebreak
                TennisTiebreak newTiebreak = new TennisTiebreak();
                newTiebreak.SetStartServer(StartServer);

                newGame = newTiebreak;
                newGame.PartOf = this;
                newGame.PointsPerGame = PartOf.TieBreakLength;
            }
            else
            {
                if (this.PartOf.Sets.Count == this.PartOf.BestOutOf && this.PartOf.Sets.Count > 1 && this.PartOf.FinalSetIsTiebreak)
                {
                    //The final set has started, and it is a tiebreak
                    TennisTiebreak newTiebreak = new TennisTiebreak();
                    newTiebreak.SetStartServer(StartServer);

                    newGame = newTiebreak;
                    newGame.PartOf = this;
                    newGame.PointsPerGame = this.PartOf.FinalSetTieBreakLength;
                }
                else
                {
                    //Start new regular game
                    newGame = new TennisGame();
                    newGame.PartOf = this;
                }
            }

            if (Games.Count > 0)
            {
                newGame.Server = PartOf.GetNextServer().ContestantNr;
            }
            else
            {
                newGame.Server = StartServer;
            }
		    newGame.Receiver = PartOf.GetReceiver().ContestantNr;		
		    newGame.Start();

		    Games.Add(newGame);		
	    }

        /// <summary>
        /// Pause the current set
        /// </summary>
        public void Pause()
        {
            if (Duration.SessionInProgress)
            {
                Duration.End();
            }
        }

        /// <summary>
        /// Resume the current set
        /// </summary>
        public void Resume()
        {
            if (!Duration.SessionInProgress)
            {
                Duration.Start();
            }
        }

	/// <summary>
	/// Add a new point to this set
	/// </summary>
	/// <param name="newPoint"></param>
	    public void Add(TennisPoint newPoint)
	    {
            //Update statistics
            Statistics.Add(newPoint);
            
            //Start new game if necessary
            if (currentGame() != null)
		    {
			    if (currentGame().State == TennisGame.StateEnum.sFinished)
			    {
				    StartNewGame();
			    }
		    }
		    else
		    {
			    StartNewGame();
		    }		
		
		    //Add this new point to the game
		    if (currentGame().GetType() == typeof(TennisTiebreak))
            {
                TennisTiebreak currentTiebreak = (TennisTiebreak)currentGame();
                currentTiebreak.Add(newPoint);
            }
            else
            {
                currentGame().Add(newPoint);
            }

		
		    //Check the score if the game is finished
		    if (currentGame().State == TennisGame.StateEnum.sFinished) 
		    {
			    RebuildScore();

			    //Check if there is a winner for this set
			    int GamesPerSet = PartOf.NumberGamesPerSet;
		
			    if ((ScoreContestant1 >= GamesPerSet && ScoreContestant2 < ScoreContestant1 - 1) ||
				    (ScoreContestant2 >= GamesPerSet && ScoreContestant1 < ScoreContestant2 - 1) ||
				    (currentGame().GetType() == typeof(TennisTiebreak)))
			    {
                    Duration.End();
				    Winner = currentGame().Winner;
			    }
			
			    if (Winner==0)
			    {
				    StartNewGame();				
			    }
		    }
	    }	
	
        /// <summary>
        /// Recalculate the score of this set
        /// </summary>
	    private void RebuildScore()
	    {
		    ScoreContestant1 = 0;
		    ScoreContestant2 = 0;

		    foreach (TennisGame game in Games) {
			
			    if (game.State == TennisGame.StateEnum.sFinished) 
			    {
				    if (game.Winner == 1) ScoreContestant1++;
				    else ScoreContestant2++;	
			    }
		    }		
	    }
	
        /// <summary>
        /// Undo the last point
        /// </summary>
        /// <returns></returns>
	    public Boolean Undo() 
	    {
		    TennisPoint Result;
		
		    if (Winner != 0)
		    {
			    PartOf.GetContestant(Winner).SetsWon--;
			    Winner = 0;
                Duration.Start();
		    }		

		    Result = currentGame().Undo();

            while (Result==null)
		    {
			    //Remove the last game todo
			   /* TennisCore m_Core = TennisCore.getInstance();
			    m_Core.DeleteGame(currentGame().ID);*/
			    Games.Remove(currentGame());

			    if (Games.Count==0) return false;
			
			    Result = currentGame().Undo();
		    }

            if (Result!=null)
            {
                Statistics.Remove(Result);

                RebuildScore();
            }		

            return Result!=null;
	    }	
	
        /// <summary>
        /// Return a string representation of the score of this set
        /// </summary>
        /// <returns></returns>
	    public String getScore()
	    {
		    String Score;
				
		    Score = ScoreContestant1 + "";
		    if (Winner==0 && currentGame().Server == 1) Score+="*";
		    Score += "-" + ScoreContestant2;
		    if (Winner==0 && currentGame().Server == 2) Score+="*";
		
		    return Score;
        }
        #endregion

        #region XML

        /// <summary>
        /// Return the XML schema.
        /// </summary>
        /// <returns>null</returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generates a match object from its XML representation.
        /// </summary>
        /// <param name="reader"></param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.Name != "Set") throw new Exception("Unexpected node encountered, Set expected");

            String _ID = reader.GetAttribute("ID");
            String _Winner = reader.GetAttribute("Winner");
            String _StartServer = reader.GetAttribute("StartServer");
            String _ScoreContestant1 = reader.GetAttribute("ScoreContestant1");
            String _ScoreContestant2 = reader.GetAttribute("ScoreContestant2");

            ID = long.Parse(_ID);
            Winner = int.Parse(_Winner);
            StartServer = int.Parse(_StartServer);
            ScoreContestant1 = int.Parse(_ScoreContestant1);
            ScoreContestant2 = int.Parse(_ScoreContestant2);

            while (reader.Read() && !(reader.Name=="Set" && reader.NodeType == System.Xml.XmlNodeType.EndElement))
            {
                if (reader.NodeType == System.Xml.XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "Games":

                            while (reader.Read() && !(reader.Name == "Games" && reader.NodeType == System.Xml.XmlNodeType.EndElement))
                            {
                                if (reader.NodeType == System.Xml.XmlNodeType.Element)
                                {
                                    switch (reader.Name)
                                    {
                                        case "Game":

                                            TennisGame newGame = new TennisGame();
                                            newGame.PartOf = this;
                                            newGame.ReadXml(reader);
                                            this.PartOf.Points.AddRange(newGame.Points);

                                            this.Games.Add(newGame);

                                            break;

                                        case "Tiebreak":

                                            TennisTiebreak newTiebreak = new TennisTiebreak();
                                            newTiebreak.ReadXml(reader);
                                            newTiebreak.PartOf = this;
                                            this.PartOf.Points.AddRange(newTiebreak.Points);

                                            this.Games.Add(newTiebreak);

                                            break;
                                    }
                                }
                            }

                            break;

                        case "Statistics":

                            Statistics.ReadXml(reader);

                            break;

                        case "Duration":

                            Duration.ReadXml(reader);

                            break;
                    }
                }
            }

        }

        /// <summary>
        /// Converts this match into its XML representation.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("Set");

            //General
            writer.WriteAttributeString("ID", this.ID.ToString());
            writer.WriteAttributeString("Winner", this.Winner.ToString());
            writer.WriteAttributeString("StartServer", this.StartServer.ToString());
            writer.WriteAttributeString("ScoreContestant1", this.ScoreContestant1.ToString());
            writer.WriteAttributeString("ScoreContestant2", this.ScoreContestant2.ToString());

            //Games
            writer.WriteStartElement("Games");
            foreach(var item in this.Games)
            {
                item.WriteXml(writer);
            }
            writer.WriteEndElement();

            //Duration
            writer.WriteStartElement("Duration");
                this.Duration.WriteXml(writer);
            writer.WriteEndElement();

            //Statistics
            writer.WriteStartElement("Statistics");
                this.Statistics.WriteXml(writer);                
            writer.WriteEndElement();           


            writer.WriteEndElement();
        }

        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Tennis_Statistics.Game_Logic
{
    public enum Statistics {TotalDuration, TotalPoints, TotalPointsWon, TotalServicePoints, TotalServicePointsWon, TotalReturnPoints, TotalReturnPointsWon,  
				FirstServicesPlayed, FirstServicesWon, SecondServicesPlayed, SecondServicesWon,
				FirstReturnPlayed, FirstReturnWon, SecondReturnPlayed, SecondReturnWon,
				Aces, DoubleFaults, ServiceBreakPointsPlayed, ServiceBreakPointsWon, ReturnBreakPointsPlayed, ReturnBreakPointsWon,
				ServiceSetPointsPlayed, ServiceSetPointsConverted, ReturnSetPointsPlayed, ReturnSetPointsConverted,
				ServiceMatchPointsPlayed, ServiceMatchPointsConverted, ReturnMatchPointsPlayed, ReturnMatchPointsConverted,
				ServiceGamesPlayed, ServiceGamesWon, ReturnGamesPlayed, ReturnGamesWon,
				PointWinners, PointForcedErrors, PointUnforcedErrors,
                ForehandWinner, ForehandForcedError, ForehandUnforcedError, 
                BackhandWinner, BackhandForcedError, BackhandUnforcedError, 
                VolleyWinner, VolleyForcedError, VolleyUnforcedError,
                OverheadWinner, OverheadForcedError, OverheadUnforcedError,
                PassingWinner, PassingForcedError, PassingUnforcedError,
                DropshotWinner, DropshotForcedError, DropshotUnforcedError,
                LobWinner, LobForcedError, LobUnforcedError,
                ApproachShotWinner, ApproachShotForcedError, ApproachShotUnforcedError
    };

    /// <summary>
    /// The totals of a statistic within a set or match.
    /// </summary>
    [DataContract]
    public class TennisStatistic
    {
	    public TennisStatistic()
        { }

        public TennisStatistic(Statistics item)
	    {
		    Item = item;
	    }

        [DataMember]
        public Statistics Item { get; set; }
        
        [DataMember]
        public int Contestant { get; set; }
        
        [DataMember]
        public int Value { get; set; }
    }

    /// <summary>
    /// The collection of statistics of a set or match
    /// </summary>
    [DataContract]
    public class TennisStatistics : IXmlSerializable {
	
        [DataMember]    
	    public TennisMatch currentMatch { get; set; }

        [DataMember]
        public List<TennisStatistic> Items { get; set; }

        [DataMember]
        public TennisSet currentSet { get; set; }
	
	    public TennisStatistics()
        { }

        public TennisStatistics(TennisMatch owner) {

            Items = new List<TennisStatistic>();
            currentMatch = owner;
            currentSet = null;
	    }

	    public TennisStatistics(TennisSet owner) {

            Items = new List<TennisStatistic>();
            currentMatch = owner.currentMatch();
            currentSet = owner;
	    }
	
	    public void CalculateStatistics()
        {
    	
    	    Items.Clear();
    	
    	    if (currentSet == null)
    	    {
			    foreach (TennisSet set in currentMatch.Sets)
			    {
				    ProcessSet(set);
			    }

                AddDuration();            
            }
    	    else
    	    {
    		    ProcessSet(currentSet);

                AddDuration();
            }
         }
	
	    private void ProcessSet(TennisSet set)
	    {
    	    int Server = 1;
    	    int Returner = 2;
    	    int PointCount = 0;
            
            foreach (TennisGame game in set.Games)
    	    {
			    Server = game.Server;
    		    Returner = 3 - Server;
    		    PointCount = 1;

    		    if (game.GetType() != typeof(TennisTiebreak))
    		    {
    			    //Tiebreaks do not count for service games won or lost
	    		    if (game.Winner!=0) 
	    		    {
	    			    IncrementItem(Statistics.ServiceGamesPlayed, Server);
		    		    IncrementItem(Statistics.ReturnGamesPlayed, Returner);
	
	    			    if (game.Winner == game.Server) IncrementItem(Statistics.ServiceGamesWon, Server);
					    else IncrementItem(Statistics.ReturnGamesWon, Returner);
	    		    }
    		    }

			    foreach (TennisPoint point in game.Points)
    		    {	
	    		    if (game.GetType() != typeof(TennisTiebreak))
	    		    {
	    			    ProcessPoint(point, point.Server, 3 - point.Server, 1);
	    		    }
	    		    else
	    		    {
	    			    TennisTiebreak tiebreak = (TennisTiebreak)game;
	    			    Server = tiebreak.GetStartServer();
	    			    if (((PointCount / 2) % 2) == 1) Server = 3 - Server;
	    			    Returner = 3 - Server;

	    			    ProcessPoint(point, Server, Returner, 1);
	    			
	    			    PointCount ++;
	    		    }
    		    }
    	    }
	    }

        private void AddDuration()
        {
            if (currentSet != null)
            {
                SetItem(Statistics.TotalDuration, 0, (int)currentSet.Duration.Duration.TotalMinutes);
            }
            else
            {
                SetItem(Statistics.TotalDuration, 0, (int)currentMatch.Duration.Duration.TotalMinutes);
            }
        }

        public void Add(TennisPoint newPoint)
        {
            ProcessPoint(newPoint, newPoint.Server, 3 - newPoint.Server, 1);
            AddDuration();
        }

        public void Remove(TennisPoint newPoint)
        {
            ProcessPoint(newPoint, newPoint.Server, 3 - newPoint.Server, -1);
        }

        

        private void ProcessPoint(TennisPoint point, int Server, int Returner, int Delta)
	    {
           
            //Total Points
            IncrementItem(Statistics.TotalPoints, Server, Delta);
            IncrementItem(Statistics.TotalPoints, Returner, Delta);
            IncrementItem(Statistics.TotalPointsWon, point.Winner, Delta);
		
		    //Service and Return Points
            IncrementItem(Statistics.TotalServicePoints, Server, Delta);
            IncrementItem(Statistics.TotalReturnPoints, Returner, Delta);

            if (point.Winner == Server) IncrementItem(Statistics.TotalServicePointsWon, Server, Delta);
            else IncrementItem(Statistics.TotalReturnPointsWon, Returner, Delta);
			
			    //First services and return points
            if (point.Serve == TennisPoint.PointServe.FirstServe)
   		    {
                IncrementItem(Statistics.FirstServicesPlayed, Server, Delta);
                IncrementItem(Statistics.FirstReturnPlayed, Returner, Delta);
                if (point.Winner == Server) IncrementItem(Statistics.FirstServicesWon, Server, Delta);
   			    else IncrementItem(Statistics.FirstReturnWon, Returner, Delta);
   		    }
			
			    //Second services and return points
            if (point.Serve == TennisPoint.PointServe.SecondServe) 
   	   	    {
                IncrementItem(Statistics.SecondServicesPlayed, Server, Delta);
                IncrementItem(Statistics.SecondReturnPlayed, Returner, Delta);
                if (point.Winner == Server) IncrementItem(Statistics.SecondServicesWon, Server, Delta);
                else IncrementItem(Statistics.SecondReturnWon, Returner, Delta);
   	   	    }
		
		    //Aces and Double Faults
            if (point.Shot.Contains(TennisPoint.PointShot.Ace)) IncrementItem(Statistics.Aces, Server, Delta);
            if (point.Error == TennisPoint.PointError.DoubleFault) IncrementItem(Statistics.DoubleFaults, Server, Delta);
		
			    //Break points
            if (point.Type.Contains(TennisPoint.PointType.BreakPoint))
			    {
                    IncrementItem(Statistics.ServiceBreakPointsPlayed, Server, Delta);
                    IncrementItem(Statistics.ReturnBreakPointsPlayed, Returner, Delta);
                    if (point.Winner == Server) IncrementItem(Statistics.ServiceBreakPointsWon, Server, Delta);
                    else IncrementItem(Statistics.ReturnBreakPointsWon, Returner, Delta);
			    }
			
			    //Set points
            if (point.Type.Contains(TennisPoint.PointType.SetPoint))
			    {
                    if (point.Type.Contains(TennisPoint.PointType.SetPointServer)) 
				    {
                        IncrementItem(Statistics.ServiceSetPointsPlayed, Server, Delta);
                        if (point.Winner == Server) IncrementItem(Statistics.ServiceSetPointsConverted, Server, Delta);
				    }

                    if (!point.Type.Contains(TennisPoint.PointType.SetPointServer))
				    {
                        IncrementItem(Statistics.ReturnSetPointsPlayed, Returner, Delta);
                        if (point.Winner == Returner) IncrementItem(Statistics.ReturnSetPointsConverted, Returner, Delta);
					
				    }
			    }
		
			    //Match points
            if (point.Type.Contains(TennisPoint.PointType.MatchPoint))
			    {
                    if (point.Type.Contains(TennisPoint.PointType.MatchPointServer)) 
				    {
                        IncrementItem(Statistics.ServiceMatchPointsPlayed, Server, Delta);
                        if (point.Winner == Server) IncrementItem(Statistics.ServiceMatchPointsConverted, Server, Delta);
				    }

                    if (!point.Type.Contains(TennisPoint.PointType.MatchPointServer))
				    {
                        IncrementItem(Statistics.ReturnMatchPointsPlayed, Returner, Delta);
                        if (point.Winner == Returner) IncrementItem(Statistics.ReturnMatchPointsConverted, Returner, Delta);
				    }
			    }
			
			    //Shots
            if (point.ResultType == TennisPoint.PointResultType.Winner)
            {
                IncrementItem(Statistics.PointWinners, point.Winner, Delta);
                foreach (TennisPoint.PointShot _shot in point.Shot)
                {
                    switch (_shot)
                    {
                        case TennisPoint.PointShot.Forehand:
                            IncrementItem(Statistics.ForehandWinner, point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.Backhand:
                            IncrementItem(Statistics.BackhandWinner, point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.Volley:
                            IncrementItem(Statistics.VolleyWinner, point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.OverheadShot:
                            IncrementItem(Statistics.OverheadWinner, point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.Dropshot:
                            IncrementItem(Statistics.DropshotWinner, point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.Passing:
                            IncrementItem(Statistics.PassingWinner, point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.Lob:
                            IncrementItem(Statistics.LobWinner, point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.ApproachShot:
                            IncrementItem(Statistics.ApproachShotWinner, point.Winner, Delta);
                            break;
                        default:
                            break;
                    }
                }
            }
            if (point.ResultType == TennisPoint.PointResultType.ForcedError)
            {
                IncrementItem(Statistics.PointForcedErrors, 3 - point.Winner, Delta);
                foreach (TennisPoint.PointShot _shot in point.Shot)
                {
                    switch (_shot)
                    {
                        case TennisPoint.PointShot.Forehand:
                            IncrementItem(Statistics.ForehandForcedError, 3 - point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.Backhand:
                            IncrementItem(Statistics.BackhandForcedError, 3 - point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.Volley:
                            IncrementItem(Statistics.VolleyForcedError, 3 - point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.OverheadShot:
                            IncrementItem(Statistics.OverheadForcedError, 3 - point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.Dropshot:
                            IncrementItem(Statistics.DropshotForcedError, 3 - point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.Passing:
                            IncrementItem(Statistics.PassingForcedError, 3 - point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.Lob:
                            IncrementItem(Statistics.LobForcedError, 3 - point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.ApproachShot:
                            IncrementItem(Statistics.ApproachShotForcedError, 3 - point.Winner, Delta);
                            break;
                        default:
                            break;
                    }
                }
            }
            if (point.ResultType == TennisPoint.PointResultType.UnforcedError)
            {
                IncrementItem(Statistics.PointUnforcedErrors, 3 - point.Winner, Delta);
                foreach (TennisPoint.PointShot _shot in point.Shot)
                {
                    switch (_shot)
                    {
                        case TennisPoint.PointShot.Forehand:
                            IncrementItem(Statistics.ForehandUnforcedError, 3 - point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.Backhand:
                            IncrementItem(Statistics.BackhandUnforcedError, 3 - point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.Volley:
                            IncrementItem(Statistics.VolleyUnforcedError, 3 - point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.OverheadShot:
                            IncrementItem(Statistics.OverheadUnforcedError, 3 - point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.Dropshot:
                            IncrementItem(Statistics.DropshotUnforcedError, 3 - point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.Passing:
                            IncrementItem(Statistics.PassingUnforcedError, 3 - point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.Lob:
                            IncrementItem(Statistics.LobUnforcedError, 3 - point.Winner, Delta);
                            break;
                        case TennisPoint.PointShot.ApproachShot:
                            IncrementItem(Statistics.ApproachShotUnforcedError, 3 - point.Winner, Delta);
                            break;
                        default:
                            break;
                    }
                }
            }

            //Number of games
            if (point.Type.Contains(TennisPoint.PointType.GamePoint) && point.Winner == point.Server)
            {
                IncrementItem(Statistics.ServiceGamesWon, point.Server, Delta);
                IncrementItem(Statistics.ServiceGamesPlayed, point.Server, Delta);
                IncrementItem(Statistics.ReturnGamesPlayed, 3 - point.Server, Delta);
            }
            if (point.Type.Contains(TennisPoint.PointType.BreakPoint) && point.Winner != point.Server)
            {
                IncrementItem(Statistics.ServiceGamesPlayed, point.Server, Delta);
                IncrementItem(Statistics.ReturnGamesWon, 3 - point.Server, Delta);
                IncrementItem(Statistics.ReturnGamesPlayed, 3 - point.Server, Delta);
            }

	    }
    
        public int GetItem(Statistics Item, int Contestant)
        {
    	    int Result;
    	
    	    Result = 0;

    	    foreach (TennisStatistic item in Items) {
			
    		    if (item.Item == Item && item.Contestant == Contestant)
    		    {
    			    Result = item.Value;
                    break;
    		    }
		    }
    	
    	    return Result;
        }
    
        /// <summary>
        /// Increments the specified statistic for the contestant with 1
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="Contestant"></param>
        public void IncrementItem(Statistics Item, int Contestant)
        {
            IncrementItem(Item, Contestant, 1);
        }

        /// <summary>
        /// Decrement the specified statistic for the given contestant with 1
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="Contestant"></param>
        public void DecrementItem(Statistics Item, int Contestant)
        {
            IncrementItem(Item, Contestant, -1);
        }
        
        /// <summary>
        /// Increments the specified statitic for the contestant with the specified Delta value;
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="Contestant"></param>
        /// <param name="Delta"></param>
        private void IncrementItem(Statistics Item, int Contestant, int Delta)
        {
            Boolean Found;

            Found = false;

            foreach (TennisStatistic item in Items)
            {

                if (item.Item == Item && item.Contestant == Contestant)
                {
                    item.Value+=Delta;
                    Found = true;
                }
            }

            if (!Found)
            {
                TennisStatistic newItem = new TennisStatistic(Item);
                newItem.Contestant = Contestant;
                newItem.Value = Delta > 0 ? Delta : 0;

                Items.Add(newItem);
            }
        }
    
        /// <summary>
        /// Increments the specified statitic for the contestant with the specified Delta value;
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="Contestant"></param>
        /// <param name="Delta"></param>
        private void SetItem(Statistics Item, int Contestant, int Value)
        {
            Boolean Found;

            Found = false;

            foreach (TennisStatistic item in Items)
            {

                if (item.Item == Item && item.Contestant == Contestant)
                {
                    item.Value = Value;
                    Found = true;
                }
            }

            if (!Found)
            {
                TennisStatistic newItem = new TennisStatistic(Item);
                newItem.Contestant = Contestant;
                newItem.Value = Value;

                Items.Add(newItem);
            }
        }

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
            if (reader.Name != "Statistics") throw new Exception("Unexpected node encountered, Statistics expected");

            while (reader.Name.StartsWith("Statistic") && reader.NodeType == System.Xml.XmlNodeType.Element)
            {
                if (reader.Name == "Statistic")
                {
                    String _Contestant = reader.GetAttribute("Contestant");
                    String _Item = reader.GetAttribute("Item");
                    String _Value = reader.GetAttribute("Value");

                    TennisStatistic newStatistic = new TennisStatistic();
                    newStatistic.Contestant = int.Parse(_Contestant);
                    newStatistic.Item = (Statistics)System.Enum.Parse(typeof(Statistics), _Item);
                    newStatistic.Value = int.Parse(_Value);
                    Items.Add(newStatistic);
                }

                reader.Read();
            }
        }

        /// <summary>
        /// Converts this match into its XML representation.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (var Statistic in Items)
            {
                writer.WriteStartElement("Statistic");

                writer.WriteAttributeString("Contestant", Statistic.Contestant.ToString());
                writer.WriteAttributeString("Item", Statistic.Item.ToString());
                writer.WriteAttributeString("Value", Statistic.Value.ToString());

                writer.WriteEndElement();
            }
        }

        #endregion

    }
}

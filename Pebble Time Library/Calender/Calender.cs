using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.Data.Json;
using Windows.Services.Maps;
using Windows.Devices.Geolocation;

namespace Pebble_Time_Manager.Calender
{
    public class Calender
    {
        #region Constructor

        public Calender()
        {

        }

        #endregion

        #region Fields

        public ObservableCollection<String> Log;
        private Connector.PebbleConnector _PebbleConnector;
        public bool Reminders;

        private List<CalenderItem> SynchronizedItems;
        private List<CalenderItem> PreviousSynchronizedItems;

        #endregion

        #region Methods

        //Synchronize calender
        public async Task Synchronize()
        {
            try
            {
                _PebbleConnector = Connector.PebbleConnector.GetInstance();
                
                //Load synchronized calender items
                PreviousSynchronizedItems = null;
                SynchronizedItems = null;

                String XMLList = await Common.LocalStorage.Load("calenderitems.xml");
                if (XMLList.Length > 0) PreviousSynchronizedItems = (List<CalenderItem>)Common.Serializer.XMLDeserialize(XMLList, typeof(List<CalenderItem>));
                if (PreviousSynchronizedItems == null) PreviousSynchronizedItems = new List<CalenderItem>();
                SynchronizedItems = new List<CalenderItem>();
                PreviousSynchronizedItems.RemoveAll(x => x.Time < (DateTime.Now - new TimeSpan(3, 0, 0, 0))); // remove old items from items

                //Retrieve all appointments for the previous 2 days and next
                var appointmentStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadOnly);

                FindAppointmentsOptions findOptions = new FindAppointmentsOptions();
                findOptions.MaxCount = 64;
                findOptions.FetchProperties.Add(AppointmentProperties.Subject);
                findOptions.FetchProperties.Add(AppointmentProperties.Location);
                findOptions.FetchProperties.Add(AppointmentProperties.StartTime);
                findOptions.FetchProperties.Add(AppointmentProperties.Duration);
                findOptions.FetchProperties.Add(AppointmentProperties.Details);
                findOptions.FetchProperties.Add(AppointmentProperties.Reminder);

                IReadOnlyList<Appointment> _appointments =
                    await appointmentStore.FindAppointmentsAsync(DateTime.Now - new TimeSpan(2, 0, 0, 0), TimeSpan.FromDays(4), findOptions);

                //Send all items to Pebble
                foreach (Appointment _appointment in _appointments)
                {
                    await AddCalenderItem(
                        _appointment.RoamingId,
                        _appointment.Subject,
                        _appointment.Location,
                        _appointment.StartTime.DateTime,
                        (int)_appointment.Duration.TotalMinutes,
                        _appointment.Details,
                        _appointment.Reminder);

                    Log.Add("Appointment: " + _appointment.Subject);

                }

                //Remove items
                foreach (CalenderItem _item in PreviousSynchronizedItems)
                {
                    if (_item.CalenderItemID.Length != 0) await RemoveCalenderItem(Guid.Parse(_item.CalenderItemID));
                    if (_item.ReminderID.Length != 0) await RemoveCalenderReminderItem(Guid.Parse(_item.ReminderID));
                }

               
                //Save synchronized items
                XMLList = Common.Serializer.XMLSerialize(SynchronizedItems);
                await Common.LocalStorage.Save(XMLList, "calenderitems.xml", false);
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// Clear the previously sent timeline items
        /// </summary>
        /// <returns></returns>
        public async Task Clear()
        {
            try
            {
                _PebbleConnector = Connector.PebbleConnector.GetInstance();

                //Load synchronized items
                PreviousSynchronizedItems = null;

                String XMLList = await Common.LocalStorage.Load("calenderitems.xml");
                if (XMLList.Length > 0)
                {
                    PreviousSynchronizedItems = (List<CalenderItem>)Common.Serializer.XMLDeserialize(XMLList, typeof(List<CalenderItem>));
                    if (PreviousSynchronizedItems != null)
                    {

                        //Send all items to Pebble
                        foreach (CalenderItem _item in PreviousSynchronizedItems)
                        {
                            if (_item.CalenderItemID.Length != 0) await RemoveCalenderItem(Guid.Parse(_item.CalenderItemID));
                            if (_item.ReminderID.Length != 0) await RemoveCalenderReminderItem(Guid.Parse(_item.ReminderID));
                        }

                        await ClearCache();

                        Log.Add("Cache cleared on phone");
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// Clear the calender cache
        /// </summary>
        /// <returns></returns>
        public async Task ClearCache()
        {
            await Common.LocalStorage.Delete("calenderitems.xml");
        }

        /// <summary>
        /// Add the calender item to the pebble
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Description"></param>
        /// <param name="Location"></param>
        /// <param name="Time"></param>
        /// <param name="Duration"></param>
        /// <param name="Content"></param>
        /// <returns></returns>
        private async Task<bool> AddCalenderItem(String ID, String Description, String Location, DateTime Time, int Duration, String Details, TimeSpan? Reminder)
        {

            try
            {
                //Create a Guid or select the present one
                Guid MessageId = Guid.NewGuid();
                System.Diagnostics.Debug.WriteLine(String.Format("Roaming ID: {0}", ID));
                System.Diagnostics.Debug.WriteLine(String.Format("New guid: {0}", MessageId.ToString()));

                foreach (var item in  PreviousSynchronizedItems.Where(x => x.RoamingID == ID))
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("Old guid: {0}",item.CalenderItemID));

                    MessageId = Guid.Parse(item.CalenderItemID);
                    item.CalenderItemID = "";
                }

                //Add item to list of synchronized/send items
                CalenderItem _newItem = new CalenderItem() { RoamingID = ID, Time = Time, CalenderItemID = MessageId.ToString(), ReminderID = "" };
                SynchronizedItems.Add(_newItem); 

                //Send item to Pebble
                P3bble.Messages.TimeLineCalenderMessage _tlcm = new P3bble.Messages.TimeLineCalenderMessage(_PebbleConnector.GetNextMessageIdentifier(), MessageId, Description, Location, Time, Duration, Details);
                _tlcm.ToBuffer();
                await _PebbleConnector.Pebble.WriteTimeLineCalenderAsync(_tlcm);

                //Send reminder
                if (Reminders && Reminder.HasValue)
                {
                    await AddCalenderItemReminder(
                        _newItem,
                        ID,
                        MessageId,
                        Description,
                        Location,
                        Time,
                        Reminder.Value,
                        Details);

                    Log.Add("Reminder: " + Reminder);
                }    

                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + Description);
            }

            return false;
        }

        /// <summary>
        /// Remove the calender item from the pebble timeline
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        private async Task<bool> RemoveCalenderItem(Guid ID)
        {

            try
            {
                //Send remove item command to Pebble
                P3bble.Messages.TimeLineCalenderRemoveMessage _tlcm = new P3bble.Messages.TimeLineCalenderRemoveMessage(_PebbleConnector.GetNextMessageIdentifier(), ID);
                _tlcm.ToBuffer();
                await _PebbleConnector.Pebble.WriteTimeLineCalenderAsync(_tlcm);

                Log.Add("Remove appointment: " + ID.ToString());

                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception:" + ID);
            }

            return true;
        }

        /// <summary>
        /// Add a calender item reminder to the pebble
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Description"></param>
        /// <param name="Location"></param>
        /// <param name="Time"></param>
        /// <param name="ReminderTimeOffset"></param>
        /// <param name="Details"></param>
        /// <returns></returns>
        private async Task<bool> AddCalenderItemReminder(CalenderItem _CalenderItem, String ID, Guid BelongsTo, String Description, String Location, DateTime Time, TimeSpan ReminderTimeOffset, String Details)
        {
            try
            {
                if ((Time - ReminderTimeOffset) < DateTime.Now)
                {
                    System.Diagnostics.Debug.WriteLine("Reminder in the past, do not send message.");
                    return false;
                }                        

                Guid MessageId = Guid.NewGuid();
                
                System.Diagnostics.Debug.WriteLine(String.Format("Roaming ID: {0}", ID));

                //Create a Guid or select the present one
                foreach (var item in PreviousSynchronizedItems.Where(x => x.RoamingID == ID))
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("Old reminder guid: {0}", item.ReminderID));

                    if (Guid.TryParse(item.ReminderID, out MessageId))
                    { 
                        item.ReminderID = ""; 
                    }
                    else
                    {
                        MessageId = Guid.NewGuid();
                    }
                }
                System.Diagnostics.Debug.WriteLine(String.Format("New reminder guid: {0}", MessageId.ToString()));

                //Add item to list of synchronized items
                _CalenderItem.ReminderID = MessageId.ToString();
                
                //Send item to Pebble
                P3bble.Messages.TimeLineCalenderReminderMessage _tlcm = new P3bble.Messages.TimeLineCalenderReminderMessage(_PebbleConnector.GetNextMessageIdentifier(), MessageId, BelongsTo, Description, Location, Time, ReminderTimeOffset, Details);
                _tlcm.ToBuffer();
                await _PebbleConnector.Pebble.WriteTimeLineCalenderAsync(_tlcm);

                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception:" + Description);
            }

            return false;  
            
            /*String Identifier = System.Guid.NewGuid().ToString("N").ToUpper();
            Identifier = Identifier.Insert(30, ":");
            Identifier = Identifier.Insert(28, ":");
            Identifier = Identifier.Insert(26, ":");
            Identifier = Identifier.Insert(24, ":");
            Identifier = Identifier.Insert(22, ":");
            Identifier = Identifier.Insert(20, ":");
            Identifier = Identifier.Insert(18, ":");
            Identifier = Identifier.Insert(16, ":");
            Identifier = Identifier.Insert(14, ":");
            Identifier = Identifier.Insert(12, ":");
            Identifier = Identifier.Insert(10, ":");
            Identifier = Identifier.Insert(8, ":");
            Identifier = Identifier.Insert(6, ":");
            Identifier = Identifier.Insert(4, ":");
            Identifier = Identifier.Insert(2, ":");

            //return;
            String HostIdentifier = "ed:43:9c:16:f6:74:42:20:95:da:45:4f:30:3f:15:e2";

            String U1 = "01:08"; // Transaction ID
            String U2 = "b1:db"; // Text endpoint
            String U3 = String.Format("{0}:72:00:{0}", Identifier);

            DateTime NowUTC = new DateTimeOffset(Time).UtcDateTime;
            long Seconds = (long)(NowUTC - new DateTime(1970, 1, 1)).TotalSeconds;
            Seconds -= ReminderTimeOffset * 60;
            String SecondsHD = Seconds.ToString("X4");
            String U4 = String.Format("{0}:{1}:{2}:{3}", SecondsHD.Substring(6, 2), SecondsHD.Substring(4, 2), SecondsHD.Substring(2, 2), SecondsHD.Substring(0, 2));

            String Header = Description;
            String Content = Location;

            UInt32 iHeaderLength = (UInt32)Header.Length;
            UInt32 iContentLength = (UInt32)Content.Length;
            UInt32 iTotalLength = iHeaderLength + 3 + iContentLength + 3 + 49;

            String HeaderLength = iHeaderLength.ToString("X2");
            String ContentLength = iContentLength.ToString("X2");
            String TotalLength = iTotalLength.ToString("X2");

            String XHeader = HeaderLength + ":00";
            String XContent = ContentLength + ":00";
            String XTotalLength = TotalLength + ":00";

            char[] cHeader = Header.ToCharArray();
            char[] cContent = Content.ToCharArray();

            int nItem;

            foreach (char item in cHeader)
            {
                nItem = (int)item;
                XHeader += ":" + nItem.ToString("X2");
            }

            foreach (char item in cContent)
            {
                nItem = (int)item;
                XContent += ":" + nItem.ToString("X2");
            }

            String Template = "xx:xx:{1}:01:{0}:01:10:{2}:yy:yy:{2}:{3}:{4}:00:00:03:01:00:03:{5}:03:03:01:{6}:0b:{7}:04:04:00:03:00:00:80:00:04:01:01:07:00:44:69:73:6d:69:73:73:01:0a:01:01:04:00:4d:6f:72:65:02:02:01:01:0d:00:4d:75:74:65:20:43:61:6c:65:6e:64:61:72";

            String Message = String.Format(Template,
                U1,         //0 = Transaction ID?
                U2,         //1 = Text endpoint ?
                Identifier, //2 = Message identifier
                HostIdentifier,  //3 = Host identifier
                U4,         //4 = Timestamp (seconds from 1970-1-1)
                XTotalLength, //5 = Length total message
                XHeader,    //6 = Header Message
                XContent    //7 = Content Message
                );


            //Add Total message length
            int Payload = (Message.Length + 1) / 3;
            Payload -= 4;
            String XPayLoadSize = "00:" + Payload.ToString("X2");
            Message = Message.Replace("xx:xx", XPayLoadSize);

            int MessageSize = Payload - 23;
            String sMessageSize = MessageSize.ToString("X2");
            String XMessageSize = sMessageSize + ":00";
            Message = Message.Replace("yy:yy", XMessageSize);

            //Convert to bytes
            //await WriteMessage(Message);

            return true;*/
        }

        /// <summary>
        /// Remove the calender reminder from the pebble timeline
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        private async Task<bool> RemoveCalenderReminderItem(Guid ID)
        {

            try
            {
                //Send remove item command to Pebble
                P3bble.Messages.TimeLineCalenderReminderRemoveMessage _tlcm = new P3bble.Messages.TimeLineCalenderReminderRemoveMessage(_PebbleConnector.GetNextMessageIdentifier(), ID);
                _tlcm.ToBuffer();
                await _PebbleConnector.Pebble.WriteTimeLineCalenderAsync(_tlcm);

                Log.Add("Remove reminder: " + ID.ToString());

                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + e.Message);
            }

            return true;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Convert the RoamingId to a GUID
        /// </summary>
        /// <param name="RoamingId"></param>
        /// <returns></returns>
        private Guid RoamingIdToGuid(String RoamingId)
        {
            String _value = Regex.Replace(RoamingId, "[^0-9]", "");
            
            if (_value.Length > 32) _value = _value.Substring(_value.Length - 32, 32);

            while (_value.Length < 32) 
            {
                _value = "0" + _value;
            }
            
            return Guid.Parse(_value);
        }

        /// <summary>
        /// Check if it is a special character (> 0x7F)
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool IsSpecialChar(char c)
        {
            if (c > 0x7F) return true;
            return false;
        }

        /// <summary>
        /// Remove special characters from string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string RemoveSpecialChars(string s)
        {
            var builder = new System.Text.StringBuilder();
            foreach (var cur in s)
            {
                if (!IsSpecialChar(cur))
                {
                    builder.Append(cur);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Convert integer to hexadecimal string representation
        /// </summary>
        /// <param name="iValue"></param>
        /// <returns></returns>
        private string ConvertToHex(Int32 iValue)
        {

            Int32 iDuration = iValue;
            byte[] bytes = BitConverter.GetBytes(iValue);
            return bytes[0].ToString("X2") + ":" + bytes[1].ToString("X2");
        }

        /// <summary>
        /// Convert to hexadecimal, little endian style
        /// </summary>
        /// <param name="iValue"></param>
        /// <returns></returns>
        private string ConvertToHexReverse(Int32 iValue)
        {

            Int32 iDuration = iValue;
            byte[] bytes = BitConverter.GetBytes(iValue);
            return bytes[1].ToString("X2") + ":" + bytes[0].ToString("X2");
        }

        #endregion

    }
}

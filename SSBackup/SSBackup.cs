using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Support;


namespace SebsBackup
{
    public partial class SSBackup : ServiceBase
    {
        public SSBackup()
        {
            serviceLogger = new Logger(_folderPath, ApplicationType.Service);
            InitializeComponent();
        }

        private readonly Logger serviceLogger; 
        private static readonly string _folderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private static readonly string _configurationPath = _folderPath + @"\configuration.bin";
        private int frequency;
        private readonly List<String> sources = new List<string>();
        private string destination; 

      

        protected override void OnStart(string[] args)
        {
            serviceLogger.WriteToLogFile("SSBackup service started.", Severity.Info);
            try
            {
                string[] configItems = Support.Support.Deserialize(_configurationPath);
                for (int i = 0; i < configItems.Length -2; i++)
                {
                    if (Support.Support.CheckPath(configItems[i], PathType.Directory))
                    {
                        sources.Add(configItems[i]);
                    }
                    else
                    {
                        serviceLogger.WriteToLogFile("Invalid source in config file: " + configItems[i], Severity.Critical);
                        this.OnStop(); 
                    }                    
                }
                destination = configItems[configItems.Length - 2]; 
                string[] configTime = Support.Support.ReturnFrequency(configItems[configItems.Length - 1]);
                DateTime timeToStart = DateTime.Parse(configTime[0] + configTime[1] + configTime[2]);
                frequency = int.Parse(configTime[3].Substring(0,2));
                serviceLogger.WriteToLogFile("Will sleep until scheduled backup at interval of: " + frequency +"h. Backup pinned time is:" + timeToStart.TimeOfDay, Severity.Info);
                if (DateTime.Now.Hour == timeToStart.Hour && timeToStart.Minute > DateTime.Now.Minute ) //We are on the hour just need to wait for minutes to line up. 
                {
                    Thread.Sleep((timeToStart.Minute - DateTime.Now.Minute)*1000*60); //Waiting for minutes //verified 
                }
                else //We are on the hour but the minutes have passed so we need to wait for next multiple, or we are not on the hour. 
                { 
                    if(timeToStart.Minute > DateTime.Now.Minute) //Not on the hour but before minutes have passed. Need to wait for minutes and hours.  //verified
                    {
                        Thread.Sleep((timeToStart.Minute - DateTime.Now.Minute)*1000*60); //Sleep until minutes line up //verified 

                    }
                    else //On the hour and minutes have passed, or not on the hour and minutes have passed 
                    {
                        Thread.Sleep(((60 - DateTime.Now.Minute)+timeToStart.Minute)*1000*60);  //Sleep until minutes line up //Verified 
                    }
                    if (Math.Abs(timeToStart.Hour - DateTime.Now.Hour) % frequency != 0 && frequency !=1) //only wait if off schedule to line hours... 
                    {
                        List<int> scheduledHours = new List<int>();
                        int i = timeToStart.Hour;
                        while(i < 24)
                        {
                            scheduledHours.Add(i);
                            i += frequency; 
                        }
                        i = timeToStart.Hour - frequency; 
                        while (i>0)
                        {
                            scheduledHours.Add(i);
                            i -= frequency; 
                        }
                        scheduledHours.Sort();
                        int hoursToWait = (from entry in scheduledHours where (entry >= DateTime.Now.Hour) select entry).FirstOrDefault() - DateTime.Now.Hour;
                        Thread.Sleep(hoursToWait*1000*60*60); 
                    }
                }
            }
            catch (Exception Ex)
            {
                serviceLogger.WriteToLogFile("An exception occurred during initilizaiton:" + Ex.Message, Severity.Error);
                serviceLogger.WriteToLogFile(Ex.StackTrace, Severity.Error);
            }
            serviceLogger.WriteToLogFile("Sleep is finished. Starting initial backup.", Severity.Info);

            this.Backup();

            serviceLogger.WriteToLogFile("Initial backup completed. Will now backup regularly at a frequency of: " + frequency + "h"  , Severity.Info);

            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = frequency * 60 * 60 * 1000 //frequency is in hours
            };

            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.Backup);
            timer.Start();
        }

        protected override void OnStop()
        {
            serviceLogger.WriteToLogFile("SSBackup service stopped", Severity.Info);
        }

        public void Backup(object sender=null, System.Timers.ElapsedEventArgs args=null)
        {
            Copyer myCopyer = new Copyer(ApplicationType.Service);
            serviceLogger.WriteToLogFile("Backup started", Severity.Info); 
            foreach(string source in sources)
            {
                try
                {
                    myCopyer.Backup(source, destination);
                }
                catch (Exception Ex)
                {
                    serviceLogger.WriteToLogFile("There was an issue backing up: " + source + "Exception is: " + Ex.Message + "Stack trace is: " + Ex.StackTrace, Severity.Error);
                    serviceLogger.WriteToLogFile("Will attempt to backup at next scheduled time", Severity.Info); 
                }
                
            }
        }
    }
}

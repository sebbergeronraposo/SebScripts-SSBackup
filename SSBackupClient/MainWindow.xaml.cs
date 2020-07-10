using Ookii.Dialogs.Wpf;
using Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Reflection;
using SSBackupClient;
using System.ServiceProcess;
using System.IO;
using System.Threading;

namespace SebsBackupClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            try
            {
                myLogger = new Logger(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ApplicationType.Client);
            }
            catch
            {
                //fatal if we can't create log file 
                PromptUser promptUser = new PromptUser("Fatal error, unable to create logger. Application will terminate");
                promptUser.ShowDialog();
                System.Windows.Application.Current.Shutdown();
            }
            try
            {              
                configFile = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\Configuration.bin";
            }
            catch
            {
                try
                {
                    File.Create(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\Configuration.bin"); 
                }
                catch(Exception Ex)
                {
                    PromptUser("Unable to find nor create configuration file. Will exit. Exception is:" + Ex.Message, Ex.StackTrace, Severity.Warning);
                    System.Windows.Application.Current.Shutdown();
                }
            }
            
            InitializeComponent();
            InitializaeUIObjects();
            Items = new List<String>();
            this.Width = 691.2;
            try
            {
                myCopyer = new Copyer(ApplicationType.Client);
            }
            catch (Exception Ex) //This should never get hit. Here just in case... 
            {
                PromptUser("Unable to create Copyer and possibly log file. Application will close. Exception is" + Ex.Message);
                System.Windows.Application.Current.Shutdown();
            }     
            //Does this if statement need to be in a Try Catch ? 
            if (Support.Support.CheckPath(configFile, PathType.File))
            {
                string[] configItems = Support.Support.Deserialize(configFile);
                Destination.Text = configItems[configItems.Length - 2]; //Need to do this before sources to calculate size difference hgb
                string[] configTime = Support.Support.ReturnFrequency(configItems[configItems.Length - 1]);
                Hours.Text = configTime[0];
                Minutes.Text = configTime[1];
                AMPM.Text = configTime[2]; 
                Frequency.Text = configTime[3];

                for (int i = 0; i < configItems.Length -2; i++)
                {
                    string searchString = @"Source" + (i+1).ToString();
                    List<Object> _sourceUIObjects = new List<Object>();
                    _sourceUIObjects = (from entry in sourceUIObjects where (entry.Key == searchString) select entry.Value).FirstOrDefault();
                    ((System.Windows.Controls.TextBox)_sourceUIObjects[0]).Text = configItems[i];
                    Items.Add(configItems[i]); 
                    ChangeVisibility(_sourceUIObjects, Visibility.Visible); 
                    if (i > 0)
                    {
                        screenSize += 0.125;
                    }                   
                }               
                sourceNumber = configItems.Length - 2;
                destinationPosition = sourceNumber + 1;
                overviewPosition = destinationPosition + 1;
                Items.Add(configItems[configItems.Length - 2]);

                for (int i = configItems.Length - 2; i < 4; i++)
                {
                    string searchString = @"Source" + (i + 1).ToString();
                    List<Object> _sourceUIObjects = new List<Object>();
                    _sourceUIObjects = (from entry in sourceUIObjects where (entry.Key == searchString) select entry.Value).FirstOrDefault();
                    ChangeVisibility(_sourceUIObjects, Visibility.Hidden);                    
                }

                for (int i = 3; i < 6; i++)
                {
                    string searchString = @"Source" + (i-1).ToString();
                    List<Object> _sourceUIObjects = new List<Object>();
                    _sourceUIObjects = (from entry in sourceUIObjects where (entry.Key == searchString) select entry.Value).FirstOrDefault();
                    if (((System.Windows.Controls.TextBox)_sourceUIObjects[0]).Text == null || ((System.Windows.Controls.TextBox)_sourceUIObjects[0]).Text == string.Empty)
                    {
                        UIGrid.RowDefinitions[i].Height = new GridLength(0); 
                    }
                }
                UIGrid.RowDefinitions[6].Height = new GridLength(0);
                SourceUIChange(); 
            }
            else
            {
                for (int i = 3; i < 7; i++)
                {
                    UIGrid.RowDefinitions[i].Height = new GridLength(0);
                    Source1.Text = "Sources Folder";
                    Destination.Text = "Destination Folder"; 

                }
            }
            
            MoveNonSource(); 
            SetGreen(SaveConfig); 
            SetScreen();
            DisplayRemoveSource();
            RetrieveServiceStatus();
            GetLastBackupTime();
        }
        
        VistaFolderBrowserDialog FolderPicker = new VistaFolderBrowserDialog();
        private readonly Logger myLogger; 
        private readonly Copyer myCopyer;
        private readonly string configFile; 
        private readonly List<string> Items;
        private Dictionary<string, List<Object>> sourceUIObjects = new Dictionary<string, List<Object>>();
        private List<Object> destinationUIObjectsList = new List<Object>();
        private List<object> overviewUIObjectsList = new List<Object>();
        private static int sourceNumber = 1;
        private static int destinationPosition = sourceNumber + 1;
        private static int overviewPosition = destinationPosition + 1; 
        private double screenSize = 0.45;
        bool isConfigSaved = true;
        private void InitializaeUIObjects()
        {
            List<Object> source1UIObjectsList = new List<Object>
            {
            Source1,
            Source1_Browse,
            Source1TotalSize,
            Source1NumberOfFiles,
            Source1ProgressBar,
            Source1BackupTime,
            Source1SizeDiff,
            Source1Rectangle,
            Source1_Backup
            };
            List<Object> source2UIObjectsList = new List<Object>
            {
            Source2,
            Source2_Browse,
            Source2TotalSize,
            Source2NumberOfFiles,
            Source2ProgressBar,
            Source2BackupTime,
            Source2SizeDiff,
            Source2Rectangle,
            Source2_Backup
            };
            List<Object> source3UIObjectsList = new List<Object>
            {
            Source3,
            Source3_Browse,
            Source3TotalSize,
            Source3NumberOfFiles,
            Source3ProgressBar,
            Source3BackupTime,
            Source3SizeDiff,
            Source3Rectangle,
            Source3_Backup
            };
            List<Object> source4UIObjectsList = new List<Object>
            {
            Source4,
            Source4_Browse,
            Source4TotalSize,
            Source4NumberOfFiles,
            Source4ProgressBar,
            Source4BackupTime,
            Source4SizeDiff,
            Source4Rectangle,
            Source4_Backup
            };

            sourceUIObjects.Add(@"Source1", source1UIObjectsList);
            sourceUIObjects.Add(@"Source2", source2UIObjectsList);
            sourceUIObjects.Add(@"Source3", source3UIObjectsList);
            sourceUIObjects.Add(@"Source4", source4UIObjectsList);

            destinationUIObjectsList.Add(Destination);
            destinationUIObjectsList.Add(Destination_Browse);
            destinationUIObjectsList.Add(DestinationTotalSize);
            destinationUIObjectsList.Add(DestinationNumberOfFiles);
            destinationUIObjectsList.Add(DestinationDiskSpace);
            destinationUIObjectsList.Add(DestinationRectangle);

            overviewUIObjectsList.Add(SaveConfig);
            overviewUIObjectsList.Add(ServiceOverViewRectangle);
            overviewUIObjectsList.Add(BackupTime);
            overviewUIObjectsList.Add(Hours);
            overviewUIObjectsList.Add(Minutes);
            overviewUIObjectsList.Add(AMPM);
            overviewUIObjectsList.Add(BackupFrequency);
            overviewUIObjectsList.Add(Frequency);
            overviewUIObjectsList.Add(ServiceStatus);
            overviewUIObjectsList.Add(ServiceStatusDisplay);
            overviewUIObjectsList.Add(ServiceLastBackupTime);
        }
        private void RetrieveServiceStatus()
        {
            var bc = new BrushConverter();
            try
            {
                ServiceController sc = new ServiceController("SSBackup");
                switch (sc.Status)
                {
                    case ServiceControllerStatus.ContinuePending:
                        ServiceStatusDisplay.Fill = (Brush)bc.ConvertFrom("#FFF16161");
                        break;
                    case ServiceControllerStatus.Paused:
                        ServiceStatusDisplay.Fill = (Brush)bc.ConvertFrom("#FFF16161");
                        break;
                    case ServiceControllerStatus.PausePending:
                        ServiceStatusDisplay.Fill = (Brush)bc.ConvertFrom("#FFF16161");
                        break;
                    case ServiceControllerStatus.Running:
                        ServiceStatusDisplay.Fill = (Brush)bc.ConvertFrom("#FF88CD76");
                        break;
                    case ServiceControllerStatus.StartPending:
                        ServiceStatusDisplay.Fill = (Brush)bc.ConvertFrom("#FF88CD76");
                        break;
                    case ServiceControllerStatus.Stopped:
                        ServiceStatusDisplay.Fill = (Brush)bc.ConvertFrom("#FFF16161");
                        break;
                    case ServiceControllerStatus.StopPending:
                        ServiceStatusDisplay.Fill = (Brush)bc.ConvertFrom("#FFF16161");
                        break;
                }
            }
            catch (Exception Ex)
            {
                PromptUser("Unable to retrieve servcice status. Exception is:" + Ex.Message, Ex.StackTrace, Severity.Warning);
                return;
            }
        }
        private void GetLastBackupTime()
        {
            if (Source1.Text  == "Sources Folder")
            {
                return; 
            }
            try
            {
                DirectoryInfo directory = new DirectoryInfo(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\Service\Log");
                FileInfo myFile = (from f in directory.GetFiles() orderby f.LastWriteTime descending select f).First();
                IEnumerable<string> allLines = File.ReadLines(myFile.FullName).Where(s => s.Contains("Succes"));
                if (allLines == null) //try every log file
                {
                    IEnumerable<FileInfo> myFiles = (from f in directory.GetFiles() orderby f.LastWriteTime descending select f);
                    foreach (FileInfo file in myFiles)
                    {
                        IEnumerable<string> _allLines = File.ReadLines(myFile.FullName).Where(s => s.Contains("Success"));
                        if(_allLines != null)
                        {
                            allLines = _allLines; 
                            break; 
                        }
                    }
                    
                    if(allLines ==null)
                    {
                        ServiceLastBackupTime.Text = @"Last backup time not found";
                        return;
                    }

                }
                string lastBackupTimeMessage = allLines.LastOrDefault();
                ServiceLastBackupTime.Text = @"Last succesful backup was at: " + lastBackupTimeMessage.Substring(0, lastBackupTimeMessage.IndexOf("|"));
            }
            catch (Exception Ex)
            {
                PromptUser("Unable to determine last backup time, exception is:" + Ex.Message, Ex.StackTrace, Severity.Warning);
                return;
            }
        }
        private void SetScreen()
        {
            UIGrid.RowDefinitions[sourceNumber+2].Height = new GridLength(5, GridUnitType.Star);
            UIGrid.RowDefinitions[sourceNumber + 3].Height = new GridLength(0);
            this.Height = (865 * screenSize);           
            CenterWindowOnScreen();
        }
        private void CenterWindowOnScreen()
        {
            //Need to find which monitor we are on 
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight; 
            double windowWidth = this.Width;
            double windowHeight = this.Height; 
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }
        private void PromptUser(string Message, string StackTrace = null, Severity MsgLevel = Severity.Error)
        {
            myLogger.WriteToLogFile(Message, MsgLevel);
            if (StackTrace != null)
            {
                myLogger.WriteToLogFile(StackTrace, Severity.Error);
            }
            
            PromptUser promptUser = new PromptUser(Message + " See log file for more info.");
            promptUser.ShowDialog();
        }
        private void SetProgressBar(System.Windows.Controls.ProgressBar Bar, System.Windows.Controls.TextBox Box)
        {
            Box.Visibility = Visibility.Hidden; 
            Bar.IsIndeterminate = true;
            Bar.Visibility = Visibility.Visible;
        }
        private void RemoveProgressBar(System.Windows.Controls.ProgressBar Bar, System.Windows.Controls.TextBox Box)
        {
            Bar.IsIndeterminate = false;
            Bar.Visibility = Visibility.Hidden;
            Box.Visibility = Visibility.Visible; 
        }
        private void SetRed(System.Windows.Controls.Button btn)
        {
            var bc = new BrushConverter();

            btn.Background = (Brush)bc.ConvertFrom("#FFF16161");

            if (btn.Name == "SaveConfig")
            {
                isConfigSaved = false; 
            }
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetRed(SaveConfig);
        }
        private void SetGreen(System.Windows.Controls.Button btn)
        {
            var bc = new BrushConverter();

            btn.Background = (Brush)bc.ConvertFrom("#FF88CD76");

            if (btn.Name == "SaveConfig")
            {
                isConfigSaved = true;
            }
        }
        private void ChangeVisibility(List<Object> uiObjects, Visibility visibility)
        {
            foreach (object uiObject in uiObjects)
            {
                if (uiObject.GetType() == typeof(System.Windows.Controls.ProgressBar)) continue;
                switch (visibility)
                {
                    case Visibility.Visible:
                        ((UIElement)uiObject).Visibility = Visibility.Visible;
                        break;
                    case Visibility.Hidden:
                        ((UIElement)uiObject).Visibility = Visibility.Hidden;
                        break;
                }
            }
        }
        private void MoveNonSource()
        {
            foreach (object item in destinationUIObjectsList)
            {
                ((UIElement)item).SetValue(Grid.RowProperty, destinationPosition);                                
            }

            foreach (object item in overviewUIObjectsList)
            {
                ((UIElement)item).SetValue(Grid.RowProperty, overviewPosition);
            }
        }
        private void AdjustUINumbers(bool increase)
        {
            if(increase)
            {
                sourceNumber++;
                destinationPosition++;
                overviewPosition++; 
            }
            else
            {
                sourceNumber--;
                destinationPosition--;
                overviewPosition--; 
            }
        }
        private void DisplayRemoveSource()
        {
            switch (sourceNumber)
            {
                case (1):
                    Source1Remove.Visibility = Visibility.Hidden;
                    Source2Remove.Visibility = Visibility.Hidden;
                    break;
                case (2):
                    Source1Remove.Visibility = Visibility.Visible;
                    Source2Remove.Visibility = Visibility.Visible;
                    Source3Remove.Visibility = Visibility.Hidden;
                    break;
                case (3):
                    Source1Remove.Visibility = Visibility.Visible;
                    Source2Remove.Visibility = Visibility.Visible;
                    Source3Remove.Visibility = Visibility.Visible;
                    Source4Remove.Visibility = Visibility.Hidden;
                    break;
                case (4):
                    Source1Remove.Visibility = Visibility.Visible;
                    Source2Remove.Visibility = Visibility.Visible;
                    Source3Remove.Visibility = Visibility.Visible;
                    Source4Remove.Visibility = Visibility.Visible; 
                    break;
            }
        }
        private List<Object> GetSourceObjects(string searchString)
        {
            return (from entry in sourceUIObjects where (entry.Key == searchString) select entry.Value).FirstOrDefault();

        }
        private string GetValueAndUnitsFromDict(Dictionary<string, double> dictionary)
        {
            double value = dictionary.Min(t => t.Value);
            string unit = (from entry in dictionary where (entry.Value == value) select entry.Key).FirstOrDefault();
            return String.Format("{0:0.00}", value) + unit;
        }
        private void FillDestinationFields()
        {
            try
            {
                DestinationTotalSize.Text = @"Total Size: " + GetValueAndUnitsFromDict(myCopyer.CalculateSourceSize(Destination.Text));
                DestinationNumberOfFiles.Text = @"Number of Files: " + myCopyer.FindNumberOfFiles(Destination.Text).ToString();
                DestinationDiskSpace.Text = @"Remaining Disk Space is: " + GetValueAndUnitsFromDict(myCopyer.FindRemainingDiskSpaceOnDestination(Destination.Text));
            }
            catch
            {
                throw;
            }

        }
        private void AddSource_Click(object sender, RoutedEventArgs e)
        {
            if (sourceNumber == 4) return;         
            AdjustUINumbers(true); 
            if (sourceNumber == 4) AddSource.IsEnabled = false; 
            screenSize += 0.125;
            MoveNonSource();
            List<Object> _sourceUIObjects = GetSourceObjects(@"Source" + sourceNumber); 
            ChangeVisibility(_sourceUIObjects, Visibility.Visible); 
            SourceUIChange();
            SetScreen();
            DisplayRemoveSource(); 
        }
        private void SourceUIChange()
        {
            UIGrid.RowDefinitions[sourceNumber+1].Height = new GridLength(5, GridUnitType.Star);
            UIGrid.RowDefinitions[sourceNumber+2].Height = new GridLength(5, GridUnitType.Star);
            switch (sourceNumber+3)
            {
                case 4:
                    UIGrid.RowDefinitions[sourceNumber+2].Height = new GridLength(0);
                    UIGrid.RowDefinitions[sourceNumber+3].Height = new GridLength(0);
                    break;
                case 5:
                    UIGrid.RowDefinitions[sourceNumber + 3].Height = new GridLength(0);
                    break;
                default:
                    break;
            }
            if (sourceNumber == 1)
            {
                SourcesRectangle.Visibility = Visibility.Hidden; 
            }
            else
            {
                SourcesRectangle.Visibility = Visibility.Visible; 
                SourcesRectangle.Margin = new Thickness(1, 1, 1, 1);
                SourcesRectangle.SetValue(Grid.RowSpanProperty, sourceNumber); 
            }           
        }
        private void RemoveSource_Click(object sender, RoutedEventArgs e)
        {
            string specificBtn = (((System.Windows.Controls.Button)sender).Name).Substring(6, 1);
           List<Object> _sourceUIObjects = GetSourceObjects(@"Source" + sourceNumber.ToString());
            if(((System.Windows.Controls.TextBox)_sourceUIObjects[0]).Text != string.Empty)
            {
                Items.Remove(Items[int.Parse(specificBtn) - 1]);
            }           
            AdjustUINumbers(false);
            ReassignSource((Int16.Parse(specificBtn)));
            ChangeVisibility(_sourceUIObjects, Visibility.Hidden);
            foreach(object _sourceUIObject in _sourceUIObjects)
            {
                if (_sourceUIObject.GetType() == typeof(System.Windows.Controls.TextBox))
                {
                    ((System.Windows.Controls.TextBox)_sourceUIObject).Text = string.Empty; 
                }
            }            
            MoveNonSource(); 
            screenSize -= 0.125;
            if (sourceNumber < 4)
            {
                AddSource.IsEnabled = true; 
            }
            SourceUIChange();
            SetScreen();
            DisplayRemoveSource();
            SetRed(SaveConfig); 
        }
        private void Source_Browse_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker.ShowDialog();            
            int _sourceNumber = int.Parse((((System.Windows.Controls.Button)sender).Name).Substring(6, 1));
            List<Object> _sourceUIObjects = GetSourceObjects((((System.Windows.Controls.Button)sender).Name).Substring(0, 7)); 
            ((System.Windows.Controls.TextBox)_sourceUIObjects[0]).Text = FolderPicker.SelectedPath;
            if(Items.Count >= _sourceNumber && Items[_sourceNumber - 1] != null)
            {
                if(Items[_sourceNumber-1] == Destination.Text)
                {
                    Items.Add(Destination.Text); 
                }
                Items[_sourceNumber -1] = FolderPicker.SelectedPath; 
            }
            else
            {
                Items.Add(FolderPicker.SelectedPath); 
            }
        }
        private void Destination_Browse_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker.ShowDialog();
            Destination.Text = FolderPicker.SelectedPath;
            if (Items.Count > sourceNumber)
            {
                Items[sourceNumber] = FolderPicker.SelectedPath;
            }
            else
            {
                Items.Add(FolderPicker.SelectedPath); 
            }
        }
        private async void Source_Backup_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, double> BackupTimer;
            List<Object> _sourceUIObjects = GetSourceObjects((((System.Windows.Controls.Button)sender).Name).Substring(0, 7)); 
            SetProgressBar((System.Windows.Controls.ProgressBar)_sourceUIObjects[4], (System.Windows.Controls.TextBox)_sourceUIObjects[5]);          
            ((System.Windows.Controls.Button)sender).IsEnabled = false;
            try
            {
                BackupTimer = await AsyncBackup(((System.Windows.Controls.TextBox)_sourceUIObjects[0]).Text, Destination.Text);
                DestinationTotalSize.Text = @"Total Size: " + string.Format("{0:0.00}", myCopyer.CalculateSourceSize(Destination.Text));
                DestinationNumberOfFiles.Text = @"Number of Files: " + myCopyer.FindNumberOfFiles(Destination.Text).ToString();
                double BackupTimerValue = BackupTimer.Min(t => t.Value);
                string BackupTimerUnits = (from entry in BackupTimer where (entry.Value == BackupTimerValue) select entry.Key).FirstOrDefault();
                ((System.Windows.Controls.TextBox)_sourceUIObjects[5]).Visibility = Visibility.Visible; 
                ((System.Windows.Controls.TextBox)_sourceUIObjects[5]).Text = @"It took: " + String.Format("{0:0.00}", BackupTimerValue) + BackupTimerUnits;
            }
            catch (Exception Ex)
            {
                SetRed((System.Windows.Controls.Button)sender);
                RemoveProgressBar((System.Windows.Controls.ProgressBar)_sourceUIObjects[4], (System.Windows.Controls.TextBox)_sourceUIObjects[5]);
                ((System.Windows.Controls.Button)sender).IsEnabled = true;
                PromptUser(@"Backup Failed, exception is:" + Ex.Message, Ex.StackTrace); 
                return;
            }
                      
            RemoveProgressBar((System.Windows.Controls.ProgressBar)_sourceUIObjects[4], (System.Windows.Controls.TextBox)_sourceUIObjects[5]);
            SetGreen((System.Windows.Controls.Button)sender);
            try
            {
                FillDestinationFields(); 
            }
            catch (Exception Ex)
            {
                PromptUser(@"Backup Succeeded but unable to determine Destination properties" + Ex.Message, Ex.StackTrace);
            }

            ((System.Windows.Controls.Button)sender).IsEnabled = true;
        }
        private Task<Dictionary<string, double>> AsyncBackup(string Source, string Destination)
        {
            return Task<Dictionary<string, double>>.Run(() =>
            {
                return Backup(Source,Destination);
            });
        }
        private Dictionary<string,double> Backup(string Source, string Destination)
        {
            try
            {
                return myCopyer.Backup(Source, Destination);
            }
            catch (Exception Ex)
            {
                throw Ex; 
            }
            
        }
        private void Source_TextChanged(object sender, TextChangedEventArgs e)
        {
            if((((System.Windows.Controls.TextBox)sender).Text) == "Sources Folder" || (((System.Windows.Controls.TextBox)sender).Text) == "Destination Folder")
            {
                return; 
            }
            if ((((System.Windows.Controls.TextBox)sender).Name) =="Destination")
            {
                if ((((System.Windows.Controls.TextBox)sender).Text).Substring(0, 2) == @"\\")
                {
                    PromptUser(@"UNC\Network Path will work, but calculating destination properties is not supported",null, Severity.Warning);                   
                    DestinationTotalSize.Text = @"UNC\Network Path";
                    DestinationNumberOfFiles.Visibility = Visibility.Hidden;
                    DestinationDiskSpace.Visibility = Visibility.Hidden; 
                    return;
                }
                DestinationTotalSize.Visibility = Visibility.Visible;
                DestinationNumberOfFiles.Visibility = Visibility.Visible;
                try
                {
                    FillDestinationFields();
                }
                catch (Exception Ex)
                {
                    PromptUser("Unable to determine destination properties, Exception is:" + Ex.Message, Ex.StackTrace, Severity.Warning); 
                    return;
                }
                try
                {
                    foreach (KeyValuePair<string, List<Object>> entry in sourceUIObjects)
                    {
                        if(((System.Windows.Controls.TextBox)entry.Value[0]).Text != string.Empty)
                        {
                            string incrementalSize = GetValueAndUnitsFromDict(myCopyer.CalculateSizeDifference(((TextBox)entry.Value[0]).Text, Destination.Text));
                            ((System.Windows.Controls.TextBox)entry.Value[6]).Text = @"Required disk space for incremental backup: " + incrementalSize;
                            if (incrementalSize == "0.00Bytes")
                            {
                                SetGreen((System.Windows.Controls.Button)entry.Value[8]);
                            }
                            else
                            {
                                SetRed((System.Windows.Controls.Button)entry.Value[8]);
                            }
                        }
                    }

                }
                catch (Exception Ex)
                {
                    PromptUser("Unable to calculate required disk space for source. Exception is: " + Ex.Message, Ex.StackTrace, Severity.Warning);
                }

                return; 
            }
            List<Object>_sourceUIObjects = GetSourceObjects((((System.Windows.Controls.TextBox)sender).Name).Substring(0, 7));
            if (((System.Windows.Controls.TextBox)sender).Text == string.Empty)
            {
                ((System.Windows.Controls.TextBox)_sourceUIObjects[2]).Visibility = Visibility.Hidden;
                ((System.Windows.Controls.TextBox)_sourceUIObjects[3]).Visibility = Visibility.Hidden;
                ((System.Windows.Controls.TextBox)_sourceUIObjects[5]).Visibility = Visibility.Hidden;
                ((System.Windows.Controls.TextBox)_sourceUIObjects[6]).Visibility = Visibility.Hidden;
                return;
            }
          
            ((System.Windows.Controls.TextBox)_sourceUIObjects[2]).Visibility = Visibility.Visible;
            ((System.Windows.Controls.TextBox)_sourceUIObjects[3]).Visibility = Visibility.Visible;
            ((System.Windows.Controls.TextBox)_sourceUIObjects[5]).Visibility = Visibility.Visible;
            ((System.Windows.Controls.TextBox)_sourceUIObjects[6]).Visibility = Visibility.Visible;
            try
            {
                ((System.Windows.Controls.TextBox)_sourceUIObjects[2]).Text = @"Total Size: " + GetValueAndUnitsFromDict(myCopyer.CalculateSourceSize(((System.Windows.Controls.TextBox)sender).Text));
                ((System.Windows.Controls.TextBox)_sourceUIObjects[3]).Text = @"Number of Files: " + myCopyer.FindNumberOfFiles(((System.Windows.Controls.TextBox)sender).Text).ToString();
                if (Destination.Text != null || Destination.Text == string.Empty)
                {
                    string incrementalSize = GetValueAndUnitsFromDict(myCopyer.CalculateSizeDifference((((System.Windows.Controls.TextBox)sender).Text), Destination.Text));
                    ((System.Windows.Controls.TextBox)_sourceUIObjects[6]).Text = @"Required disk space for incremental backup: " + incrementalSize; 
                    if(incrementalSize == "0.00Bytes")
                    {
                        SetGreen((System.Windows.Controls.Button)_sourceUIObjects[8]); 
                    }
                    else
                    {
                        SetRed((System.Windows.Controls.Button)_sourceUIObjects[8]);
                    }
                }
                else
                {
                    ((System.Windows.Controls.TextBox)_sourceUIObjects[6]).Text = @"Required disk space for incremental backup: " + ((System.Windows.Controls.TextBox)_sourceUIObjects[2]).Text;
                }
            }
            catch(Exception Ex)
            {
                PromptUser("Unable to calculate size of source or required disk space, exception is:" + Ex.Message, Ex.StackTrace, Severity.Warning); 
            }
            SetRed(SaveConfig); 
        }
        private void ReassignSource(int SourceRemoved)
        {
            switch (SourceRemoved)
            {
                case 1:
                    switch (sourceNumber)
                    {
                        case 1:
                            Source1.Text = Source2.Text;
                            break;
                        case 2:
                            Source1.Text = Source2.Text;
                            Source2.Text = Source3.Text;
                            break;
                        case 3:
                            Source1.Text = Source2.Text;
                            Source2.Text = Source3.Text;
                            Source3.Text = Source4.Text; 
                           break;
                    }
                    break;

                case 2:
                    switch (sourceNumber)
                    {
                        case 1:
                            Source2.Text = string.Empty;
                            break;
                        case 2:
                            Source2.Text = Source3.Text;
                            break;
                        case 3:
                            Source2.Text = Source3.Text;
                            Source3.Text = Source4.Text;
                            break;
                    }
                    break;
                case 3:
                    switch (sourceNumber)
                    {
                        case 3:
                            Source3.Text = Source4.Text;
                            Source4.Text = string.Empty;
                            break;
                    }
                    break;
            }
        }
        private  void Save_Config_Click(object sender, RoutedEventArgs e)
        {
            if (Source1.Text == string.Empty || Source1.Text == null)
            {
                PromptUser("You need at least 1 source", null, Severity.Info);
                SetRed((System.Windows.Controls.Button)sender);
                return; 
            }

            if (Destination.Text == string.Empty || Destination.Text == null)
            {
                PromptUser("You need to specify a destination", null, Severity.Info);
                SetRed((System.Windows.Controls.Button)sender);
                return;
            }
            if (Hours.SelectedItem == null || Minutes.SelectedItem == null || AMPM.SelectedItem == null || Frequency.SelectedItem == null)
            {
                PromptUser("Please select a frequency for backups", null, Severity.Info); 
                SetRed(SaveConfig);
                return;
            }
            else if(Support.Support.CheckPath(Items[Items.Count -1], PathType.Directory))
            {                
                string backupInterval = Hours.Text + Minutes.Text + AMPM.Text + Frequency.Text;
                Items.Add(backupInterval); 
            }
            try
            {
                Support.Support.Serialize(Items.ToArray(), configFile);
                SetGreen((System.Windows.Controls.Button)sender); 
            }
            catch (Exception Ex )
            {
                PromptUser("Saving to config file failed, exception is:" + Ex.Message, Ex.StackTrace, Severity.Error); 
                SetRed((System.Windows.Controls.Button)sender);
                return;
            }
            try
            {
                ServiceController sc = new ServiceController("SSBackup");
                if(sc.Status == ServiceControllerStatus.StartPending)
                {
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                }
                if (sc.Status == ServiceControllerStatus.StopPending)
                {
                    sc.WaitForStatus(ServiceControllerStatus.Stopped); 
                }
                if(sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(2));
                    sc.Start(); 
                }
                else
                {
                    sc.Start(); 
                }

                RetrieveServiceStatus(); 
            }
            catch (Exception Ex)
            {
                PromptUser("There was an issue restarting the service, the exception is:" + Ex.Message, Ex.StackTrace, Severity.Error); 
            }           
        }
        private void SSBackup_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isConfigSaved)
            {
                PromptUser promptUser = new PromptUser("You haven't saved your changes! All changes will be lost. Select OK if you want to continue closing...");
                promptUser.ShowDialog(); 
                if (promptUser.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}

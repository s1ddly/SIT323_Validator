using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace App2
{
    public partial class Form1 : Form
    {
        public string workdir = "";
        public static List<string> loglines = new List<string>();
        public static List<string> Allocs = new List<string>();
        public static bool debug = false;
        public static List<string> TimeToAlloc = new List<string>();
        Config configcheck = new Config();
        Tan tancheck = new Tan();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void tabPage1_Focus(object sender, EventArgs e)
        {
            button2_Click(sender, e);
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        public void button1_Click_1(object sender, EventArgs e)
        {
            opentan();
        }

        public void opentan(Tan intan = null, Config incon = null)
        {
            if (intan == null || incon == null)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    //MessageBox.Show(ofd.FileName);
                    loglines.Add("Ingesting File: " + ofd.FileName);
                    string TanText = File.ReadAllText(ofd.FileName);
                    if (tancheck.InitTan(TanText, loglines))
                    {
                        try
                        {
                            label1.Text = ofd.FileName;
                            loglines.Add("Ingesting File: " + ofd.FileName.Replace(ofd.SafeFileName, "") + @tancheck.ConfigFile.Replace("\r", ""));
                            string ConfigText = File.ReadAllText(ofd.FileName.Replace(ofd.SafeFileName, "") + @tancheck.ConfigFile.Replace("\r", ""));
                            if (configcheck.InitConfig(ConfigText, loglines))
                            {

                                    label4.Text = ofd.FileName.Replace(ofd.SafeFileName, "") + @tancheck.ConfigFile.Replace("\r", "");
                                    label5.Text = ofd.FileName.Replace(ofd.SafeFileName, "") + configcheck.DefaultLogfile.Replace("\r", "");
                                    label7.Text = ofd.FileName.Replace(ofd.SafeFileName, "") + @"Allocation.all";
                                    //MessageBox.Show("The Tan and Config files have successfully been ingested! Check logs for more information");
                                    loglines.Add("The Tan and Config files have successfully been ingested!");

                            }
                            else
                            {
                                loglines.Add("Configuration file initialisation failed");
                                MessageBox.Show("Configuration file initialisation failed, Check logs for more information");
                            }
                        }
                        catch (Exception ex)
                        {
                            loglines.Add("Ingestion failed, Details: " + ex.ToString());
                        }
                    }
                    else
                    {
                        loglines.Add("Tan file initialisation failed");
                        MessageBox.Show("Tan file initialisation failed, Check logs for more information");
                    }
                }
            }
            else
            {
                tancheck = intan;
                configcheck = incon;
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string logtext = "";
            foreach(string f in loglines)
            {
                logtext = logtext + f + "\r\n";
            }
            textBox1.Text = logtext;
        }



        private void tabPage4_Autorefresh(object sender, EventArgs e)
        {
            button2_Click(sender, e);
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(Time(configcheck, tancheck, Allocs))
            {
                loglines.Add("Allocations completed successfuly!");
            }
            string Alloctext = "";
            foreach (string f in Allocs)
            {
                Alloctext = Alloctext + f + "\r\n";
            }
            textBox2.Text = Alloctext;
        }

        private void tabPage3_Autorefresh(object sender, EventArgs e)
        {
            string Alloctext = "";
            foreach (string f in Allocs)
            {
                Alloctext = Alloctext + f + "\r\n";
            }
            textBox2.Text = Alloctext;
        }

        private static string[] RegSplit(string instr)
        {
            try
            {
                return Regex.Split(instr, @",");
            } catch(Exception ex)
            {
                loglines.Add("Error with parsing: " + instr + ", " + ex.ToString());
                return null;
            }
        }


        public class Config
        {
            //This class acts as an interface with the config text
            //****************************************************************************************************************************
            //Sets all values to be used by external classes
            //****************************************************************************************************************************
            public string DefaultLogfile { get; set; }
            public int LimitsTasksMin { get; set; }
            public int LimitsTasksMax { get; set; }
            public int LimitsProcessorsMin { get; set; }
            public int LimitsProcessorsMax { get; set; }
            public int LimitsProcessorFrequenciesMin { get; set; }
            public int LimitsProcessorFrequenciesMax { get; set; }
            public int ProgramMaximumDuration { get; set; }
            public int ProgramTasks { get; set; }
            public int ProgramProcessors { get; set; }
            public double RuntimeReference { get; set; }
            public List<int> TaskRun { get; set; } = new List<int>();
            public List<double> ProcessorFreq { get; set; } = new List<double>();
            public List<int> QuadCoef { get; set; } = new List<int>();
            private int checkmin;
            private int checkmax;
            private string Config_Alltxt { get; set; }
            private string[] Config_Lines
            {
                get
                {
                    return Config_Alltxt.Split(new string[] { "\n" }, StringSplitOptions.None);
                }
            }

            //****************************************************************************************************************************
            //End of values inititalisation
            //****************************************************************************************************************************

            //Below is the initialisation and validation, takes the config as text from the file and validates it, then turns it into attributes for the config object.
            public bool InitConfig(string intext, List<string> loglines)
            {
                Config_Alltxt = intext;
                //below validates the input config file line by line for the values that must always exist
                foreach (string f in Config_Lines)
                {
                    //Below is the validation for comments
                    if (f.Contains("//"))
                    {
                        //loglines.Add(">");
                        string chk = f.Split(new string[] { "//" }, StringSplitOptions.None)[0];
                        foreach (char chk_char in chk.ToCharArray())
                        {
                            if (!chk_char.Equals((char)32) || chk_char.Equals(""))
                            {
                                loglines.Add("Errored out at: " + f + "\n    -> at Character: " + chk_char);
                                //return false;
                            }
                        }
                    }
                    //Below is the initialisation for Default-Logfile
                    if (f.Contains("DEFAULT-LOGFILE,"))
                    {
                        try
                        {
                            DefaultLogfile = RegSplit(f)[1].Replace("\"", "");
                            if (DefaultLogfile == "")
                            {
                                loglines.Add("Error with setting DEFAULT-LOGFILE, Please check configuration file: ");
                                return false;
                            }
                        }
                        catch (Exception e)
                        {
                            loglines.Add("Error with setting DEFAULT-LOGFILE on line: " + f + "\n  ->Error Details: " + e);
                            //return false;
                        }
                    }

                    //Below inits LIMITS-TASKS
                    if (f.Contains("LIMITS-TASKS,"))
                    {
                        try
                        {
                            LimitsTasksMin = Convert.ToInt32(RegSplit(f)[1]);
                            LimitsTasksMax = Convert.ToInt32(RegSplit(f)[2]);
                        }
                        catch (Exception e)
                        {
                            loglines.Add("Error with parsing LIMITS-TASKS min and max on line: " + f + "\n  ->Error Details: " + e);
                            //return false;
                        }
                    }

                    //Below inits LIMITS-PROCESSORS
                    if (f.Contains("LIMITS-PROCESSORS,"))
                    {
                        try
                        {
                            LimitsProcessorsMin = Convert.ToInt32(RegSplit(f)[1]);
                            LimitsProcessorsMax = Convert.ToInt32(RegSplit(f)[2]);
                        }
                        catch (Exception e)
                        {
                            loglines.Add("Error with parsing LIMITS-PROCESSORS min and max on line: " + f + "\n  ->Error Details: " + e);
                            //return false;
                        }
                    }

                    //Below inits LIMITS-PROCESSOR-FREQUENCIES
                    if (f.Contains("LIMITS-PROCESSOR-FREQUENCIES,"))
                    {
                        try
                        {
                            LimitsProcessorFrequenciesMin = Convert.ToInt32(RegSplit(f)[1]);
                            LimitsProcessorFrequenciesMax = Convert.ToInt32(RegSplit(f)[2]);
                        }
                        catch (Exception e)
                        {
                            loglines.Add("Error with parsing LIMITS-PROCESSOR-FREQUENCIES min and max on line: " + f + "\n  ->Error Details: " + e);
                            //return false;
                        }
                    }

                    //Below inits PROGRAM-MAXIMUM-DURATION
                    if (f.Contains("PROGRAM-MAXIMUM-DURATION,"))
                    {
                        try
                        {
                            ProgramMaximumDuration = Convert.ToInt32(RegSplit(f)[1]);
                        }
                        catch (Exception e)
                        {
                            loglines.Add("Error with parsing PROGRAM-MAXIMUM-DURATION on line: " + f + "\n  ->Error Details: " + e);
                            //return false;
                        }
                    }

                    //Below inits PROGRAM-TASKS
                    if (f.Contains("PROGRAM-TASKS,"))
                    {
                        try
                        {
                            ProgramTasks = Convert.ToInt32(RegSplit(f)[1]);
                        }
                        catch (Exception e)
                        {
                            loglines.Add("Error with parsing PROGRAM-TASKS on line: " + f + "\n  ->Error Details: " + e);
                            //return false;
                        }
                    }

                    //Below inits PROGRAM-PROCESSORS
                    if (f.Contains("PROGRAM-PROCESSORS,"))
                    {
                        try
                        {
                            ProgramProcessors = Convert.ToInt32(RegSplit(f)[1]);
                        }
                        catch (Exception e)
                        {
                            loglines.Add("Error with parsing PROGRAM-PROCESSORS on line: " + f + "\n  ->Error Details: " + e);
                            //return false;
                        }
                    }

                    //Below inits RUNTIME-REFERENCE-FREQUENCY
                    if (f.Contains("RUNTIME-REFERENCE-FREQUENCY,"))
                    {
                        try
                        {
                            RuntimeReference = Convert.ToDouble(RegSplit(f)[1]);
                        }
                        catch (Exception e)
                        {
                            loglines.Add("Error with parsing RUNTIME-REFERENCE-FREQUENCY on line: " + f + "\n  ->Error Details: " + e);
                            //return false;
                        }
                    }
                }

                //^end of line by line config checks, beginning multiline value initialisations
                //Get the runtimes for each Task ID
                try
                {
                    checkmax = 0;
                    checkmin = 0;
                    for (int x = 0; x < Config_Lines.Length; x++)
                    {
                        if (Config_Lines[x].Contains("TASK-ID,RUNTIME"))
                        {
                            checkmin = x;
                        }
                        if (checkmin != 0 && Config_Lines[x].Length == 1)
                        {
                            if (Convert.ToChar(Config_Lines[x]) == 13)
                            {
                                checkmax = x - 1;
                                break;
                            }
                        }
                    }
                    if (checkmin == -1 || checkmin == 0 || checkmax == 0)
                    {
                        loglines.Add("Error with parsing TASK-ID and RUNTIME values");
                        //return false;
                    }
                    for (int x = checkmin + 1; x <= checkmax; x++)
                    {
                        TaskRun.Add(Convert.ToInt32(Config_Lines[x].Split(',')[1]));
                    }
                }
                catch (Exception e)
                {
                    loglines.Add("Error with parsing TASK-ID and RUNTIME values \n  ->Error Details: " + e);
                    //return false;
                }

                //Get the Frequencies for each Processor
                try
                {
                    checkmax = 0;
                    checkmin = 0;
                    for (int x = 0; x < Config_Lines.Length; x++)
                    {
                        if (Config_Lines[x].Contains("PROCESSOR-ID,FREQUENCY"))
                        {
                            checkmin = x;
                        }
                        if (checkmin != 0 && Config_Lines[x].Length == 1)
                        {
                            if (Convert.ToChar(Config_Lines[x]) == 13)
                            {
                                checkmax = x - 1;
                                break;
                            }
                        }
                    }
                    if (checkmin == -1 || checkmin == 0 || checkmax == 0)
                    {
                        loglines.Add("Error with parsing PROCESSOR-ID and FREQUENCY values");
                        //return false;
                    }
                    for (int x = checkmin + 1; x <= checkmax; x++)
                    {
                        ProcessorFreq.Add(Convert.ToDouble(Config_Lines[x].Split(',')[1]));
                    }
                }
                catch (Exception e)
                {
                    loglines.Add("Error with parsing PROCESSOR-ID and FREQUENCY values \n  ->Error Details: " + e);
                    //return false;
                }

                //Gets the quadratic coefficients 
                try
                {
                    checkmax = 0;
                    checkmin = 0;
                    for (int x = 0; x < Config_Lines.Length; x++)
                    {
                        if (Config_Lines[x].Contains("COEFFICIENT-ID,VALUE"))
                        {
                            checkmin = x;
                        }
                        if (checkmin != 0 && !Config_Lines[x].Contains(","))
                        {
                            checkmax = x - 1;
                            break;
                        }
                    }

                    if (checkmin == -1 || checkmin == 0 || checkmax == 0)
                    {
                        loglines.Add("Error with parsing COEFFICIENT-ID and VALUE values");
                        //return false;
                    }
                    for (int x = checkmin + 1; x <= checkmax; x++)
                    {
                        QuadCoef.Add(Convert.ToInt32(Config_Lines[x].Split(',')[1]));
                    }
                }
                catch (Exception e)
                {
                    loglines.Add("Error with parsing COEFFICIENT-ID and VALUE values \n  ->Error Details: " + e);
                    //return false;
                }

                //Checks to see that all required values have been initialised
                if (DefaultLogfile == null || LimitsTasksMax == 0 || LimitsProcessorsMax == 0 || LimitsProcessorFrequenciesMax == 0 || ProgramMaximumDuration == 0 || ProgramTasks == 0 || ProgramProcessors == 0 || RuntimeReference == 0)
                {
                    loglines.Add("Error with setting the Variables, one of more of the variables are empty.");
                    return false;
                }

                //Checks that all >0 components are >0
                if (LimitsTasksMin < 0 || LimitsTasksMax < 0 || LimitsProcessorsMin < 0 || LimitsProcessorsMax < 0 || LimitsProcessorFrequenciesMin < 0 || LimitsProcessorFrequenciesMax < 0 || ProgramMaximumDuration < 0 || ProgramTasks < 0 || ProgramProcessors < 0 || RuntimeReference < 0)
                {
                    loglines.Add("Error with setting the Variables, one of more of the variables is negative.");
                    return false;
                }

                //if it passes all the above validations it will continue to do the below
                return true;
            }

            //****************************************************************************************************************************
            //End of InitConfig
            //****************************************************************************************************************************

        }

        public class Tan
        {
            public string ConfigFile { get; set; }
            public int Tasks { get; set; }
            public int Processors { get; set; }
            public int Allocations { get; set; }
            public List<string> AllocationList { get; set; } = new List<string>();
            public string[] Tan_Lines
            {
                get
                {
                    return Tan_Text.Split(new string[] { "\n" }, StringSplitOptions.None);
                }
            }

            public string Tan_Text { get; set; }

            public bool InitTan(string intan, List<string> loglines)
            {
                //This class serves as an interface for the program to visualise 

                Tan_Text = intan;
                foreach (string f in Tan_Lines)
                {
                    //Below is the validation for comments
                    if (f.Contains("//"))
                    {
                        //loglines.Add(">");
                        string chk = f.Split(new string[] { "//" }, StringSplitOptions.None)[0];
                        foreach (char chk_char in chk.ToCharArray())
                        {
                            if (!chk_char.Equals((char)32) || chk_char.Equals(""))
                            {
                                loglines.Add("Errored out at: " + f + "\n    -> at Character: " + chk_char);
                                //return false;
                            }
                        }
                    }
                    //Below is the initialisation for Config-File
                    if (f.Contains("CONFIG-FILE,"))
                    {
                        try
                        {
                            ConfigFile = RegSplit(f)[1].Replace("\"", "");
                            if (ConfigFile == "")
                            {
                                loglines.Add("Error with setting CONFIG-FILE, Please check tan file");
                                //return false;
                            }
                        }
                        catch (Exception e)
                        {
                            loglines.Add("Error with setting CONFIG-FILE on line: " + f + "\n  ->Error Details: " + e);
                            //return false;
                        }
                    }

                    //Below inits LIMITS-TASKS
                    if (f.Contains("TASKS,"))
                    {
                        try
                        {
                            Tasks = Convert.ToInt32(RegSplit(f)[1]);
                        }
                        catch (Exception e)
                        {
                            loglines.Add("Error with parsing LIMITS-TASKS min and max on line: " + f + "\n  ->Error Details: " + e);
                            //return false;
                        }
                    }

                    //Below inits Processors
                    if (f.Contains("PROCESSORS,"))
                    {
                        try
                        {
                            Allocations = Convert.ToInt32(RegSplit(f)[1]);
                        }
                        catch (Exception e)
                        {
                            loglines.Add("Error with parsing ALLOCATIONS on line: " + f + "\n  ->Error Details: " + e);
                            //return false;
                        }
                    }

                    //Below inits ALLOCATIONS
                    if (f.Contains("ALLOCATIONS,"))
                    {
                        try
                        {
                            Allocations = Convert.ToInt32(RegSplit(f)[1]);
                        }
                        catch (Exception e)
                        {
                            loglines.Add("Error with parsing ALLOCATIONS on line: " + f + "\n  ->Error Details: " + e);
                            //return false;
                        }
                    }

                }

                //Initiliases the actual allocations as a list
                /*
                 I'm gonna be honest with you, this piece of code is really confusing and was not written with the intent to be supported or worked on after this assignment.
                 I'll give it my best go at explaining it though.
                 the first part adds the line numbers of the beginning and end of each allocation section to 2 arrays, begin and end respectively.

                 */
                try
                {
                    int[] begin = new int[Allocations];
                    int[] end = new int[Allocations];
                    int count = 0;
                    bool switcher = false;
                    for (int i = 0; i < Tan_Lines.Length; i++)
                    {
                        if (Tan_Lines[i].Contains("ALLOCATION-ID,"))
                        {
                            begin[count] = i + 1;
                            switcher = true;
                        }
                        if (switcher == true)
                        {
                            if (!Tan_Lines[i].Contains(","))
                            {
                                end[count] = i;
                                count++;
                                switcher = false;
                            }
                        }
                    }

                    /*
                    This below part is the confusing bit,
                    Basically there are 3 nested loops for each allocation.
                    Loop, lets call it i, is used to reference the beginning and end arrays to find the lines that have allocations in them
                    Loop x parses every line in each block of allocations
                    Loop y parses every character in each line.
                    The if statement in y basically concatenates the 3 lines from the format:
                        1: Yes, No,Yes,No ,No 
                        2: No ,Yes, No, No,Yes
                        3: No , No,No ,Yes,No
                    To the format:
                        1,2,1,3,2
                    Which makes a bit more sense and is easier to work with 
                    */

                    for (int i = 0; i < Allocations; i++)
                    {
                        int[] tempalloc = new int[Tasks];
                        int[] tempproc = new int[Tasks];
                        for (int x = begin[i]; x < end[i]; x++)
                        {
                            int procid = x - begin[i] + 1;
                            //loglines.Add(procid);
                            for (int y = 0; y < Tasks; y++)
                            {
                                if (Tan_Lines[x].Split(',')[y].Contains("1"))
                                {
                                    if (tempproc[y] == 0)
                                    {
                                        tempproc[y] = procid;
                                    }
                                    else
                                    {
                                        loglines.Add("The same task has beeen assigned to multiple processors, please check the lines: " + Convert.ToString(begin[i] + 1) + "-" + Convert.ToString(end[i]));
                                        //return false;
                                    }
                                }

                            }

                        }
                        string allocationin = "";
                        foreach (int p in tempproc)
                        {
                            allocationin += Convert.ToString(p);
                        }
                        AllocationList.Add(allocationin);
                    }
                }
                catch (Exception e)
                {
                    loglines.Add("Error with parsing the allocations \n  ->Error Details: " + e);
                    //return false;
                }


                return true;
            }
        }

        private bool Time(Config incon, Tan intan, List<string> Allocs)
        {
            try
            {
                if (intan.AllocationList.Count > 0)
                {
                    Allocs.Add("The timing for each allocation and each individual task are printed below:");
                    foreach (string i in intan.AllocationList)
                    {
                        double timetotal = 0;
                        double consumptiontotal = 0;
                        string timeperalloc = "";
                        if (i.Contains("0"))
                        {
                            loglines.Add("The allocation: " + i + " has an invalid task ");
                            return false;
                        }
                        Allocs.Add("Allocation: " + i);
                        Allocs.Add("Processor \t Task \t Time \t Energy Consumption");
                        for (int x = 0; x < i.Length; x++)
                        {
                            try
                            {
                                if (debug)
                                {
                                    Allocs.Add(Convert.ToString(incon.ProcessorFreq[Convert.ToInt32(Convert.ToString(i[x])) - 1]));//incon.ProcessorFreq[x]
                                    Allocs.Add(Convert.ToString(incon.RuntimeReference));
                                    Allocs.Add(Convert.ToString(incon.TaskRun[x]));
                                }
                                double time = Math.Round((incon.TaskRun[x] * incon.RuntimeReference / incon.ProcessorFreq[Convert.ToInt32(Convert.ToString(i[x])) - 1]), 2);
                                if (time > Convert.ToDouble(configcheck.ProgramMaximumDuration))
                                {
                                    Allocs.Add("Error, the allocation: " + i + ", is invalid because execution time: " + time + ", is over the program maximum duration: " + configcheck.ProgramMaximumDuration);
                                    loglines.Add("Error, the allocation: " + i + ", is invalid because execution time: " + time + ", is over the program maximum duration: " + configcheck.ProgramMaximumDuration);
                                    return false;
                                }
                                Allocs.Add(i[x] + " \t " + Convert.ToString(x + 1) + " \t " + Convert.ToString(time) + " \t " + Convert.ToString(Consumption(time, incon.ProcessorFreq[Convert.ToInt32(Convert.ToString(i[x])) - 1], incon.QuadCoef)));
                                timetotal += time;
                                consumptiontotal += Consumption(time, incon.ProcessorFreq[Convert.ToInt32(Convert.ToString(i[x])) - 1], incon.QuadCoef);
                                timeperalloc += Convert.ToString(time) + ",";
                            }
                            catch (Exception e)
                            {
                                loglines.Add("Error in parsing allocation: " + i + ", on Allocation: " + Convert.ToString(i) + "\n");
                                loglines.Add(e.ToString());
                                return false;
                            }

                        }
                        Allocs.Add("Total time: " + Convert.ToString(timetotal) + "; Total Energy Consumption: " + Convert.ToString(consumptiontotal));
                        TimeToAlloc.Add(timeperalloc);
                    }
                    if (debug)
                    {
                        foreach (string f in TimeToAlloc)
                        {
                            Console.WriteLine(f);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select a tan file before trying to display allocations");
                }
            }
            catch (Exception ex)
            {
                loglines.Add("Error in calculating allocations, details: " + ex.ToString());
                return false;
            }
            
            return true;
        }

        private double Consumption(double time, double freq, List<int> coefs)
        {
            double output = (Convert.ToDouble(coefs[2]) * freq * freq) + (Convert.ToDouble(coefs[1]) * freq) + Convert.ToDouble(coefs[0]);
            try
            {
                return output;
            }
            catch (Exception e)
            {
                loglines.Add("Error in calculating consumption, details: " + e);
            }
            return 0.0;
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            //save logs
            try
            {
                File.WriteAllLines(label5.Text, textBox1.Lines);
            } catch(Exception ex)
            {
                MessageBox.Show("Error in saving Logs: " + ex.ToString());
                loglines.Add("Error in saving Logs: " + ex.ToString());
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //save Allocations
            try
            {
                File.WriteAllLines(label7.Text, textBox2.Lines);
            }
            catch (Exception ex)
            {
                loglines.Add("Error in saving Allocations: " + ex.ToString());
            }
        }
    }
}

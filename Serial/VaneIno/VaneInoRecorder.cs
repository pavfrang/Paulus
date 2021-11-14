using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paulus.Serial.VaneIno
{
    public class VaneInoRecorder : Recorder
    {
        public VaneInoRecorder(string basePath, int timeStepInMs,
           VaneInoCommander agent) :
           base("VaneIno", basePath, timeStepInMs)
        {
            this.agent = agent;
        }

        VaneInoCommander agent { get; }

        public override Variable[] Variables
        {
            get
            {
                return new Variable[]
                    {
                        new Variable("Analog"),
                        new Variable("Vane 1"),
                        new Variable("Vane 2"),
                        new Variable("Vane 3"),
                        new Variable("Vane 4"),
                        new Variable("Automation Status"),
                        new Variable("Vane Step")
                    };
            }
        }

        protected override object[] Values
        {
            get
            {
                return new object[]
                {
                    agent.VaneIno.Analog.ActualValue,
                    agent.VaneIno.Vane(1).ActualValue,
                    agent.VaneIno.Vane(2).ActualValue,
                    agent.VaneIno.Vane(3).ActualValue,
                    agent.VaneIno.Vane(4).ActualValue,
                    agent.CurrentAutomationStep,
                    agent.CurrentAutomationStep ==AutomationStep.Started ? agent.CurrentVaneStep+1 : 0
                };
            }
        }

        double f = (double)Stopwatch.Frequency;

        protected override void writeFirstLine()
        {
            using (StreamWriter writer = new StreamWriter(currentFile))
            {
                writer.Write("Time\tAbsolute_Time\t" + string.Join("\t", from v in Variables select v.Name) + "\t");
                writer.WriteLine("$iStartTime\t");

                writer.Write("s\t-\t" + string.Join("\t", from v in Variables select v.Unit) + "\t");
                writer.WriteLine("\t"); //for the recording time

                //DateTime currentTime = DateTime.Now;
                //double time = (currentTime - startTime).TotalSeconds; //; (iRecordStep++) * _timeStep / 1000.0f;
                //writer.Write($"{time:0.000}\t{currentTime:HH:mm:ss.fff}\t");

                //write second line
                //long currentTime = Stopwatch.GetTimestamp();
                //double time2 = (currentTime - startTime2) / f ;

                //writer.Write($"{time2:0.000}\t{startTime.AddSeconds(time2):HH:mm:ss.fff}\t");
                //var values = new object[] {
                //agent.VaneIno.Analog2.ActualValue,
                //    agent.VaneIno.Vane(1).ActualValue,
                //    agent.VaneIno.Vane(2).ActualValue,
                //    agent.VaneIno.Vane(3).ActualValue,
                //    agent.VaneIno.Vane(4).ActualValue,
                //    agent.CurrentAutomationStep,
                //    agent.CurrentAutomationStep == AutomationStep.Started ? agent.CurrentVaneStep + 1 : 0
                //};
                //writer.Write(string.Join("\t", values) + "\t");

                //writer.WriteLine($"{GetRecordingTime()}\t");
                writer.Flush();
            }
            firstTime = true;
        }
        bool firstTime = false;

        protected override void TmrRecorder_Tick(object sender, TimeEventArgs e)
        {
            //base.TmrRecorder_Tick(sender, e);

            //in fact we write 2 lines now!
            using (StreamWriter writer = new StreamWriter(currentFile, true))
            {

                //DateTime currentTime = DateTime.Now;
                //double time = (currentTime - startTime).TotalSeconds; //; (iRecordStep++) * _timeStep / 1000.0f;
                //writer.Write($"{time:0.000}\t{currentTime:HH:mm:ss.fff}\t");
                //long currentTime = Stopwatch.GetTimestamp();
                double time1 = e.TimeSpan.TotalSeconds; //(currentTime - startTime2) /f;
                double time2 = time1 + agent.VaneIno.TimeBetween.ActualValue * 1e-6;

                writer.Write($"{time1:0.000}\t{startTime.AddSeconds(time1):HH:mm:ss.fff}\t");
                var values = new object[] {
                agent.VaneIno.Analog1.ActualValue,
                    agent.VaneIno.Vane(1).ActualValue,
                    agent.VaneIno.Vane(2).ActualValue,
                    agent.VaneIno.Vane(3).ActualValue,
                    agent.VaneIno.Vane(4).ActualValue,
                    agent.CurrentAutomationStep,
                    agent.CurrentAutomationStep == AutomationStep.Started ? agent.CurrentVaneStep + 1 : 0
                };
                if(!firstTime)
                writer.WriteLine(string.Join("\t", values) + "\t\t");
                else
                {
                    writer.WriteLine(string.Join("\t", values) + $"\t{GetRecordingTime()}\t");
                    firstTime = false;
                }



                //write second line
                writer.Write($"{time2:0.000}\t{startTime.AddSeconds(time2):HH:mm:ss.fff}\t");
                values = new object[] {
                agent.VaneIno.Analog2.ActualValue,
                    agent.VaneIno.Vane(1).ActualValue,
                    agent.VaneIno.Vane(2).ActualValue,
                    agent.VaneIno.Vane(3).ActualValue,
                    agent.VaneIno.Vane(4).ActualValue,
                    agent.CurrentAutomationStep,
                    agent.CurrentAutomationStep == AutomationStep.Started ? agent.CurrentVaneStep + 1 : 0
                };
                writer.WriteLine(string.Join("\t", values) + "\t\t");

                writer.Flush();
            }
        }


    }
}

using System;
using System.Threading;
using System.Windows;
using System.Timers;
using System.Configuration;
using System.Xml.Linq;

using YarpManagerCS;
using System.Diagnostics;

namespace FACERobotControl
{
    /// <summary>
    /// Interaction logic for YarpMonitor.xaml
    /// </summary>
    public partial class YarpMonitor : Window
    {
        private YarpPort port;

        private EventWaitHandle _wh = new AutoResetEvent(false);
        private System.Threading.Thread _worker;
        private int counter = 0;
        string received;
        string portNameReader;
        System.Timers.Timer t=null;

        public YarpMonitor(string portName)
        {
            var dllDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/lib";
            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + dllDirectory);

            InitializeComponent();

            t = new System.Timers.Timer();
            t.Interval = 300;
            t.Elapsed += new ElapsedEventHandler(rc);
           // t.Start();

            _worker = new System.Threading.Thread(Work);
            _worker.Start();

            portNameReader = portName;

            InitYarp(portName);

          
        }

        private void InitYarp(string portName)
        {
            port = new YarpPort();
            port.openReceiver(portName, "/YarpReceiver");

        }

     

        private void Work()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();


            while (true)
            {
                //string data = "";
                //port.receivedData(out data);

                //if (data != null && data != "")
                //{
                //    string s = data.Substring(0, data.Length - 2);
              
                //    TimeSpan ts = stopWatch.Elapsed;
                //    string elapsedTime = string.Format("{0:fff} ms", stopWatch.Elapsed);
                //    stopWatch.Restart();

                //    counter++;
                //    textBox1.Dispatcher.Invoke(
                //        System.Windows.Threading.DispatcherPriority.Background,
                //        new Action(() => textBox1.Text = textBox1.Text + "[" + counter + "] " + "[" + elapsedTime + "] " + data)
                //    );
                //    stopWatch.Restart();

                //}
                //else
                //{
                //    // No more tasks - wait for a signal
                //    // _wh.WaitOne();
                //}




                string data = "";
                port.receivedData(out data);

                if (data != null && data != "")
                {


                    string xml = data;
                    xml = xml.Replace(@"\", "");
                    xml = xml.Substring(1, xml.Length - 2);
                    //xml = xml.Replace(@"?", "");
                    if (xml.Substring(0, 1) == "?")
                        xml = xml.Remove(0, 1);

                    if (xml.Substring(0, 5) == "<?xml")
                        data = XElement.Parse(xml).ToString();


                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    // string elapsedTime = String.Format("{0:00}.{1:00}", ts.Seconds, ts.Milliseconds / 10);
                    string elapsedTime = string.Format("{0:fff} ms", stopWatch.Elapsed);
                    counter++;
                    //sceneData = ComUtils.XmlUtils.Deserialize<Scene>(data);
                    textBox1.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Background,
                        new Action(() => textBox1.Text = "[" + counter + "] " + "[" + elapsedTime + "] \n\r" + data)
                    );
                    stopWatch.Restart();

                }
            }


        }


        private void rc(object sender, ElapsedEventArgs e)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();


            string data = "";
            port.receivedData(out data);

            if (data != null && data != "")
            {


                string xml = data;
                xml = xml.Replace(@"\", "");
                xml = xml.Substring(1, xml.Length - 2);
                //xml = xml.Replace(@"?", "");
                if (xml.Substring(0, 1) == "?")
                    xml = xml.Remove(0, 1);

                if (xml.Substring(0, 5) == "<?xml") 
                    data = XElement.Parse(xml).ToString();


                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                // string elapsedTime = String.Format("{0:00}.{1:00}", ts.Seconds, ts.Milliseconds / 10);
                string elapsedTime = string.Format("{0:ffff} ms", stopWatch.Elapsed);
                counter++;
                //sceneData = ComUtils.XmlUtils.Deserialize<Scene>(data);
                textBox1.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Background,
                    new Action(() => textBox1.Text = "[" + counter + "] " + "[" + elapsedTime + "] \n\r" +data)
                );
                stopWatch.Restart();

            }

        }

        public void EnqueueTask(string task)
        {
            _wh.Set();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            textBox1.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => textBox1.Text = "")
            );
        }

        private void Window_Closed(object sender, EventArgs e)
        {

            if (t != null)
                t.Stop();

            if (_worker != null)
                _worker.Abort();

            port.Disconect("/YarpReceiver", portNameReader);
           
        }
    }
}

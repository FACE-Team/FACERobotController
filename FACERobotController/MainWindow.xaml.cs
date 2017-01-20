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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Xml;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Threading;
using System.Timers;

using FACEBodyControl;
using FACELibrary;
using YarpManagerCS;

namespace FACERobotController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private YarpPort yarpPortLookAt;
        private YarpPort yarpPortSetECS;
        private YarpPort yarpPortSetFacialExpression;
        private YarpPort yarpPortReflexes;

        private YarpPort yarpPortSetFacialMotors;

        private YarpPort yarpPortFeedbackXML;
        private YarpPort yarpPortFeedbackJSON;
        private YarpPort yarpPortFeedbackBOTTLE;
        private YarpPort yarpPortFeedback;


        private string colYarp;// Ellipse color Yarp
        private string colLookAt;// Ellipse color Yarp Attention Module
        private string colECS;// Ellipse color Yarp Expression
        private string colExp;
        private string colRef;

        private string colMot;



        private string lookAt_out = ConfigurationManager.AppSettings["YarpPortLookAt_OUT"].ToString();
        private string lookAt_in = ConfigurationManager.AppSettings["YarpPortLookAt_IN"].ToString();

        private string ecs_in = ConfigurationManager.AppSettings["YarpPortECS_IN"].ToString();
        private string ecs_out = ConfigurationManager.AppSettings["YarpPortECS_OUT"].ToString();

        private string facialexpression_in = ConfigurationManager.AppSettings["YarpPortFacialExpression_IN"].ToString();
        private string facialexpression_out = ConfigurationManager.AppSettings["YarpPortFacialExpression_OUT"].ToString();

        private string reflexes_in = ConfigurationManager.AppSettings["YarpPortReflexes_IN"].ToString();
        private string reflexes_out = ConfigurationManager.AppSettings["YarpPortReflexes_OUT"].ToString();

        /*--------------------------------------------------------------------------------------------------------------------*/
        private string feedbackXML_out = ConfigurationManager.AppSettings["YarpPortFeedbackXML_OUT"].ToString();
        private string feedbackJSON_out = ConfigurationManager.AppSettings["YarpPortFeedbackJSON_OUT"].ToString();
        private string feedbackBOTTLE_out = ConfigurationManager.AppSettings["YarpPortFeedbackBOTTLE_OUT"].ToString();
        private string feedback_in = ConfigurationManager.AppSettings["YarpPortFeedback_IN"].ToString();
        private string feedback_out = ConfigurationManager.AppSettings["YarpPortFeedback_OUT"].ToString();


        private string setFacialMotors_out = ConfigurationManager.AppSettings["YarpPortSetFacialMotors_OUT"].ToString();
        private string setFacialMotors_in = ConfigurationManager.AppSettings["YarpPortSetFacialMotors_IN"].ToString();

    


        string receivedLookAtData = "";
        string receivedECSData = "";
        string receivedFacialExpression = "";
        string receivedReflexes = "";
        string receivedFeedback = "";


        private System.Timers.Timer TimerCheckStatusYarp;

        private System.Threading.Thread senderThreadFeedBack = null;
        private System.Threading.Thread receiverThreadFeedBack = null;
        private System.Threading.Thread receiverThreadLookAt = null;
        private System.Threading.Thread receiveThreadECS = null;
        private System.Threading.Thread receiveThreadFacialExpression = null;

        


        private List<ServoMotor> currentSmState;
        private ECS ecs;

        FACEMotion motionLookAt;
        FACEMotion motionECS;
        FaceExpression exp;
        List<ServoMotor> listMotorsFacialExpression;
        List<ServoMotor> listMotorsFeedback;
        List<float> ListFeedbackOnlyPositions;


        float old_PosX = 0;
        float old_PosY = 0;

        public enum reflexes { None, Yes, No, OpenEyes, CloseEyes };


        private YarpMonitor monitor = null;
        private Winner SubjectWin = new Winner();

        public MainWindow()
        {
            

            InitializeComponent();

            Init();
            InitYarp();

        }


        private void Init()
        {
            ecs = ECS.LoadFromXmlFormat("ECS.xml");
            foreach (ECSMotor ecsM in ecs.ECSMotorList)
            {
                ecsM.FillMap();
            }

            FACEBody.LoadConfigFile("Config.xml");
            currentSmState = FACEBody.CurrentMotorState;

            listMotorsFeedback = new List<ServoMotor>();
            listMotorsFacialExpression = new List<ServoMotor>();
            motionLookAt = new FACEMotion(FACEBody.CurrentMotorState.Count);
            motionECS = new FACEMotion(FACEBody.CurrentMotorState.Count);
            ListFeedbackOnlyPositions = new List<float>();
        }
        /// <summary>
        /// Initialize Timer for Test Yarp
        /// </summary>
        /// <param name="panel"></param>
        private void InitYarp()
        {
            yarpPortLookAt = new YarpPort();
            yarpPortLookAt.openReceiver(lookAt_out, lookAt_in);

            yarpPortSetECS = new YarpPort();
            yarpPortSetECS.openReceiver(ecs_out, ecs_in);

            yarpPortSetFacialExpression = new YarpPort();
            yarpPortSetFacialExpression.openReceiver(facialexpression_out, facialexpression_in);

            yarpPortReflexes = new YarpPort();
            yarpPortReflexes.openReceiver(reflexes_out, reflexes_in);

            yarpPortFeedbackXML = new YarpPort();
            yarpPortFeedbackXML.openSender(feedbackXML_out);

            yarpPortFeedbackJSON = new YarpPort();
            yarpPortFeedbackJSON.openSender(feedbackJSON_out);

            yarpPortFeedbackBOTTLE = new YarpPort();
            yarpPortFeedbackBOTTLE.openSender(feedbackBOTTLE_out);

            yarpPortFeedback = new YarpPort();
            yarpPortFeedback.openReceiver(feedback_out, feedback_in);

            yarpPortSetFacialMotors = new YarpPort();
            yarpPortSetFacialMotors.openSender(setFacialMotors_out);


            // controllo se la connessione con le porte sono attive(unico metodo funzionante)
            colYarp = "red";

            colLookAt = "red";
            colECS = "red";
            colExp = "red";
            colRef = "red";

            colMot = "red";

            lblLookAt.Content=lookAt_in;
            lblExp.Content=ecs_in;
            lblSetFace.Content = facialexpression_in;
            lblRef.Content = reflexes_in;

            lblFeedXML.Content = feedbackXML_out;
            lblFeedJson.Content = feedbackJSON_out;
            lblFeedBottle.Content = feedbackBOTTLE_out;
            lblMot.Content = setFacialMotors_out;


            TimerCheckStatusYarp = new System.Timers.Timer();
            TimerCheckStatusYarp.Elapsed += new ElapsedEventHandler(CheckStatusYarp);
            TimerCheckStatusYarp.Interval = (1000) * (5);
            TimerCheckStatusYarp.Enabled = true;
            TimerCheckStatusYarp.Start();

            CheckStatusYarp(null , null);


            senderThreadFeedBack = new System.Threading.Thread(SendFeedBack);
            //senderThreadFeedBack.Start();

            receiverThreadFeedBack = new System.Threading.Thread(ReceiveDataFeedback);
            receiverThreadFeedBack.Start();

            receiverThreadLookAt = new System.Threading.Thread(ReceiverDataLookAt);
            receiverThreadLookAt.Start();

            receiveThreadECS = new System.Threading.Thread(ReceiveDataECS);
            receiveThreadECS.Start();

            receiveThreadFacialExpression = new System.Threading.Thread(ReceiveDataFacialExpression);
            receiveThreadFacialExpression.Start();


           //ThreadPool.QueueUserWorkItem(ReceiveDataFeedback);
           // ThreadPool.QueueUserWorkItem(ReceiverDataLookAt);
           // ThreadPool.QueueUserWorkItem(ReceiveDataECS);
           // ThreadPool.QueueUserWorkItem(ReceiveDataFacialExpression);



        }

     
//         void ReceiverDataLookAt(object sender, ElapsedEventArgs e)

        void ReceiverDataLookAt(object sender)
        {
            while (true)
            {
                yarpPortLookAt.receivedData(out receivedLookAtData);

                if (receivedLookAtData != null && receivedLookAtData != "")
                {

                    try
                    {
                        SubjectWin = ComUtils.XmlUtils.Deserialize<Winner>(receivedLookAtData);
                        LookAt(SubjectWin.spinX, SubjectWin.spinY);

                        receivedLookAtData = null;
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine("Error XML Winner: " + exc.Message);
                    }

                }
            }
        }

        void ReceiveDataECS(object sender)
        {
            while (true)
            {
                yarpPortSetECS.receivedData(out receivedECSData);
                if (receivedECSData != null && receivedECSData != "")
                {

                    try
                    {

                        exp = ComUtils.XmlUtils.Deserialize<FaceExpression>(receivedECSData);
                        SetFacialExpression(exp.valence, exp.arousal);

                        receivedECSData = null;
                        exp = null;
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine("Error XML Set expression: " + exc.Message);
                    }

                }
            }
        }

        void ReceiveDataFacialExpression(object sender)
        {
            while (true)
            {
                yarpPortSetFacialExpression.receivedData(out receivedFacialExpression);
                if (receivedFacialExpression != null && receivedFacialExpression != "")//&& yarpExpressionOn)
                {
                    try
                    {

                        //Json
                        //listMotors = ComUtils.JsonNetSerializer.Deserialize<List<ServoMotor>>(receivedSetMotors);

                        //foreach (ServoMotor serv in listMotorsFacialExpression)
                        //{
                        //    if (!((serv.PulseWidthNormalized >= 0 && serv.PulseWidthNormalized <= 1) || serv.PulseWidthNormalized == -1.0))
                        //    {

                        //        MessageBox.Show("Error PulseWidthNormalized of ServoMotor " + serv.Name +" PulseWidthNormalized:"+serv.PulseWidthNormalized);
                        //        return;
                        //    }
                        //    else if(serv.Name=="Jaw" && (serv.PulseWidthNormalized<=0.25|| serv.PulseWidthNormalized>=0.75))
                        //    {
                        //        MessageBox.Show("Error PulseWidthNormalized of ServoMotor " + serv.Name +" PulseWidthNormalized:"+serv.PulseWidthNormalized);
                        //        return;
                        //    } 
                        //}

                        if (listMotorsFacialExpression.FindAll(a => (a.PulseWidthNormalized >= 0 && a.PulseWidthNormalized <= 1) || a.PulseWidthNormalized == -1.0).Count != 32)
                        {
                            MessageBox.Show("Error PulseWidthNormalized ");//of ServoMotor " + serv.Name + " PulseWidthNormalized:" + serv.PulseWidthNormalized);
                            return;
                        }
                        else if (listMotorsFacialExpression.FindAll(a => (a.PulseWidthNormalized <= 0.25 || a.PulseWidthNormalized >= 0.75) || a.Name == "Jaw").Count == 1)
                        {
                            MessageBox.Show("Error PulseWidthNormalized of ServoMotor Jaw ");// + serv.Name + " PulseWidthNormalized:" + serv.PulseWidthNormalized);
                            return;
                        }

                        //Bottle
                        //listMotors.AddRange(currentSmState);
                        //string[] res= receivedSetMotors.Split(' ');

                        //for (int i = 0; i <= res.Length; i++)
                        //{
                        //    if ((float.Parse(res[i]) >= 0 && float.Parse(res[i]) <= 1) || float.Parse(res[i]) == -1.0)
                        //        listMotors[i].PulseWidthNormalized = float.Parse(res[i]);
                        //    else
                        //    {
                        //        return;
                        //    }
                        //}

                        yarpPortSetFacialMotors.sendData(ComUtils.XmlUtils.Serialize<List<ServoMotor>>(listMotorsFacialExpression));

                        receivedFacialExpression = null;

                        listMotorsFacialExpression.Clear();

                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine("Error XML Set motors: " + exc.Message);
                    }

                }
            }
        }

        void ReceiveDataReflexes(object sender, ElapsedEventArgs e)
        {
            yarpPortReflexes.receivedData(out receivedReflexes);
            if (receivedReflexes != null && receivedReflexes != "")//&& yarpExpressionOn)
            {
                try
                {
                    if(Enum.IsDefined(typeof(reflexes),receivedReflexes))
                        yarpPortSetFacialMotors.sendData(receivedReflexes);

                    receivedReflexes = null;
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Error Reflexes: " + exc.Message);
                }

            }
        }

//        void ReceiveDataFeedback(object sender, ElapsedEventArgs e)

        void ReceiveDataFeedback(object sender)
        {
            while (true)
            {
                yarpPortFeedback.receivedData(out receivedFeedback);
                if (receivedFeedback != null && receivedFeedback != "")
                {
                    try
                    {



                        listMotorsFeedback = ComUtils.XmlUtils.Deserialize<List<ServoMotor>>(receivedFeedback);

                       // currentSmState = listMotorsFeedback;


                        yarpPortFeedbackXML.sendData(ComUtils.XmlUtils.Serialize<List<ServoMotor>>(listMotorsFeedback));
                        yarpPortFeedbackJSON.sendData(ComUtils.JsonUtils.Serialize<List<ServoMotor>>(listMotorsFeedback));

                        foreach (ServoMotor sm in listMotorsFeedback)
                        {
                            ListFeedbackOnlyPositions.Add(sm.PulseWidthNormalized);
                        }

                        yarpPortFeedbackBOTTLE.sendData(ListFeedbackOnlyPositions);

                        receivedFeedback = null;
                        listMotorsFeedback.Clear();
                        ListFeedbackOnlyPositions.Clear();
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine("Error XML Feedback: " + exc.Message);
                    }

                }
            }
        }


        private void SendFeedBack()
        {

            while (true)
            {
                lock (this)
                {
                    if (currentSmState == null)
                        continue;

                    yarpPortFeedbackXML.sendData(ComUtils.XmlUtils.Serialize<List<ServoMotor>>(currentSmState));
                    yarpPortFeedbackJSON.sendData(ComUtils.JsonUtils.Serialize<List<ServoMotor>>(currentSmState));

                    List<float> ListOnlyPositions = new List<float>();
                    foreach (ServoMotor sm in currentSmState)
                    {
                        ListOnlyPositions.Add(sm.PulseWidthNormalized);
                    }

                    yarpPortFeedbackBOTTLE.sendData(ListOnlyPositions);
                }

                System.Threading.Thread.Sleep(200);
            }
        }

        

        private void SetFacialExpression(float pleasure, float arousal)
        {
            try
            {
                motionECS.ServoMotorsList.Where(w => w.PulseWidthNormalized != -1).ToList().ForEach(s => s.PulseWidthNormalized = -1);

                foreach (ECSMotor m in ecs.ECSMotorList)
                {
                    float f = ecs.GetECSValue(m.Channel, pleasure, arousal);
                    motionECS.ServoMotorsList[m.Channel].PulseWidthNormalized = f;
                }

                lock (this)
                {
                    yarpPortSetFacialMotors.sendData(ComUtils.XmlUtils.Serialize<List<ServoMotor>>(motionECS.ServoMotorsList));
                }

               
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void LookAt(float x, float y)
        {
            try
            {

                int lookAtDuration = 120; //the lookat function works every 110msec so it can't act longer requests
                float minAmplitudeX = 0.002f, minAmplitudeY = 0.002f, maxAmplitudeX = 1.0f, maxAmplitudeY = 1.0f;

                 old_PosX = currentSmState[(int)MotorsNames.Turn].PulseWidthNormalized;
                 old_PosY = currentSmState[(int)MotorsNames.UpperNod].PulseWidthNormalized;


                motionLookAt.ServoMotorsList.Where(w => w.PulseWidthNormalized != -1).ToList().ForEach(s => s.PulseWidthNormalized = -1);
                motionLookAt.Duration = lookAtDuration;
                motionLookAt.Priority = 10;

                motionLookAt.ServoMotorsList[(int)MotorsNames.Turn].PulseWidthNormalized = old_PosX;
                motionLookAt.ServoMotorsList[(int)MotorsNames.UpperNod].PulseWidthNormalized = old_PosY;

                if (Math.Abs(x - old_PosX) > minAmplitudeX || Math.Abs(y - old_PosY) > minAmplitudeY)
                {
                    if (Math.Abs(x - old_PosX) > minAmplitudeX)
                    {
                        if (Math.Abs(x - old_PosX) < maxAmplitudeX)
                            motionLookAt.ServoMotorsList[(int)MotorsNames.Turn].PulseWidthNormalized = x;
                        else
                        {
                            if (x - old_PosX > 0)
                                motionLookAt.ServoMotorsList[(int)MotorsNames.Turn].PulseWidthNormalized = old_PosX + maxAmplitudeX;
                            else
                                motionLookAt.ServoMotorsList[(int)MotorsNames.Turn].PulseWidthNormalized = old_PosX - maxAmplitudeX;
                        }
                    }

                    if (Math.Abs(y - old_PosY) > minAmplitudeY)
                    {
                        if (Math.Abs(x - old_PosY) < maxAmplitudeY)
                            motionLookAt.ServoMotorsList[(int)MotorsNames.UpperNod].PulseWidthNormalized = y;
                        else
                        {
                            if (y - old_PosY > 0)
                                motionLookAt.ServoMotorsList[(int)MotorsNames.UpperNod].PulseWidthNormalized = old_PosY + maxAmplitudeY;
                            else
                                motionLookAt.ServoMotorsList[(int)MotorsNames.UpperNod].PulseWidthNormalized = old_PosY - maxAmplitudeY;
                        }
                    }

                    //motionLookAt.ServoMotorsList[(int)MotorsNames.Tilt].PulseWidthNormalized = 1 - motionLookAt.ServoMotorsList[(int)MotorsNames.Turn].PulseWidthNormalized;
                    lock (this)
                    {
                        yarpPortSetFacialMotors.sendData(ComUtils.XmlUtils.Serialize<List<ServoMotor>>(motionLookAt.ServoMotorsList));

                    }
                   // Console.WriteLine("Turn " + motionToTest.ServoMotorsList[(int)MotorsNames.Turn].PulseWidthNormalized + " old Position: " + old_PosX.ToString() + " New Position: " + x.ToString());
                    // Console.WriteLine("UpperNod " + motionToTest.ServoMotorsList[(int)MotorsNames.UpperNod].PulseWidthNormalized + " old Position: " + old_PosY.ToString() + " New Position: " + y.ToString());


                    if (motionLookAt.ServoMotorsList[(int)MotorsNames.Turn].PulseWidthNormalized < 0)
                        Console.WriteLine("Error Turn " + motionLookAt.ServoMotorsList[(int)MotorsNames.Turn].PulseWidthNormalized + " old Position: " + old_PosX.ToString() + " New Position: " + x.ToString());
                    else if (motionLookAt.ServoMotorsList[(int)MotorsNames.UpperNod].PulseWidthNormalized < 0)
                        Console.WriteLine("Error UpperNod " + motionLookAt.ServoMotorsList[(int)MotorsNames.UpperNod].PulseWidthNormalized + " old Position: " + old_PosY.ToString() + " New Position: " + y.ToString());


                    //currentSmState = motionToTest.ServoMotorsList;

                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CheckStatusYarp(object source, ElapsedEventArgs e)
        {
            #region lookAt_out
            if (yarpPortLookAt.PortExists(lookAt_out) && colLookAt == "red")
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate() 
                    { 
                        EllLookAt.Fill = Brushes.Green; 
                    }));
                colLookAt = "green";
            }
            else if (!yarpPortLookAt.PortExists(lookAt_out) && colLookAt == "green")
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate() { EllLookAt.Fill = Brushes.Red; }));
                colLookAt = "red";
            }
            #endregion

            #region ecs_out
            if (yarpPortSetECS.PortExists(ecs_out) && colECS == "red")
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate() { Ellexp.Fill = Brushes.Green; }));
                colECS = "green";
            }
            else if (!yarpPortSetECS.PortExists(ecs_out) && colECS == "green")
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate() { Ellexp.Fill = Brushes.Red; }));
                colECS = "red";
            }
            #endregion

            #region facialexpression_out

            if (yarpPortSetFacialExpression.PortExists(facialexpression_out) && colExp == "red")
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate() { EllSetFace.Fill = Brushes.Green; }));
                colExp = "green";
            }
            else if (!yarpPortSetFacialExpression.PortExists(facialexpression_out) && colExp == "green")
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate() { EllSetFace.Fill = Brushes.Red; }));
                colExp = "red";
            }
            #endregion

            #region reflexes_out

            if (yarpPortSetFacialExpression.PortExists(reflexes_out) && colRef == "red")
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate() { EllRef.Fill = Brushes.Green; }));
                colRef = "green";
            }
            else if (!yarpPortSetFacialExpression.PortExists(reflexes_out) && colRef == "green")
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate() { EllRef.Fill = Brushes.Red; }));
                colRef = "red";
            }
            #endregion

            #region setFacialMotors_in

            if (yarpPortSetFacialExpression.PortExists(setFacialMotors_in) && colMot == "red")
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate() { EllMot.Fill = Brushes.Green; }));
                colMot = "green";
            }
            else if (!yarpPortSetFacialExpression.PortExists(setFacialMotors_in) && colMot == "green")
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate() { EllMot.Fill = Brushes.Red; }));
                colMot = "red";
            }
            #endregion


            #region check Network
            if (yarpPortLookAt.NetworkExists() && colYarp == "red")
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate() 
                { 
                    Ellyarp.Fill = Brushes.Green;
                    EllFeedXML.Fill = Brushes.Green;
                    EllFeedJson.Fill = Brushes.Green;
                    EllFeedBottle.Fill = Brushes.Green;
                }));
                colYarp = "green";
            }
            else if (!yarpPortLookAt.NetworkExists() && colYarp == "green")
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate() 
                    { 
                        Ellyarp.Fill = Brushes.Red;
                        EllFeedXML.Fill = Brushes.Red;
                        EllFeedJson.Fill = Brushes.Red;
                        EllFeedBottle.Fill = Brushes.Red;
                    
                    }));
                colYarp = "red";
            }

       

            #endregion
        }

        public reflexes GetReflexName(int i)
        {
            string name = Enum.GetName(typeof(reflexes), i);
            if (null == name) throw new Exception();
            return (reflexes)Enum.Parse(typeof(reflexes), name);
        }


        private void YarpDisconnect()
        {
            if (senderThreadFeedBack != null)
                senderThreadFeedBack.Abort();

            receiverThreadFeedBack.Abort();
            receiverThreadLookAt.Abort();
            receiveThreadECS.Abort();
            receiveThreadFacialExpression.Abort();

            if (TimerCheckStatusYarp != null)
            { 
                TimerCheckStatusYarp.Elapsed -= new ElapsedEventHandler(CheckStatusYarp);
                TimerCheckStatusYarp.Stop();
            }

          
            if (yarpPortLookAt != null)
                yarpPortLookAt.Close();

            if (yarpPortSetECS != null)
                yarpPortSetECS.Close();

            if (yarpPortSetFacialExpression != null)
                yarpPortSetFacialExpression.Close();

            if (yarpPortFeedbackJSON != null)
                yarpPortFeedbackJSON.Close();

            if (yarpPortFeedbackXML != null)
                yarpPortFeedbackXML.Close();

            if (yarpPortFeedbackBOTTLE != null)
                yarpPortFeedbackBOTTLE.Close();

        }

        private void Window_Closing(object sender, EventArgs e)
        {
            YarpDisconnect();
        }

        private void lbl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Label lbl = sender as Label;

            string port = lbl.Content.ToString();
            string portName="";

    
            if (port == lookAt_in && colLookAt != "red")
                portName = lookAt_out;
            else if (port == ecs_in && colECS != "red")
                portName = ecs_out;
            else if (port == facialexpression_in && colExp != "red")
                portName = facialexpression_out;
            else if (port == reflexes_in && colRef != "red")
                portName = reflexes_out;
            else if (port.Last() == 'o')
                portName = port;


            if (portName == "")
                MessageBox.Show("Output Port not connected");
            else
            {
                monitor = new YarpMonitor(portName);
                monitor.ShowDialog();

                portName = "";
                monitor = null;
            }
        }

      
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;
namespace activeWindow
{
    public partial class TCAssistant
    {
        class AgtInfo {
            public String ip;
            public String os="windows";
            public override string ToString()
            {
                return this.ip;
            }
        }

        private StringBuilder allRunLog = new StringBuilder();

        private void btnAddHost_Click(object sender, EventArgs e)
        {
            if (txtHostAdd.Text != null && txtHostAdd.Text.Length > 0)
            {
                AgtInfo info = new AgtInfo();
                info.ip = txtHostAdd.Text;
                info.os = this.cbOs.SelectedItem.ToString();
                lstHostList.Items.Add(info);
            }
        }

        private void btnDelHost_Click(object sender, EventArgs e)
        {
            if (lstHostList.SelectedIndex >= 0)
            {
                String hostName = lstHostList.SelectedItem.ToString();

                int index = lstHostList.SelectedIndex;

                lstHostList.Items.RemoveAt(index);

                if (lstRunResult.Items.Count > index)
                {
                    lstRunResult.Items.RemoveAt(index);
                }
            }
        }

        private void save()
        {
            if (!Directory.Exists("cfg"))
            {
                Directory.CreateDirectory("cfg");
            }
            saveItemsToFile("cfg\\HostList.txt", lstHostList);
            saveStringToFile("cfg\\Command.txt", txtCommand.Text);
            saveStringToFile("cfg\\CheckCondition.txt", txtCheckCondition.Text);
        }

        private void saveItemsToFile(String filename, ListBox box)
        {
            FileInfo f = new FileInfo(filename);
            StreamWriter writer = f.CreateText();

            foreach (Object o in box.Items)
            {
                AgtInfo info = (AgtInfo)o;

                writer.WriteLine(info.ip+" "+info.os);
            }

            writer.Close();
        }

        private void loadItemsToListBox(String filename, ListBox box)
        {
            if (File.Exists(filename))
            {
                StreamReader re = File.OpenText(filename);
                string input = null;
                while ((input = re.ReadLine()) != null)
                {
                    input=input.Trim();
                    if (input.Length == 0)
                        continue;
                    string[] agt = input.Split(new string[] { " "}, StringSplitOptions.None);
                    AgtInfo info = new AgtInfo();
                    info.ip = agt[0].Trim();
                    if(agt.Length>1)
                        info.os = agt[1].Trim();
                    box.Items.Add(info);
                }
                re.Close();
            }
        }

        private void saveStringToFile(String filename, String text)
        {
            FileInfo f = new FileInfo(filename);
            StreamWriter writer = f.CreateText();

            writer.WriteLine(text);

            writer.Close();
        }

        private String readText(String filename)
        {
            String text = "";
            if (File.Exists(filename))
            {
                StreamReader re = File.OpenText(filename);
                text = re.ReadToEnd();
                re.Close();
            }
            return text;
        }

        class PingInfo
        {
            public ProcessStartInfo startInfo;
            public int index;

        }

        private delegate void listBoxHandler(int index, String result);

        private void updateResultListBoxItemResult(int index, String result)
        {
            lstRunResult.Items[index] = result;
        }

        void ping(Object pinfo)
        {
            PingInfo info = (PingInfo)pinfo;

            ProcessStartInfo startInfo = info.startInfo;

            // Start the process with the info we specified.
            // Call WaitForExit and then the using statement will close.
            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
                StreamReader reader = exeProcess.StandardOutput;
                String outtext = reader.ReadToEnd();
                if (outtext.Contains("PONG"))
                {
                    listBoxHandler handler = new listBoxHandler(updateResultListBoxItemResult);
                    lstRunResult.Invoke(handler, new Object[] { info.index, "OK" });
                    //lstRunResult.Items[info.index] = "OK";
                }
                else
                {
                    listBoxHandler handler = new listBoxHandler(updateResultListBoxItemResult);
                    lstRunResult.Invoke(handler, new Object[] { info.index, outtext.Replace("\r\n", "") });
                    //lstRunResult.Items[info.index] = outtext.Replace("\r\n", "");
                }
            }
        }

        private void btnPing_Click(object sender, EventArgs e)
        {
            // staf 192.168.152.131 ping ping

            lstRunResult.Items.Clear();

            for (int i = 0; i < lstHostList.Items.Count; i++)
            {
                lstRunResult.Items.Add("ping...");
                
                try
                {
                    // Use ProcessStartInfo class
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.CreateNoWindow = true;
                    startInfo.UseShellExecute = false;
                    startInfo.FileName = "staf.exe";
                    startInfo.WorkingDirectory = "C:\\STAF";
                    startInfo.RedirectStandardOutput = true;
                    startInfo.Arguments = "%host ping ping";
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.Arguments = startInfo.Arguments.Replace("%host", lstHostList.Items[i].ToString());
                    PingInfo info = new PingInfo();
                    info.index = i;
                    info.startInfo = startInfo;

                    Thread pingThread = new Thread(ping);

                    pingThread.Start(info);

                }
                catch
                {
                    // Log error.
                }

            }  // end of for loop

        }

        /** is output OK, for FS operations */
        private bool isOutputOK(String output)
        {
            output = output.Replace("\r\n", "");
            if (output.Trim("- ".ToCharArray()).Equals("Response") || !output.Contains("Error"))
            {
                return true;
            }
            return false;
        }

        private delegate void TxtHandeler(String text);
        private delegate void Enabler();

        private void enableAdd()
        {
            btnAddHost.Enabled = true;
        }

        private void enableDelete()
        {
            btnDelHost.Enabled = true;
        }

        private void updateText(String text)
        {
            txtResult.Text = text;

            //  focus on the last line of the textbox
            txtResult.SelectionStart = txtResult.Text.Length;
            txtResult.ScrollToCaret();

        }

        private void showLog()
        {
            TxtHandeler handler = new TxtHandeler(updateText);
            //txtResult.Text = runLog.ToString();
            txtResult.Invoke(handler, new Object[] { allRunLog.ToString() });
        }
        private void btnRun_Click(object sender, EventArgs e)
        {
            run_Click(sender, e, false);
        }
        private void run_Click(object sender, EventArgs e, bool disableParallel)
        {
            //if(chkStafCommand.Checked) // run STAF command other than OS command: 
            {
                //TODO : support multi commands:

                String command = txtCommand.Text.Trim(" \r\n".ToCharArray());

                if (!command.Contains("%host")) // run for only 1 user specified host, and return:
                {
                    String commandWithoutStafPrefix = command.Substring(command.IndexOf(' ') + 1);

                    //running single command, and save output:
                    String output = runStafCmd(commandWithoutStafPrefix);
                    return;
                }
            }

            lstRunResult.Items.Clear();

            allRunLog = new StringBuilder();

            int i = 0;

            if (disableParallel)
            {

                // 1 thread to drive all STAF host one by one
                // Here I use thread to avoid block main thread(GUI) from responding to end user:
                Thread processThread = new Thread(delegate()
                {

                    btnAddHost.Enabled = false;
                    btnDelHost.Enabled = false;

                    foreach (Object host in lstHostList.Items)
                    {
                        //String localName = System.Net.Dns.GetHostName();

                        // FIXME: does it get the correct IP address ?? (Use should be able to determine the IP add):
                        //String localIP = System.Net.Dns.GetHostEntry(localName).AddressList[0].ToString();

                        lstRunResult.Items.Add("running...");
                        String hostAdd = host.ToString();
                        String localIP = getLocalIPForHost(hostAdd);
                        HostInfo info = new HostInfo(localIP, hostAdd, i++);
                        info.OS = ((AgtInfo)host).os;
                        processHost(info);
                    }

                    //                     btnAddHost.Enabled = true;
                    //                     btnDelHost.Enabled = true;
                    Enabler addEnabler = new Enabler(enableAdd);
                    Enabler deleEnabler = new Enabler(enableDelete);
                    btnAddHost.Invoke(addEnabler, new object[] { });
                    btnDelHost.Invoke(deleEnabler, new object[] { });
                });

                processThread.Start();

            }
            else
            {
                btnAddHost.Enabled = false;
                btnDelHost.Enabled = false;

                foreach (Object host in lstHostList.Items)
                {
                    lstRunResult.Items.Add("running...");
                }

                // start an invoker thread to invoke tasks on all STAF hosts, and wait the threads to join
                Thread invoker = new Thread(delegate()
                {
                    Thread[] threads = new Thread[lstHostList.Items.Count];

                    int m = 0;

                    // assign one thread to drive each STAF host 
                    foreach (Object host in lstHostList.Items)
                    {
                        // FIXME: does it get the correct IP address ?? (Use should be able to determine the IP add):
                        //String localIP = System.Net.Dns.GetHostEntry(localName).AddressList[0].ToString();

                        String hostAdd = host.ToString();
                        String localIP = getLocalIPForHost(hostAdd);

                        HostInfo info = new HostInfo(localIP, hostAdd, i++);
                        info.OS = ((AgtInfo)host).os;
                        threads[m] = new Thread(processHost);
                        threads[m++].Start(info);

                    } // end of for each host

                    while (m > 0)
                    {
                        threads[--m].Join();
                    }

                    //                     btnAddHost.Enabled = true;
                    //                     btnDelHost.Enabled = true;

                    Enabler addEnabler = new Enabler(enableAdd);
                    Enabler deleEnabler = new Enabler(enableDelete);
                    btnAddHost.Invoke(addEnabler, new object[] { });
                    btnDelHost.Invoke(deleEnabler, new object[] { });
                });

                invoker.Start();
            }
        }

        private Regex regex = new Regex(@"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b", RegexOptions.Compiled);

        public bool matchIPAddress(String ipString)
        {
            return regex.IsMatch(ipString);
        }

        public int findCommonPrefixLength(String a, String b)
        {
            int commonStringLength = 0;

            //find longest common string length of address and hostAddress:
            int m = 0;
            int n = 0;
            while (m < a.Length && n < b.Length)
            {
                if (a[m] == b[n])
                {
                    m++;
                    n++;
                    continue;
                }
                else
                {
                    break;
                }
            }
            commonStringLength = Math.Min(m, n);

            return commonStringLength;
        }

        // there may be several IP addresses for local host, especially when virtual machine installed.
        // try to get local IP address which has the longest common prefix with host machine's IP (same network)
        public String getLocalIPForHost(String hostName)
        {
            String hostIPAddress = hostName;
            if (!matchIPAddress(hostName))
            {
                try
                {
                    hostIPAddress = System.Net.Dns.GetHostEntry(hostName).AddressList[0].ToString();
                }
                catch (System.Exception ex)
                {
                    allRunLog.Append(ex.ToString()).AppendLine();
                    showLog();
                }

            }

            //To test: host, ip bridged/hosted in virtual machine
            String localName = System.Net.Dns.GetHostName();
            System.Net.IPAddress[] localAddresses = System.Net.Dns.GetHostEntry(localName).AddressList;

            // default local IP to the first IP address of local machine:
            String localIP = localAddresses[0].ToString();

            int indexOfMaxSimilarAddress = -1;
            int maxCommonPrefixLen = 0;

            // find the loal IP address which has the longest prefix with a host address:
            for (int i = 0; i < localAddresses.Length; i++)
            {
                String localAddress = localAddresses[i].ToString();

                int commonPrefixLen = findCommonPrefixLength(localAddress, hostIPAddress);
                if (commonPrefixLen > maxCommonPrefixLen)
                {
                    maxCommonPrefixLen = commonPrefixLen;
                    indexOfMaxSimilarAddress = i;
                }

            }

            if (indexOfMaxSimilarAddress >= 0)
            {
                // set the local IP to the Address which has longest common prefix with hostAddress:
                localIP = localAddresses[indexOfMaxSimilarAddress].ToString();
            }

            return localIP;
        }

        class HostInfo
        {
            public String localIP;
            public String hostAdd;
            public int index;
            public String OS = "windows";
            public HostInfo(String localIP, String hostAdd, int index)
            {
                this.localIP = localIP;
                this.hostAdd = hostAdd;
                this.index = index;
            }
            public override string ToString()
            {
                return hostAdd;
            }
        }
        private void processHost(object obj)
        {
            HostInfo info = (HostInfo)obj;

            String localIP = info.localIP;           
            String hostAdd = info.hostAdd;

            bool isOk = true;

            DateTime startTime = DateTime.Now;
            
            // run STAF command on remote STAF host:
            isOk = runSTAFCommandOnHost(hostAdd, info.OS, isOk);

            DateTime stopTime = DateTime.Now;

            /* Compute the duration between the initial and the end time. */
            TimeSpan duration = stopTime - startTime;

            allRunLog.Append("Total time for processing host: " + hostAdd + " is " + duration).AppendLine();

            showLog();

            //update final result on Result List Box:
            if (isOk)
            {
                listBoxHandler handler = new listBoxHandler(updateResultListBoxItemResult);
                lstRunResult.Invoke(handler, new Object[] { info.index, "OK" });
                //lstRunResult.Items[info.index] = "OK";

            }
            else
            {
                listBoxHandler handler = new listBoxHandler(updateResultListBoxItemResult);
                lstRunResult.Invoke(handler, new Object[] { info.index, "Failed" });
                //lstRunResult.Items[info.index] = logs;
            }


        }

        //run STAF command on remote STAF host
        private bool runSTAFCommandOnHost(String hostAdd, String os, bool isOk)
        {
            try
            {
                String command = txtCommand.Text.Trim(" \r\n".ToCharArray());
                String home = ConfigurationManager.AppSettings["win_home"];

                if (!os.ToLower().StartsWith("win"))
                {
                    home = ConfigurationManager.AppSettings["unix_home"];
                }

                command = command.Replace("%host", hostAdd).Replace("%home",home);

                allRunLog.Append("[REQ]" + command).AppendLine();
                showLog();

                // for process command, add STDERRTOSTDOUT and stdout %out
                String lowerCmd = command.ToLower();
                if (lowerCmd.Contains("process"))  // for running process, would redirect the console output and check result:
                {
                    if (lowerCmd.StartsWith("staf"))
                    {
                        // get command Without "staf" Prefix:
                        command = command.Substring(command.IndexOf(' ') + 1);
                        command += " stderrtostdout returnstdout wait";
                    }

                    String output = runStafCmd(command);

                    allRunLog.Append("[RESP " + hostAdd + "]").Append(output);
                    showLog();

                    return isOutputOK(output) && checkRegexOnConsoleOutput(output);
                }
                else  // for running other services besides PROCESS, would not check result:
                {
                    String commandWithoutStafPrefix = command.Substring(command.IndexOf(' ') + 1);

                    String output = runStafCmd(commandWithoutStafPrefix);

                    allRunLog.Append("[RESP]").AppendLine().Append(output);
                    showLog();


                    if (!isOutputOK(output))
                    {
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                allRunLog.Append(ex.ToString());
            }

            return isOk;
        }

        private String processOneOSCommand(String hostAdd, String outFile, String inputCmd)
        {
            //Run staf command:
            // support format 1: 
            // staf 192.168.1.103 process start shell command c:\dcerat\case.bat PARMS 551020 
            // WAIT STDERRTOSTDOUT RETURNSTDOUT WORKDIR c:\dcerat
            // format 2: staf 192.168.1.103 process start shell command "java -version" WAIT
            // format 3: staf 192.168.1.103 process start shell command hello.exe WAIT
            //String inputCmd = txtCommand.Text.Trim(" \r\n".ToCharArray());
            String command = inputCmd;  // command to run in STAF command
            String parms = "";
            String workdir = "";

            // Here because on windows, RETURNSTDOUT not work always, 
            // so I use stdout to redirect the command output to a random file under C:\,
            // and then get back the output file by STAF command
            // START SHELL COMMAND ... STDERRTOSTDOUT RETURNSTDOUT
            String stafCmd = "START SHELL COMMAND %cmd %parms STDERRTOSTDOUT stdout %out  WAIT  %workdir ";

            //parse command and param from textbox input command:
            if (inputCmd.Contains(" "))
            {
                command = inputCmd.Substring(0, inputCmd.IndexOf(' '));

                if (inputCmd.Length > inputCmd.IndexOf(' ') + 1)
                {
                    parms = "PARMS " + inputCmd.Substring(inputCmd.IndexOf(' ') + 1);
                }
            }

            //parse WORKDIR:
            if (command.LastIndexOf('\\') > 0)
            {
                workdir = "WORKDIR " + command.Substring(0, command.LastIndexOf('\\'));
            }

            stafCmd = stafCmd.Replace("%cmd", command);
            stafCmd = stafCmd.Replace("%parms", parms);
            stafCmd = stafCmd.Replace("%out", outFile);

            stafCmd = stafCmd.Replace("%workdir", workdir);

            // run STAF command on remote STAF host:
            String output = runStafCmd(hostAdd, "PROCESS", stafCmd);


            allRunLog.Append("[RESP]").Append(output);
            showLog();

            return output;
        }

        // check Regext on console output, console output of STAF command has been saved to local consoleOutputFile
        private bool checkRegexOnConsoleOutput(String consoleOutput)
        {
            Regex regex = new Regex(txtCheckCondition.Text.Trim(), RegexOptions.Multiline);

            if (regex.IsMatch(consoleOutput))
            {
                allRunLog.Append("Output checked successfully!").AppendLine();
                showLog();
                return true;
            }
            else  // failed to check result:
            {
                String failureMsgOfCheckResult = "Failed to check console output again regular expression! Expected: " + txtCheckCondition.Text.Trim();
                allRunLog.Append(failureMsgOfCheckResult).AppendLine();
                showLog();
                return false;
            }
        }

        // get entry type of a name by "staf host FS get entry"
        public char getEntryType(String host, String fsEntryName)
        {
            String cmd = "GET ENTRY %entryName TYPE";
            cmd = cmd.Replace("%entryName", fsEntryName);

            String result = runStafCmd(host, "FS", cmd);
            result = result.Replace("\r\n", "");
            result = result.Trim("- \0".ToCharArray());

            return result[result.Length - 1];
        }


        // run STAF command, and append log to hostRunLog, if hostRunLog is null, would not save log to hostRunLog:
        private String runStafCmd(String commandWithoutStafPrefix)
        {
            StringBuilder output = new StringBuilder(1024);
            try
            {
                // Use ProcessStartInfo class
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.FileName = "staf.exe";
                startInfo.WorkingDirectory = "C:\\STAF";
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = false;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.Arguments = commandWithoutStafPrefix;

                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();

                    StreamReader reader = exeProcess.StandardOutput;
                    output.Append(reader.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                output.Append(e.ToString());
            }
            return output.ToString();
        }

        private String runStafCmd(String host, String service, String parameters)
        {
            return runStafCmd(" " + host + " " + service + " " + parameters);
        }

        private void lstRunResult_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstRunResult.SelectedIndex >= 0)
            {
                txtResult.Text = lstRunResult.SelectedItem.ToString();
            }
        }

        private void lstHostList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstHostList.SelectedIndex >= 0)
            {
                txtHostAdd.Text = lstHostList.SelectedItem.ToString();
            }
        }

        private void bRunS_Click(object sender, EventArgs e)
        {
            run_Click(sender, e, true);
        }

        private void bSave_Click(object sender, EventArgs e)
        {
            save();
        }

    }
}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors.Controls;
using static Elogon256.sqlConns;
using EtravSQL;
using DevExpress.DataAccess.Excel;
using System.Data.SqlClient;
using System.Data;
using testlogon;
using System.Reflection;
using System.IO;
using System.IO.Compression;
using Updater;


namespace EmployeeManager
{
    public partial class Form1 : Form
    {
        static string CallingAppName = "EmployeeManager.exe"; //the name of this app used for login dll
        static string devXpresspack = "DevExpress16.2.zip"; //devxpress pack used  for his apps funcionality
        const String ConnStr = "Data Source=etrav-hack;Initial Catalog=qcrr;Persist Security Info=True;User ID=Application;Password=noitacilppa";//US
        const String ConnStr1 = "Data Source=etrav-hack;Initial Catalog=qcrr;Persist Security Info=True;User ID=Application;Password=noitacilppa";//CHINA "was using as a test"
        private string ETRAV = null;
        private string serverver;
        public string LoggedinEmployeeID { get; private set; }
        private bool loggedIN;
        private string employeeID;
        private string employeeName;
        private int rights;
        private int authorizedlevel;
        public string xfile;
        private string NemployeeName;
        private string NemployeeID;
        private string NemployeeLOC;
        private string NemployeeTitle;
        private string NemployeeDeptID;
        private string NemployeeHireDate;
        private string NemployeeSupervisorName;
        private string NemployeeFName;
        private string NemployeeMName;
        private string NemployeeLName;
        private string LOCAREA;
        private string EMPLOYEETYPE;
        public bool state { get; private set; }

        public Form1()
        {

            InitializeComponent();

            //initialize Employee Status
            object[] itemValues = new object[] { 0, 1, 2 };
            string[] itemDescriptions = new string[] { "Full Time", "Part Time", "Reg_Temp" };
            for (int i = 0; i < itemValues.Length; i++)
            {
                radioGroup1.Properties.Items.Add(new RadioGroupItem(itemValues[i], itemDescriptions[i]));
            }
            //initialize Location Status 2
            object[] itemValues2 = new object[] { 0, 1 };
            string[] itemDescriptions2 = new string[] { "US", "CHINA" };
            for (int j = 0; j < itemValues2.Length; j++)
            {
                radioGroup2.Properties.Items.Add(new RadioGroupItem(itemValues2[j], itemDescriptions2[j]));
            }

            SqlConnection SqlConn = new SqlConnection(ConnStr);
            SqlConn.Open();
            SqlCommand SelectCommand = new SqlCommand("select distinct SupervisorName from dbo.Employee_Information", SqlConn);
            SqlDataReader myreader = SelectCommand.ExecuteReader();

            while (myreader.Read())
            {
                txtBoss.Properties.Items.Add(myreader["SupervisorName"].ToString());

            }
            myreader.Close();

            ConfirmUser();
            radioGroup1.EditValue = 2;//defualt Employee Type = R
            lblstat.Text = "READY!";
            
            //set version info
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            lblVersion.Text = (String.Format(lblVersion.Text, version.Major, version.Minor, version.Build, version.Revision));
            lblVersion.Text = "Version:" + version;
            CheckGetUpdates(version);
        }

        private void simpleButton1_Click(object sender, EventArgs e)//ADD ONE NEW EMPLOYEE
        {
            //Check Data
            NemployeeName = string.Format("{0},{1}", NemployeeLName, NemployeeFName);
            if (NemployeeMName != "") { NemployeeName = string.Format("{0} {2},{1}", NemployeeLName, NemployeeMName, NemployeeFName); }
            SqlConnection SqlConn = new SqlConnection(ETRAV);
            SqlConn.Open();
            SqlDataAdapter sda = new SqlDataAdapter(string.Format("SELECT COUNT(*) FROM dbo.Employee_Information where Name='{0}'", NemployeeName), SqlConn);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            //ifOK Upload new employee
            if (dt.Rows[0][0].ToString() == "0")
            {

                string newemployee = string.Format("insert into dbo.Employee_Information (EmployeeID,Location,Name,Title,DeptID,HireDate,SupervisorName,FullPart,Reg_Temp,) Values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}'", NemployeeID, NemployeeLOC, NemployeeName, NemployeeTitle, NemployeeDeptID, NemployeeHireDate, NemployeeSupervisorName, EMPLOYEETYPE, EMPLOYEETYPE);
                try
                {
                    mySQL mSQL = new mySQL();
                    bool success = mSQL.myInsert("Etrav-Hack", newemployee);

                }

                catch
                {
                    MessageBox.Show("SQL Insert Failed For Query: \n" + newemployee);
                    //hyperlinkLabelControl1.Text = "SQL Insert Failed";
                    return;
                }
            }
            //else error message

        }

        private void simpleButton2_Click(object sender, EventArgs e)//DELETE ONE CURRENT EMPLOYEE
        {
            //Check Data
            NemployeeName = string.Format("{0},{1}", NemployeeLName, NemployeeFName);
            if (NemployeeMName != "") { NemployeeName = string.Format("{0} {2},{1}", NemployeeLName, NemployeeMName, NemployeeFName); }
            SqlConnection SqlConn = new SqlConnection(ETRAV);
            SqlConn.Open();
            SqlDataAdapter sda = new SqlDataAdapter(string.Format("SELECT COUNT(*) FROM dbo.Employee_Information where Name='{0}'", NemployeeName), SqlConn);
            DataTable dt = new DataTable();
            sda.Fill(dt);

            //ifOK Upload new employee
            if (dt.Rows[0][0].ToString() == "1")
            {

                string deleteemployee = string.Format("delete from  dbo.Employee_Information where EmployeeID='{0}', Name='{1}'", NemployeeID, NemployeeName);
                try
                {
                    mySQL mSQL = new mySQL();
                    bool success = mSQL.myInsert("Etrav-Hack", deleteemployee);
                    lblstat.Text = "Deleted Employee: " + NemployeeName;

                }

                catch
                {
                    MessageBox.Show("SQL Insert Failed For Query: \n" + deleteemployee);
                    lblstat.Text = "ERROR! "; 
                    return;
                }
            }
            //else error message

        }

        private void simpleButton3_Click(object sender, EventArgs e)//UPLOAD (INSERT) EMPLOYEE FILE(XLS,CVS)
        {
        

            //OPEN DIALOG BOX
            DialogResult result = openFileDialog1.ShowDialog();
            //Check Data
            

            //ifOK Upload UPDATED EMPLOYEE LIST
            if (result == DialogResult.OK) // Test result.
            {
              
                xfile = openFileDialog1.FileName;
                excelDataSource1.FileName = xfile;
                excelDataSource2.FileName = xfile;
                lblstat.Text = "Loaded File: " + xfile;

                //determine file type
                if (xfile.ToLower().Contains("xlsx")) { handeldataexcel(); }
                if (xfile.ToLower().Contains("xls")) { handeldataexcel(); }
                if (xfile.ToLower().Contains("xlsm")) { handeldataexcel(); }
                if (xfile.ToLower().Contains("csv")) { handeldatacsv(); }

            }
            Console.WriteLine(result); // <-- For debugging use.

            
        }

        public void handeldataexcel()//us&china excel file types
        {
            ExcelDataSource excelDataSource1 = new ExcelDataSource() { FileName = xfile };
            ExcelSourceOptions myOptions = new ExcelSourceOptions();
            ExcelWorksheetSettings cellRangeSettings = new ExcelWorksheetSettings("Sheet1");
            myOptions.ImportSettings = cellRangeSettings;
            excelDataSource1.SourceOptions = myOptions;
            gridControl1.DataSource = excelDataSource1;
            gridControl1.RefreshDataSource();
            try
            { excelDataSource1.Fill(); }
            catch(ArgumentException)
            {
                MessageBox.Show("File is in wrong format! Use Excel file type please. Columns:EmployeeID	Location	Name	Title	DeptID	EmpGroup	SupervisorName	FullPart	Shift	Reg_Temp	Work_Center	CostCenter	HireDate"); return;


            }//else error message

            

            for (int i = 0; i < gridView1.RowCount; i++)
            {
                int Info;
                NemployeeName = gridView1.GetRowCellDisplayText(i, gridView1.Columns["Name"]);
                NemployeeID = gridView1.GetRowCellDisplayText(i, gridView1.Columns["EmployeeID"]);
                NemployeeLOC = gridView1.GetRowCellDisplayText(i, gridView1.Columns["Location"]);
                NemployeeTitle = gridView1.GetRowCellDisplayText(i, gridView1.Columns["Title"]);
                NemployeeDeptID = gridView1.GetRowCellDisplayText(i, gridView1.Columns["DeptID"]);
                NemployeeHireDate = gridView1.GetRowCellDisplayText(i, gridView1.Columns["HireDate"]);
                NemployeeSupervisorName = gridView1.GetRowCellDisplayText(i, gridView1.Columns["SupervisorName"]);
                string S1 = string.Format("SELECT COUNT(*) FROM dbo.Employee_Information where Name='{0}' and EmployeeID='{1}'", NemployeeName, NemployeeID);
                string S2 = "insert into  Employee_Information (EmployeeID,Location,Name,Title,DeptID,HireDate,SupervisorName) Values('";
                SqlConnection SqlConn = new SqlConnection(ETRAV);
                SqlConn.Open();
                SqlCommand sda = new SqlCommand(S1, SqlConn);
                Info = (int)sda.ExecuteScalar();
                if (Info >= 1)
                {

                    string S3 = string.Format("update dbo.Employee_Information set EmployeeID='{0}',Location='{1}',Name='{2}',Title='{3}',DeptID='{4}',HireDate='{5}',SupervisorName='{6}'where EmployeeID='{0}' ", NemployeeID, NemployeeLOC, NemployeeName, NemployeeTitle, NemployeeDeptID, NemployeeHireDate, NemployeeSupervisorName);

                    try
                    {
                        mySQL mSQL = new mySQL();
                        if (radioGroup2.SelectedIndex == 0)
                        {
                            bool success = mSQL.myInsert("Etrav-Hack", S3);
                            lblstat.Text = "Updated Employee: " + NemployeeName;
                        }
                        if (radioGroup2.SelectedIndex == 1)
                        {
                            bool success = mSQL.myInsert("Etrav-Hack", S2);//china
                            lblstat.Text = "Updated Employee: " + NemployeeName;
                        }
                    }

                    catch
                    {
                        MessageBox.Show("SQL Insert Failed For Query: \n" + S2);

                        return;
                    }
                }//enter an update option here
                else
                {
                    S2 = string.Format("{0}{1}','", S2, NemployeeID);
                    S2 = string.Format("{0}{1}','", S2, NemployeeLOC);
                    S2 = string.Format("{0}{1}','", S2, NemployeeName);
                    S2 = string.Format("{0}{1}','", S2, NemployeeTitle);
                    S2 = string.Format("{0}{1}','", S2, NemployeeDeptID);
                    S2 = string.Format("{0}{1}','", S2, NemployeeHireDate);
                    S2 = string.Format("{0}{1}')", S2, NemployeeSupervisorName);
                    try
                    {
                        mySQL mSQL = new mySQL();
                        if (radioGroup2.SelectedIndex == 0)
                        {
                            bool success = mSQL.myInsert("Etrav-Hack", S2);
                            lblstat.Text = "Added Employee: " + NemployeeName;
                        }
                        if (radioGroup2.SelectedIndex == 1)
                        {
                            bool success = mSQL.myInsert("Etrav-Hack", S2);//china
                            lblstat.Text = "Added Employee: " + NemployeeName;
                        }
                    }

                    catch
                    {
                        MessageBox.Show("SQL Insert Failed For Query: \n" + S2);

                        return;
                    }
                }


            }
        }

        public void handeldatacsv()//us&china  csv file type
        {
            ExcelDataSource excelDataSource2 = new ExcelDataSource() { FileName = xfile, SourceOptions = new CsvSourceOptions() { CellRange = "A1:L1000" } };
            excelDataSource2.SourceOptions.SkipEmptyRows = false;
            excelDataSource2.SourceOptions.UseFirstRowAsHeader = true;
            gridControl1.DataSource = excelDataSource2;
            gridControl2.RefreshDataSource();

            try
            { excelDataSource2.Fill(); }
            catch (ArgumentException)
            {
                MessageBox.Show("File is in wrong format! Use Excel file type please. Columns:EmployeeID	Location	Name	Title	DeptID	EmpGroup	SupervisorName	FullPart	Shift	Reg_Temp	Work_Center	CostCenter	HireDate"); return;


            }//else error message



            for (int i = 0; i < gridView1.RowCount; i++)
            {
                int Info;
                NemployeeName = gridView2.GetRowCellDisplayText(i, gridView2.Columns["Name"]);
                NemployeeID = gridView2.GetRowCellDisplayText(i, gridView2.Columns["EmployeeID"]);
                NemployeeLOC = gridView2.GetRowCellDisplayText(i, gridView2.Columns["Location"]);
                NemployeeTitle = gridView2.GetRowCellDisplayText(i, gridView2.Columns["Title"]);
                NemployeeDeptID = gridView2.GetRowCellDisplayText(i, gridView2.Columns["DeptID"]);
                NemployeeHireDate = gridView2.GetRowCellDisplayText(i, gridView2.Columns["HireDate"]);
                NemployeeSupervisorName = gridView2.GetRowCellDisplayText(i, gridView2.Columns["SupervisorName"]);
                string S1 = string.Format("SELECT COUNT(*) FROM dbo.Employee_Information where Name='{0}' and EmployeeID='{1}'", NemployeeName, NemployeeID);
                string S2 = "insert into  Employee_Information (EmployeeID,Location,Name,Title,DeptID,HireDate,SupervisorName) Values('";
                SqlConnection SqlConn = new SqlConnection(ETRAV);
                SqlConn.Open();
                SqlCommand sda = new SqlCommand(S1, SqlConn);
                Info = (int)sda.ExecuteScalar();
                if (Info >= 1)
                {

                    string S3 = string.Format("update dbo.Employee_Information set EmployeeID='{0}',Location='{1}',Name='{2}',Title='{3}',DeptID='{4}',HireDate='{5}',SupervisorName='{6}'where EmployeeID='{0}' ", NemployeeID, NemployeeLOC, NemployeeName, NemployeeTitle, NemployeeDeptID, NemployeeHireDate, NemployeeSupervisorName);

                    try
                    {
                        mySQL mSQL = new mySQL();
                        if (radioGroup2.SelectedIndex == 0)
                        {
                            bool success = mSQL.myInsert("Etrav-Hack", S3);
                            lblstat.Text = "Updated Employee: " + NemployeeName;
                        }
                        if (radioGroup2.SelectedIndex == 1)
                        {
                            bool success = mSQL.myInsert("Etrav-Hack", S2);//china
                            lblstat.Text = "Updated Employee: " + NemployeeName;
                        }
                    }

                    catch
                    {
                        MessageBox.Show("SQL Insert Failed For Query: \n" + S2);

                        return;
                    }
                }//enter an update option here
                else
                {
                    S2 = string.Format("{0}{1}','", S2, NemployeeID);
                    S2 = string.Format("{0}{1}','", S2, NemployeeLOC);
                    S2 = string.Format("{0}{1}','", S2, NemployeeName);
                    S2 = string.Format("{0}{1}','", S2, NemployeeTitle);
                    S2 = string.Format("{0}{1}','", S2, NemployeeDeptID);
                    S2 = string.Format("{0}{1}','", S2, NemployeeHireDate);
                    S2 = string.Format("{0}{1}')", S2, NemployeeSupervisorName);
                    try
                    {
                        mySQL mSQL = new mySQL();
                        if (radioGroup2.SelectedIndex == 0)
                        {
                            bool success = mSQL.myInsert("Etrav-Hack", S2);
                            lblstat.Text = "Added Employee: " + NemployeeName;
                        }
                        if (radioGroup2.SelectedIndex == 1)
                        {
                            bool success = mSQL.myInsert("Etrav-Hack", S2);//china
                            lblstat.Text = "Added Employee: " + NemployeeName;
                        }
                    }

                    catch
                    {
                        MessageBox.Show("SQL Insert Failed For Query: \n" + S2);

                        return;
                    }
                }


            }
        }
        private bool ConfirmUser()
        {
            getlogon getin = new getlogon();
            string value = "Employee ID";
            //InputBox.Show("New document", "New document name:", ref value);
            if (InputBox("Confirm Current User?", "Comfirm Employee ID:", ref value) == DialogResult.OK)
            {
                LoggedinEmployeeID = value; // Do a try/catch encase network connection is down.7
                loggedIN = getin.getpersistant(LoggedinEmployeeID);
                employeeID = LoggedinEmployeeID;
                employeeName = getin.getlog(LoggedinEmployeeID, out rights, CallingAppName, out authorizedlevel);
                checkArea();

            }
            else { Environment.Exit(1); }

            //make input box asking for password
            string value2 = "Password";

            if (InputBoxPass("Password?", "Please Enter your Admin Password or Cancel to setup new user:", ref value2) == DialogResult.OK)//create option to get new password
            {

                getlogon getpass = new getlogon();
                //state = getpass.getauthpassword(value2,employeeName, employeeID,false);
                state = getpass.getfulllog(employeeName, employeeID, CallingAppName, true, value2, false);
                if (state == true) { getpass.getpersistant(employeeID); } //set persistant
                if (state == false) { MessageBox.Show("Password not accepted!"); Environment.Exit(1); }//return state
                //if (state == true && employeeID == "07840" || employeeID == "06539") { btnbarManagment.Enabled = true; simpleButton6.Enabled = true; }

            }
            else { Environment.Exit(1); }
            return state;

        }

        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        public static DialogResult InputBoxPass(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            Button buttonNewPass = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;
            textBox.PasswordChar = '%';

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonNewPass.Text = "Get New Pass";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonNewPass.DialogResult = DialogResult.Yes;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonNewPass.SetBounds(12,72,75,23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonNewPass.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;
            

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        private void hyperLinkEdit1_OpenLink(object sender, OpenLinkEventArgs e)//Open US/China xls sheet
        {
            xfile = "C:\\sql\\Sheet1.csv";
            //Application excel;
            SqlConnection conn = new SqlConnection(ETRAV);
            conn.Open();
            SqlCommand cmd = new SqlCommand("select * from  Qcrr.dbo.Employee_Information", conn);
            SqlDataReader dr = cmd.ExecuteReader();

            using (StreamWriter fs = new StreamWriter(xfile))
            {
                // Loop through the fields and add headers
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    string name = dr.GetName(i);
                    if (name.Contains(","))
                        name = string.Format("\"{0}\"", name);

                    fs.Write(name + ",");
                }
                fs.WriteLine();

                // Loop through the rows and output the data
                while (dr.Read())
                {
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(","))
                            value = string.Format("\"{0}\"", value);

                        fs.Write(value + ",");
                    }
                    fs.WriteLine();
                }

                fs.Close();
            }

            System.Diagnostics.Process.Start("notepad.exe", xfile); MessageBox.Show("This is a Current LIVE Employee List! If you save your changes and UPLOAD, it will replace the current list");
            lblstat.Text = "File saved as: C:\\sql\\Sheet1.csv ";
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            lblstat.Text = "Processing File...";
        }

        private void txtFirstName_EditValueChanged(object sender, EventArgs e)
        {
            NemployeeFName = txtFirstName.Text;
        }

        private void txtLastName_EditValueChanged(object sender, EventArgs e)
        {
            NemployeeLName = txtLastName.Text;
        }

        private void txtJobTitle_EditValueChanged(object sender, EventArgs e)
        {
            NemployeeTitle = txtJobTitle.Text;
        }

        private void txtBoss_EditValueChanged(object sender, EventArgs e)
        {

            NemployeeSupervisorName = txtBoss.Text;

        }

        private void txtDeptID_EditValueChanged(object sender, EventArgs e)
        {
            NemployeeDeptID = txtDeptID.Text;
        }

        private void txtLOC_EditValueChanged(object sender, EventArgs e)
        {
            NemployeeLOC = txtLOC.Text;
        }

        private void txtEmployeeID_EditValueChanged(object sender, EventArgs e)
        {
            NemployeeID = txtEmployeeID.Text;
        }

        private void txtDate_EditValueChanged(object sender, EventArgs e)
        {
            NemployeeHireDate = txtDate.Text;
        }

        

        private void txtBoss_SelectedIndexChanged(object sender, EventArgs e)
        {
            NemployeeSupervisorName = txtBoss.Text;
        }

        private void radioGroup1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (radioGroup1.SelectedIndex == 0) { EMPLOYEETYPE = "F"; }
            if (radioGroup1.SelectedIndex == 1) { EMPLOYEETYPE = "P"; }
            if (radioGroup1.SelectedIndex == 2) { EMPLOYEETYPE = "T"; }
            
        }

        private void radioGroup2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (radioGroup2.SelectedIndex == 0) { LOCAREA = "US"; txtLOC.Text = "36"; }
            if (radioGroup2.SelectedIndex == 1) { LOCAREA = "CHINA"; txtLOC.Text = "16"; }
        }

        private void checkArea()
        {
            try
            {
                if (Convert.ToInt32(employeeID) > 80000) { LOCAREA = "16";  radioGroup2.EditValue = 1; ETRAV = ConnStr1; txtLOC.Text = LOCAREA; radioGroup2.Update(); }
                else { LOCAREA = "36"; radioGroup2.EditValue = 0; ETRAV = ConnStr; txtLOC.Text = LOCAREA; radioGroup2.Update(); }
            }
            catch { MessageBox.Show("EmployeeID can not be Null!"); }
        }

        public void CheckGetUpdates(Version cver)
        {

            SqlConnection SqlConn2 = new SqlConnection(ConnStr);
            SqlConn2.Open();
            SqlCommand SelectCommand1 = new SqlCommand(string.Format("select FileVersion from images.dbo.tblVSapplications where Applicationname = '{0}'", CallingAppName), SqlConn2);
            SqlDataReader myreader1 = SelectCommand1.ExecuteReader();

            while (myreader1.Read())
            {
                serverver = string.Format("{0}", myreader1.GetString(0));
                bool result = serverver.Equals(cver.ToString(), StringComparison.Ordinal);
                if (result == false)
                { MessageBox.Show("There is a newer version available!:" + serverver); lblVersion.Text = "Version Out Of Date!"; lblVersion.ForeColor = Color.Red; Getlatest(); }//Getlatest();

            }
            myreader1.Close();
            return;
        }
        public void Getlatest()
        {

            string value = string.Format("Yes UpDate Now to: {0} , Select OK", serverver);

            //check if latest update.dll and devXpress pack is in sql folder
            CheckPack();

            if (InputBox("Yes UpDate Now, Select OK?", "Yes UpDate Now, Select OK", ref value) == DialogResult.OK)
            {
                //updater gogetit = new updater();
                //gogetit.getupdateApp(taskName, CallingFormfrx, null);
                //call dll  getupdateApp("CallingAppName", "CallingFormfrx", null);
                FileStream fP;                          // Writes the BLOB to a file (*.bmp).
                BinaryWriter bw;                        // Streams the BLOB to the FileStream object.
                const int bufferSize = 1000;                   // Size of the BLOB buffer.
                byte[] outbyte = new byte[bufferSize];  // The BLOB byte[] buffer to be filled by GetBytes.
                long retval;                            // The bytes returned from GetBytes.
                long startIndex = 0;                    // The starting position in the BLOB output.
                bool result = false;



                using (SqlConnection SqlConn = new SqlConnection(ConnStr))
                using (SqlCommand command = SqlConn.CreateCommand())
                {
                    //varPathToNewLocation = userRoot;
                    SqlConn.Open();
                    command.Parameters.AddWithValue("@varUP", CallingAppName);
                    command.CommandText = string.Format("SELECT VSApplication FROM images.dbo.tblVSapplications WHERE Applicationname='{0}'", CallingAppName);
                    using (SqlDataReader sqlQueryResult = command.ExecuteReader(CommandBehavior.SequentialAccess))

                        while (sqlQueryResult.Read())
                        {
                            //fP = new FileStream(sqlQueryResult.ToString(), FileMode.OpenOrCreate, FileAccess.Write);
                            try
                            {


                                fP = new FileStream(string.Format("C:\\Sql\\{0}", CallingAppName), FileMode.OpenOrCreate, FileAccess.Write);
                                result = true;
                            }
                            catch (IOException)
                            {

                                return;
                            }
                            bw = new BinaryWriter(fP);
                            startIndex = 0;
                            retval = sqlQueryResult.GetBytes(0, startIndex, outbyte, 0, bufferSize);
                            // Continue reading and writing while there are bytes beyond the size of the buffer.
                            while (retval == bufferSize)
                            {
                                bw.Write(outbyte);
                                bw.Flush();

                                // Reposition the start index to the end of the last buffer and fill the buffer.
                                startIndex += bufferSize;
                                retval = sqlQueryResult.GetBytes(0, startIndex, outbyte, 0, bufferSize);
                            }

                            // Write the remaining buffer.
                            bw.Write(outbyte, 0, (int)retval);
                            bw.Flush();

                            // Close the output file.
                            bw.Close();
                            fP.Close();
                            //checkedListBox1.SetItemChecked(0, true);



                        }
                }
            }
            else return;
        }

        public bool CheckPack()//update to download devxpress pack needed.
        {
            bool result = false;
            string CP = "Updater";
            bool unzipactive = true;
            string startPath = @"C:\Sql\DevExpress16.2\start";
            string zipPath = @"C:\Sql\DevExpress16.2\result.zip";
            string extractPath = @"C:\Sql\DevExpress16.2\extract";

            if (File.Exists(@"C:\Sql\updater.dll") == true && File.Exists(@"C:\Sql\DevExpress16.2.zip")== true)
            {
                result = true;
            }
            else
            {
                if (File.Exists(@"C:\Sql\DevExpress16.2.zip") != true) { CP = devXpresspack; unzipactive = true; }
                if (File.Exists(@"C:\Sql\updater.dll") != true) { CP = "Updater"; unzipactive = false; }
                FileStream fP;                          // Writes the BLOB to a file (*.bmp).
                BinaryWriter bw;                        // Streams the BLOB to the FileStream object.
                const int bufferSize = 1000;                   // Size of the BLOB buffer.
                byte[] outbyte = new byte[bufferSize];  // The BLOB byte[] buffer to be filled by GetBytes.
                long retval;                            // The bytes returned from GetBytes.
                long startIndex = 0;                    // The starting position in the BLOB output.



                using (SqlConnection SqlConn = new SqlConnection(ConnStr))
                using (SqlCommand command = SqlConn.CreateCommand())
                {
                    //varPathToNewLocation = userRoot;
                    SqlConn.Open();
                    command.Parameters.AddWithValue("@varDLL", CP);
                    command.CommandText = string.Format("SELECT VSApplication FROM images.dbo.tblVSapplications WHERE Applicationname='{0}'", CP );
                    using (SqlDataReader sqlQueryResult = command.ExecuteReader(CommandBehavior.SequentialAccess))

                        while (sqlQueryResult.Read())
                        {
                            //fP = new FileStream(sqlQueryResult.ToString(), FileMode.OpenOrCreate, FileAccess.Write);
                            try
                            {


                                fP = new FileStream(string.Format("C:\\Sql\\{0}", CP), FileMode.OpenOrCreate, FileAccess.Write);
                                result = true;
                            }
                            catch (IOException)
                            {

                                return result;
                            }
                            bw = new BinaryWriter(fP);
                            startIndex = 0;
                            retval = sqlQueryResult.GetBytes(0, startIndex, outbyte, 0, bufferSize);
                            // Continue reading and writing while there are bytes beyond the size of the buffer.
                            while (retval == bufferSize)
                            {
                                bw.Write(outbyte);
                                bw.Flush();

                                // Reposition the start index to the end of the last buffer and fill the buffer.
                                startIndex += bufferSize;
                                retval = sqlQueryResult.GetBytes(0, startIndex, outbyte, 0, bufferSize);
                            }

                            // Write the remaining buffer.
                            bw.Write(outbyte, 0, (int)retval);
                            bw.Flush();

                            // Close the output file.
                            bw.Close();
                            fP.Close();
                            //checkedListBox1.SetItemChecked(0, true);



                        }
                }
            }
            if(unzipactive == true) {
                System.IO.Compression.ZipFile.CreateFromDirectory(startPath, zipPath);
                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);
            }
            return result;

        }

        private void txtMiddleName_EditValueChanged(object sender, EventArgs e)
        {
             NemployeeMName= txtMiddleName.Text;
    }

        
    }
}

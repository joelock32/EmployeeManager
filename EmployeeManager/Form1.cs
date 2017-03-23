using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors.Controls;
using static Elogon256.sqlConns;
using DevExpress.DataAccess.Excel;


namespace EmployeeManager
{
    public partial class Form1 : Form
    {
        static string CallingAppName = "EmployeeManager.exe"; //the name of this app used for login dll
        const String ConnStr = "Data Source=etrav-hack;Initial Catalog=qcrr;Persist Security Info=True;User ID=Application;Password=noitacilppa";
        public string LoggedinEmployeeID { get; private set; }
        private bool loggedIN;
        private string employeeID;
        private string employeeName;
        private int rights;
        private int authorizedlevel;
        public string xfile;
        public bool state { get; private set; }

        public Form1()
        {
            ConfirmUser();
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
            string[] itemDescriptions2 = new string[] { "US", "CHINA"};
            for (int j = 0; j < itemValues2.Length; j++)
            {
                radioGroup2.Properties.Items.Add(new RadioGroupItem(itemValues2[j], itemDescriptions2[j]));
            }

            
        }

        private void simpleButton1_Click(object sender, EventArgs e)//ADD ONE NEW EMPLOYEE
        {
            //Check Data

            //ifOK Upload new employee

            //else error message

        }

        private void simpleButton2_Click(object sender, EventArgs e)//DELETE ONE CURRENT EMPLOYEE
        {
            //Check Data
            
            //ifOK delete employee
           
            //else error message

        }

        private void simpleButton3_Click(object sender, EventArgs e)//UPLOAD EMPLOYEE FILE(XLS,CVS)
        {
            ExcelDataSource excelDataSource1 = new ExcelDataSource() { FileName = xfile };
            //OPEN DIALOG BOX
            DialogResult result = openFileDialog1.ShowDialog();
            //Check Data

            //ifOK Upload UPDATED EMPLOYEE LIST
            if (result == DialogResult.OK) // Test result.
            {
                xfile = openFileDialog1.FileName;
                excelDataSource1.FileName = xfile;
                //DevExpress.DataAccess.Excel.ExcelDataSource myExcelSource = new DevExpress.DataAccess.Excel.ExcelDataSource();
                //myExcelSource.FileName = xfile;
                //hyperlinkLabelControl1.Text = string.Format("ADP FILE: {0} is Loaded!", xfile);

            }
            Console.WriteLine(result); // <-- For debugging use.

            
            ExcelSourceOptions myOptions = new ExcelSourceOptions();
            ExcelWorksheetSettings cellRangeSettings = new ExcelWorksheetSettings("sheet1");
            myOptions.ImportSettings = cellRangeSettings;
            excelDataSource1.SourceOptions = myOptions;
            //gridControl1.DataSource = excelDataSource1;

            //else error message

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
                //txtAuthor.Text = employeeID;

            }
            //make input box asking for password
            string value2 = "Password";

            if (InputBoxPass("Password?", "Please Enter your Admin Password or Cancel to setup new user:", ref value2) == DialogResult.OK)
            {

                getlogon getpass = new getlogon();
                //state = getpass.getauthpassword(value2,employeeName, employeeID,false);
                state = getpass.getfulllog(employeeName, employeeID, CallingAppName, true, value2, false);
                if (state == true) { getpass.getpersistant(employeeID);  } //set persistant
                if (state == false) { MessageBox.Show("Password not accepted!");  }//return state
                //if (state == true && employeeID == "07840" || employeeID == "06539") { btnbarManagment.Enabled = true; simpleButton6.Enabled = true; }

            }
            
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

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;
            textBox.PasswordChar = '%';

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

        private void hyperLinkEdit1_OpenLink(object sender, OpenLinkEventArgs e)//Open US/China xls sheet
        {
            //open "C:\\sql\\US Employee Information Book.xlsx"
            //Application excel;
           




        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;

namespace DMR
{
	public partial class frmDownloadContacts : Form
	{
		public ContactsForm parentForm;

		public frmDownloadContacts()
		{
			InitializeComponent();
		}

		private void addPrivateContact(string id,string callsignAndName)
		{
			int minIndex = ContactForm.data.GetMinIndex();
			ContactForm.data.SetIndex(minIndex, 1);
			ContactForm.ContactOne value = new ContactForm.ContactOne(minIndex);
			value.Name = callsignAndName;
			value.CallId = string.Format("{0:d8}", int.Parse(id));
			value.CallTypeS = ContactForm.SZ_CALL_TYPE[1];// Private call 
			value.RingStyleS = ContactForm.DefaultContact.RingStyleS;
			value.CallRxToneS = ContactForm.SZ_CALL_RX_TONE[0];// Call tone off
			ContactForm.data[minIndex] = value;

			int[] array = new int[3] {8,10,7};// Note array index 1 appears to be Private call in terms of the tree view

			(parentForm.MdiParent as MainForm).InsertTreeViewNode(parentForm.Node, minIndex, typeof(ContactForm), array[1], ContactForm.data);


		}

		private void btnDownload_Click(object sender, EventArgs e)
		{
			if (txtIDStart.Text == "" || int.Parse(txtIDStart.Text) == 0)
			{
				MessageBox.Show("Please enter ID number starting digits. e.g. 505 for Australia.");
				return;
			}
			lblMessage.Text = "Downloading";
			this.Refresh();
			WebClient wc = new WebClient();
			string str = wc.DownloadString("https://ham-digital.org/user_by_lh.php?id=" + txtIDStart.Text + "&cnt=1000");
			lblMessage.Text = "Parsing";
			this.Refresh();

			dataGridView1.SuspendLayout();
			string[] linesArr = str.Split('\n');
			string[] lineArr;
			bool found;
			
			for (int i = linesArr.Length - 2; i >1; i--)
			{

				lineArr = linesArr[i].Split(';');
				found = false;

				for (int j = 0; j < ContactForm.data.Count; j++)
				{
					if (ContactForm.data.DataIsValid(j))
					{
						if (int.Parse(ContactForm.data[j].CallId) == int.Parse(lineArr[2]))
						{
							found = true;
							break;
						}
					}
				}
				if (found == false)
				{
					this.dataGridView1.Rows.Insert(0, lineArr[2], lineArr[1], lineArr[3], lineArr[4]);
				}
			}
			lblMessage.Text = "Added " + this.dataGridView1.RowCount;
			dataGridView1.ResumeLayout();
		}

		private void btnImport_Click(object sender, EventArgs e)
		{
			if (this.dataGridView1.SelectedRows.Count == 0)
			{
				MessageBox.Show("Please select the contacts you would like to import");
			}
			else
			{
				foreach (DataGridViewRow row in this.dataGridView1.SelectedRows)
				{
					addPrivateContact(row.Cells[0].Value+"", row.Cells[1].Value + " " + row.Cells[2].Value);
				}
				parentForm.DispData();
				(parentForm.MdiParent as MainForm).RefreshRelatedForm(base.GetType());
				this.Close();
			}

		}
	}
}
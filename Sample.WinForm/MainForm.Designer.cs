namespace Sample.WinForm
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Print = new System.Windows.Forms.Button();
			this.Colors = new System.Windows.Forms.ListBox();
			this.exceptionNullReference = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.innerException = new System.Windows.Forms.Button();
			this.fileNotFoundException = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.randomTimedEvent = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// Print
			// 
			this.Print.Location = new System.Drawing.Point(109, 113);
			this.Print.Name = "Print";
			this.Print.Size = new System.Drawing.Size(75, 23);
			this.Print.TabIndex = 0;
			this.Print.Text = "Print";
			this.Print.UseVisualStyleBackColor = true;
			this.Print.Click += new System.EventHandler(this.Print_Click);
			// 
			// Colors
			// 
			this.Colors.FormattingEnabled = true;
			this.Colors.Items.AddRange(new object[] {
            "Red",
            "Green",
            "Blue",
            "Yellow",
            "Black"});
			this.Colors.Location = new System.Drawing.Point(12, 12);
			this.Colors.Name = "Colors";
			this.Colors.Size = new System.Drawing.Size(172, 95);
			this.Colors.TabIndex = 1;
			// 
			// exceptionNullReference
			// 
			this.exceptionNullReference.Location = new System.Drawing.Point(26, 29);
			this.exceptionNullReference.Name = "exceptionNullReference";
			this.exceptionNullReference.Size = new System.Drawing.Size(145, 23);
			this.exceptionNullReference.TabIndex = 2;
			this.exceptionNullReference.Text = "Null Reference";
			this.exceptionNullReference.UseVisualStyleBackColor = true;
			this.exceptionNullReference.Click += new System.EventHandler(this.exceptionNullReference_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.innerException);
			this.groupBox1.Controls.Add(this.fileNotFoundException);
			this.groupBox1.Controls.Add(this.exceptionNullReference);
			this.groupBox1.Location = new System.Drawing.Point(206, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(192, 124);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Exceptions";
			// 
			// innerException
			// 
			this.innerException.Location = new System.Drawing.Point(26, 87);
			this.innerException.Name = "innerException";
			this.innerException.Size = new System.Drawing.Size(145, 23);
			this.innerException.TabIndex = 4;
			this.innerException.Text = "Inner Exception";
			this.innerException.UseVisualStyleBackColor = true;
			this.innerException.Click += new System.EventHandler(this.innerException_Click);
			// 
			// fileNotFoundException
			// 
			this.fileNotFoundException.Location = new System.Drawing.Point(26, 58);
			this.fileNotFoundException.Name = "fileNotFoundException";
			this.fileNotFoundException.Size = new System.Drawing.Size(145, 23);
			this.fileNotFoundException.TabIndex = 3;
			this.fileNotFoundException.Text = "File Not Found";
			this.fileNotFoundException.UseVisualStyleBackColor = true;
			this.fileNotFoundException.Click += new System.EventHandler(this.fileNotFoundException_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.randomTimedEvent);
			this.groupBox2.Location = new System.Drawing.Point(206, 142);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(192, 100);
			this.groupBox2.TabIndex = 4;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Timed Event";
			// 
			// randomTimedEvent
			// 
			this.randomTimedEvent.Location = new System.Drawing.Point(26, 29);
			this.randomTimedEvent.Name = "randomTimedEvent";
			this.randomTimedEvent.Size = new System.Drawing.Size(145, 23);
			this.randomTimedEvent.TabIndex = 5;
			this.randomTimedEvent.Text = "Random";
			this.randomTimedEvent.UseVisualStyleBackColor = true;
			this.randomTimedEvent.Click += new System.EventHandler(this.randomTimedEvent_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(410, 311);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.Colors);
			this.Controls.Add(this.Print);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "MainForm";
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button Print;
		private System.Windows.Forms.ListBox Colors;
		private System.Windows.Forms.Button exceptionNullReference;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button fileNotFoundException;
		private System.Windows.Forms.Button innerException;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button randomTimedEvent;
	}
}
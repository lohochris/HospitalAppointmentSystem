<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormPatientManagement
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.panelTop = New System.Windows.Forms.Panel()
        Me.lblTitle = New System.Windows.Forms.Label()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.panelLeft = New System.Windows.Forms.Panel()
        Me.grpPatientInfo = New System.Windows.Forms.GroupBox()
        Me.dtpDOB = New System.Windows.Forms.DateTimePicker()
        Me.cmbGender = New System.Windows.Forms.ComboBox()
        Me.txtEmail = New System.Windows.Forms.TextBox()
        Me.txtPhone = New System.Windows.Forms.TextBox()
        Me.txtLastName = New System.Windows.Forms.TextBox()
        Me.txtFirstName = New System.Windows.Forms.TextBox()
        Me.lblEmail = New System.Windows.Forms.Label()
        Me.lblPhone = New System.Windows.Forms.Label()
        Me.lblGender = New System.Windows.Forms.Label()
        Me.lblDOB = New System.Windows.Forms.Label()
        Me.lblLastName = New System.Windows.Forms.Label()
        Me.lblFirstName = New System.Windows.Forms.Label()
        Me.lblPatientID = New System.Windows.Forms.Label()
        Me.txtPatientID = New System.Windows.Forms.TextBox()
        Me.panelButtons = New System.Windows.Forms.Panel()
        Me.btnDelete = New System.Windows.Forms.Button()
        Me.btnClear = New System.Windows.Forms.Button()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.panelRight = New System.Windows.Forms.Panel()
        Me.dgvPatients = New System.Windows.Forms.DataGridView()
        Me.panelSearch = New System.Windows.Forms.Panel()
        Me.txtSearch = New System.Windows.Forms.TextBox()
        Me.lblSearch = New System.Windows.Forms.Label()
        Me.lblRecordCount = New System.Windows.Forms.Label()
        Me.panelVitals = New System.Windows.Forms.Panel()
        Me.btnRecordVitals = New System.Windows.Forms.Button()
        Me.btnViewVitals = New System.Windows.Forms.Button()
        Me.panelTop.SuspendLayout()
        Me.panelLeft.SuspendLayout()
        Me.grpPatientInfo.SuspendLayout()
        Me.panelButtons.SuspendLayout()
        Me.panelRight.SuspendLayout()
        CType(Me.dgvPatients, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.panelSearch.SuspendLayout()
        Me.panelVitals.SuspendLayout()
        Me.SuspendLayout()
        '
        'panelTop
        '
        Me.panelTop.BackColor = System.Drawing.Color.FromArgb(CType(CType(41, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(185, Byte), Integer))
        Me.panelTop.Controls.Add(Me.lblTitle)
        Me.panelTop.Controls.Add(Me.btnClose)
        Me.panelTop.Dock = System.Windows.Forms.DockStyle.Top
        Me.panelTop.Location = New System.Drawing.Point(0, 0)
        Me.panelTop.Name = "panelTop"
        Me.panelTop.Size = New System.Drawing.Size(1200, 60)
        Me.panelTop.TabIndex = 0
        '
        'lblTitle
        '
        Me.lblTitle.AutoSize = True
        Me.lblTitle.Font = New System.Drawing.Font("Segoe UI", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTitle.ForeColor = System.Drawing.Color.White
        Me.lblTitle.Location = New System.Drawing.Point(12, 13)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Size = New System.Drawing.Size(255, 32)
        Me.lblTitle.TabIndex = 0
        Me.lblTitle.Text = "Patient Management"
        '
        'btnClose
        '
        Me.btnClose.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnClose.BackColor = System.Drawing.Color.FromArgb(CType(CType(231, Byte), Integer), CType(CType(76, Byte), Integer), CType(CType(60, Byte), Integer))
        Me.btnClose.FlatAppearance.BorderSize = 0
        Me.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnClose.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnClose.ForeColor = System.Drawing.Color.White
        Me.btnClose.Location = New System.Drawing.Point(1140, 10)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(48, 40)
        Me.btnClose.TabIndex = 1
        Me.btnClose.Text = "✕"
        Me.btnClose.UseVisualStyleBackColor = False
        '
        'panelLeft
        '
        Me.panelLeft.BackColor = System.Drawing.Color.White
        Me.panelLeft.Controls.Add(Me.grpPatientInfo)
        Me.panelLeft.Controls.Add(Me.panelButtons)
        Me.panelLeft.Dock = System.Windows.Forms.DockStyle.Left
        Me.panelLeft.Location = New System.Drawing.Point(0, 60)
        Me.panelLeft.Name = "panelLeft"
        Me.panelLeft.Padding = New System.Windows.Forms.Padding(10)
        Me.panelLeft.Size = New System.Drawing.Size(380, 640)
        Me.panelLeft.TabIndex = 1
        '
        'grpPatientInfo
        '
        Me.grpPatientInfo.Controls.Add(Me.dtpDOB)
        Me.grpPatientInfo.Controls.Add(Me.cmbGender)
        Me.grpPatientInfo.Controls.Add(Me.txtEmail)
        Me.grpPatientInfo.Controls.Add(Me.txtPhone)
        Me.grpPatientInfo.Controls.Add(Me.txtLastName)
        Me.grpPatientInfo.Controls.Add(Me.txtFirstName)
        Me.grpPatientInfo.Controls.Add(Me.lblEmail)
        Me.grpPatientInfo.Controls.Add(Me.lblPhone)
        Me.grpPatientInfo.Controls.Add(Me.lblGender)
        Me.grpPatientInfo.Controls.Add(Me.lblDOB)
        Me.grpPatientInfo.Controls.Add(Me.lblLastName)
        Me.grpPatientInfo.Controls.Add(Me.lblFirstName)
        Me.grpPatientInfo.Controls.Add(Me.lblPatientID)
        Me.grpPatientInfo.Controls.Add(Me.txtPatientID)
        Me.grpPatientInfo.Dock = System.Windows.Forms.DockStyle.Fill
        Me.grpPatientInfo.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpPatientInfo.Location = New System.Drawing.Point(10, 10)
        Me.grpPatientInfo.Name = "grpPatientInfo"
        Me.grpPatientInfo.Padding = New System.Windows.Forms.Padding(10)
        Me.grpPatientInfo.Size = New System.Drawing.Size(360, 540)
        Me.grpPatientInfo.TabIndex = 0
        Me.grpPatientInfo.TabStop = False
        Me.grpPatientInfo.Text = "Patient Information"
        '
        'dtpDOB
        '
        Me.dtpDOB.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.dtpDOB.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.dtpDOB.Location = New System.Drawing.Point(20, 295)
        Me.dtpDOB.Name = "dtpDOB"
        Me.dtpDOB.Size = New System.Drawing.Size(320, 25)
        Me.dtpDOB.TabIndex = 7
        '
        'cmbGender
        '
        Me.cmbGender.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbGender.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbGender.FormattingEnabled = True
        Me.cmbGender.Items.AddRange(New Object() {"Male", "Female", "Other"})
        Me.cmbGender.Location = New System.Drawing.Point(20, 355)
        Me.cmbGender.Name = "cmbGender"
        Me.cmbGender.Size = New System.Drawing.Size(320, 25)
        Me.cmbGender.TabIndex = 9
        '
        'txtEmail
        '
        Me.txtEmail.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtEmail.Location = New System.Drawing.Point(20, 475)
        Me.txtEmail.Name = "txtEmail"
        Me.txtEmail.Size = New System.Drawing.Size(320, 25)
        Me.txtEmail.TabIndex = 13
        '
        'txtPhone
        '
        Me.txtPhone.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtPhone.Location = New System.Drawing.Point(20, 415)
        Me.txtPhone.MaxLength = 15
        Me.txtPhone.Name = "txtPhone"
        Me.txtPhone.Size = New System.Drawing.Size(320, 25)
        Me.txtPhone.TabIndex = 11
        '
        'txtLastName
        '
        Me.txtLastName.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtLastName.Location = New System.Drawing.Point(20, 235)
        Me.txtLastName.MaxLength = 100
        Me.txtLastName.Name = "txtLastName"
        Me.txtLastName.Size = New System.Drawing.Size(320, 25)
        Me.txtLastName.TabIndex = 5
        '
        'txtFirstName
        '
        Me.txtFirstName.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtFirstName.Location = New System.Drawing.Point(20, 175)
        Me.txtFirstName.MaxLength = 100
        Me.txtFirstName.Name = "txtFirstName"
        Me.txtFirstName.Size = New System.Drawing.Size(320, 25)
        Me.txtFirstName.TabIndex = 3
        '
        'lblEmail
        '
        Me.lblEmail.AutoSize = True
        Me.lblEmail.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblEmail.Location = New System.Drawing.Point(17, 453)
        Me.lblEmail.Name = "lblEmail"
        Me.lblEmail.Size = New System.Drawing.Size(39, 15)
        Me.lblEmail.TabIndex = 12
        Me.lblEmail.Text = "Email:"
        '
        'lblPhone
        '
        Me.lblPhone.AutoSize = True
        Me.lblPhone.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPhone.ForeColor = System.Drawing.Color.Red
        Me.lblPhone.Location = New System.Drawing.Point(17, 393)
        Me.lblPhone.Name = "lblPhone"
        Me.lblPhone.Size = New System.Drawing.Size(97, 15)
        Me.lblPhone.TabIndex = 10
        Me.lblPhone.Text = "Phone Number:*"
        '
        'lblGender
        '
        Me.lblGender.AutoSize = True
        Me.lblGender.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblGender.Location = New System.Drawing.Point(17, 333)
        Me.lblGender.Name = "lblGender"
        Me.lblGender.Size = New System.Drawing.Size(48, 15)
        Me.lblGender.TabIndex = 8
        Me.lblGender.Text = "Gender:"
        '
        'lblDOB
        '
        Me.lblDOB.AutoSize = True
        Me.lblDOB.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDOB.Location = New System.Drawing.Point(17, 273)
        Me.lblDOB.Name = "lblDOB"
        Me.lblDOB.Size = New System.Drawing.Size(76, 15)
        Me.lblDOB.TabIndex = 6
        Me.lblDOB.Text = "Date of Birth:"
        '
        'lblLastName
        '
        Me.lblLastName.AutoSize = True
        Me.lblLastName.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLastName.ForeColor = System.Drawing.Color.Red
        Me.lblLastName.Location = New System.Drawing.Point(17, 213)
        Me.lblLastName.Name = "lblLastName"
        Me.lblLastName.Size = New System.Drawing.Size(71, 15)
        Me.lblLastName.TabIndex = 4
        Me.lblLastName.Text = "Last Name:*"
        '
        'lblFirstName
        '
        Me.lblFirstName.AutoSize = True
        Me.lblFirstName.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblFirstName.ForeColor = System.Drawing.Color.Red
        Me.lblFirstName.Location = New System.Drawing.Point(17, 153)
        Me.lblFirstName.Name = "lblFirstName"
        Me.lblFirstName.Size = New System.Drawing.Size(72, 15)
        Me.lblFirstName.TabIndex = 2
        Me.lblFirstName.Text = "First Name:*"
        '
        'lblPatientID
        '
        Me.lblPatientID.AutoSize = True
        Me.lblPatientID.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPatientID.Location = New System.Drawing.Point(17, 33)
        Me.lblPatientID.Name = "lblPatientID"
        Me.lblPatientID.Size = New System.Drawing.Size(63, 15)
        Me.lblPatientID.TabIndex = 0
        Me.lblPatientID.Text = "Patient ID:"
        '
        'txtPatientID
        '
        Me.txtPatientID.BackColor = System.Drawing.Color.WhiteSmoke
        Me.txtPatientID.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtPatientID.Location = New System.Drawing.Point(20, 55)
        Me.txtPatientID.Name = "txtPatientID"
        Me.txtPatientID.ReadOnly = True
        Me.txtPatientID.Size = New System.Drawing.Size(320, 25)
        Me.txtPatientID.TabIndex = 1
        Me.txtPatientID.Text = "[Auto-Generated]"
        '
        'panelButtons
        '
        Me.panelButtons.Controls.Add(Me.btnDelete)
        Me.panelButtons.Controls.Add(Me.btnClear)
        Me.panelButtons.Controls.Add(Me.btnSave)
        Me.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.panelButtons.Location = New System.Drawing.Point(10, 550)
        Me.panelButtons.Name = "panelButtons"
        Me.panelButtons.Size = New System.Drawing.Size(360, 80)
        Me.panelButtons.TabIndex = 1
        '
        'btnDelete
        '
        Me.btnDelete.BackColor = System.Drawing.Color.FromArgb(CType(CType(231, Byte), Integer), CType(CType(76, Byte), Integer), CType(CType(60, Byte), Integer))
        Me.btnDelete.FlatAppearance.BorderSize = 0
        Me.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnDelete.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnDelete.ForeColor = System.Drawing.Color.White
        Me.btnDelete.Location = New System.Drawing.Point(245, 20)
        Me.btnDelete.Name = "btnDelete"
        Me.btnDelete.Size = New System.Drawing.Size(100, 45)
        Me.btnDelete.TabIndex = 2
        Me.btnDelete.Text = "🗑 Delete"
        Me.btnDelete.UseVisualStyleBackColor = False
        '
        'btnClear
        '
        Me.btnClear.BackColor = System.Drawing.Color.FromArgb(CType(CType(149, Byte), Integer), CType(CType(165, Byte), Integer), CType(CType(166, Byte), Integer))
        Me.btnClear.FlatAppearance.BorderSize = 0
        Me.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnClear.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnClear.ForeColor = System.Drawing.Color.White
        Me.btnClear.Location = New System.Drawing.Point(130, 20)
        Me.btnClear.Name = "btnClear"
        Me.btnClear.Size = New System.Drawing.Size(100, 45)
        Me.btnClear.TabIndex = 1
        Me.btnClear.Text = "🔄 Clear"
        Me.btnClear.UseVisualStyleBackColor = False
        '
        'btnSave
        '
        Me.btnSave.BackColor = System.Drawing.Color.FromArgb(CType(CType(39, Byte), Integer), CType(CType(174, Byte), Integer), CType(CType(96, Byte), Integer))
        Me.btnSave.FlatAppearance.BorderSize = 0
        Me.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnSave.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnSave.ForeColor = System.Drawing.Color.White
        Me.btnSave.Location = New System.Drawing.Point(15, 20)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(100, 45)
        Me.btnSave.TabIndex = 0
        Me.btnSave.Text = "💾 Save"
        Me.btnSave.UseVisualStyleBackColor = False
        '
        'panelRight
        '
        Me.panelRight.BackColor = System.Drawing.Color.White
        Me.panelRight.Controls.Add(Me.dgvPatients)
        Me.panelRight.Controls.Add(Me.panelVitals)
        Me.panelRight.Controls.Add(Me.panelSearch)
        Me.panelRight.Dock = System.Windows.Forms.DockStyle.Fill
        Me.panelRight.Location = New System.Drawing.Point(380, 60)
        Me.panelRight.Name = "panelRight"
        Me.panelRight.Padding = New System.Windows.Forms.Padding(10)
        Me.panelRight.Size = New System.Drawing.Size(820, 640)
        Me.panelRight.TabIndex = 2
        '
        'dgvPatients
        '
        Me.dgvPatients.AllowUserToAddRows = False
        Me.dgvPatients.AllowUserToDeleteRows = False
        Me.dgvPatients.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.dgvPatients.BackgroundColor = System.Drawing.Color.White
        Me.dgvPatients.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.dgvPatients.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvPatients.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvPatients.Location = New System.Drawing.Point(10, 80)
        Me.dgvPatients.MultiSelect = False
        Me.dgvPatients.Name = "dgvPatients"
        Me.dgvPatients.ReadOnly = True
        Me.dgvPatients.RowHeadersWidth = 51
        Me.dgvPatients.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.dgvPatients.Size = New System.Drawing.Size(800, 480)
        Me.dgvPatients.TabIndex = 1
        '
        'panelVitals
        '
        Me.panelVitals.Controls.Add(Me.btnRecordVitals)
        Me.panelVitals.Controls.Add(Me.btnViewVitals)
        Me.panelVitals.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.panelVitals.Location = New System.Drawing.Point(10, 560)
        Me.panelVitals.Name = "panelVitals"
        Me.panelVitals.Size = New System.Drawing.Size(800, 70)
        Me.panelVitals.TabIndex = 2
        '
        'btnRecordVitals
        '
        Me.btnRecordVitals.BackColor = System.Drawing.Color.FromArgb(CType(CType(39, Byte), Integer), CType(CType(174, Byte), Integer), CType(CType(96, Byte), Integer))
        Me.btnRecordVitals.FlatAppearance.BorderSize = 0
        Me.btnRecordVitals.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnRecordVitals.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnRecordVitals.ForeColor = System.Drawing.Color.White
        Me.btnRecordVitals.Location = New System.Drawing.Point(15, 15)
        Me.btnRecordVitals.Name = "btnRecordVitals"
        Me.btnRecordVitals.Size = New System.Drawing.Size(180, 45)
        Me.btnRecordVitals.TabIndex = 0
        Me.btnRecordVitals.Text = "📋 Record Vitals"
        Me.btnRecordVitals.UseVisualStyleBackColor = False
        '
        'btnViewVitals
        '
        Me.btnViewVitals.BackColor = System.Drawing.Color.FromArgb(CType(CType(52, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(219, Byte), Integer))
        Me.btnViewVitals.FlatAppearance.BorderSize = 0
        Me.btnViewVitals.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnViewVitals.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnViewVitals.ForeColor = System.Drawing.Color.White
        Me.btnViewVitals.Location = New System.Drawing.Point(210, 15)
        Me.btnViewVitals.Name = "btnViewVitals"
        Me.btnViewVitals.Size = New System.Drawing.Size(180, 45)
        Me.btnViewVitals.TabIndex = 1
        Me.btnViewVitals.Text = "📊 View Vitals History"
        Me.btnViewVitals.UseVisualStyleBackColor = False
        '
        'panelSearch
        '
        Me.panelSearch.Controls.Add(Me.txtSearch)
        Me.panelSearch.Controls.Add(Me.lblSearch)
        Me.panelSearch.Controls.Add(Me.lblRecordCount)
        Me.panelSearch.Dock = System.Windows.Forms.DockStyle.Top
        Me.panelSearch.Location = New System.Drawing.Point(10, 10)
        Me.panelSearch.Name = "panelSearch"
        Me.panelSearch.Size = New System.Drawing.Size(800, 70)
        Me.panelSearch.TabIndex = 0
        '
        'txtSearch
        '
        Me.txtSearch.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtSearch.Location = New System.Drawing.Point(15, 30)
        Me.txtSearch.Name = "txtSearch"
        Me.txtSearch.Size = New System.Drawing.Size(400, 29)
        Me.txtSearch.TabIndex = 1
        '
        'lblSearch
        '
        Me.lblSearch.AutoSize = True
        Me.lblSearch.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSearch.Location = New System.Drawing.Point(11, 7)
        Me.lblSearch.Name = "lblSearch"
        Me.lblSearch.Size = New System.Drawing.Size(215, 19)
        Me.lblSearch.TabIndex = 0
        Me.lblSearch.Text = "🔍 Search (Name or Phone):"
        '
        'lblRecordCount
        '
        Me.lblRecordCount.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblRecordCount.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblRecordCount.Location = New System.Drawing.Point(450, 30)
        Me.lblRecordCount.Name = "lblRecordCount"
        Me.lblRecordCount.Size = New System.Drawing.Size(335, 25)
        Me.lblRecordCount.TabIndex = 2
        Me.lblRecordCount.Text = "Total Patients: 0"
        Me.lblRecordCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'FormPatientManagement
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1200, 700)
        Me.Controls.Add(Me.panelRight)
        Me.Controls.Add(Me.panelLeft)
        Me.Controls.Add(Me.panelTop)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "FormPatientManagement"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Patient Management"
        Me.panelTop.ResumeLayout(False)
        Me.panelTop.PerformLayout()
        Me.panelLeft.ResumeLayout(False)
        Me.grpPatientInfo.ResumeLayout(False)
        Me.grpPatientInfo.PerformLayout()
        Me.panelButtons.ResumeLayout(False)
        Me.panelRight.ResumeLayout(False)
        CType(Me.dgvPatients, System.ComponentModel.ISupportInitialize).EndInit()
        Me.panelSearch.ResumeLayout(False)
        Me.panelSearch.PerformLayout()
        Me.panelVitals.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents panelTop As Panel
    Friend WithEvents lblTitle As Label
    Friend WithEvents btnClose As Button
    Friend WithEvents panelLeft As Panel
    Friend WithEvents grpPatientInfo As GroupBox
    Friend WithEvents lblPatientID As Label
    Friend WithEvents txtPatientID As TextBox
    Friend WithEvents lblFirstName As Label
    Friend WithEvents txtFirstName As TextBox
    Friend WithEvents lblLastName As Label
    Friend WithEvents txtLastName As TextBox
    Friend WithEvents lblDOB As Label
    Friend WithEvents dtpDOB As DateTimePicker
    Friend WithEvents lblGender As Label
    Friend WithEvents cmbGender As ComboBox
    Friend WithEvents lblPhone As Label
    Friend WithEvents txtPhone As TextBox
    Friend WithEvents lblEmail As Label
    Friend WithEvents txtEmail As TextBox
    Friend WithEvents panelButtons As Panel
    Friend WithEvents btnSave As Button
    Friend WithEvents btnClear As Button
    Friend WithEvents btnDelete As Button
    Friend WithEvents panelRight As Panel
    Friend WithEvents dgvPatients As DataGridView
    Friend WithEvents panelSearch As Panel
    Friend WithEvents lblSearch As Label
    Friend WithEvents txtSearch As TextBox
    Friend WithEvents lblRecordCount As Label
    Friend WithEvents panelVitals As Panel
    Friend WithEvents btnRecordVitals As Button
    Friend WithEvents btnViewVitals As Button
End Class

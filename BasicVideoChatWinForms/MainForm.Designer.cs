using System;

namespace BasicVideoChatWinForms
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "MainForm";

            this.PublisherVideo = new WinFormsRenderer.WinFormsVideoRenderer();           
            this.PublisherVideo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.PublisherVideo.Location = new System.Drawing.Point(600, 300);
            this.PublisherVideo.Margin = new System.Windows.Forms.Padding(12);
            this.PublisherVideo.Name = "PublisherVideo";
            this.PublisherVideo.Size = new System.Drawing.Size(177, 100);
            this.PublisherVideo.TabIndex = 0;
            this.Controls.Add(this.PublisherVideo);

            this.SubscriberVideo = new WinFormsRenderer.WinFormsVideoRenderer();
            this.SubscriberVideo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SubscriberVideo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SubscriberVideo.Location = new System.Drawing.Point(0, 0);
            this.SubscriberVideo.Margin = new System.Windows.Forms.Padding(12);
            this.SubscriberVideo.Name = "SubscriberVideo";
            this.SubscriberVideo.Size = new System.Drawing.Size(800, 400);
            this.SubscriberVideo.TabIndex = 1;
            this.Controls.Add(this.SubscriberVideo);         
        }

        #endregion

        private WinFormsRenderer.WinFormsVideoRenderer PublisherVideo;
        private WinFormsRenderer.WinFormsVideoRenderer SubscriberVideo;
    }
}


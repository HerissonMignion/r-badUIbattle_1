using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace C_FormTest1
{
	public class oRollerDialog
	{
		private Form forme;
		private PictureBox ImageBox;
		private Button btn;


		public string Message
		{
			get { return this.forme.Text; }
			set { this.forme.Text = value; }
		}


		
		public int Height = 400;
		public Font Font = new Font("consolas", 10f);
		public List<string> choices = new List<string>(); //liste des choix

		public string Answer = ""; //variable qui contient la valeur que le dialogue retourne


		public void ShowDialog()
		{


			
			this.RefreshSize();
			//image d'arrière plan
			this.CreateBackImg();
			this.TimerFrame.Start();


			//position de la fenetre
			Point mpos = Cursor.Position;
			this.forme.Top = mpos.Y - (this.forme.Height / 2);
			this.forme.Left = mpos.X - (this.forme.Width / 2);
			if (this.forme.Top < 0) { this.forme.Top = 0; }
			if (this.forme.Left < 0) { this.forme.Left = 0; }

			this.forme.ShowDialog();

		}


		//void new()
		public oRollerDialog()
		{

			this.forme = new Form();
			this.forme.FormBorderStyle = FormBorderStyle.FixedSingle;
			this.forme.MaximizeBox = false;
			this.forme.MinimizeBox = false;
			this.forme.StartPosition = FormStartPosition.Manual;


			this.ImageBox = new PictureBox();
			this.ImageBox.Parent = this.forme;
			this.ImageBox.BorderStyle = BorderStyle.FixedSingle;


			this.btn = new Button();
			this.btn.Parent = this.forme;
			this.btn.Text = "Submit";
			this.btn.Font = new Font("consolas", 30f);
			this.btn.Click += new EventHandler(this.btn_Click);


			
			this.CreateTimer();
			
		}
		private void btn_Click(object sender, EventArgs e)
		{
			int index = this.GetIndexOfActualAngle();

			this.Answer = this.choices[index];

			GC.Collect();
			this.TimerFrame.Stop();
			this.forme.Close();
			
		}


		private void RefreshSize()
		{

			this.forme.Width = this.Height + 200;
			this.forme.Height = Height + 120;


			this.ImageBox.Location = new Point(2, 2);
			this.ImageBox.Width = this.forme.Width - 18 - this.ImageBox.Left;
			this.ImageBox.Height = this.forme.Height - 40 - 70 - this.ImageBox.Top;


			this.btn.Width = this.forme.Width - 19;
			this.btn.Location = new Point(this.ImageBox.Left, this.ImageBox.Top + this.ImageBox.Height + 2);
			this.btn.Height = this.forme.Height - 40 - this.btn.Top;

			
		}






		private Timer TimerFrame;
		private void CreateTimer()
		{
			this.TimerFrame = new Timer();
			this.TimerFrame.Interval = 1; // 5
			this.TimerFrame.Tick += new EventHandler(this.TimerFrame_Tick);
		}
		private void TimerFrame_Tick(object sender, EventArgs e)
		{
			this.ActualAngle += 0.1d; // 0.1d
			if (this.ActualAngle >= 2d * Math.PI) { this.ActualAngle -= 2d * Math.PI; }

			this.RefreshImage();
		}



		//image d'arrière plan. il ne sert à rien de recrée cette image à chaque frame
		private Bitmap backimg;
		private void CreateBackImg()
		{

			int imgWidth = this.ImageBox.Width;
			int imgHeight = this.ImageBox.Height;

			Bitmap img = new Bitmap(this.ImageBox.Width, this.ImageBox.Height);
			Graphics g = Graphics.FromImage(img);
			g.Clear(Color.White);


			//dessine les point et les string associés
			double len = (double)(this.Height / 2) * 0.9d;
			for (int i = 0; i < this.choices.Count; i++)
			{
				double angle = 2d * Math.PI / (double)(this.choices.Count) * (double)i;


				int posx = (imgWidth / 2) + (int)(len * Math.Cos(angle));
				int posy = (imgHeight / 2) - (int)(len * Math.Sin(angle));

				//img.SetPixel(posx, posy, Color.Red);
				int cr = 5; //rayon du cercle
				g.FillEllipse(Brushes.Black, posx - cr, posy - cr, 2 * cr, 2 * cr);


				//draw the string
				string text = this.choices[i];
				bool DrawToTheRight = angle < Math.PI / 2d || angle > 3d * Math.PI / 2d;
				SizeF TextSize = g.MeasureString(text, this.Font);
				if (DrawToTheRight)
				{
					g.DrawString(text, this.Font, Brushes.Black, posx + cr, posy - (int)(TextSize.Height / 2f));
				}
				else
				{
					g.DrawString(text, this.Font, Brushes.Black, posx - cr - (int)(TextSize.Width), posy - (int)(TextSize.Height / 2f));

				}



			}




			g.Dispose();
			this.backimg = img;
		}

		private void RefreshImage()
		{
			//this.ImageBox.Image = this.backimg;


			int imgWidth = this.backimg.Width;
			int imgHeight = this.backimg.Height;

			//Bitmap img = new Bitmap(imgWidth, imgHeight);
			Bitmap img = new Bitmap(this.backimg);
			Graphics g = Graphics.FromImage(img);
			//g.DrawImage(this.backimg, 0, 0);
			double arrowlength = (double)(this.Height / 2) * 0.8d;



			double arrowWidth = arrowlength * Math.Cos(this.ActualAngle);
			double arrowHeight = arrowlength * Math.Sin(this.ActualAngle);
			//dessine la flèche
			g.DrawLine(Pens.Black, imgWidth / 2, imgHeight / 2, (imgWidth / 2) + (int)(arrowWidth), (imgHeight / 2) - (int)(arrowHeight));


			g.Dispose();
			if (this.ImageBox.Image != null) { this.ImageBox.Image.Dispose(); }
			this.ImageBox.Image = img;
		}


		private double ActualAngle = 0d; //angle actuel de la flèche RADIAN
		private int GetIndexOfActualAngle()
		{
			double ChoiceWidth = 2d * Math.PI / (double)(this.choices.Count); //"largeur" d'un des choix

			double tempangle = this.ActualAngle + (ChoiceWidth / 2d);
			int index = (int)(tempangle / ChoiceWidth);

			//check de bound
			if (index < 0) { index = 0; }
			if (index >= this.choices.Count) { index = this.choices.Count - 1; } //ce check est nécessaire pour que cette fonction fonctionne correctement

			return index;
		}






	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace ShittyMouse
{
	public class SMouse
	{
		private Form forme;
		private PictureBox ImageBox;



		private int wSize = 50;
		private Color TColor = Color.FromArgb(3, 3, 3);



		public void Show()
		{
			this.forme.Show();
			this.Timer.Start();

			Cursor.Hide();
		}
		public void Close()
		{
			this.Timer.Stop();
			this.forme.Close();

			Cursor.Show();

			this.Timer.Dispose();
			this.ImageBox.Dispose();
			this.forme.Dispose();
		}



		public SMouse()
		{
			this.forme = new Form();
			this.forme.FormBorderStyle = FormBorderStyle.None;
			this.forme.StartPosition = FormStartPosition.Manual;
			this.forme.TopMost = true;
			this.forme.ShowInTaskbar = false;
			this.forme.MinimumSize = new Size(1, 1);
			this.forme.Size = new Size(this.wSize, this.wSize);
			this.forme.TransparencyKey = this.TColor;


			this.ImageBox = new PictureBox();
			this.ImageBox.Parent = this.forme;
			this.ImageBox.Dock = DockStyle.Fill;

			



			this.Timer = new Timer();
			this.Timer.Interval = 25;
			this.Timer.Tick += new EventHandler(this.Timer_Tick);



		}




		private Point FarLastMousePos = new Point(0, 0); //dernière position de la souris, mais à une distance résonable
		private double CurrentAngle = 0d;


		private Timer Timer;
		private void Timer_Tick(object sender, EventArgs e)
		{
			Point mpos = Cursor.Position;

			//vérifie si l'angle de la souris est à mettre à jour
			if (20 <= ((mpos.X - this.FarLastMousePos.X) * (mpos.X - this.FarLastMousePos.X)) + ((mpos.Y - this.FarLastMousePos.Y) * (mpos.Y - this.FarLastMousePos.Y)))
			{
				//on recalcul l'angle de la souris
				this.CurrentAngle = Math.Atan2(mpos.Y - this.FarLastMousePos.Y, mpos.X - this.FarLastMousePos.X);

				this.FarLastMousePos = mpos;



				//this.CurrentAngle = -0.7853d;
				//Program.wdebug(this.CurrentAngle);

			}

			
			this.RefreshForm();
		}



		//repositionne la forme et refresh l'image de la souris
		private void RefreshForm()
		{
			Point mpos = Cursor.Position;



			//l'angle actuel
			double angle = this.CurrentAngle;

			double dwSize = (double)(this.wSize);

			//décalage supplémentaire sur la position de la forme
			int supdX = 0;
			int supdY = 0;


			double pi4 = Math.PI / 4d;
			Point ppos = new Point(0, 0); //position de l'extrémité de la souris dans l'image finale
			if (angle > -pi4 && angle < pi4)
			{
				ppos.X = this.wSize;
				ppos.Y = (int)((dwSize / 2d) + (Math.Sin(angle) * dwSize / 2d));

				supdX = -1;
			}
			else if (angle <= -pi4 && angle > -pi4 * 3d)
			{
				ppos.Y = 0;
				ppos.X = (int)((dwSize / 2d) + (Math.Cos(angle) * dwSize / 2d));

				supdY = 2;
			}
			else if (angle >= pi4 && angle < pi4 * 3d)
			{
				ppos.Y = this.wSize;
				ppos.X = (int)((dwSize / 2d) + (Math.Cos(angle) * dwSize / 2d));

				supdY = -2;
			}
			else
			{
				ppos.X = 0;
				ppos.Y = (int)((dwSize / 2d) + (Math.Sin(angle) * dwSize / 2d));

				supdX = 3;
			}




			Point newformpos = mpos;
			newformpos.X -= ppos.X;
			newformpos.Y -= ppos.Y;
			newformpos.X += supdX;
			newformpos.Y += supdY;
			this.forme.Location = newformpos;
			//this.forme.Top = mpos.Y + 1;
			//this.forme.Left = mpos.X + 1;


			//crée l'image du curseur
			Bitmap cimg = new Bitmap(20, 20);
			Graphics cg = Graphics.FromImage(cimg);
			cg.Clear(this.TColor);
			this.forme.Cursor.Draw(cg, new Rectangle(1, 0, 1, 1));
			cg.Dispose();


			//on crée l'image
			Bitmap img = new Bitmap(this.wSize, this.wSize);
			Graphics g = Graphics.FromImage(img);
			g.Clear(this.TColor);

			//g.FillRectangle(Brushes.Red, new Rectangle(ppos.X - 1, ppos.Y - 1, 3, 3));


			g.TranslateTransform((float)(ppos.X), (float)(ppos.Y));
			g.RotateTransform((float)(angle / Math.PI * 180d) + 90);
			g.TranslateTransform((float)(-ppos.X), (float)(-ppos.Y));
			//Program.wdebug(angle);
			g.DrawImage(cimg, ppos.X, ppos.Y);
			//this.forme.Cursor.Draw(g, new Rectangle(ppos.X, ppos.Y, 1, 1));


			g.Dispose();
			cimg.Dispose();
			if (this.ImageBox.Image != null) { this.ImageBox.Image.Dispose(); }
			this.ImageBox.Image = img;
		}







		
	}
}

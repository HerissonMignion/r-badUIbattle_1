using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace C_FormTest1
{


	public class ScrollBallEventArgs : EventArgs
	{
		public int DeltaX;
		public int DeltaY;
		public ScrollBallEventArgs(int StartDeltaX, int StartDeltaY)
		{
			this.DeltaX = StartDeltaX;
			this.DeltaY = StartDeltaY;
		}
	}

	public class ScrollBall
	{
		private Point MousePos { get { return this.ImageBox.PointToClient(Cursor.Position); } }

		private Form forme;
		private PictureBox ImageBox;




		public event EventHandler<ScrollBallEventArgs> Scroll;
		private void Raise_Scroll(int sdx, int sdy)
		{
			if (this.Scroll != null)
			{
				this.Scroll(this, new ScrollBallEventArgs(sdx, sdy));
			}
		}


		public void SetPos(Point p) { this.SetPos(p.X, p.Y); }
		public void SetPos(int mLeft, int mTop)
		{
			this.forme.Top = mTop - (this.forme.Height / 2);
			this.forme.Left = mLeft - (this.forme.Width / 2);

		}



		public void Show()
		{


			this.forme.Show();
			this.RefreshImage();

		}
		public void Close()
		{

			this.AnalyTimer.Stop(); //juste pour être sûr
			this.forme.Close();

		}

		public ScrollBall()
		{
			this.forme = new Form();
			this.forme.Text = "Scroll Ball";
			this.forme.FormBorderStyle = FormBorderStyle.FixedToolWindow;
			this.forme.Opacity = 0.85d;
			this.forme.StartPosition = FormStartPosition.Manual;
			this.forme.ShowInTaskbar = false;
			this.forme.TopMost = true;
			this.forme.Size = new Size(120, 140);
			this.forme.MaximizeBox = false;
			this.forme.MinimizeBox = false;
			this.forme.TransparencyKey = Color.Blue;


			this.ImageBox = new PictureBox();
			this.ImageBox.Parent = this.forme;
			this.ImageBox.Dock = DockStyle.Fill;
			this.ImageBox.MouseDown += new MouseEventHandler(this.ImageBox_MouseDown);
			this.ImageBox.MouseUp += new MouseEventHandler(this.ImageBox_MouseUp);



			this.CreateMap();
			this.CreateTimer();

		}
		private void ImageBox_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				this.MouseDown();
			}
		}
		private void ImageBox_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				this.MouseUp();
			}
		}




		#region map
		private DotMap map;

		private class Dot
		{
			public double x = 1d;
			public double y = 1d;
			public double z = 1d;

			public void AdjustRadius(double r = 1d)
			{
				double div = Math.Sqrt((this.x * this.x) + (this.y * this.y) + (this.z * this.z)) / r;
				this.x /= div;
				this.y /= div;
				this.z /= div;
			}

			public void RotateOnY(double rad)
			{
				this.RotateOnY(Math.Sin(rad), Math.Cos(rad));
			}
			public void RotateOnY(double sinn, double coss)
			{
				double newx = (this.x * coss) - (this.z * sinn);
				double newz = (this.x * sinn) + (this.z * coss);
				this.x = newx;
				this.z = newz;
			}
			public void RotateOnX(double rad)
			{
				this.RotateOnX(Math.Sin(rad), Math.Cos(rad));
			}
			public void RotateOnX(double sinn, double coss)
			{
				double newz = (this.z * coss) - (this.y * sinn);
				double newy = (this.z * sinn) + (this.y * coss);
				this.z = newz;
				this.y = newy;
			}

			public Dot() { }
			public Dot(double sx, double sy, double sz)
			{
				this.x = sx;
				this.y = sy;
				this.z = sz;
			}
		}
		private class DotMap
		{
			public List<Dot> listDot = new List<Dot>();

			//les nombre complex sont utilisé pour effecter les rotation
			public void RotateOnY(double rad)
			{
				double sinn = Math.Sin(rad);
				double coss = Math.Cos(rad);

				foreach (Dot d in this.listDot)
				{
					d.RotateOnY(sinn, coss);
				}
			}
			public void RotateOnX(double rad)
			{
				double sinn = Math.Sin(rad);
				double coss = Math.Cos(rad);

				foreach (Dot d in this.listDot)
				{
					d.RotateOnX(sinn, coss);
				}
			}


			public void MakeSphere(double r)
			{
				foreach (Dot d in this.listDot)
				{
					d.AdjustRadius(r);
				}
			}
			
			public DotMap()
			{

			}
		}

		private void CreateMap()
		{
			this.map = new DotMap();

			//crée les dot
			for (double t = 0d; t < Math.PI * 2d; t += Math.PI * 2d / 40d)
			{
				Dot d1 = new Dot();
				d1.z = 0d;
				d1.y = Math.Sin(t);
				d1.x = Math.Cos(t);
				this.map.listDot.Add(d1);

				Dot d2 = new Dot();
				d2.z = Math.Sin(t);
				d2.y = 0d;
				d2.x = Math.Cos(t);
				this.map.listDot.Add(d2);
				
				Dot d3 = new Dot();
				d3.z = Math.Cos(t);
				d3.y = Math.Sin(t);
				d3.x = 0d;
				this.map.listDot.Add(d3);
			}



		}

		#endregion




		private bool IsMouseLeftDown = false;
		private void MouseDown()
		{
			this.IsMouseLeftDown = true;
			this.IsAutoScroll = false;

			this.LastMousePos = this.MousePos;

			this.AnalyTimer.Start();

		}
		private void MouseUp()
		{
			this.IsMouseLeftDown = false;

			Point mpos = this.MousePos;
			//check si l'user a donné un élant à la souris
			int dx = mpos.X - this.LastMousePos.X;
			int dy = mpos.Y - this.LastMousePos.Y;
			if ((dx * dx) + (dy * dy) <= 10)
			{
				this.AnalyTimer.Stop();
				this.IsAutoScroll = false;
			}
			else
			{
				this.IsAutoScroll = true;

				if (dy > 0) { this.autoDown = dy; } else { this.autoDown = 0; }
				if (dy < 0) { this.autoUp = -dy; } else { this.autoUp = 0; }
				if (dx > 0) { this.autoRight = dx; } else { this.autoRight = 0; }
				if (dx < 0) { this.autoLeft = -dx; } else { this.autoLeft = 0; }


			}
		}



		private Point LastMousePos = new Point(-1, -1); //dernière position de la souris, la framme précédante

		private bool IsAutoScroll = false; //indique si l'user a donné un élant de rotation et donc que la balle continue de rouler tout seul
		private int autoUp = 0;
		private int autoDown = 0;
		private int autoRight = 0;
		private int autoLeft = 0;



		private Timer AnalyTimer;
		private void AnalyTimer_Tick(object sender, EventArgs e)
		{
			double mulfact = 0.025d;
			if (!this.IsAutoScroll)
			{
				Point mpos = this.MousePos;
				//mesure le déplacement de la souris
				int dx = mpos.X - this.LastMousePos.X;
				int dy = mpos.Y - this.LastMousePos.Y;
				
				//effectue la rotation
				this.map.RotateOnY((double)dx * mulfact);
				this.map.RotateOnX((double)dy * mulfact);
				
				//save la position de la souris
				this.LastMousePos = mpos;

				//raise l'event du scroll
				this.Raise_Scroll(dx, dy);

			}
			else
			{
				//rotation auto
				if (this.autoDown > 0) { this.map.RotateOnX((double)(this.autoDown) * mulfact); this.autoDown--; }
				if (this.autoUp > 0) { this.map.RotateOnX((double)(-this.autoUp) * mulfact); this.autoUp--; }
				if (this.autoRight > 0) { this.map.RotateOnY((double)(this.autoRight) * mulfact); this.autoRight--; }
				if (this.autoLeft > 0) { this.map.RotateOnY((double)(-this.autoLeft) * mulfact); this.autoLeft--; }

				if (this.autoDown <= 0 && this.autoUp <= 0 && this.autoRight <= 0 && this.autoLeft <= 0) { this.AnalyTimer.Stop(); }

				//raise l'event du scroll
				int dx = 0;
				int dy = 0;
				if (this.autoDown > 0) { dy = this.autoDown; }
				if (this.autoUp > 0) { dy = -this.autoUp; }
				if (this.autoRight > 0) { dx = this.autoRight; }
				if (this.autoLeft > 0) { dx = -this.autoLeft; }
				this.Raise_Scroll(dx, dy);

			}



			this.RefreshImage();
		}


		private void CreateTimer()
		{
			this.AnalyTimer = new Timer();
			this.AnalyTimer.Interval = 50; // 100
			this.AnalyTimer.Tick += new EventHandler(this.AnalyTimer_Tick);


		}





		private void RefreshImage()
		{
			int imgWidth = this.ImageBox.Width;
			int imgHeight = this.ImageBox.Height;
			Bitmap img = new Bitmap(imgWidth, imgHeight);
			Graphics g = Graphics.FromImage(img);
			g.Clear(Color.Blue);

			double UiRadius = (double)((imgWidth / 2) - 3);
			g.FillEllipse(Brushes.White, (imgWidth / 2) - (int)UiRadius, (imgHeight / 2) - (int)UiRadius, (int)UiRadius * 2, (int)UiRadius * 2);
			g.DrawEllipse(Pens.DimGray, (imgWidth / 2) - (int)UiRadius, (imgHeight / 2) - (int)UiRadius, (int)UiRadius * 2, (int)UiRadius * 2);


			foreach (Dot d in this.map.listDot)
			{
				//on dessine le dot seulement s'il n'est pas du côté arrière de la balle
				if (d.z <= 0d)
				{
					int uix = (int)(((double)imgWidth / 2d) + (d.x * UiRadius));
					int uiy = (int)(((double)imgHeight / 2d) - (d.y * UiRadius));

					try
					{
						//img.SetPixel(uix, uiy, Color.Black);

						g.FillRectangle(Brushes.Black, uix, uiy, 2, 2);

					}
					catch { }

				}
			}




			g.Dispose();
			if (this.ImageBox.Image != null) { this.ImageBox.Image.Dispose(); }
			this.ImageBox.Image = img;
		}





		
	}
}

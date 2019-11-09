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

		private Random rnd = new Random();


		public string Message
		{
			get { return this.forme.Text; }
			set { this.forme.Text = value; }
		}



		public int Height = 400;
		public Font Font = new Font("consolas", 10f);
		public bool RotateChoice = true;
		public bool RandomizeChoiceRotation = true; //uniquement si RotateChoice
		public bool ColorsAndRotateColors = true; //uniquement si RotateChoice
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






		//quelque variable qui permettent de control les changement après un certain nombre de tick
		private int ChangeChoiceSpeed_MinFrameLength = 10;
		private int ChangeChoiceSpeed_MaxFraneLength = 20;
		private double ChangeChoiceSpeed_SpeedRadius = 0.02d; //valeur aléatoire maximale de la vitesse des choix, autour de 0
		private int zzzChangeChoiceSpeed_Left = 0; //nombre de frame restante avant le changement
		private double zzzChangeChoiceSpeed_Speed = -0.01d; //vitesse actuel de déplacement des choix

		private Color[] zzzColorArray = new Color[] { Color.Black, Color.Blue, Color.Red, Color.Green };
		private int RotateColor_FrameLength = 40; //nombre de frame pour rotater les couleur de 1
		private int zzzRotateColor_Delta = 0; //décalage actuel des couleur
		private int zzzRotateColor_Left = 10; //nombre de frame restante

		private Timer TimerFrame;
		private void CreateTimer()
		{
			this.TimerFrame = new Timer();
			this.TimerFrame.Interval = 1; // 5
			this.TimerFrame.Tick += new EventHandler(this.TimerFrame_Tick);
		}
		private void TimerFrame_Tick(object sender, EventArgs e)
		{
			//change la couleur, s'il le faut
			if (this.ColorsAndRotateColors)
			{
				//check et execute la rotation des couleur
				this.zzzRotateColor_Left--;
				if (this.zzzRotateColor_Left < 0)
				{
					//décale les couleur
					this.zzzRotateColor_Delta++;

					//reset les left frame
					this.zzzRotateColor_Left = this.RotateColor_FrameLength;
				}
			}

			this.ActualAngle += 0.1d; // 0.1d

			if (this.RotateChoice)
			{
				////fait décaler les choix d'angle
				this.ActualChoiceAngle += zzzChangeChoiceSpeed_Speed;

				if (this.RandomizeChoiceRotation) //seulement si on veut randomizer la direction des choix, on effectue les truc en lien
				{
					////check et execute le changement aléatoire de la vitesse des choix
					this.zzzChangeChoiceSpeed_Left--;
					if (this.zzzChangeChoiceSpeed_Left < 0) //si le nombre de frame est écoulé, on change de vitesse et on re-crinque la variable
					{
						//décide d'une nouvelle vitesse, aléatoire
						this.zzzChangeChoiceSpeed_Speed = -1d * this.ChangeChoiceSpeed_SpeedRadius * this.rnd.NextDouble();

						//recrinque le nombre de frame restante
						this.zzzChangeChoiceSpeed_Left = this.rnd.Next(this.ChangeChoiceSpeed_MinFrameLength, this.ChangeChoiceSpeed_MaxFraneLength);

					}
				}
			}

			//make sure que les valeurs des angles sont dans les bound
			if (this.ActualAngle >= 2d * Math.PI) { this.ActualAngle -= 2d * Math.PI; }
			if (this.ActualAngle < 0d) { this.ActualAngle += 2d * Math.PI; }

			if (this.ActualChoiceAngle >= 2d * Math.PI) { this.ActualChoiceAngle -= 2d * Math.PI; }
			if (this.ActualChoiceAngle < 0d) { this.ActualChoiceAngle += 2d * Math.PI; }


			if (this.RotateChoice)
			{
				this.CreateBackImg();
			}

			this.RefreshImage(); //refresh la flèche
		}



		//image d'arrière plan. il ne sert à rien de recrée cette image à chaque frame
		private Bitmap backimg = null;
		private void CreateBackImg()
		{

			int imgWidth = this.ImageBox.Width;
			int imgHeight = this.ImageBox.Height;

			Bitmap img = new Bitmap(this.ImageBox.Width, this.ImageBox.Height);
			Graphics g = Graphics.FromImage(img);
			g.Clear(Color.White);


			//dessine les point et les string associés
			double len = (double)(this.Height / 2) * 0.9d; // 0.9d
			for (int i = 0; i < this.choices.Count; i++)
			{
				double angle = this.MakeSureAngleInBound(2d * Math.PI / (double)(this.choices.Count) * (double)i + this.ActualChoiceAngle);


				int posx = (imgWidth / 2) + (int)(len * Math.Cos(angle));
				int posy = (imgHeight / 2) - (int)(len * Math.Sin(angle));

				//img.SetPixel(posx, posy, Color.Red);
				int cr = 5; //rayon du cercle
				g.FillEllipse(Brushes.Black, posx - cr, posy - cr, 2 * cr, 2 * cr);


				//draw the string
				string text = this.choices[i];
				bool DrawToTheRight = angle < Math.PI / 2d || angle > 3d * Math.PI / 2d;
				SizeF TextSize = g.MeasureString(text, this.Font);
				Brush TextBrush = Brushes.Black;
				if (this.ColorsAndRotateColors)
				{
					TextBrush = new SolidBrush(this.zzzColorArray[(i + this.zzzRotateColor_Delta) % this.zzzColorArray.Length]);
				}

				if (DrawToTheRight)
				{
					g.DrawString(text, this.Font, TextBrush, posx + cr, posy - (int)(TextSize.Height / 2f));
				}
				else
				{
					g.DrawString(text, this.Font, TextBrush, posx - cr - (int)(TextSize.Width), posy - (int)(TextSize.Height / 2f));

				}

			}
			

			g.Dispose();
			if (this.backimg != null) { this.backimg.Dispose(); }
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
			double arrowlength = (double)(this.Height / 2) * 0.8d; // 0.8d
			double pointeLength = (double)(this.Height / 2) * 0.1d;



			double arrowWidth = arrowlength * Math.Cos(this.ActualAngle);
			double arrowHeight = arrowlength * Math.Sin(this.ActualAngle);

			double pointeWidth1 = pointeLength * Math.Sin(this.ActualAngle + (Math.PI / 4d));
			double pointeHeight1 = pointeLength * Math.Cos(this.ActualAngle + (Math.PI / 4d));
			double pointeWidth2 = pointeLength * Math.Sin(this.ActualAngle - (Math.PI / 4d));
			double pointeHeight2 = pointeLength * Math.Cos(this.ActualAngle - (Math.PI / 4d));

			////dessine la flèche
			//l'arrière de la flèche
			g.DrawLine(Pens.Black, imgWidth / 2, imgHeight / 2, (imgWidth / 2) - (int)(arrowWidth), (imgHeight / 2) + (int)(arrowHeight)); //dessine la ligne dans le sens opposé où elle pointe

			//pointe
			g.DrawLine(Pens.Black, imgWidth / 2, imgHeight / 2, (imgWidth / 2) - (int)pointeWidth1, (imgHeight / 2) - (int)pointeHeight1); // 1
			g.DrawLine(Pens.Black, imgWidth / 2, imgHeight / 2, (imgWidth / 2) + (int)pointeWidth2, (imgHeight / 2) + (int)pointeHeight2); // 2


			g.Dispose();
			if (this.ImageBox.Image != null) { this.ImageBox.Image.Dispose(); }
			this.ImageBox.Image = img;
		}


		private double ActualAngle = 0d; //angle actuel de la flèche RADIAN
		private double ActualChoiceAngle = 0d; //angle actuel qui décale les choix
		private int GetIndexOfActualAngle()
		{
			if (!this.RotateChoice)
			{
				double ChoiceWidth = 2d * Math.PI / (double)(this.choices.Count); //"largeur" d'un des choix

				//si ActualAngle est plus grand que 2pi-cwidth/2 alors il est plus proche de l'index 0
				if (this.ActualAngle > 2d * Math.PI - (ChoiceWidth / 2d)) { return 0; }


				double tempangle = this.ActualAngle + (ChoiceWidth / 2d);
				int index = (int)(tempangle / ChoiceWidth);

				//check de bound
				if (index < 0) { index = 0; }
				if (index >= this.choices.Count) { index = this.choices.Count - 1; } //ce check est nécessaire pour que cette fonction fonctionne correctement

				return index;
			}
			else
			{
				double ChoiceWidth = 2d * Math.PI / (double)(this.choices.Count); //"largeur" d'un des choix

				//angle ajusté
				double AngleToUse = this.MakeSureAngleInBound(this.ActualAngle - this.ActualChoiceAngle);
				//si le AngleToUse est plus grand que 2pi-cwidth/2 alors il est plus proche de l'index 0
				if (AngleToUse > 2d * Math.PI - (ChoiceWidth / 2d)) { return 0; }


				double tempangle = AngleToUse + (ChoiceWidth / 2d);

				int index = (int)(tempangle / ChoiceWidth);

				//check de bound
				if (index < 0) { index = 0; }
				if (index >= this.choices.Count) { index = this.choices.Count - 1; } //ce check est nécessaire pour que cette fonction fonctionne correctement

				return index;

			}
			//return -1;
		}



		//retourne le même angle, mais modulo 2pi
		private double MakeSureAngleInBound(double angle)
		{
			double rep = angle;
			double pi2 = 2d * Math.PI;
			while (rep < 0d)
			{
				rep += pi2;
			}
			while (rep >= pi2)
			{
				rep -= pi2;
			}
			return rep;
		}



	}
}

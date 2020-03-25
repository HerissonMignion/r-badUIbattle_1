using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace C_FormTest1
{
	public class o3dBallDialog
	{
		private Form forme;
		private PictureBox ImageBox;

		private Random rnd = new Random();


		

		public bool StereoscopicMode = true;





		public string Title
		{
			get { return this.forme.Text; }
			set { this.forme.Text = value; }
		}

		public List<string> Choices = new List<string>();
		public void AddChoice(string newchoice) { this.Choices.Add(newchoice); }


		public string Answer = "";
		public bool IsCanceled = true;







		public void ShowDialog()
		{

			//place la form autour de la souris
			Point mpos = Cursor.Position;
			this.forme.Top = mpos.Y - (this.forme.Height / 2);
			this.forme.Left = mpos.X - (this.forme.Width / 2);
			if (this.forme.Top < 0) { this.forme.Top = 0; }
			if (this.forme.Left < 0) { this.forme.Left = 0; }
			

			
			////fin
			this.AnalyTimer.Start();

			this.CreateMessages();
			this.CreateChoices();

			this.RefreshImage();
			this.forme.ShowDialog();
		}
		public o3dBallDialog()
		{
			this.forme = new Form();
			this.forme.Size = new Size(600, 600);
			this.forme.StartPosition = FormStartPosition.Manual;
			this.forme.MinimizeBox = false;
			this.forme.MaximizeBox = false;
			this.forme.ShowIcon = false;
			this.forme.ShowInTaskbar = false;
			this.forme.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.forme.Load += new EventHandler(this.forme_Load);
			this.forme.FormClosing += new FormClosingEventHandler(this.form1_FormClosing);


			this.ImageBox = new PictureBox();
			this.ImageBox.Parent = this.forme;
			this.ImageBox.Dock = DockStyle.Fill;
			this.ImageBox.BackColor = Color.Blue;
			this.ImageBox.MouseLeave += new EventHandler(this.ImageBox_MouseLeave);
			this.ImageBox.MouseMove += new MouseEventHandler(this.ImageBox_MouseMove);
			this.ImageBox.MouseDown += new MouseEventHandler(this.ImageBox_MouseDown);




			////create
			this.CreateTimer();



		}
		private void forme_Load(object sender, EventArgs e)
		{
			//téléporte la souris au milieu de la form
			Cursor.Position = this.ImageBox.PointToScreen(new Point(this.ImageBox.Width / 2, this.ImageBox.Height / 2));

		}
		private void form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.AnalyTimer.Stop();
		}

		private void ImageBox_MouseLeave(object sender, EventArgs e)
		{
			this.ImageBox.Cursor = Cursors.Arrow;
		}
		private void ImageBox_MouseMove(object sender, MouseEventArgs e)
		{
			//le curseur devient une hand seulement si la souris est dessus un message qui est un url
			if (e.Y <= this.uiMessageHeight)
			{
				//check si le message a un url
				if (this.listMessage[this.ActualMessage].url.Length > 1)
				{
					this.ImageBox.Cursor = Cursors.Hand;
				}
				else
				{
					this.ImageBox.Cursor = Cursors.Arrow;
				}
			}
			else
			{
				this.ImageBox.Cursor = Cursors.Arrow;
			}


		}
		private void ImageBox_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				//check si l'user a clické sur la zone du message
				if (e.Y <= this.uiMessageHeight)
				{
					//check si le message a un url
					if (this.listMessage[this.ActualMessage].url.Length > 1)
					{
						System.Diagnostics.Process.Start("cmd.exe", "/c start " + this.listMessage[this.ActualMessage].url);
					}
				}
				else
				{
					//l'user n'a pas clické sur la zone du message

					Choice c = this.GetClosestChoiceToCamera();
					if (c != null)
					{
						this.IsCanceled = false;
						this.Answer = c.value;
						this.AnalyTimer.Stop();
						this.forme.Close();
					}






				}
			}
			if (e.Button == MouseButtons.Right)
			{
				//shuffle
				foreach (Choice c in this.map.listChoice)
				{
					c.x = 2d * this.SphereRadius * this.rnd.NextDouble() - this.SphereRadius;
					c.y = 2d * this.SphereRadius * this.rnd.NextDouble() - this.SphereRadius;
					c.z = 2d * this.SphereRadius * this.rnd.NextDouble() - this.SphereRadius;
					c.AdjustRadius(this.SphereRadius);
				}
			}
		}




		#region choice map
		private double CameraZ = -4d; // -7d coordonné z de la caméra. la caméra est dans les z négatif et regarde vers les z positif
		private double SphereRadius = 1d; //rayon de la sphère



		private ChoiceMap map;

		private class Choice
		{
			public double x = 1d;
			public double y = 1d;
			public double z = 1d;
			public string value = "novalue"; //le choix donné à l'utilisateur

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

			public Choice() { }
			public Choice(string StartValue)
			{
				this.value = StartValue;
			}
			public Choice(string StartValue, double sx, double sy, double sz)
			{
				this.value = StartValue;
				this.x = sx;
				this.y = sy;
				this.z = sz;
			}
		}
		private class ChoiceMap
		{
			public List<Choice> listChoice = new List<Choice>();

			//les nombre complex sont utilisé pour effecter les rotation
			public void RotateOnY(double rad)
			{
				double sinn = Math.Sin(rad);
				double coss = Math.Cos(rad);

				foreach (Choice c in this.listChoice)
				{
					c.RotateOnY(sinn, coss);
				}
			}
			public void RotateOnX(double rad)
			{
				double sinn = Math.Sin(rad);
				double coss = Math.Cos(rad);

				foreach (Choice c in this.listChoice)
				{
					c.RotateOnX(sinn, coss);
				}
			}


			public void MakeSphere(double r)
			{
				foreach (Choice c in this.listChoice)
				{
					c.AdjustRadius(r);
				}
			}

			public ChoiceMap()
			{

			}
		}


		private Choice GetClosestChoiceToCamera()
		{
			double z = 2d * this.SphereRadius;
			Choice rep = null;
			foreach (Choice c in this.map.listChoice)
			{
				if (c.z < z)
				{
					z = c.z;
					rep = c;
				}
			}
			return rep;
		}
		//return something that has similar variable than the average dist between points
		private double GetTotalDist()
		{
			double sum = 0;
			if (this.map.listChoice.Count >= 2)
			{
				for (int i = 0; i < this.map.listChoice.Count - 1; i++)
				{
					Choice c1 = this.map.listChoice[i];
					for (int j = i + 1; j < this.map.listChoice.Count; j++)
					{
						Choice c2 = this.map.listChoice[j];
						//calcul la distance, mais au carrée pour éviter la racine carrée qui ne sert à rien
						double dx = c2.x - c1.x;
						double dy = c2.y - c1.y;
						double dz = c2.z - c1.z;
						double dist = (dx * dx) + (dy * dy) + (dz * dz);
						sum += Math.Sqrt(dist); // /!\ /!\ /!\ WARNING to anybody who would try to reproduce the code : THIS square root is very important. do not remove it for optimisation purpose because it will break the spreading of the choices around the surface. they will form packs of multiple choices very close together (they almost fusion) and these packs will kinda badly spread on the surface.
					}
				}

			}
			return sum;
		}

		
		private void TryToIncreaseDist_Fast(double delta)
		{
			foreach (Choice c in this.map.listChoice)
			{
				double savex = c.x;
				double savey = c.y;
				double savez = c.z;

				double ActualDist = this.GetTotalDist();

				double reverse = -1.1d;

				//ont choisi une variable aléatoirment
				int rndvar = this.rnd.Next(0, 2);
				if (rndvar == 0) // x
				{
					c.RotateOnY(delta);
					double NewDist = this.GetTotalDist();
					if (NewDist < ActualDist)
					{
						c.RotateOnY(reverse * delta);
					}
				}
				else // y
				{
					c.RotateOnX(delta);
					double NewDist = this.GetTotalDist();
					if (NewDist < ActualDist)
					{
						c.RotateOnX(reverse * delta);
					}
				}

			}
		}
		private void TryToIncreaseDist_Fast(double delta, int loop)
		{
			for (int i = 1; i <= loop; i++)
			{
				this.TryToIncreaseDist_Fast(delta);
			}
		}



		private void CreateChoices()
		{
			this.map = new ChoiceMap();
			

			foreach (string s in this.Choices)
			{
				Choice c = new Choice(s, 0, 0, 0);
				c.x = 2d * this.SphereRadius * this.rnd.NextDouble() - this.SphereRadius;
				c.y = 2d * this.SphereRadius * this.rnd.NextDouble() - this.SphereRadius;
				c.z = 2d * this.SphereRadius * this.rnd.NextDouble() - this.SphereRadius;
				c.AdjustRadius(this.SphereRadius);
				this.map.listChoice.Add(c);
			}
			
			//this.map.MakeSphere(this.SphereRadius);
		}
		#endregion
		#region analy timer
		private Timer AnalyTimer;

		private void AnalyTimer_Tick(object sender, EventArgs e)
		{
			//fait avancer les message
			this.msgTickLeft--;
			if (this.msgTickLeft <= 0)
			{
				if (this.ActualMsgState == msgState.Showing)
				{
					this.ActualMsgState = msgState.Show;
					this.msgTickLeft = this.msgtlShownTime;

				}
				else if (this.ActualMsgState == msgState.Show)
				{
					this.ActualMsgState = msgState.Hiding;
					this.msgTickLeft = this.msgtlShowHideTime;

				}
				else if (this.ActualMsgState == msgState.Hiding)
				{
					this.ActualMsgState = msgState.Showing;
					this.msgTickLeft = this.msgtlShowHideTime;
					this.ActualMessage = (this.ActualMessage + 1) % this.listMessage.Count;
				}
			}


			//fait tourner les choix selon la position de la souris
			Point mpos = this.ImageBox.PointToClient(Cursor.Position);
			//juste pour être sûr que la souris est dans l'image box
			int uiOutDeadZone = this.uiMessageHeight; // 10 distance au bord de l'image box dans laquelle la souris peut bouger sans déclancher de rotation
			if (mpos.X >= uiOutDeadZone && mpos.Y >= uiOutDeadZone && mpos.X < this.ImageBox.Width - uiOutDeadZone && mpos.Y < this.ImageBox.Height - uiOutDeadZone)
			{
				int uiDeadZone = 60; //rayon (carrée) zone au milieu dans laquel la souris peut bouger sans déclancher de rotation

				//obtien la position horizontale et verticale de la souris relativement au milieu
				int mx = mpos.X - (this.ImageBox.Width / 2);
				if (Math.Abs(mx) < uiDeadZone) { mx = 0; }
				else
				{
					//on ramène ca à une transition plus douce, qui part de 0
					if (mx > 0)
					{ //à droite
						mx -= uiDeadZone;
						if (this.ImageBox.Width - uiOutDeadZone - mpos.X < mx) { mx = this.ImageBox.Width - uiOutDeadZone - mpos.X; } //transition tranquille
					}
					else //à gauche
					{
						mx += uiDeadZone;
						if (mpos.X - uiOutDeadZone < -mx) { mx = uiOutDeadZone - mpos.X; } //transition tranquille
					}
				}
				int my = mpos.Y - (this.ImageBox.Height / 2);
				if (Math.Abs(my) < uiDeadZone) { my = 0; }
				else
				{
					//on ramène ca à une transition plus douce, qui part de 0
					if (my > 0)
					{ //à droite
						my -= uiDeadZone;
						if (this.ImageBox.Height - uiOutDeadZone - mpos.Y < my) { my = this.ImageBox.Height - uiOutDeadZone - mpos.Y; } //transition tranquille
					}
					else //à gauche
					{
						my += uiDeadZone;
						if (mpos.Y - uiOutDeadZone < -my) { my = uiOutDeadZone - mpos.Y; } //transition tranquille
					}
				}


				double mulval = 0.001d;
				this.map.RotateOnY((double)mx * mulval);
				this.map.RotateOnX((double)my * mulval);
				
			}


			if (this.map.listChoice.Count > 50)
			{
				this.TryToIncreaseDist_Fast(0.02d, 3); // 0.02d 3
			}
			else
			{

				this.TryToIncreaseDist_Fast(0.02d, 10);
			}


			//fin
			this.RefreshImage();
		}



		private void CreateTimer()
		{
			this.AnalyTimer = new Timer();
			this.AnalyTimer.Interval = 100;
			this.AnalyTimer.Tick += new EventHandler(this.AnalyTimer_Tick);

		}

		#endregion
		#region image
		private double vChoiceFontHeight = 15d; //hauteur graphique des caractère se sitant à z=0. converti en float lors de DrawString
		private string uiChoiceFontName = "calibri";

		//alpha = 16
		private Brush ui3dRedBrush = new SolidBrush(Color.FromArgb(16, 255, 0, 0));
		private Brush ui3dBlueBrush = new SolidBrush(Color.FromArgb(16, 0, 255, 255));

		private Font StateFont = new Font("calibri", 15f);

		private void RefreshImage()
		{
			int imgwidth = this.ImageBox.Width;
			int imgheight = this.ImageBox.Height;
			Bitmap img = new Bitmap(imgwidth, imgheight);
			Graphics g = Graphics.FromImage(img);
			g.Clear(Color.White);


			double hmul = (-this.CameraZ / ((this.SphereRadius / 2d) + 0.2d)) * (double)imgwidth / 2d / 2d; //sradius/2 +qqc parce que les objet ont une largeur

			////dessine tout les choix en 3d
			foreach (Choice c in this.map.listChoice)
			{
				double z = c.z - this.CameraZ; //la distance z qui le sépare virtuellement de la caméra

				//on calcul la position graphique du text
				int uix = (imgwidth / 2) + (int)((c.x) / z * hmul);
				int uiy = (imgheight / 2) - (int)((c.y) / z * hmul);


				//dessine le nom du choix
				try
				{
					Font cf = new Font(this.uiChoiceFontName, (float)(this.vChoiceFontHeight * -this.CameraZ / z));
					//calcul la taille du texte
					SizeF sf = g.MeasureString(c.value, cf);

					if (!this.StereoscopicMode)
					{
						g.DrawString(c.value, cf, Brushes.Black, (float)uix - (sf.Width / 2f), (float)uiy - (sf.Height / 2f));
					}
					else
					{
						float uiTopLeftX = (float)uix - (sf.Width / 2f);
						float uiTopLeftY = (float)uiy - (sf.Height / 2f);

						float uidecal = (float)-c.z * 10f; //c.z en négatif car c'est dans les valeur négative qu'ils sont proche de la caméra. le décalage graphique doit augmenter au fur et à mesure que ca se raproche de la caméra.

						for (int i = 1; i <= 30; i++)
						{
							g.DrawString(c.value, cf, this.ui3dRedBrush, uiTopLeftX - (uidecal / 2f), uiTopLeftY);

							g.DrawString(c.value, cf, this.ui3dBlueBrush, uiTopLeftX + (uidecal / 2f), uiTopLeftY);

						}

					}
					
				}
				catch { }
			}
			

			////dessine le message à l'écran
			Message msg = this.listMessage[this.ActualMessage];
			Brush brushMsg = Brushes.Black;
			try
			{
				if (this.ActualMsgState == msgState.Showing)
				{
					//transition
					Color cmsg = Color.FromArgb(msg.MsgColor.R + (this.msgTickLeft * (255 - msg.MsgColor.R) / this.msgtlShowHideTime), msg.MsgColor.G + (this.msgTickLeft * (255 - msg.MsgColor.G) / this.msgtlShowHideTime), msg.MsgColor.B + (this.msgTickLeft * (255 - msg.MsgColor.B) / this.msgtlShowHideTime));
					brushMsg = new SolidBrush(cmsg);
				}
				if (this.ActualMsgState == msgState.Show)
				{
					brushMsg = new SolidBrush(msg.MsgColor);
				}
				if (this.ActualMsgState == msgState.Hiding)
				{
					//transition
					Color cmsg = Color.FromArgb(255 - (this.msgTickLeft * (255 - msg.MsgColor.R) / this.msgtlShowHideTime), 255 - (this.msgTickLeft * (255 - msg.MsgColor.G) / this.msgtlShowHideTime), 255 - (this.msgTickLeft * (255 - msg.MsgColor.B) / this.msgtlShowHideTime));
					brushMsg = new SolidBrush(cmsg);
				}
			}
			catch { brushMsg = Brushes.Black; }
			g.DrawString(msg.msg, msg.Font, brushMsg, 1f, 1f);


			
			//indique que c'est le mode stereoscopic si c'est activé
			if (this.StereoscopicMode)
			{
				g.DrawString("Stereoscopic mode activated", this.StateFont, Brushes.Black, 0f, (float)imgheight - 24f);
			}
			
			g.Dispose();
			if (this.ImageBox.Image != null) { this.ImageBox.Image.Dispose(); }
			this.ImageBox.Image = img;
			GC.Collect();
		}


		#endregion
		#region messages
		private class Message
		{
			public string msg = "nomessage";
			public Font Font = new Font("calibri", 20f);
			public Color MsgColor = Color.Black;
			public string url = "";
			public Message(string StartMsg, Font StartFont, Color StartMsgColor, string StartUrl = "")
			{
				this.msg = StartMsg;
				this.Font = StartFont;
				this.MsgColor = StartMsgColor;
				this.url = StartUrl;
			}
		}
		private List<Message> listMessage = new List<Message>();


		private enum msgState
		{
			Showing, //en train d'apparaitre
			Show, //est affiché
			Hiding //en train de disparaitre
		}

		private int msgTickLeft = 0; //nombre de tick restant avant la prochaine phase
		private int ActualMessage = 0; //message actuellement affiché
		private msgState ActualMsgState = msgState.Showing;

		private int msgtlShowHideTime = 7; // 10 nombre de tick qu'il faut pour la phase apparition et de disparition.
		private int msgtlShownTime = 35; // 30 nombre de tick que le message reste affiché sans changer de couleur

		private int uiMessageHeight = 30; // 30 vertical area reserved to the messages at the top of the image


		private void CreateMessages()
		{
			this.listMessage.Add(new Message("Move the mouse around to rotate the choices.", new Font("calibri", 15f), Color.Black));
			this.listMessage.Add(new Message("Click with the mouse to confirm your choice.", new Font("calibri", 15f), Color.Black));
			if (this.StereoscopicMode)
			{
				this.listMessage.Add(new Message("Where can i buy 3D glasses?", new Font("calibri", 15f, FontStyle.Underline), Color.Blue, "https://www.google.ca/search?q=3d+glasses+red+blue+to+buy"));
			}
			this.listMessage.Add(new Message("Press right click to shuffle.", new Font("calibri", 15f), Color.Black));

			//initiale l'état actuel de message
			this.msgTickLeft = this.msgtlShowHideTime;
			this.ActualMsgState = msgState.Showing;
		}
		#endregion




	}
}

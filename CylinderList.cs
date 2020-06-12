using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace C_FormTest1
{
	public class clElement
	{
		private string zzzText = "";
		public string Text { get { return this.zzzText; } }
		private double zzzVirtualTextWidth = 0d;
		public double VirtualTextWidth { get { return this.zzzVirtualTextWidth; } }

		public object Tag = null;

		//position horizontale de la partie gauche du texte
		private double zzzx = 0d;
		public double x { get { return this.zzzx; } }

		//position verticale sur la surface du cylindre
		private double zzzangle = 0d;
		public double angle { get { return this.zzzangle; } }

		public clElement(string sText, object sTag, double sx, double sangle, double sVirtualTextWidth)
		{
			this.zzzText = sText;
			this.Tag = sTag;
			this.zzzx = sx;
			this.zzzangle = sangle;
			this.zzzVirtualTextWidth = sVirtualTextWidth;
		}


	}

	public class clElementEventArgs : EventArgs
	{
		public int index = -1;
		public clElement Elem = null;
		public clElementEventArgs() { }
		public clElementEventArgs(clElement sElem, int sindex)
		{
			this.Elem = sElem;
			this.index = sindex;
		}
	}

	public class CylinderList
	{
		
		private PictureBox ImageBox;
		public Control Parent
		{
			get { return this.ImageBox.Parent; }
			set { this.ImageBox.Parent = value; }
		}
		public void SetPos(int newLeft, int newTop)
		{
			this.ImageBox.Location = new Point(newLeft, newTop);
		}
		public int Top
		{
			get { return this.ImageBox.Top; }
			set { this.ImageBox.Top = value; }
		}
		public int Left
		{
			get { return this.ImageBox.Left; }
			set { this.ImageBox.Left = value; }
		}
		public void SetSize(int newWidth, int newHeight)
		{
			this.ImageBox.Size = new Size(newWidth, newHeight);
		}
		public int Width
		{
			get { return this.ImageBox.Width; }
			set { this.ImageBox.Width = value; }
		}
		public int Height
		{
			get { return this.ImageBox.Height; }
			set { this.ImageBox.Height = value; }
		}
		public AnchorStyles Anchor
		{
			get { return this.ImageBox.Anchor; }
			set { this.ImageBox.Anchor = value; }
		}
		public DockStyle Dock
		{
			get { return this.ImageBox.Dock; }
			set { this.ImageBox.Dock = value; }
		}



		//return true s'il y a au moins 1 élément
		public bool AnyElement { get { return this.listElem.Count > 0; } }

		public clElement SelectedElement
		{
			get
			{
				if (this.AnyElement)
				{
					try
					{
						return this.listElem[this.SelectedIndex];
					}
					catch
					{
						return null;
					}
				}
				else
				{
					return null;
				}
			}
		}



		private double VirtualRightEnd = 1d; //la position horizontale x de la fin du cylindre à droite. est augmenté au fur et à mesure que les éléments sont ajoutés

		private List<clElement> listElem = new List<clElement>();
		public clElement AddElement(string Text, object Tag)
		{
			//calcul la position du nouveau élément
			double x = 0d;
			double angle = 0d;
			if (this.listElem.Count > 0)
			{
				angle = this.listElem[this.listElem.Count - 1].angle + this.ItemSpace;
				x = this.listElem[this.listElem.Count - 1].x + this.GetMinimumDeltaX();
			}

			//on fait calculer la largeur virtuel de la chaine de text, pour savoir quel deltax on doit appliquer à chaques prochains élément à être ajouté pour qu'après 1 tour, ils ne chevauchent pas le texte de l'élément actuel
			double VirtualTextWidth = this.GetVirtualTextWidth(Text);
			//on prend la largeur du texte et on la divise par le nombre d'élément qui fait 1 tour complet
			double MinimumDeltaX = VirtualTextWidth / (double)(this.FullTurnItemCount()) + 0.02d;
			//on ajoute le deltax minimum actuel à la liste des deltax minimum du dernier tour
			this.AddToLastDeltaX(MinimumDeltaX);

			//calcule la position horizontale de la fin du cylindre à droite
			double NewElementEnd = x + VirtualTextWidth + this.CylinderBorderWidth;
			if (NewElementEnd > this.VirtualRightEnd) { this.VirtualRightEnd = NewElementEnd; }

			clElement e = new clElement(Text, Tag, x, angle, VirtualTextWidth);
			this.listElem.Add(e);
			return e;
		}
		public clElement AddElement(string Text)
		{
			return this.AddElement(Text, null);
		}

		//retire tout les élément. il vide la liste
		public void ClearElements()
		{
			////on make sure que tout est à l'arrêt
			//on check le déplacement animé
			this.MoveTimer.Stop();
			this.targetX = 0d;
			this.targetAngle = 0d;
			//make sure que les listes de movements sont vide
			while (this.animX.Count > 0) { this.animX.RemoveAt(0); }
			while (this.animAngle.Count > 0) { this.animAngle.RemoveAt(0); }

			////autres choses
			//on reset la position de la caméra
			this.camX = 0d;
			this.camAngle = 0d;

			this.SelectedIndex = 0;
			this.VirtualRightEnd = 1d;

			//vide la liste des dernier deltax minimum
			while (this.listMinimumDeltaX.Count > 0) { this.listMinimumDeltaX.RemoveAt(0); }

			////retire tout les élément
			while (this.listElem.Count > 0) { this.listElem.RemoveAt(0); }
			
		}


		//cette liste contient le delta x minimum à affectuer, pour tout les élément du dernier tour du cylindre, pour qu'après un autre tour complet du cylindre, la colonne suivante ne chevauche pas le texte. de la colonne précédante.
		private List<double> listMinimumDeltaX = new List<double>();
		//ceci retourne combien d'item font 1 tour complet du cylindre. la valeur n'a pas besoin d'être exact, mais elle doit être supérieur ou égale à la quantité d'item qui font 1 tour complet.
		private int FullTurnItemCount()
		{
			return (int)(Math.PI * 2d / this.ItemSpace) + 2;
		}
		//ajoute une nouvelle valeur à la liste des deltax minimaux. prend automatiquement en charge de retirer les items qui datent d'il y a plus d'un tour
		private void AddToLastDeltaX(double newdeltax)
		{
			this.listMinimumDeltaX.Add(newdeltax);
			while (this.listMinimumDeltaX.Count > this.FullTurnItemCount())
			{
				this.listMinimumDeltaX.RemoveAt(0);
			}
		}
		//retourne le deltax minimum à effectuer pour qu'après 1 tour complet, les nouveaux éléments ne chevauchent pas les anciens
		private double GetMinimumDeltaX()
		{
			double rep = 0d;
			foreach (double dx in this.listMinimumDeltaX)
			{
				if (dx > rep) { rep = dx; }
			}
			return rep;
		}

		#region measure string
		private Graphics zzzg = Graphics.FromImage(new Bitmap(10, 10));
		private SizeF privateMeasurestring(string text, Font font)
		{
			return this.zzzg.MeasureString(text, font);
		}

		private double GetVirtualTextWidth(string text)
		{
			double imgHeight = 20d;
			return (double)(this.zzzg.MeasureString(text, new Font(this.uiFontName, (float)(this.uiCloseFontMulFactor) * (float)imgHeight)).Width) * (this.camZ - 1d) / imgHeight * this.ViewAngle * 2d;
		}

		#endregion



		public event EventHandler<clElementEventArgs> ElementMouseClick;
		public event EventHandler<clElementEventArgs> ElementMouseDoubleClick;
		private void Raise_ElementMouseClick(clElement elem, int index)
		{
			if (this.ElementMouseClick != null)
			{
				this.ElementMouseClick(this, new clElementEventArgs(elem, index));
			}
		}
		private void Raise_ElementMouseDoubleClick(clElement elem, int index)
		{
			if (this.ElementMouseDoubleClick != null)
			{
				this.ElementMouseDoubleClick(this, new clElementEventArgs(elem, index));
			}
		}




		public CylinderList()
		{
			this.ImageBox = new PictureBox();
			this.ImageBox.BorderStyle = BorderStyle.FixedSingle;
			this.ImageBox.SizeChanged += new EventHandler(this.ImageBox_SizeChanged);
			this.ImageBox.MouseWheel += new MouseEventHandler(this.ImageBox_MouseWheel);
			this.ImageBox.MouseDown += new MouseEventHandler(this.ImageBox_MouseDown);
			this.ImageBox.MouseUp += new MouseEventHandler(this.ImageBox_MouseUp);
			this.ImageBox.MouseClick += new MouseEventHandler(this.ImageBox_MouseClick);
			this.ImageBox.MouseDoubleClick += new MouseEventHandler(this.ImageBox_MouseDoubleClick);


			this.CreateArrow();
			this.CreateMoveTimer();
			this.CreateInterface();

			this.Refresh();
		}
		private void ImageBox_SizeChanged(object sender, EventArgs e)
		{
			this.ResizeInterface();
			this.Refresh();
		}
		private void ImageBox_MouseWheel(object sender, MouseEventArgs e)
		{
			if (e.Delta > 0)
			{
				//up
				this.DefineSelectedIndex(this.SelectedIndex - 1);
			}
			else
			{
				//down
				this.DefineSelectedIndex(this.SelectedIndex + 1);
			}

			this.Refresh();
		}
		private void ImageBox_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				//check si la souris est sur un control
				if (!this.Check_MouseLeftDown())
				{
					//la souris n'est pas sur un control




				}
				this.Refresh();
			}
			if (e.Button == MouseButtons.Right)
			{

			}
		}
		private void ImageBox_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				this.Check_MouseLeftUp();
				
				this.Refresh();
			}
			if (e.Button == MouseButtons.Right)
			{
				


			}
		}
		private void ImageBox_MouseClick(object sender, MouseEventArgs e)
		{
			//make sure que la souris n'est pas sur un control
			if (!this.IsMouseOnAnyControl())
			{
				//make sure qu'il y a des élément
				if (this.AnyElement)
				{
					//on récupère l'élément et raise l'event
					clElement elem = this.listElem[this.SelectedIndex];
					this.Raise_ElementMouseClick(elem, this.SelectedIndex);
				}
			}
		}
		private void ImageBox_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			//make sure que la souris n'est pas sur un control
			if (!this.IsMouseOnAnyControl())
			{
				//make sure qu'il y a des élément
				if (this.AnyElement)
				{
					//on récupère l'élément et raise l'event
					clElement elem = this.listElem[this.SelectedIndex];
					this.Raise_ElementMouseDoubleClick(elem, this.SelectedIndex);
				}
			}
		}



		#region 3D et 2D
		
		/*
		 *     y
		 *     |
		 *     |
		 *     |
		 *     +----------------------x
		 *    /          camX
		 *   /
		 *  /        positive value of camAngle make the camera go down in this direction \/ down. it makes the camera rotate around the cylinder
		 * z  camZ
		 * 
		 * camZ reste CONSTANT.    le côté du cylindre le plus proche de la caméra est donc lorsque z = 1 et y = 0. z=1 parce que le cylindre a un rayon de 1.
		 * camX NE reste PAS constant
		 * 
		 * la caméra reste toujours à y=0 donc il n'y a pas de variable mit en place pour ajuster le y de la caméra.
		 * 
		 * deplus, le cylindre a un rayon de 1. 
		 */
		 
		//le cylindre a un rayon de 1u donc si la position de la caméra est défini, il ne reste aucune variable à faire.
		//cette valeur ne change pas au cours de l'exécution.
		private double camZ = 10d;
		private double camX = 0d; //position horizontale actuel de la caméra, au milieux de l'image. cette valeur change pendant l'exécution.
		private double camAngle = 0d; //angle verticale de la caméra autour du cylindre.
		
		/*                                  --------__________
		 *                         ---------       /          \
		 * view Angle here---------              /              \
		 *      ----------                      |                |
		 * cam  z------------------------------ | radius=1       |
		 * 
		 * peut importe la taille graphique de ImageBox ou les paramètres virtuels de la scène en 3d, l'image affiché à l'user doit toujours être zoomé Correctement pour
		 * que la parti visible du haut et du bas du cylindre soient ajusté pour être en haut et en bas de l'image, avec juste un très petit espacement pour gaspiller
		 * le moins possible l'espace disponible.
		 * 
		 * la vision fonctionnant avec des angles, il calculer l'angle que prend le cylindre dans le champs de vision, et puisqu'on met la direction dans laquel on
		 * regarde au milieu de l'image, il faut calculer la moitier de l'angle que prend le cylindre dans le champs de vision. la moitier supérieur ou inférieur
		 * du champs de vision forme un triangle rectangle avec le rayon du cylindre et l'axe z. le "view angle" est alors arcsin(1 / camz). la variable view angle
		 * représente l'angle que prend dans le champs de vision la moitier supérieur du cylindre.
		 * 
		 * il ne faut pas oublier qu'après tout cela, on n'utilise pas l'arctengente pour calculer la position d'un point à l'image, on utilise l'aprox atan(y/z) = y/z.
		 * cela signifie que concrètement, view angle représentera le rapport y/z maximale pour lequel le point est encore visible à l'écran, et non pas un angle.
		 * ce ne sera pas une très grande différence lorsqu'on effectura le calcul d'un point à l'image, mais on risque de se perdre dans les formules si on oublie cela.
		 * 
		 */
		private double ViewAngle
		{
			get
			{
				return Math.Asin(1d / this.camZ) + 0.04d; //+delta pour qu'il y ait un petit espacement entre le haut et la bas du cylindre et les bords de l'image
			}
		}

		
		private double ItemSpace = 2d * Math.PI / 16.7d; //angle, à partir du milieux du cylindre, qui sépare verticalement chaques éléments    _\


		private double uiCloseFontMulFactor = 0.1d; //pour obtenir la taille "em" graphique de la font pour une chaine de texte virtuel situé le plus proche possible de la caméra (à une distance de (this.camz - 1d)), il faut prendre la hauteur de l'image et la multiplier par cette valeur
		private string uiFontName = "consolas";

		private Font uiBtnFont = new Font("consolas", 10f);

		private Brush CylinderBrush = Brushes.LightGray;
		private double CylinderBorderWidth = 0.5d; //largeur suplémentaire aux extrémités gauche et droite




		//un point avec des coordonnée 3d et une position 2d d'integer
		private struct p3d2d
		{
			//graphique
			public int uix;
			public int uiy;
			//virtuel
			public double x;
			public double y;
			public double z;

			public p3d2d(int suix, int suiy)
			{
				this.uix = suix;
				this.uiy = suiy;
				this.x = 0d;
				this.y = 0d;
				this.z = 0d;
			}

		}


		//[0]=z  [1]=y    il retourne  un array de double d'une taille 2
		//effectue une rotation de coordonné le long de l'axe x, dans la direction de z+ vers y+
		private double[] RotateOnX(double y, double z, double coss, double sinn)
		{
			return new double[] { (z * coss) - (y * sinn), (z * sinn) + (y * coss) };
		}
		private double[] RotateOnX(double y, double z, double angle)
		{
			return this.RotateOnX(y, z, Math.Cos(angle), Math.Sin(angle));
		}

		//cette fonction convertie une coordonné virtuel en sa position dans l'image, et sa position virtuel relative à la caméra, en prenant également en considération l'angle verticale actuel de la caméra.
		private p3d2d convVirtualToUI(double x, double y, double z, int imgWidth, int imgHeight, double coss, double sinn)
		{
			double MulFactor = (double)imgHeight / 2d / this.ViewAngle; //il suffit de multiplier un angle/rapport par cette valeur pour connaitre en pixel la distance graphique qu'il représente à l'image

			//il faut effectuer une rotation sur l'axe x, dans la direction de z+ vers y+, et d'un angle de rotation étant this.camAngle.
			//la multiplication complexe est utilisé pour effectuer cette rotation.
			//double zz = (z * coss) - (y * sinn);
			//double yy = (z * sinn) + (y * coss);
			double[] rotated = this.RotateOnX(y, z, coss, sinn);
			double zz = rotated[0];
			double yy = rotated[1];

			double dist = this.camZ - zz; //il faut calculer la distance z (z relatif à la caméra) entre le point et la caméra, qui a changé à cause de la rotation sur l'axe x
			double deltaX = x - this.camX;
			double deltaY = yy;

			p3d2d rep = new p3d2d(0, 0);
			rep.uix = (int)(((double)imgWidth / 2d) + (deltaX / dist * MulFactor) + 0.5d); //+0.5d pour l'arrondir correctement
			rep.uiy = (int)(((double)imgHeight / 2d) - (deltaY / dist * MulFactor) + 0.5d);
			rep.x = deltaX;
			rep.y = yy;
			rep.z = zz;

			return rep;
		}

		#endregion


		public void Refresh()
		{
			int imgWidth = this.Width;
			int imgHeight = this.Height;
			if (imgWidth < 100) { imgWidth = 100; }
			if (imgHeight < 100) { imgHeight = 100; }
			Bitmap img = new Bitmap(imgWidth, imgHeight);
			Graphics g = Graphics.FromImage(img);
			g.Clear(Color.White);

			double MulFactor = (double)imgHeight / 2d / this.ViewAngle;

			//ces 2 valeurs sont utilisées pour effectuer une rotation des coordonnés sur le cylindre
			double coss = Math.Cos(this.camAngle);
			double sinn = Math.Sin(this.camAngle);


			////dessine le cylindre
			int uicTopSpace = (int)((this.ViewAngle - (1d / this.camZ)) * MulFactor); //espacement verticale entre le cylindre et les bords de l'image en haut et en bas
			int uicHeight = imgHeight - uicTopSpace - uicTopSpace; //hauteur graphique du cylindre

			//position graphique de la gauche du cylindre
			int uicLeft = (imgWidth / 2) - (int)((this.CylinderBorderWidth + this.camX) / this.camZ * MulFactor);
			if (uicLeft < 0) { uicLeft = 0; }

			//position graphique de la droite du cylindre
			int uicRight = (imgWidth / 2) + (int)((this.VirtualRightEnd - this.camX) / this.camZ * MulFactor);

			//dessine le corps principale du cylindre
			g.FillRectangle(this.CylinderBrush, uicLeft, uicTopSpace, uicRight - uicLeft, uicHeight);
			//dessine le bord gauche s'il le faut
			if (uicLeft > 0)
			{
				//calcule la position graphique du bord gauche, mais la position du cercle qui est la plus proche de la caméra
				int uicCloseLeft = (imgWidth / 2) - (int)((this.CylinderBorderWidth + this.camX) / (this.camZ - 1d) * MulFactor);
				g.FillEllipse(this.CylinderBrush, uicCloseLeft, uicTopSpace, 2 * (uicLeft - uicCloseLeft), uicHeight);
			}
			//dessine le bord droit s'il le faut
			if (uicRight < imgWidth)
			{
				//calcule la position graphique du bord droit, mais la position du cercle qui est la plus proche de la caméra
				int uicCloseRight = (imgWidth / 2) + (int)((this.VirtualRightEnd - this.camX) / (this.camZ - 1d) * MulFactor);
				g.FillEllipse(this.CylinderBrush, 2 * uicRight - uicCloseRight, uicTopSpace, 2 * (uicCloseRight - uicRight), uicHeight);
			}

			

			//si l'élément actuellement sélectionné est dans l'image (ca devrait être presque toujours le cas), cette variable sera true et le p3d2d sera gardé dans l'autre variable
			bool SelectedElemFound = false; //devient true si l'élément actuel est dans l'image
			clElement SelectedElem = null;
			p3d2d SelectedPos = new p3d2d(0, 0);

			double minimumZ = this.ViewAngle; // coordonné z+ minimum pour qu'un point de la surface du cylindre soit sur la face visible par la caméra

			////dessine les élément
			int index = 0;
			while (index < this.listElem.Count)
			{
				clElement elem = this.listElem[index];

				//on doit d'abord obtenir ses coordonnés réel.
				double elemX = elem.x;
				double elemY = -Math.Sin(elem.angle);
				double elemZ = Math.Cos(elem.angle);

				//maintenant on obtien ses coordonnés relatives à la caméra
				p3d2d relpos = this.convVirtualToUI(elemX, elemY, elemZ, imgWidth, imgHeight, coss, sinn);
				double dist = this.camZ - relpos.z;

				//check s'il sort de l'écran. si c'est le cas, on peut arrêter car tout les suivant seront eux aussi dehors de l'image
				if (relpos.uix > imgWidth) { break; }

				//check si l'item est actuellement visible à l'écran en vérifiant si le côté droit du texte est dans l'image
				int uiWidth = (int)(elem.VirtualTextWidth / dist * MulFactor);
				if (relpos.uix + uiWidth >= 0)
				{

					//si on est sur l'élément sélectionné par l'user, on le garde de côté
					if (index == this.SelectedIndex)
					{
						SelectedElemFound = true;
						SelectedElem = elem;
						SelectedPos = relpos;
					}

					//on check si l'élément est du côté visible du cylindre
					if (relpos.z > minimumZ)
					{

						//on calcul l'applatissement verticale du text.
						float TextYScale = (float)(relpos.z);

						//on calcule le height que la font doit avoir
						Font elemFont = new Font(this.uiFontName, (float)((double)imgHeight * this.uiCloseFontMulFactor * (this.camZ - 1d) / dist));
						//on calcule la taille graphique qu'aura le text
						SizeF elemTextSize = g.MeasureString(elem.Text, elemFont);
						//on dessine le text de l'élément, centré verticalement
						g.TranslateTransform((float)(relpos.uix), (float)(relpos.uiy) - (elemTextSize.Height * TextYScale / 2f));
						g.ScaleTransform(1f, TextYScale);
						g.DrawString(elem.Text, elemFont, Brushes.Black, 0f, 0f);
						g.ResetTransform();

						//g.DrawString(elem.Text, elemFont, Brushes.Black, (float)(relpos.uix), (float)(relpos.uiy) - (elemTextSize.Height / 2f));
						elemFont.Dispose();

					}
				}

				//next iteration
				index++;
			}

			//si l'élément actuellement sélectionné a été rencontré, on dessine la flèche bleu à côté
			if (SelectedElemFound)
			{
				//make sure que l'élément actuel est sur la face visible
				if (SelectedPos.z > minimumZ)
				{
					int arrowHeight = (int)((double)(this.imgArrow.Height) * SelectedPos.z);
					if (arrowHeight >= this.imgArrow.Height - 5) { arrowHeight = this.imgArrow.Height; }

					g.DrawImage(this.imgArrow, SelectedPos.uix - 3 - this.imgArrow.Width, SelectedPos.uiy - (arrowHeight / 2), this.imgArrow.Width, arrowHeight);
				}
			}

			////dessine les composant de l'interface graphique
			foreach (uiButton b in this.listButton)
			{
				Brush BackBrush = Brushes.Silver;
				if (b.IsMouseLeftDown) { BackBrush = Brushes.White; }

				g.FillRectangle(BackBrush, b.rec);
				g.DrawRectangle(Pens.Black, b.rec);

				//dessine le text du button, au milieux
				SizeF btnTextSizeF = g.MeasureString(b.Text, this.uiBtnFont);
				g.DrawString(b.Text, this.uiBtnFont, Brushes.Black, (float)(b.Left + (b.Width / 2)) - (btnTextSizeF.Width / 2f), (float)(b.Top + (b.Height / 2)) - (btnTextSizeF.Height / 2f));
				
			}



			g.Dispose();
			if (this.ImageBox.Image != null) { this.ImageBox.Image.Dispose(); }
			this.ImageBox.Image = img;
			this.ImageBox.Refresh();

		}



		#region déplacement et élément actuellement sélectionné

		private int SelectedIndex = 0; //index de l'élément actuellement sélectionné

		private void DefineSelectedIndex(int newindex)
		{
			//make sure qu'il y a des élément. sinon on make sure que l'index actuel est 0 et que la caméra est à sa position de départ
			if (this.AnyElement)
			{
				int ni = newindex;
				//on check les bounds
				if (ni < 0) { ni = 0; }
				if (ni >= this.listElem.Count) { ni = this.listElem.Count - 1; }

				//on check s'il est différent de celui délà sélectionner
				if (ni != this.SelectedIndex)
				{
					this.SelectedIndex = ni;
					//on obtient l'élément et on lance le déplacement animé jusqu'à cet élément
					clElement elem = this.listElem[ni];
					this.TargetPosition(elem.x, elem.angle);
				}
			}
			else //s'il n'y a aucun élément
			{
				this.SelectedIndex = 0;
				this.camX = 0d;
				this.camAngle = 0d;
			}
		}

		//la position finale du déplacement
		private double targetX = 0d;
		private double targetAngle = 0d;

		private void TargetPosition(double tX, double tAngle)
		{
			//on save la "destination" de la caméra
			this.targetX = tX;
			this.targetAngle = tAngle;

			//on prépare la liste des movements
			//on commence par make sure que les liste sont vide
			while (this.animX.Count > 0) { this.animX.RemoveAt(0); }
			while (this.animAngle.Count > 0) { this.animAngle.RemoveAt(0); }

			int TotalDiv = 30;
			double deltaAngle = this.targetAngle - this.camAngle;
			double deltaX = this.targetX - this.camX;
			for (int i = 1; i < TotalDiv; i++)
			{
				//x comprit dans [0, 1]
				double P1(double x)
				{
					return (-8d * x * x * x) + (12d * x * x) + (-3d * x);
				}
				this.animX.Add(this.camX + (deltaX * (double)i / (double)TotalDiv));

				//x comprit dans [0, 1]
				double P2(double x)
				{
					return (-1.5d * x * x * x) + (2.2d * x * x) + (0.3d * x);
				}
				this.animAngle.Add(this.camAngle + (deltaAngle * P2((double)i / (double)TotalDiv)));
			}


			//on démare le timer de l'animation
			this.MoveTimer.Start();
		}

		//pendant un déplacement, ces listes contiennent toutes les positions que le movetimer doit donner à la caméra. ces 2 listes ont toujours la même quantité d'objet à l'intérieur.
		//lorsque le timer a vidé les 2 listes, il make sure une dernière fois que la caméra est à la target position et le timer s'arrête.
		private List<double> animX = new List<double>();
		private List<double> animAngle = new List<double>();

		private Timer MoveTimer;
		private void CreateMoveTimer()
		{
			this.MoveTimer = new Timer();
			this.MoveTimer.Interval = 50;
			this.MoveTimer.Tick += new EventHandler(this.MoveTimer_Tick);
		}
		private void MoveTimer_Tick(object sender, EventArgs e)
		{
			//on check s'il y a une autre position à faire
			if (this.animX.Count > 0)
			{
				//puisqu'il y a une autre position à faire, on la fait
				this.camX = this.animX[0];
				this.camAngle = this.animAngle[0];
				this.animX.RemoveAt(0);
				this.animAngle.RemoveAt(0);
				this.Refresh();

			}
			else //l'animation est terminé alors on met la caméra à sa position finale et on arrête le timer
			{
				this.MoveTimer.Stop();
				this.camX = this.targetX;
				this.camAngle = this.targetAngle;
				this.Refresh();
			}


		}


		private Bitmap imgArrow;
		private void CreateArrow()
		{
			int imgWidth = 50; // 30
			Bitmap img = new Bitmap(imgWidth, imgWidth);
			Graphics g = Graphics.FromImage(img);
			//g.Clear(Color.DimGray);
			g.Clear(Color.Transparent);


			int w = imgWidth;
			int wd2 = imgWidth / 2;
			int wd4 = (int)((double)(imgWidth) / 3.5d);
			int wd6 = imgWidth / 6;
			int wd8 = imgWidth / 8;

			Point[] plArrow = new Point[] { new Point(w - 1, wd2), new Point(wd2 - 1, 0), new Point(wd2 - 1, wd4), new Point(0, wd4), new Point(0, w - wd4), new Point(wd2 - 1, w - wd4), new Point(wd2 - 1, w) };

			Point[] plArrowUp = new Point[] { new Point(w - 1, wd2), new Point(wd2 - 1, 0), new Point(wd2 - 1, wd4), new Point(0, wd4), new Point(0, wd2) };
			Point[] plArrowDown = new Point[] { new Point(w - 1, wd2), new Point(0, wd2), new Point(0, w - wd4), new Point(wd2 - 1, w - wd4), new Point(wd2 - 1, w) };
			Point[] plArrowMiddle = new Point[] { new Point(w - 1, wd2), new Point(wd2 + 1, wd6), new Point(wd2 + 1, wd4 + wd8), new Point(0, wd4 + wd8), new Point(0, w - wd4 - wd8), new Point(wd2 + 1, w - wd4 - wd8), new Point(wd2 + 1, w - wd6) };

			g.FillPolygon(Brushes.SteelBlue, plArrowUp);
			g.FillPolygon(Brushes.CornflowerBlue, plArrowDown);
			g.FillPolygon(Brushes.DodgerBlue, plArrowMiddle);
			g.DrawPolygon(Pens.White, plArrow);

			g.Dispose();
			this.imgArrow = img;
		}

		#endregion

		#region UI
		
		private uiButton interBtnUp1;
		private uiButton interBtnDown1;
		private uiButton interBtnUp10;
		private uiButton interBtnDown10;

		private void CreateInterface()
		{
			Size btnSize = new Size(70, 25);

			this.interBtnUp1 = new uiButton(this);
			this.interBtnUp1.SetSize(btnSize);
			this.interBtnUp1.Text = "up";
			this.interBtnUp1.MouseLeftDown += new EventHandler(this.interBtnUp1_MouseLeftDown);

			this.interBtnDown1 = new uiButton(this);
			this.interBtnDown1.SetSize(btnSize);
			this.interBtnDown1.Text = "down";
			this.interBtnDown1.MouseLeftDown += new EventHandler(this.interBtnDown1_MouseLeftDown);

			this.interBtnUp10 = new uiButton(this);
			this.interBtnUp10.SetSize(btnSize);
			this.interBtnUp10.Text = "up 10";
			this.interBtnUp10.MouseLeftDown += new EventHandler(this.interBtnUp10_MouseLeftDown);

			this.interBtnDown10 = new uiButton(this);
			this.interBtnDown10.SetSize(btnSize);
			this.interBtnDown10.Text = "down 10";
			this.interBtnDown10.MouseLeftDown += new EventHandler(this.interBtnDown10_MouseLeftDown);

			this.ResizeInterface();
		}
		private void ResizeInterface()
		{
			this.interBtnUp1.Left = 2;
			this.interBtnUp1.Top = (this.Height / 2) - 1 - this.interBtnUp1.Height;

			this.interBtnDown1.Left = this.interBtnUp1.Left;
			this.interBtnDown1.Top = this.interBtnUp1.Top + this.interBtnUp1.Height + 2;

			this.interBtnUp10.Left = this.interBtnUp1.Left;
			this.interBtnUp10.Top = this.interBtnUp1.Top - 2 - this.interBtnUp10.Height;

			this.interBtnDown10.Left = this.interBtnDown1.Left;
			this.interBtnDown10.Top = this.interBtnDown1.Top + this.interBtnDown1.Height + 2;

		}

		private void interBtnUp1_MouseLeftDown(object sender, EventArgs e)
		{
			this.DefineSelectedIndex(this.SelectedIndex - 1);
		}
		private void interBtnDown1_MouseLeftDown(object sender, EventArgs e)
		{
			this.DefineSelectedIndex(this.SelectedIndex + 1);
		}
		private void interBtnUp10_MouseLeftDown(object sender, EventArgs e)
		{
			this.DefineSelectedIndex(this.SelectedIndex - 10);
		}
		private void interBtnDown10_MouseLeftDown(object sender, EventArgs e)
		{
			this.DefineSelectedIndex(this.SelectedIndex + 10);
		}
		



		private Point MousePos { get { return this.ImageBox.PointToClient(Cursor.Position); } }
		private Rectangle MouseRec { get { return new Rectangle(this.MousePos, new Size(1, 1)); } }

		private List<uiButton> listButton = new List<uiButton>();

		//retourne true si la souris est mouse left down sur un controle et effectue la chaine pour faire caller les event.
		//retourne false si la souris est dans le vide
		private bool Check_MouseLeftDown()
		{
			Rectangle mrec = this.MouseRec;
			foreach (uiButton b in this.listButton)
			{
				if (b.rec.IntersectsWith(mrec))
				{
					b.Check_MouseLeftDown();
					return true;
				}
			}

			return false;
		}

		private void Check_MouseLeftUp()
		{
			foreach (uiButton b in this.listButton)
			{
				b.Check_MouseLeftUp();
			}
		}

		//retourne si la souris est sur un control
		private bool IsMouseOnAnyControl()
		{
			Rectangle mrec = this.MouseRec;
			foreach (uiButton b in this.listButton)
			{
				if (b.rec.IntersectsWith(mrec))
				{
					return true;
				}
			}
			return false;
		}



		//s'ajoute automatiquement à son parent
		private class uiButton
		{
			public bool IsMouseLeftDown = false; //indique si mouse left est down sur this

			public CylinderList Parent = null;
			public Rectangle rec = new Rectangle(0, 0, 100, 30);
			public int Top
			{
				get { return this.rec.Y; }
				set { this.rec.Y = value; }
			}
			public int Left
			{
				get { return this.rec.X; }
				set { this.rec.X = value; }
			}
			public void SetSize(int newWidth, int newHeight)
			{
				this.Width = newWidth;
				this.Height = newHeight;
			}
			public void SetSize(Size newSize)
			{
				this.Width = newSize.Width;
				this.Height = newSize.Height;
			}
			public int Width
			{
				get { return this.rec.Width; }
				set { this.rec.Width = value; }
			}
			public int Height
			{
				get { return this.rec.Height; }
				set { this.rec.Height = value; }
			}

			public string Text = "notext";

			public uiButton(CylinderList sParent)
			{
				this.Parent = sParent;
				sParent.listButton.Add(this);

			}
			

			public event EventHandler MouseLeftDown;
			private void Raise_MouseLeftDown()
			{
				if (this.MouseLeftDown != null)
				{
					this.MouseLeftDown(this, new EventArgs());
				}
			}


			//ceci est callé par l'extérieur seulement si la souris est bel et bien sur this lors du mouse left down.
			public void Check_MouseLeftDown()
			{
				this.IsMouseLeftDown = true;
				this.Raise_MouseLeftDown();
			}

			//ceci est callé par l'extérieur lors de mouse left up, peut importe la position de la souris
			public void Check_MouseLeftUp()
			{
				this.IsMouseLeftDown = false;

			}

		}

		#endregion


	}
}

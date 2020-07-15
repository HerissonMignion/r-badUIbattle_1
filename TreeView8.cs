using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace FileExplorer4DirectionTreeView
{
	public class TreeView8
	{
		private Point MousePos { get { return this.ImageBox.PointToClient(Cursor.Position); } }
		private Rectangle MouseRec { get { return new Rectangle(this.MousePos, new Size(1, 1)); } }

		private PictureBox ImageBox;

		public Control Parent
		{
			get { return this.ImageBox.Parent; }
			set { this.ImageBox.Parent = value; }
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




		#region gestion de l'arbre

		private TreeObject Root = null;
		private TreeObject ActualElement = null; //élément actuellement sélectionné par l'user




		private enum TOType
		{
			folder,
			file
		}
		private enum Dir
		{
			Up, // 1
			UpLeft, // 4
			Left, // 7
			DownLeft, // 2
			Down, // 5
			DownRight, // 8
			Right, // 3
			UpRight, // 6
		}
		private class TreeObject
		{
			private TreeView8 TV = null;

			public TreeObject Parent = null;
			public Dir dir = Dir.DownLeft;
			public void SetThisAsNextDirectionOf(Dir d)
			{
				if (d == Dir.Up) { this.dir = Dir.DownLeft; }
				else if (d == Dir.DownLeft) { this.dir = Dir.Right; }
				else if (d == Dir.Right) { this.dir = Dir.UpLeft; }
				else if (d == Dir.UpLeft) { this.dir = Dir.Down; }
				else if (d == Dir.Down) { this.dir = Dir.UpRight; }
				else if (d == Dir.UpRight) { this.dir = Dir.Left; }
				else if (d == Dir.Left) { this.dir = Dir.DownRight; }
				else if (d == Dir.DownRight) { this.dir = Dir.Up; }
			}

			public TOType tType = TOType.file;
			public bool IsFolder { get { return this.tType == TOType.folder; } }
			public string Path = "";
			public string UiName = "";


			//text finale à afficher. si this est un dossier, il contiendra déjà le + ou le - au bon endroit.
			public string UiFinalText
			{
				get
				{
					//si this est un dossier, il faut rajouter le + ou le - au bon endroit
					if (this.IsFolder)
					{
						string c = "+";
						if (this.IsOpen) { c = "-"; }

						//si le caractère va au début
						if (this.dir == Dir.Right || this.dir == Dir.UpRight || this.dir == Dir.Up || this.dir == Dir.UpLeft)
						{
							return c + this.UiName;
						}

						//si le caractère va à la fin
						return this.UiName + c;
					}

					//si this est un fichier
					return this.UiName;
				}
			}
			private float zzzUiFinalTextWidth = 0f; //laugeur graphique du text finale s'il est parallère "au sol".
			private float UiFinalTextWidth
			{
				get
				{
					if (!this.isFinalComputed) { this.ComputeFinal(); }
					return this.zzzUiFinalTextWidth;
				}
			}
			private bool isFinalComputed = false;
			private void ComputeFinal()
			{
				this.zzzUiFinalTextWidth = this.TV.MeasureString(this.UiFinalText, this.TV.uiItemFont).Width;
				this.isFinalComputed = true;
			}



			//folder properties
			public bool NotEmpty { get { return this.listChild.Count > 0; } }
			public bool IsOpen = false; //indique si le dossier est actuellement ouvert ou fermé à l'écran
			public bool ChildLoaded = false; //indique si les élément enfant de this ont été chargé dans la liste des enfant de this
			public List<TreeObject> listChild = new List<TreeObject>();

			public void LoadChild()
			{
				if (this.IsFolder && !this.ChildLoaded)
				{
					//load les dossier
					try
					{
						string[] arFolders = System.IO.Directory.GetDirectories(this.Path);
						foreach (string FolderPath in arFolders)
						{
							TreeObject newto = new TreeObject(this, FolderPath, TOType.folder, this.TV);
						}

						//load les fichier
						string[] arFiles = System.IO.Directory.GetFiles(this.Path);
						foreach (string FilePath in arFiles)
						{
							TreeObject newto = new TreeObject(this, FilePath, TOType.file, this.TV);
						}
					}
					catch { }

					this.ChildLoaded = true;
				}
			}
			public void Open()
			{
				if (this.IsFolder)
				{
					//make sure que les enfant ont été loadé
					if (!this.ChildLoaded) { this.LoadChild(); }
					this.IsOpen = true;
				}
			}
			public void Close()
			{
				if (this.IsFolder)
				{
					this.IsOpen = false;
				}
			}
			public void RecursiveCloseAll()
			{
				if (this.IsFolder && this.IsOpen)
				{
					foreach (TreeObject to in this.listChild)
					{
						to.RecursiveCloseAll();
					}
					this.Close();
				}
			}
			public void RecursiveOpenAll()
			{
				if (this.IsFolder)
				{
					if (!this.IsOpen)
					{
						this.Open();
					}
					foreach (TreeObject to in this.listChild)
					{
						to.RecursiveOpenAll();
					}
				}
			}


			//ce constructeur c'est pour quand les propriété sont défini depuis l'extérieur après l'initialisation de l'object
			public TreeObject(string sUiName, TreeView8 sTV)
			{
				this.TV = sTV;

				this.Parent = null;
				this.Path = "";
				this.UiName = sUiName;
			}
			//this s'ajoute automatiquement au parent spécifié
			public TreeObject(TreeObject sParent, string sPath, TOType stType, TreeView8 sTV)
			{
				this.TV = sTV;

				this.Parent = sParent;
				if (sParent != null)
				{
					sParent.listChild.Add(this);
					this.SetThisAsNextDirectionOf(sParent.dir);
				}

				this.Path = sPath;
				this.UiName = System.IO.Path.GetFileName(sPath);

				this.tType = stType;

			}





			//dessine le nom et les sous objet de this à partir de la coordonné graphique spécifié
			public void DrawFullAt(float uix, float uiy, Graphics g)
			{

				this.DrawNameAt(uix, uiy, g);

				//si this est un folder et qu'il est ouvert, on dessine la racine
				if (this.IsFolder)
				{
					if (this.IsOpen)
					{
						float R2 = (float)(Math.Sqrt(2d));
						float rooty = uiy;
						float rootx = uix + this.UiFinalTextWidth; //si le text est à dessiner horizontalement à droite

						//si le text est à dessiner horizontalement à gauche
						if (this.dir == Dir.DownLeft || this.dir == Dir.Down || this.dir == Dir.Left)
						{
							rootx = uix - this.UiFinalTextWidth;
						}

						//les cas où le text est à dessiner en diagonalde
						if (this.dir == Dir.UpLeft)
						{
							rooty = uiy - (this.UiFinalTextWidth / R2);
							rootx = uix + (this.UiFinalTextWidth / R2);
						}
						if (this.dir == Dir.DownRight)
						{
							rooty = uiy + (this.UiFinalTextWidth / R2);
							rootx = uix - (this.UiFinalTextWidth / R2);
						}




						//on fait dessiner la root à l'endroit prévu
						this.DrawSubAt(rootx, rooty, g);



						////on rajoute le caractère "+" à UiName parce que les dossier ont toujours un caractère de plus qui indique s'ils sont ouvert ou fermé
						//int namewidth = (int)(g.MeasureString(this.UiName + "+", this.TV.uiItemFont).Width);
						////position horizontal à laquelle dessiner la root
						//int rootx = uix + namewidth;
						//if (this.dir == Dir.UpLeft || this.dir == Dir.DownLeft) { rootx = uix - namewidth; }

						////fait dessiner la root à l'endroit prévu
						//this.DrawSubAt(rootx, uiy, g);

					}
				}


			}


			//basé sur les dernière coordonné graphique enregistré, retourne l'objet situé sous la coordonné graphique spécifié
			public TreeObject GetObjetUnderPos(int uix, int uiy)
			{
				//on commence par checker this. le vérification sera différente si le parent se déroule dans une direction horizontale (Right, Left) parce que dans ce cas les éléments sont dessinés en diagonalde
				if (this.dir != Dir.UpLeft && this.dir != Dir.DownRight)
				{
					if (this.uiLastX <= uix && uix < this.uiLastX + this.uiLastWidth)
					{
						if (this.uiLastY <= uiy && uiy < this.uiLastY + (int)(this.TV.uifItemTextHeight))
						{
							return this;
						}
					}
				}
				else //this est dessiné en diagonalde
				{
					//le parent va vers la droite ou vers la gauche
					if (this.dir == Dir.UpLeft || this.dir == Dir.DownRight)
					{
						int x1 = this.uiLastX;
						int y1 = this.uiLastY;
						int x2 = uix;
						int y2 = uiy;
						float R2 = (float)(Math.Sqrt(2d));

						if (y2 - y1 <= x2 - x1)
						{
							if (y2 - y1 >= x2 - x1 - (int)(2f * (float)(this.uiLastWidth) / R2))
							{
								if (Math.Abs(x2 - ((x1 + x2 + y1 - y2) / 2)) <= (int)(this.TV.uifItemTextHeight / 2f / R2))
								{
									return this;
								}
							}
						}

					}
				}

				//si this est un dossier OUVERT, il faut maintenant vérifier les enfant
				if (this.IsFolder && this.IsOpen)
				{
					foreach (TreeObject to in this.listChild)
					{
						TreeObject torep = to.GetObjetUnderPos(uix, uiy);
						if (torep != null) { return torep; }
					}
				}

				return null;
			}

			//coordonné graphique du text en haut à gauche lors du dernier refresh graphique. mis à jour par DrawNameAt(,,)
			public int uiLastX = 0;
			public int uiLastY = 0;
			public int uiLastWidth = 0;

			//dessine le nom de this à partir de la coordonné graphique spécifié
			public void DrawNameAt(float uix, float uiy, Graphics g)
			{
				float uifItemTextHeight = this.TV.uifItemTextHeight;
				float R2 = this.TV.Sqrt2;

				//si le nom est à dessiner à droite
				if (this.dir == Dir.Up || this.dir == Dir.Right || this.dir == Dir.UpRight)
				{
					//	Rectangle NameRect = new Rectangle(uix, uiy - (int)(this.TV.uifItemTextHeight / 2f), NameWidth, (int)(this.TV.uifItemTextHeight));
					Rectangle NameRect = new Rectangle((int)uix, (int)(uiy - (uifItemTextHeight / 2f)), (int)(this.UiFinalTextWidth), (int)uifItemTextHeight);
					this.uiLastX = NameRect.X;
					this.uiLastY = NameRect.Y;
					this.uiLastWidth = NameRect.Width;

					//on dessine l'item
					if (this.IsFolder)
					{
						g.FillRectangle(this.TV.uiFolderBackBrush, NameRect);
					}
					g.DrawString(this.UiFinalText, this.TV.uiItemFont, Brushes.White, uix, uiy - (uifItemTextHeight / 2f));

					//si this est l'élément actuellement sélectionné par l'user, on dessine un rectangle autour du nom
					if (this.TV.ActualElement == this)
					{
						g.DrawRectangle(this.TV.uiHighlightPen, NameRect);
					}

				}
				
				//si le nom est à dessiner à gauche
				if (this.dir == Dir.DownLeft || this.dir == Dir.Down || this.dir == Dir.Left)
				{
					//	Rectangle NameRect = new Rectangle(uix, uiy - (int)(this.TV.uifItemTextHeight / 2f), NameWidth, (int)(this.TV.uifItemTextHeight));
					Rectangle NameRect = new Rectangle((int)(uix - this.UiFinalTextWidth), (int)(uiy - (uifItemTextHeight / 2f)), (int)(this.UiFinalTextWidth), (int)uifItemTextHeight);
					this.uiLastX = NameRect.X;
					this.uiLastY = NameRect.Y;
					this.uiLastWidth = NameRect.Width;

					//on dessine l'item
					if (this.IsFolder)
					{
						g.FillRectangle(this.TV.uiFolderBackBrush, NameRect);
					}
					g.DrawString(this.UiFinalText, this.TV.uiItemFont, Brushes.White, uix - this.UiFinalTextWidth, uiy - (uifItemTextHeight / 2f));

					//si this est l'élément actuellement sélectionné par l'user, on dessine un rectangle autour du nom
					if (this.TV.ActualElement == this)
					{
						g.DrawRectangle(this.TV.uiHighlightPen, NameRect);
					}

				}


				//si le nom est à dessiner en diagonalde, le parent défile vers la droite
				if (this.dir == Dir.UpLeft)
				{
					//	Rectangle NameRect = new Rectangle(uix, uiy - (int)(this.TV.uifItemTextHeight / 2f), NameWidth, (int)(this.TV.uifItemTextHeight));
					Rectangle NameRect = new Rectangle((int)(uix), (int)(uiy - (uifItemTextHeight / 2f)), (int)(this.UiFinalTextWidth), (int)uifItemTextHeight);
					this.uiLastX = NameRect.X;
					this.uiLastY = NameRect.Y + (int)(uifItemTextHeight / 2f); //il faut ajouter la moitier de la hauteur parce que la formule qui calcul si un point est dessus un rectangle en angle de 45 considère que la coordonné (x,y) est le milieu du côté vertical, et Non le coin supérieur gauche
					this.uiLastWidth = NameRect.Width;


					g.TranslateTransform(uix, uiy);
					g.RotateTransform(-45f);

					//on dessine l'item
					if (this.IsFolder)
					{
						g.FillRectangle(this.TV.uiFolderBackBrush, 0, (int)(-NameRect.Height / 2f), (int)(NameRect.Width), (int)(NameRect.Height));
					}
					g.DrawString(this.UiFinalText, this.TV.uiItemFont, Brushes.White, 0f, uifItemTextHeight / -2f);

					//si this est l'élément actuellement sélectionné par l'user, on dessine un rectangle autour du nom
					if (this.TV.ActualElement == this)
					{
						g.DrawRectangle(this.TV.uiHighlightPen, 0, (int)(-NameRect.Height / 2f), (int)(NameRect.Width), (int)(NameRect.Height));
					}

					g.ResetTransform();

				}
				//si le nom est à dessiner en diagonalde, le parent défile vers la gauche
				if (this.dir == Dir.DownRight)
				{
					//	Rectangle NameRect = new Rectangle(uix, uiy - (int)(this.TV.uifItemTextHeight / 2f), NameWidth, (int)(this.TV.uifItemTextHeight));
					Rectangle NameRect = new Rectangle((int)(uix), (int)(uiy - (uifItemTextHeight / 2f)), (int)(this.UiFinalTextWidth), (int)uifItemTextHeight);
					this.uiLastX = NameRect.X - (int)(this.UiFinalTextWidth / R2);
					this.uiLastY = NameRect.Y + (int)(this.UiFinalTextWidth / R2) + (int)(uifItemTextHeight / 2f); //il faut ajouter la moitier de la hauteur parce que la formule qui calcul si un point est dessus un rectangle en angle de 45 considère que la coordonné (x,y) est le milieu du côté vertical, et Non le coin supérieur gauche
					this.uiLastWidth = NameRect.Width;


					g.TranslateTransform(uix, uiy);
					g.RotateTransform(-45f);

					//on dessine l'item
					if (this.IsFolder)
					{
						g.FillRectangle(this.TV.uiFolderBackBrush, -(NameRect.Width), (int)(-NameRect.Height / 2f), (int)(NameRect.Width), (int)(NameRect.Height));
					}
					g.DrawString(this.UiFinalText, this.TV.uiItemFont, Brushes.White, -this.UiFinalTextWidth, uifItemTextHeight / -2f);

					//si this est l'élément actuellement sélectionné par l'user, on dessine un rectangle autour du nom
					if (this.TV.ActualElement == this)
					{
						g.DrawRectangle(this.TV.uiHighlightPen, (int)(-NameRect.Width), (int)(-NameRect.Height / 2f), (int)(NameRect.Width), (int)(NameRect.Height));
					}

					g.ResetTransform();

				}



				////fait la chaine de text qui est le nom
				//string strName = this.UiName;

				////dessine le nom selon que le nom doit aller à droite ou à gauche
				//if (this.dir == Dir.DownRight || this.dir == Dir.UpRight)
				//{
				//	//on ajoute le caractère qui indique si le dossier est ouvert ou fermé
				//	if (this.IsOpen)
				//	{
				//		strName = "-" + strName;
				//	}
				//	else { strName = "+" + strName; }

				//	//on obtien le rectangle d'arrière plan
				//	int NameWidth = (int)(g.MeasureString(strName, this.TV.uiItemFont).Width);
				//	Rectangle NameRect = new Rectangle(uix, uiy - (int)(this.TV.uifItemTextHeight / 2f), NameWidth, (int)(this.TV.uifItemTextHeight));
				//	//sauvgarde la position
				//	this.uiLastX = NameRect.X;
				//	this.uiLastY = NameRect.Y;
				//	this.uiLastWidth = NameRect.Width;

				//	//on remplit l'arrière plan
				//	g.FillRectangle(this.TV.uiFolderBackBrush, NameRect);

				//	//on le dessine à droite
				//	g.DrawString(strName, this.TV.uiItemFont, Brushes.White, (float)uix, (float)uiy - (this.TV.uifItemTextHeight / 2f));

				//	//si this est l'élément actuellement sélectionné par l'user, on dessine un rectangle autour du nom
				//	if (this.TV.ActualElement == this)
				//	{
				//		g.DrawRectangle(this.TV.uiHighlightPen, NameRect);
				//	}

				//}
				//if (this.dir == Dir.UpLeft || this.dir == Dir.DownLeft)
				//{
				//	//on ajoute le caractère qui indique si le dossier est ouvert ou fermé
				//	if (this.IsOpen)
				//	{
				//		strName += "-";
				//	}
				//	else { strName += "+"; }

				//	//on obtien le rectangle d'arrière plan
				//	int NameWidth = (int)(g.MeasureString(strName, this.TV.uiItemFont).Width);
				//	Rectangle NameRect = new Rectangle(uix - NameWidth, uiy - (int)(this.TV.uifItemTextHeight / 2f), NameWidth, (int)(this.TV.uifItemTextHeight));
				//	//sauvgarde la position
				//	this.uiLastX = NameRect.X;
				//	this.uiLastY = NameRect.Y;
				//	this.uiLastWidth = NameRect.Width;

				//	//on remplit l'arrière plan
				//	g.FillRectangle(this.TV.uiFolderBackBrush, NameRect);

				//	//on le dessine à gauche
				//	g.DrawString(strName, this.TV.uiItemFont, Brushes.White, (float)(uix - NameWidth), (float)uiy - (this.TV.uifItemTextHeight / 2f));

				//	//si this est l'élément actuellement sélectionné par l'user, on dessine un rectangle autour du nom
				//	if (this.TV.ActualElement == this)
				//	{
				//		g.DrawRectangle(this.TV.uiHighlightPen, NameRect);
				//	}

				//}


			}

			//si this est un dossier, dessine tout les élément enfant, dans la bonne direction, en commencant la racine à l'endroit spécifié
			public void DrawSubAt(float uix, float uiy, Graphics g)
			{
				if (this.IsFolder)
				{
					Pen penLineDiag = new Pen(Color.Silver, 2f); //Pens.Silver;
					float uiDiagSpace = this.TV.uiDiagSpace;
					float R2 = this.TV.Sqrt2;
					

					if (this.dir == Dir.Up)
					{
						float ActualUiX = uix;
						float ActualUiY = uiy;

						float CurrentMaxHWidth = this.UiFinalTextWidth - uiDiagSpace; //width horizontale maximal actuel

						float AccumulatedDownHeight = 0f;

						int index = 0;
						while (index < this.listChild.Count)
						{
							TreeObject to = this.listChild[index];
							//on monte d'un item et on dessine la ligne
							g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX, ActualUiY - uiDiagSpace);
							ActualUiY -= uiDiagSpace;
							CurrentMaxHWidth += uiDiagSpace;


							//check si c'est un dossier ouvert
							if (to.IsFolder && to.IsOpen && to.NotEmpty)
							{
								//on ajoute le upheight et le accumulated down height
								float dist = to.UpHeight + AccumulatedDownHeight;
								g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX, ActualUiY - dist);
								ActualUiY -= dist;
								CurrentMaxHWidth += dist;

								AccumulatedDownHeight = to.DownHeight + (uiDiagSpace * R2);
							}

							//on check si l'élément est trop large pour la hauteur actuel
							float diff = (2f * to.Width / R2) - to.DownHeight - CurrentMaxHWidth + uiDiagSpace;
							if (diff > 0f)
							{
								g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX, ActualUiY - diff);
								ActualUiY -= diff;
								CurrentMaxHWidth += diff;
							}

							//le down height accumulé diminue
							AccumulatedDownHeight -= uiDiagSpace;
							if (AccumulatedDownHeight < 0f) { AccumulatedDownHeight = 0f; }
							if (AccumulatedDownHeight < to.DownHeight + (uiDiagSpace * R2)) { AccumulatedDownHeight = to.DownHeight + (uiDiagSpace * R2); }


							////on fait dessiner l'item
							//on dessine une "coche"
							g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - uiDiagSpace, ActualUiY);

							//on lui fait dessiner l'item
							to.DrawFullAt(ActualUiX - uiDiagSpace, ActualUiY, g);


							//next iteration
							index++;
						}

					}
					else if (this.dir == Dir.DownLeft)
					{

						float ActualUiX = uix;
						float ActualUiY = uiy;

						float CurrentMaxEffectiveWidth = this.UiFinalTextWidth - uiDiagSpace; //largeur actuel maximal d'un élément. augmente en descendant

						int index = 0;
						while (index < this.listChild.Count)
						{
							TreeObject to = this.listChild[index];
							//on descend d'un item et on dessine la ligne
							g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - uiDiagSpace, ActualUiY + uiDiagSpace);
							ActualUiX -= uiDiagSpace;
							ActualUiY += uiDiagSpace;
							CurrentMaxEffectiveWidth += uiDiagSpace;


							////analyse s'il faut descendre encore d'avantage parce que l'élément est trop large pour la hauteur actuel
							//if (to.Width > CurrentMaxWidth)
							//{
							//	//on se déplace de la distance nécessaire pour que l'élément puisse rentrer au complet
							//	float diff = to.Width - CurrentMaxWidth;
							//	g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - diff, ActualUiY + diff);
							//	ActualUiX -= diff;
							//	ActualUiY += diff;
							//	CurrentMaxWidth += diff;
							//}

							//si c'est un dossier ouvert
							if (to.IsFolder && to.IsOpen && to.NotEmpty)
							{
								//on doit descendre de la hauteur du upheight
								float uhdr2 = to.UpHeight / R2;
								g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - uhdr2, ActualUiY + uhdr2);
								ActualUiX -= uhdr2;
								ActualUiY += uhdr2;
								CurrentMaxEffectiveWidth += uhdr2;
							}

							//on check si le effective width et trop grand pour la hauteur actuel
							if (to.EffectiveWidth > CurrentMaxEffectiveWidth)
							{
								float diff = to.EffectiveWidth - CurrentMaxEffectiveWidth;
								g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - diff, ActualUiY + diff);
								ActualUiX -= diff;
								ActualUiY += diff;
								CurrentMaxEffectiveWidth += diff;
							}



							////on fait dessiner l'item
							//on dessine une "coche"
							g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + uiDiagSpace, ActualUiY);

							//on lui fait dessiner l'item
							to.DrawFullAt(ActualUiX + uiDiagSpace, ActualUiY, g);


							//next iteration
							index++;
						}


					}
					else if (this.dir == Dir.Right)
					{
						float ActualUiX = uix;
						float ActualUiY = uiy;


						//le downheight accumulé précédement. lorsqu'un dossier est ouvert, il doit se déplacer de cette distance pour ne pas chevaucher les dossier précédant
						float AccumulatedDownHeight = 0f;

						//hauteur graphique maximal actuel
						float CurrentMaxVWidth = this.UiFinalTextWidth - (uiDiagSpace * R2);

						int index = 0;
						while (index < this.listChild.Count)
						{
							TreeObject to = this.listChild[index];
							//on descend d'un item et on dessine la ligne
							g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + (uiDiagSpace * R2), ActualUiY);
							ActualUiX += uiDiagSpace * R2;
							CurrentMaxVWidth += uiDiagSpace * R2;
							

							//on check si c'est un dossier ouvert
							if (to.IsFolder && to.IsOpen && to.NotEmpty)
							{
								//on doit ajouter le upheight et le accumulated down height
								float dist = to.UpHeight + AccumulatedDownHeight;
								g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + dist, ActualUiY);
								ActualUiX += dist;
								CurrentMaxVWidth += dist;

								//on check si le dossier a trop de width verticale pour la position horizontale actuel
								float diff = (to.Width / R2) - to.DownHeight + to.EffectiveWidth - CurrentMaxVWidth; //(2f * to.Width / R2) + to.UpHeight - CurrentMaxVWidth;
								if (diff > 0f)
								{
									g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + diff, ActualUiY);
									ActualUiX += diff;
									CurrentMaxVWidth += diff;
								}

								AccumulatedDownHeight = to.DownHeight + (uiDiagSpace * R2);
							}

							//le down height accumulé diminue
							AccumulatedDownHeight -= uiDiagSpace * R2;
							if (AccumulatedDownHeight < 0f) { AccumulatedDownHeight = 0f; }
							if (AccumulatedDownHeight < to.DownHeight + (uiDiagSpace * R2)) { AccumulatedDownHeight = to.DownHeight + (uiDiagSpace * R2); }


							////on fait dessiner l'item
							//on dessine une "coche"
							g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + uiDiagSpace, ActualUiY - uiDiagSpace);

							//on lui fait dessiner l'item
							to.DrawFullAt(ActualUiX + uiDiagSpace, ActualUiY - uiDiagSpace, g);


							//next iteration
							index++;
						}
					}
					else if (this.dir == Dir.UpLeft)
					{
						float ActualUiX = uix;
						float ActualUiY = uiy;

						float AccumulatedDownHeight = 0f; //le down height qui a été accumulé depuis les élément précédant

						float CurrentMaxWidth = 0f; //le width (ici c'est à la verticale) maximale qu'un dossier peut avoir à chaque instant

						int index = 0;
						while (index < this.listChild.Count)
						{
							TreeObject to = this.listChild[index];
							//on monte d'un item et on dessine la ligne
							g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - uiDiagSpace, ActualUiY - uiDiagSpace);
							ActualUiX -= uiDiagSpace;
							ActualUiY -= uiDiagSpace;
							CurrentMaxWidth += uiDiagSpace;


							//on check si c'est un dossier ouvert
							if (to.IsFolder & to.IsOpen && to.NotEmpty)
							{

								//on ajoute le upheight et le accumulated down height
								float dist = to.UpHeight + AccumulatedDownHeight;
								g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - (dist / R2), ActualUiY - (dist / R2));
								ActualUiX -= dist / R2;
								ActualUiY -= dist / R2;
								CurrentMaxWidth += dist / R2;

								//on check si le dossier a trop de width pour la hauteur actuel
								float diff = (to.Width - (to.DownHeight / R2)) - CurrentMaxWidth;
								if (diff > 0f)
								{
									//on se déplace d'autant qu'il le faut pour que le dossier ait assez de place
									g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - diff, ActualUiY - diff);
									ActualUiX -= diff;
									ActualUiY -= diff;
									CurrentMaxWidth += diff;
								}


								AccumulatedDownHeight = to.DownHeight + (uiDiagSpace * R2);
							}

							//le down height accumulé diminue
							AccumulatedDownHeight -= uiDiagSpace * R2;
							if (AccumulatedDownHeight < 0f) { AccumulatedDownHeight = 0f; }
							if (AccumulatedDownHeight < to.DownHeight + (uiDiagSpace * R2)) { AccumulatedDownHeight = to.DownHeight + (uiDiagSpace * R2); }


							////on fait dessiner l'item
							//on dessine une "coche"
							g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - uiDiagSpace, ActualUiY);

							//on lui fait dessiner l'item
							to.DrawFullAt(ActualUiX - uiDiagSpace, ActualUiY, g);

							
							//next iteration
							index++;
						}
						


					}
					else if (this.dir == Dir.Down)
					{
						float ActualUiX = uix;
						float ActualUiY = uiy;

						float CurrentMaxHWidth = this.UiFinalTextWidth - uiDiagSpace; //width horizontale maximal actuel

						float AccumulatedDownHeight = 0f;

						int index = 0;
						while (index < this.listChild.Count)
						{
							TreeObject to = this.listChild[index];
							//on monte d'un item et on dessine la ligne
							g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX, ActualUiY + uiDiagSpace);
							ActualUiY += uiDiagSpace;
							CurrentMaxHWidth += uiDiagSpace;


							//check si c'est un dossier ouvert
							if (to.IsFolder && to.IsOpen && to.NotEmpty)
							{
								//on ajoute le upheight et le accumulated down height
								float dist = to.UpHeight + AccumulatedDownHeight;
								g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX, ActualUiY + dist);
								ActualUiY += dist;
								CurrentMaxHWidth += dist;

								AccumulatedDownHeight = to.DownHeight + (uiDiagSpace * R2);
							}

							//on check si l'élément est trop large pour la hauteur actuel
							float diff = (2f * to.Width / R2) - to.DownHeight - CurrentMaxHWidth + uiDiagSpace;
							if (diff > 0f)
							{
								g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX, ActualUiY + diff);
								ActualUiY += diff;
								CurrentMaxHWidth += diff;
							}

							//le down height accumulé diminue
							AccumulatedDownHeight -= uiDiagSpace;
							if (AccumulatedDownHeight < 0f) { AccumulatedDownHeight = 0f; }
							if (AccumulatedDownHeight < to.DownHeight + (uiDiagSpace * R2)) { AccumulatedDownHeight = to.DownHeight + (uiDiagSpace * R2); }


							////on fait dessiner l'item
							//on dessine une "coche"
							g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + uiDiagSpace, ActualUiY);

							//on lui fait dessiner l'item
							to.DrawFullAt(ActualUiX + uiDiagSpace, ActualUiY, g);


							//next iteration
							index++;
						}
					}
					else if (this.dir == Dir.UpRight)
					{

						float ActualUiX = uix;
						float ActualUiY = uiy;

						float CurrentMaxEffectiveWidth = this.UiFinalTextWidth - uiDiagSpace; //largeur actuel maximal d'un élément. augmente en descendant

						int index = 0;
						while (index < this.listChild.Count)
						{
							TreeObject to = this.listChild[index];
							//on descend d'un item et on dessine la ligne
							g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + uiDiagSpace, ActualUiY - uiDiagSpace);
							ActualUiX += uiDiagSpace;
							ActualUiY -= uiDiagSpace;
							CurrentMaxEffectiveWidth += uiDiagSpace;

							
							//si c'est un dossier ouvert
							if (to.IsFolder && to.IsOpen && to.NotEmpty)
							{
								//on doit descendre de la hauteur du upheight
								float uhdr2 = to.UpHeight / R2;
								g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + uhdr2, ActualUiY - uhdr2);
								ActualUiX += uhdr2;
								ActualUiY -= uhdr2;
								CurrentMaxEffectiveWidth += uhdr2;
							}

							//on check si le effective width et trop grand pour la hauteur actuel
							if (to.EffectiveWidth > CurrentMaxEffectiveWidth)
							{
								float diff = to.EffectiveWidth - CurrentMaxEffectiveWidth;
								g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + diff, ActualUiY - diff);
								ActualUiX += diff;
								ActualUiY -= diff;
								CurrentMaxEffectiveWidth += diff;
							}



							////on fait dessiner l'item
							//on dessine une "coche"
							g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - uiDiagSpace, ActualUiY);

							//on lui fait dessiner l'item
							to.DrawFullAt(ActualUiX - uiDiagSpace, ActualUiY, g);


							//next iteration
							index++;
						}



					}
					else if (this.dir == Dir.Left)
					{
						float ActualUiX = uix;
						float ActualUiY = uiy;
						
						//le downheight accumulé précédement. lorsqu'un dossier est ouvert, il doit se déplacer de cette distance pour ne pas chevaucher les dossier précédant
						float AccumulatedDownHeight = 0f;

						//hauteur graphique maximal actuel
						float CurrentMaxVWidth = this.UiFinalTextWidth - (uiDiagSpace * R2);

						int index = 0;
						while (index < this.listChild.Count)
						{
							TreeObject to = this.listChild[index];
							//on descend d'un item et on dessine la ligne
							g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - (uiDiagSpace * R2), ActualUiY);
							ActualUiX -= uiDiagSpace * R2;
							CurrentMaxVWidth += uiDiagSpace * R2;


							//on check si c'est un dossier ouvert
							if (to.IsFolder && to.IsOpen && to.NotEmpty)
							{
								//on doit ajouter le upheight et le accumulated down height
								float dist = to.UpHeight + AccumulatedDownHeight;
								g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - dist, ActualUiY);
								ActualUiX -= dist;
								CurrentMaxVWidth += dist;

								//on check si le dossier a trop de width verticale pour la position horizontale actuel
								float diff = (to.Width / R2) - to.DownHeight + to.EffectiveWidth - CurrentMaxVWidth; //(2f * to.Width / R2) + to.UpHeight - CurrentMaxVWidth;
								if (diff > 0f)
								{
									g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - diff, ActualUiY);
									ActualUiX -= diff;
									CurrentMaxVWidth += diff;
								}

								AccumulatedDownHeight = to.DownHeight + (uiDiagSpace * R2);
							}

							//le down height accumulé diminue
							AccumulatedDownHeight -= uiDiagSpace * R2;
							if (AccumulatedDownHeight < 0f) { AccumulatedDownHeight = 0f; }
							if (AccumulatedDownHeight < to.DownHeight + (uiDiagSpace * R2)) { AccumulatedDownHeight = to.DownHeight + (uiDiagSpace * R2); }


							////on fait dessiner l'item
							//on dessine une "coche"
							g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - uiDiagSpace, ActualUiY + uiDiagSpace);

							//on lui fait dessiner l'item
							to.DrawFullAt(ActualUiX - uiDiagSpace, ActualUiY + uiDiagSpace, g);


							//next iteration
							index++;
						}
					}
					else if (this.dir == Dir.DownRight)
					{

						float ActualUiX = uix;
						float ActualUiY = uiy;

						float AccumulatedDownHeight = 0f; //le down height qui a été accumulé depuis les élément précédant

						float CurrentMaxWidth = 0f; //le width (ici c'est à la verticale) maximale qu'un dossier peut avoir à chaque instant

						int index = 0;
						while (index < this.listChild.Count)
						{
							TreeObject to = this.listChild[index];
							//on monte d'un item et on dessine la ligne
							g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + uiDiagSpace, ActualUiY + uiDiagSpace);
							ActualUiX += uiDiagSpace;
							ActualUiY += uiDiagSpace;
							CurrentMaxWidth += uiDiagSpace;


							//on check si c'est un dossier ouvert
							if (to.IsFolder & to.IsOpen && to.NotEmpty)
							{

								//on ajoute le upheight et le accumulated down height
								float dist = to.UpHeight + AccumulatedDownHeight;
								g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + (dist / R2), ActualUiY + (dist / R2));
								ActualUiX += dist / R2;
								ActualUiY += dist / R2;
								CurrentMaxWidth += dist / R2;

								//on check si le dossier a trop de width pour la hauteur actuel
								float diff = (to.Width - (to.DownHeight / R2)) - CurrentMaxWidth;
								if (diff > 0f)
								{
									//on se déplace d'autant qu'il le faut pour que le dossier ait assez de place
									g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + diff, ActualUiY + diff);
									ActualUiX += diff;
									ActualUiY += diff;
									CurrentMaxWidth += diff;
								}


								AccumulatedDownHeight = to.DownHeight + (uiDiagSpace * R2);
							}

							//le down height accumulé diminue
							AccumulatedDownHeight -= uiDiagSpace * R2;
							if (AccumulatedDownHeight < 0f) { AccumulatedDownHeight = 0f; }
							if (AccumulatedDownHeight < to.DownHeight + (uiDiagSpace * R2)) { AccumulatedDownHeight = to.DownHeight + (uiDiagSpace * R2); }


							////on fait dessiner l'item
							//on dessine une "coche"
							g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + uiDiagSpace, ActualUiY);

							//on lui fait dessiner l'item
							to.DrawFullAt(ActualUiX + uiDiagSpace, ActualUiY, g);


							//next iteration
							index++;
						}




					}

					penLineDiag.Dispose();
				}
			}




			public float Width = 0f;
			public float UpHeight = 0f;
			public float DownHeight = 0f;

			//utilisé par les direction left et right
			public float EffectiveWidth = 0f;




			public SizeWUD RecursiveComputeSize()
			{
				float aWidth = 0f;
				float aUpHeight = 0f;
				float aDownHeight = 0f;
				float aEffectiveWidth = 0f;


				float R2 = this.TV.Sqrt2;
				float uiDiagSpace = this.TV.uiDiagSpace;

				//check si this est un dossier ouvert
				if (this.IsFolder && this.IsOpen && this.listChild.Count > 0)
				{
					if (this.dir == Dir.Up || this.dir == Dir.Down)
					{

						aWidth = this.UiFinalTextWidth;

						float height = 0f; //le height total de this est le plus grand width qui existe à l'intérieur

						float CurrentMaxHWidth = this.UiFinalTextWidth - uiDiagSpace; //width horizontale maximal actuel

						float AccumulatedDownHeight = 0f;

						int index = 0;
						while (index < this.listChild.Count)
						{
							TreeObject to = this.listChild[index];
							SizeWUD s = to.RecursiveComputeSize();
							
							//le width continue d'augmenter
							aWidth += uiDiagSpace;
							CurrentMaxHWidth += uiDiagSpace;


							//check si c'est un dossier ouvert
							if (to.IsFolder && to.IsOpen && to.NotEmpty)
							{
								//on ajoute le upheight et le accumulated down height
								float dist = to.UpHeight + AccumulatedDownHeight;
								aWidth += dist;
								CurrentMaxHWidth += dist;

								AccumulatedDownHeight = to.DownHeight + (uiDiagSpace * R2);
							}
							
							//on check si l'élément est trop large pour la hauteur actuel
							float diff = (2f * to.Width / R2) - to.DownHeight - CurrentMaxHWidth + uiDiagSpace;
							if (diff > 0f)
							{
								aWidth += diff;
								CurrentMaxHWidth += diff;
							}

							//le down height accumulé diminue
							AccumulatedDownHeight -= uiDiagSpace;
							if (AccumulatedDownHeight < 0f) { AccumulatedDownHeight = 0f; }
							if (AccumulatedDownHeight < to.DownHeight + (uiDiagSpace * R2)) { AccumulatedDownHeight = to.DownHeight + (uiDiagSpace * R2); }


							//on check le height de this
							if (to.Width > height) { height = to.Width; }
							
							//next iteration
							index++;
						}
						aDownHeight = this.UiFinalTextWidth * R2;
						aUpHeight = height - aDownHeight;
						if (aUpHeight < 0f) { aUpHeight = 0f; }


					}
					else if (this.dir == Dir.UpLeft || this.dir == Dir.DownRight)
					{
						aDownHeight = this.UiFinalTextWidth * R2;
						aWidth = this.UiFinalTextWidth; //va augmenter en parcourant les élément enfant
						aEffectiveWidth = 0f; //va augmenter en parcourant les élément enfant

						float height = 0f; //le height total correspond au plus grand width qui existe à l'intérieur

						float AccumulatedDownHeight = 0f; //le down height qui a été accumulé depuis les élément précédant

						float CurrentMaxWidth = 0f; //le width maximale qu'un dossier peut avoir à chaque instant

						int index = 0;
						while (index < this.listChild.Count)
						{
							TreeObject to = this.listChild[index];
							SizeWUD s = to.RecursiveComputeSize();

							//le width continue d'augmenter
							aWidth += uiDiagSpace * R2;
							CurrentMaxWidth += uiDiagSpace;


							if (to.IsFolder && to.IsOpen && to.NotEmpty)
							{
								//on ajoute le upheight et le accumulated down height
								float dist = to.UpHeight + AccumulatedDownHeight;
								aWidth += dist;
								CurrentMaxWidth += dist / R2;

								//on check si le dossier a trop de width pour la hauteur actuel
								float diff = (to.Width - ((to.DownHeight) / R2)) - CurrentMaxWidth;
								if (diff > 0f)
								{
									aWidth += diff * R2;
									CurrentMaxWidth += diff;
								}

								AccumulatedDownHeight = to.DownHeight + (uiDiagSpace * R2);
							}

							//le down height accumulé diminue
							AccumulatedDownHeight -= uiDiagSpace * R2;
							if (AccumulatedDownHeight < 0f) { AccumulatedDownHeight = 0f; }
							if (AccumulatedDownHeight < to.DownHeight + (uiDiagSpace * R2)) { AccumulatedDownHeight = to.DownHeight + (uiDiagSpace * R2); }

							//on check pour effective width
							if (aWidth / R2 + uiDiagSpace + to.UiFinalTextWidth > aEffectiveWidth)
							{
								aEffectiveWidth = (aWidth / R2) + uiDiagSpace + to.UiFinalTextWidth;
							}

							//on check si le height total est à augmenter
							if (to.Width > height) { height = to.Width; }
							
							//next iteration
							index++;
						}

						//le upheight est le height restant, qui n'est pas pris par downheight
						aUpHeight = height - aDownHeight;
						if (aUpHeight < 0f) { aUpHeight = 0f; }



					}
					else if (this.dir == Dir.DownLeft || this.dir == Dir.UpRight)
					{
						
						//le height total est égale au plus grand width à l'intérieur, et est au minimum la largeur du text du dossier
						float height = this.UiFinalTextWidth; //le height de départ. si qqc à l'intérieur a un width plus grand, alors on va donner ce width à cette variable

						aWidth = this.UiFinalTextWidth * R2;

						float CurrentMaxEffectiveWidth = this.UiFinalTextWidth - uiDiagSpace;

						int index = 0;
						while (index < this.listChild.Count)
						{
							TreeObject to = this.listChild[index];
							SizeWUD s = to.RecursiveComputeSize();
							CurrentMaxEffectiveWidth += uiDiagSpace;

							//on check le height total de this
							if (s.Width + uiDiagSpace > height) { height = s.Width + uiDiagSpace; }

							////on check si l'item est trop large pour la hauteur actuel
							//if (to.Width > CurrentMaxWidth)
							//{
							//	aWidth += (to.Width - CurrentMaxWidth) * R2;
							//	CurrentMaxWidth = to.Width;
							//}

							//on accumule sur la variable width
							aWidth += uiDiagSpace * R2;
							//si c'est un dossier ouvert, il faut ajouter à width le upheight du dossier
							if (to.IsFolder && to.IsOpen && to.NotEmpty)
							{
								aWidth += to.UpHeight;
								CurrentMaxEffectiveWidth += to.UpHeight / R2;
							}
							
							//on check si le effective width de l'élément est trop grand pour la hauteur actuel
							if (to.EffectiveWidth > CurrentMaxEffectiveWidth)
							{
								float diff = to.EffectiveWidth - CurrentMaxEffectiveWidth;
								aWidth += diff * R2;
								CurrentMaxEffectiveWidth += diff;
							}


							//next iteration
							index++;
						}

						//on calcul upheight et height à partir de height
						aDownHeight = this.UiFinalTextWidth;
						aUpHeight = height - aDownHeight;


					}
					else if (this.dir == Dir.Right || this.dir == Dir.Left)
					{
						aDownHeight = 0f;
						aUpHeight = 0f; //this.UiFinalTextWidth / R2; //sera augmenté lorsqu'on trouvera des enfant ayant un width plus grand que notre upheight
						
						aWidth = this.UiFinalTextWidth; //sera augmenté au fur et à mesure qu'on avance dans les enfant
						aEffectiveWidth = this.UiFinalTextWidth;

						//le downheight accumulé précédement. lorsqu'un dossier est ouvert, il doit se déplacer de cette distance pour ne pas chevaucher les dossier précédant
						float AccumulatedDownHeight = 0f;

						//hauteur graphique maximal actuel
						float CurrentMaxVWidth = this.UiFinalTextWidth - (uiDiagSpace * R2);

						int index = 0;
						while (index < this.listChild.Count)
						{
							TreeObject to = this.listChild[index];
							SizeWUD s = to.RecursiveComputeSize();


							//on avance horizontalement
							aWidth += uiDiagSpace * R2;
							CurrentMaxVWidth += uiDiagSpace * R2;

							//on check si c'est un dossier ouvert et non vide
							if (to.IsFolder && to.IsOpen && to.NotEmpty)
							{
								//on doit ajouter le upheight et le accumulated down height
								float dist = to.UpHeight + AccumulatedDownHeight;
								aWidth += dist;
								CurrentMaxVWidth += dist;

								//on check si le dossier a trop de width verticale pour la position horizontale actuel
								float diff = (to.Width / R2) - to.DownHeight + to.EffectiveWidth - CurrentMaxVWidth; //(2f * to.Width / R2) + to.UpHeight - CurrentMaxVWidth;
								if (diff > 0f)
								{
									aWidth += diff;
									CurrentMaxVWidth += diff;
								}

								AccumulatedDownHeight = to.DownHeight + (uiDiagSpace * R2);
							}

							//on vérifie effective width
							if (aWidth + uiDiagSpace + (to.UiFinalTextWidth / R2) > aEffectiveWidth)
							{
								aEffectiveWidth = aWidth + uiDiagSpace + (to.UiFinalTextWidth / R2);
							}


							//le down height accumulé diminue
							AccumulatedDownHeight -= uiDiagSpace * R2;
							if (AccumulatedDownHeight < 0f) { AccumulatedDownHeight = 0f; }
							if (AccumulatedDownHeight < to.DownHeight + (uiDiagSpace * R2)) { AccumulatedDownHeight = to.DownHeight + (uiDiagSpace * R2); }


							//on check pour le upheight de this
							if ((uiDiagSpace * R2) + to.Width > aUpHeight) { aUpHeight = (uiDiagSpace * R2) + to.Width; }
							

							//next iteration
							index++;
						}
						

					}
				}
				else //this est un fichier ou un dossier fermé
				{
					//les dossier fermé, ou ouvert et vide, et les fichier sont comparable donc on les traite de la même facon
					if (this.dir == Dir.Up || this.dir == Dir.Down)
					{
						aUpHeight = 0f;
						aDownHeight = this.UiFinalTextWidth * R2;
						aWidth = this.UiFinalTextWidth;
					}
					else if (this.dir == Dir.UpLeft || this.dir == Dir.DownRight)
					{
						aUpHeight = 0f;
						aWidth = this.UiFinalTextWidth;
						aDownHeight = this.UiFinalTextWidth * R2;
					}
					else if (this.dir == Dir.DownLeft || this.dir == Dir.UpRight)
					{
						aWidth = this.UiFinalTextWidth * R2;
						aUpHeight = 0f;
						aDownHeight = this.UiFinalTextWidth;
					}
					else if (this.dir == Dir.Right || this.dir == Dir.Left)
					{
						aWidth = this.UiFinalTextWidth;
						aUpHeight = 0f; //aWidth / R2;
						aDownHeight = 0f;
						aEffectiveWidth = this.UiFinalTextWidth;
					}
				}


				this.Width = aWidth;
				this.UpHeight = aUpHeight;
				this.DownHeight = aDownHeight;
				this.EffectiveWidth = aEffectiveWidth;
				return new SizeWUD(aWidth, aUpHeight, aDownHeight);
				//return sd.Add(this, Width, UpHeight, DownHeight);
			}



			//ce n'est pas la bonne facon d'implémenter un hash code mais je m'en fou pour autant que ca marche bien, ce qui est le cas ici
			private int zzzHashCode = TreeView8.GetNextHashCode();
			public override int GetHashCode()
			{
				return this.zzzHashCode;
			}
		}
		private static int NextHashCode = 0;
		private static int GetNextHashCode()
		{
			NextHashCode++;
			return NextHashCode;
		}




		private struct SizeWUD
		{
			public float Width;
			public float UpHeight;
			public float DownHeight;
			public SizeWUD(float sWidth, float sUpHeight, float sDownHeight)
			{
				this.Width = sWidth;
				this.UpHeight = sUpHeight;
				this.DownHeight = sDownHeight;
			}
		}

		//private class SDictionnary
		//{
		//	public Dictionary<TreeObject, SizeWUD> dict = new Dictionary<TreeObject, SizeWUD>();
		//	public SizeWUD Add(TreeObject to, float Width, float UpHeight, float DownHeight)
		//	{
		//		SizeWUD s = new SizeWUD(Width, UpHeight, DownHeight);
		//		this.dict.Add(to, s);
		//		return s;
		//	}
		//	public SizeWUD GetSize(TreeObject to)
		//	{
		//		return this.dict[to];
		//	}
		//	public SDictionnary()
		//	{
		//	}
		//}





		private void BuildTreeRoot()
		{
			TreeObject r = new TreeObject("This PC", this);
			r.tType = TOType.folder;
			r.dir = Dir.Up; // Up

			//TreeObject toC = new TreeObject(r, "C:\\", TOType.folder, this);
			//toC.UiName = "C:\\";
			//TreeObject toD = new TreeObject(r, "D:\\", TOType.folder, this);
			//toD.UiName = "D:\\";


			string[] arDrives = System.IO.Directory.GetLogicalDrives();
			foreach (string l in arDrives)
			{
				TreeObject to = new TreeObject(r, l, TOType.folder, this);
				to.UiName = l;
			}



			//////TESTEST
			//TreeObject newtest = new TreeObject(r, "test", TOType.folder, this);
			//newtest.UiName = "test asdhfiuahrguiaheugiauerhg";
			//newtest.ChildLoaded = true;
			//for (int i = 1; i <= 10; i++)
			//{
			//	string name = "f" + i.ToString();
			//	TreeObject newto = new TreeObject(newtest, "test:\\" + name, TOType.file, this);
			//	newto.UiName = name;
			//}
			//////END TESTEST
			


			r.ChildLoaded = true;

			this.Root = r;
			this.ActualElement = r;
		}

		#endregion
		#region navigation

		//à caller depuis l'extérieur, par celui qui recoit les touche
		public void KeyDown(KeyEventArgs e)
		{

			//navigation verticale dans le dossier
			if (e.KeyCode == Keys.Up)
			{
				////on remonte dans le dossier
				//on check si l'objet actuel a un parent
				if (this.ActualElement.Parent != null)
				{
					//nous recherchons l'index de l'élément actuel
					int ActualIndex = this.ActualElement.Parent.listChild.IndexOf(this.ActualElement);
					//on remonte de 1
					ActualIndex--;
					//si le nouveau index est négatif, alors on remonte au parent
					if (ActualIndex < 0)
					{
						this.ActualElement = this.ActualElement.Parent;
					}
					else
					{
						//si le nouveau index n'est pas négatif, il change pour l'item du nouveau index
						this.ActualElement = this.ActualElement.Parent.listChild[ActualIndex];
					}

				}
				else
				{
					//si l'objet actuel n'a pas de parent, alors nous somme à la racine et il n'y a rien à faire

				}

			}
			if (e.KeyCode == Keys.Down)
			{
				////on dessant dans le dossier
				//on check si l'objet actuel a un parent
				if (this.ActualElement.Parent != null)
				{
					//on obtien l'index de l'élément actuel
					int ActualIndex = this.ActualElement.Parent.listChild.IndexOf(this.ActualElement);
					//on augmente l'index pour passer à l'élément suivant
					ActualIndex++;
					//on make sure que l'index existe
					if (ActualIndex < this.ActualElement.Parent.listChild.Count)
					{
						//on set l'élément suivant comme le nouveau élément actuel
						this.ActualElement = this.ActualElement.Parent.listChild[ActualIndex];
					}

				}
				else
				{
					////si l'object actuel n'a pas de parent, ca veut dire qu'on est à la racine. on change donc pour le premier enfant
					//make sure que la racine (considéré comme un dossier) est ouverte
					if (!this.ActualElement.IsOpen) { this.ActualElement.Open(); }
					//on set l'élément actuel en tant que le premier élément enfant
					//met on check d'abord s'il y a des enfant à l'intérieur du dossier
					if (this.ActualElement.listChild.Count > 0)
					{
						this.ActualElement = this.ActualElement.listChild[0];
					}
				}

			}

			//touche enter qui fait ouvrire ou fermer un dossier
			if (e.KeyCode == Keys.Return)
			{
				//check si c'est un dossier
				if (this.ActualElement.IsFolder)
				{
					//si c'est fermé on l'ouvre ou l'inverse
					if (this.ActualElement.IsOpen)
					{
						this.ActualElement.Close();
						this.Root.RecursiveComputeSize();
					}
					else
					{
						this.ActualElement.Open();
						this.Root.RecursiveComputeSize();
					}
				}
			}

			//touche gauche droite qui fait sortir ou entrer dans un dossier
			if (e.KeyCode == Keys.Right)
			{
				//si c'est un dossier, on entre à l'intérieur
				if (this.ActualElement.IsFolder)
				{
					//make sure que le dossier est ouvert
					if (!this.ActualElement.IsOpen)
					{
						this.ActualElement.Open();
					}
					//si le dossier a des enfant, on défini l'élément actuel comme son premier enfant
					if (this.ActualElement.listChild.Count > 0)
					{
						this.ActualElement = this.ActualElement.listChild[0];
					}

				}
			}
			if (e.KeyCode == Keys.Left)
			{
				//on remonte au parent, s'il y en a un
				if (this.ActualElement.Parent != null)
				{
					this.ActualElement = this.ActualElement.Parent;
				}
			}

			//reset la position actuel
			if (e.KeyCode == Keys.Space)
			{
				this.RootDrawPos = new Point(0, 0);
			}

			this.RefreshImage();
		}



		private void ImageBox_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				//on make sure que l'user n'a pas clické sur un button
				if (!this.IsMouseOnAnyControl())
				{
					//on récupère l'object qui se trouve sous la souris
					TreeObject to = this.GetObjectUnderMouse();
					if (to != null)
					{
						//on défini cet élément comme l'élément "actuel" ou ayant le focus
						this.ActualElement = to;

						//si c'est un fichier, on fait rien. si c'est un dossier, on l'ouvre ou on le ferme
						if (to.IsFolder)
						{
							//on conserve sa position graphique
							int toLastX = to.uiLastX;
							int toLastY = to.uiLastY;

							//on ouvre ou on ferme le dossier
							if (to.IsOpen) { to.Close(); }
							else { to.Open(); }

							//maintenant on leur fait refresher leur coordonné graphique
							int imgWidth = this.Width;
							int imgHeight = this.Height;
							if (imgWidth < 50) { imgWidth = 50; }
							if (imgHeight < 50) { imgHeight = 50; }
							Point ppMiddle = new Point(imgWidth / 2, imgHeight / 2);
							Bitmap asdfimg = new Bitmap(10, 10);
							Graphics g = Graphics.FromImage(asdfimg);
							this.Root.RecursiveComputeSize();
							this.Root.DrawFullAt(ppMiddle.X + this.RootDrawPos.X, ppMiddle.Y + this.RootDrawPos.Y, g);
							g.Dispose();
							asdfimg.Dispose();



							//on réajuste la position graphique pour le dossier revient au même endroit où il était
							this.RootDrawPos.X -= to.uiLastX - toLastX;
							this.RootDrawPos.Y -= to.uiLastY - toLastY;


						}
						this.RefreshImage();
					}
					else //si la fonction a retourné null, alors il n'y a pas d'objet sous la souris et il faut plutot commencer un scroll
					{
						this.StartScroll();

					}
				}
				else //l'user a clické sur un button
				{
					this.Control_MouseLeftDown();
				}
			}
		}
		private void ImageBox_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				//si l'user est en train de scroller on arrête le scroll
				if (this.IsScrolling)
				{
					this.StopScroll();
				}


			}
		}
		private void ImageBox_MouseMove(object sender, MouseEventArgs e)
		{
			//if (this.IsScrolling)
			//{
			//	this.ReshowNetralScroll();
			//}
		}
		private void ImageBox_DoubleMouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (!this.IsMouseOnAnyControl())
				{
					//on récupère l'object qui se trouve sous la souris
					TreeObject to = this.GetObjectUnderMouse();
					if (to != null)
					{
						//on make sure que c'est un fichier
						if (to.tType == TOType.file)
						{
							try
							{
								//on run le fichier
								System.Diagnostics.Process.Start(to.Path);

							}
							catch { }
						}
					}
				}
			}
		}



		//retourne le TreeObject graphiquement actuellement sous la souris. retourne null s'il n'y en a aucun
		private TreeObject GetObjectUnderMouse()
		{
			return this.Root.GetObjetUnderPos(this.MousePos.X, this.MousePos.Y);
		}




		private Timer ScrollTimer = null;
		private void CreateScroll()
		{
			this.ScrollTimer = new Timer();
			this.ScrollTimer.Interval = 100; // 250
			this.ScrollTimer.Tick += new EventHandler(this.ScrollTimer_Tick);

		}
		private void ScrollTimer_Tick(object sender, EventArgs e)
		{
			Point mpos = this.MousePos;
			//calcul le déplacement de la souris
			int dx = mpos.X - this.ScrollStartPos.X;
			int dy = mpos.Y - this.ScrollStartPos.Y;

			//applique le déplacement
			this.RootDrawPos.X -= dx;
			this.RootDrawPos.Y -= dy;
			this.RefreshImage();

			//redessine la position neutre du scrolling
			this.ReshowNetralScroll();

		}

		private bool IsScrolling = false; //indique si l'user est en train de scroller
		private Point ScrollStartPos = new Point(0, 0); //point de départ du scrolling
		private void StartScroll()
		{
			this.IsScrolling = true;
			this.ScrollStartPos = this.MousePos;
			this.ScrollTimer.Start();
		}
		private void StopScroll()
		{
			this.IsScrolling = false;
			this.ScrollTimer.Stop();
			this.ImageBox.Refresh();
		}


		//réaffiche graphiquement la position neutre du scrolling avec la flèche qui pointe vers la souris
		private void ReshowNetralScroll()
		{
			this.ImageBox.Refresh();
			////redessine la position neutre du scrolling
			Graphics g = this.ImageBox.CreateGraphics();
			//g.FillRectangle(Brushes.White, this.ScrollStartPos.X - 3, this.ScrollStartPos.Y - 3, 6, 6);

			Point mpos = this.MousePos;
			//obtien le décalage de la position de la souris relativement à la position neutre du scrolling
			int dx = mpos.X - this.ScrollStartPos.X;
			int dy = mpos.Y - this.ScrollStartPos.Y;

			//obtien l'angle de la souris
			double radAngle = 0d;
			if (dx != 0d || dy != 0d)
			{
				radAngle = Math.Atan2((double)dy, (double)dx);
			}

			//converti en degré
			double degAngle = radAngle / Math.PI * 180d;

			//obtien et dessine l'image de la flèche
			Bitmap img = this.GetArrowAtAngle(degAngle);
			g.DrawImage(img, this.ScrollStartPos.X - (img.Width / 2), this.ScrollStartPos.Y - (img.Height / 2));
			img.Dispose();

			g.Dispose();
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

			g.FillPolygon(Brushes.MediumAquamarine, plArrowUp);
			g.FillPolygon(Brushes.SeaGreen, plArrowDown);
			g.FillPolygon(Brushes.MediumSeaGreen, plArrowMiddle);
			g.DrawPolygon(Pens.White, plArrow);

			g.Dispose();
			this.imgArrow = img;
		}


		private Bitmap GetArrowAtAngle(double angle)
		{
			int imgWidth = (int)((double)(this.imgArrow.Width) * 1.4142d);
			Bitmap img = new Bitmap(imgWidth, imgWidth);
			Graphics g = Graphics.FromImage(img);
			g.Clear(Color.Transparent);

			g.TranslateTransform((float)imgWidth / 2f, (float)imgWidth / 2f);
			g.RotateTransform((float)angle);
			g.TranslateTransform((float)imgWidth / -2f, (float)imgWidth / -2f);

			int delta = (imgWidth / 2) - (this.imgArrow.Width / 2);
			g.DrawImage(this.imgArrow, delta, delta);


			g.Dispose();
			return img;
		}



		#endregion


		public TreeView8()
		{
			this.ImageBox = new PictureBox();
			this.ImageBox.BackColor = Color.Black;
			this.ImageBox.SizeChanged += new EventHandler(this.ImageBox_SizeChanged);
			this.ImageBox.MouseDown += new MouseEventHandler(this.ImageBox_MouseDown);
			this.ImageBox.MouseUp += new MouseEventHandler(this.ImageBox_MouseUp);
			this.ImageBox.MouseMove += new MouseEventHandler(this.ImageBox_MouseMove);
			this.ImageBox.MouseDoubleClick += new MouseEventHandler(this.ImageBox_DoubleMouseClick);

			this.CreateScroll();
			this.CreateArrow();
			this.CreateInterface();

			this.BuildTreeRoot();
		}
		private void ImageBox_SizeChanged(object sender, EventArgs e)
		{
			this.RefreshImage();
		}



		#region graphique

		//juste pour le measure string
		private Graphics zzzggg = Graphics.FromImage(new Bitmap(10, 10));
		public SizeF MeasureString(string text, Font font)
		{
			return this.zzzggg.MeasureString(text, font);
		}



		private float Sqrt2 = (float)(Math.Sqrt(2d));


		private Font uiItemFont = new Font("consolas", 10f); // 10f
		private float uifItemTextHeight = 20f; // 8f ajusté lors de RefreshImage()

		private Pen uiHighlightPen = Pens.White; //pen utilisé pour dessiner un rectangle qui indique quel est l'élément actuellement focusé, ou juste "actuel"
		private Brush uiFolderBackBrush = new SolidBrush(Color.FromArgb(96, 96, 0)); //brush utilisé pour filler l'arrière plan du nom d'un dossier

		private float uiDiagSpace = 18; // 18 distance horizontale et vertical à parcourir pour passer immédiatement au prochain enfant en diagonalde



		private Point RootDrawPos = new Point(0, 0); //position graphique, relative au milieu de l'écran, à laquelle dessiner la root


		public void RefreshImage()
		{
			int imgWidth = this.Width;
			int imgHeight = this.Height;
			if (imgWidth < 50) { imgWidth = 50; }
			if (imgHeight < 50) { imgHeight = 50; }
			Bitmap img = new Bitmap(imgWidth, imgHeight);
			Graphics g = Graphics.FromImage(img);
			g.Clear(Color.FromArgb(16, 16, 16));

			Point ppMiddle = new Point(imgWidth / 2, imgHeight / 2);
			PointF fppMiddle = new PointF((float)imgWidth / 2f, (float)imgHeight / 2f);

			//obtien la hauteur du text d'un item
			float fItemTextHeight = this.MeasureString("asdfgathSTHS", this.uiItemFont).Height;
			this.uifItemTextHeight = fItemTextHeight;


			
			//this.Root.RecursiveComputeSize();



			this.Root.DrawFullAt(ppMiddle.X + this.RootDrawPos.X, ppMiddle.Y + this.RootDrawPos.Y, g);




			////testest
			//int asdf = 5000;
			//g.DrawLine(Pens.Lime, ppMiddle.X + this.RootDrawPos.X - this.uiDiagSpace, ppMiddle.Y + this.RootDrawPos.Y, ppMiddle.X + this.RootDrawPos.X - asdf - this.uiDiagSpace, ppMiddle.Y + this.RootDrawPos.Y - asdf);



			//on dessine les button
			foreach (uiButton btn in this.listButton)
			{
				g.FillRectangle(Brushes.DimGray, btn.rec);
				g.DrawRectangle(Pens.Silver, btn.rec);

				SizeF TextSizeF = g.MeasureString(btn.Text, btn.Font);
				g.DrawString(btn.Text, btn.Font, Brushes.White, (float)(btn.Left) + (btn.Width / 2f) - (TextSizeF.Width / 2f), (float)(btn.Top) + (btn.Height / 2f) - (TextSizeF.Height / 2f));
				

			}

			

			//////fin
			g.Dispose();
			if (this.ImageBox.Image != null) { this.ImageBox.Image.Dispose(); }
			this.ImageBox.Image = img;
			this.ImageBox.Refresh();
		}


		#endregion
		#region interface
		


		private void CreateInterface()
		{
			uiButton btnCloseAll = new uiButton(this);
			btnCloseAll.SetSize(100, 30);
			btnCloseAll.SetPos(5, 5);
			btnCloseAll.Text = "Close all";
			btnCloseAll.MouseLeftDown += new EventHandler((o, e) =>
			{
				this.Root.RecursiveCloseAll();
				this.RootDrawPos = new Point(0, 0);
				this.ActualElement = this.Root;
				this.RefreshImage();
			});

			uiButton btnExpandAll = new uiButton(this);
			btnExpandAll.SetSize(120, 30);
			btnExpandAll.SetPos(btnCloseAll.Left + btnCloseAll.Width + 5, 5);
			btnExpandAll.Text = "Expand all";
			btnExpandAll.MouseLeftDown += new EventHandler((o, e) =>
			{
				this.Root.RecursiveOpenAll();
				this.RootDrawPos = new Point(0, 0);
				this.ActualElement = this.Root;
				this.RefreshImage();
			});


		}
		#endregion
		

		private List<uiButton> listButton = new List<uiButton>();

		private bool IsMouseOnAnyControl()
		{
			Rectangle mrec = this.MouseRec;
			foreach (uiButton btn in this.listButton)
			{
				if (btn.rec.IntersectsWith(mrec))
				{
					return true;
				}
			}
			return false;
		}
		private void Control_MouseLeftDown()
		{
			Rectangle mrec = this.MouseRec;
			foreach (uiButton btn in this.listButton)
			{
				if (btn.rec.IntersectsWith(mrec))
				{
					btn.Raise_MouseLeftDown();
				}
			}
		}
		

		private class uiButton
		{
			private TreeView8 zzzParent;

			public Rectangle rec = new Rectangle(0, 0, 50, 30);
			public void SetPos(int newLeft, int newTop)
			{
				this.Left = newLeft;
				this.Top = newTop;
			}
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
			public Font Font = new Font("consolas", 10f);

			public uiButton(TreeView8 sParent)
			{
				this.zzzParent = sParent;
				sParent.listButton.Add(this);
			}

			public event EventHandler MouseLeftDown;
			public void Raise_MouseLeftDown()
			{
				if (this.MouseLeftDown != null)
				{
					this.MouseLeftDown(this, new EventArgs());
				}
			}



		}

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace FileExplorer4DirectionTreeView
{
	public class TreeView4
	{
		private Point MousePos { get { return this.ImageBox.PointToClient(Cursor.Position); } }

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
			DownLeft,
			DownRight,
			UpRight,
			UpLeft
		}
		private class TreeObject
		{
			private TreeView4 TV = null;

			public TreeObject Parent = null;
			public Dir dir = Dir.DownLeft;
			public void SetThisAsNextDirectionOf(Dir d)
			{
				if (d == Dir.DownLeft) { this.dir = Dir.DownRight; }
				if (d == Dir.DownRight) { this.dir = Dir.UpRight; }
				if (d == Dir.UpRight) { this.dir = Dir.UpLeft; }
				if (d == Dir.UpLeft) { this.dir = Dir.DownLeft; }
			}

			public TOType tType = TOType.file;
			public bool IsFolder { get { return this.tType == TOType.folder; } }
			public string Path = "";
			public string UiName = "";


			//folder properties
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

			//ce constructeur c'est pour quand les propriété sont défini depuis l'extérieur après l'initialisation de l'object
			public TreeObject(string sUiName, TreeView4 sTV)
			{
				this.TV = sTV;

				this.Parent = null;
				this.Path = "";
				this.UiName = sUiName;
			}
			//this s'ajoute automatiquement au parent spécifié
			public TreeObject(TreeObject sParent, string sPath, TOType stType, TreeView4 sTV)
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
			public void DrawFullAt(int uix, int uiy, Graphics g)
			{

				this.DrawNameAt(uix, uiy, g);

				//si this est un folder et qu'il est ouvert, on dessine la racine
				if (this.IsFolder)
				{
					if (this.IsOpen)
					{
						//on rajoute le caractère "+" à UiName parce que les dossier ont toujours un caractère de plus qui indique s'ils sont ouvert ou fermé
						int namewidth = (int)(g.MeasureString(this.UiName + "+", this.TV.uiItemFont).Width);
						//position horizontal à laquelle dessiner la root
						int rootx = uix + namewidth;
						if (this.dir == Dir.UpLeft || this.dir == Dir.DownLeft) { rootx = uix - namewidth; }

						//fait dessiner la root à l'endroit prévu
						this.DrawSubAt(rootx, uiy, g);
						
					}
				}


			}


			//basé sur les dernière coordonné graphique enregistré, retourne l'objet situé sous la coordonné graphique spécifié
			public TreeObject GetObjetUnderPos(int uix, int uiy)
			{
				//on commence par checker this
				if (this.uiLastX <= uix && uix < this.uiLastX + this.uiLastWidth)
				{
					if (this.uiLastY <= uiy && uiy < this.uiLastY + (int)(this.TV.uifItemTextHeight))
					{
						return this;
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
			public void DrawNameAt(int uix, int uiy, Graphics g)
			{
				if (this.IsFolder)
				{
					//fait la chaine de text qui est le nom
					string strName = this.UiName;

					//dessine le nom selon que le nom doit aller à droite ou à gauche
					if (this.dir == Dir.DownRight || this.dir == Dir.UpRight)
					{
						//on ajoute le caractère qui indique si le dossier est ouvert ou fermé
						if (this.IsOpen)
						{
							strName = "-" + strName;
						}
						else { strName = "+" + strName; }

						//on obtien le rectangle d'arrière plan
						int NameWidth = (int)(g.MeasureString(strName, this.TV.uiItemFont).Width);
						Rectangle NameRect = new Rectangle(uix, uiy - (int)(this.TV.uifItemTextHeight / 2f), NameWidth, (int)(this.TV.uifItemTextHeight));
						//sauvgarde la position
						this.uiLastX = NameRect.X;
						this.uiLastY = NameRect.Y;
						this.uiLastWidth = NameRect.Width;

						//on remplit l'arrière plan
						g.FillRectangle(this.TV.uiFolderBackBrush, NameRect);

						//on le dessine à droite
						g.DrawString(strName, this.TV.uiItemFont, Brushes.White, (float)uix, (float)uiy - (this.TV.uifItemTextHeight / 2f));

						//si this est l'élément actuellement sélectionné par l'user, on dessine un rectangle autour du nom
						if (this.TV.ActualElement == this)
						{
							g.DrawRectangle(this.TV.uiHighlightPen, NameRect);
						}

					}
					if (this.dir == Dir.UpLeft || this.dir == Dir.DownLeft)
					{
						//on ajoute le caractère qui indique si le dossier est ouvert ou fermé
						if (this.IsOpen)
						{
							strName += "-";
						}
						else { strName += "+"; }

						//on obtien le rectangle d'arrière plan
						int NameWidth = (int)(g.MeasureString(strName, this.TV.uiItemFont).Width);
						Rectangle NameRect = new Rectangle(uix - NameWidth, uiy - (int)(this.TV.uifItemTextHeight / 2f), NameWidth, (int)(this.TV.uifItemTextHeight));
						//sauvgarde la position
						this.uiLastX = NameRect.X;
						this.uiLastY = NameRect.Y;
						this.uiLastWidth = NameRect.Width;

						//on remplit l'arrière plan
						g.FillRectangle(this.TV.uiFolderBackBrush, NameRect);
						
						//on le dessine à gauche
						g.DrawString(strName, this.TV.uiItemFont, Brushes.White, (float)(uix - NameWidth), (float)uiy - (this.TV.uifItemTextHeight / 2f));

						//si this est l'élément actuellement sélectionné par l'user, on dessine un rectangle autour du nom
						if (this.TV.ActualElement == this)
						{
							g.DrawRectangle(this.TV.uiHighlightPen, NameRect);
						}

					}

				}
				else //si this est un fichier
				{
					//on check dans quel direction il faut dessiner le nom
					bool DrawToTheRight = true;
					if (this.Parent != null)
					{
						DrawToTheRight = this.Parent.dir == Dir.DownLeft || this.Parent.dir == Dir.DownRight;
					}

					//on dessine le nom
					if (DrawToTheRight)
					{
						//on obtien le rectangle d'arrière plan
						int NameWidth = (int)(g.MeasureString(this.UiName, this.TV.uiItemFont).Width);
						Rectangle NameRect = new Rectangle(uix, uiy - (int)(this.TV.uifItemTextHeight / 2f), NameWidth, (int)(this.TV.uifItemTextHeight));
						//sauvgarde la position
						this.uiLastX = NameRect.X;
						this.uiLastY = NameRect.Y;
						this.uiLastWidth = NameRect.Width;

						g.DrawString(this.UiName, this.TV.uiItemFont, Brushes.White, (float)uix, (float)uiy - (this.TV.uifItemTextHeight / 2f));

						//si this est l'élément actuellement sélectionné par l'user, on dessine un rectangle autour du nom
						if (this.TV.ActualElement == this)
						{
							g.DrawRectangle(this.TV.uiHighlightPen, NameRect);
						}

					}
					else
					{
						//on obtien le rectangle d'arrière plan
						int NameWidth = (int)(g.MeasureString(this.UiName, this.TV.uiItemFont).Width);
						Rectangle NameRect = new Rectangle(uix - NameWidth, uiy - (int)(this.TV.uifItemTextHeight / 2f), NameWidth, (int)(this.TV.uifItemTextHeight));
						//sauvgarde la position
						this.uiLastX = NameRect.X;
						this.uiLastY = NameRect.Y;
						this.uiLastWidth = NameRect.Width;

						g.DrawString(this.UiName, this.TV.uiItemFont, Brushes.White, (float)uix - g.MeasureString(this.UiName, this.TV.uiItemFont).Width, (float)uiy - (this.TV.uifItemTextHeight / 2f));

						//si this est l'élément actuellement sélectionné par l'user, on dessine un rectangle autour du nom
						if (this.TV.ActualElement == this)
						{
							g.DrawRectangle(this.TV.uiHighlightPen, NameRect);
						}

					}

				}
			}

			//si this est un dossier, dessine tout les élément enfant, dans la bonne direction, en commencant la racine à l'endroit spécifié
			public void DrawSubAt(int uix, int uiy, Graphics g)
			{
				if (this.IsFolder)
				{
					Pen penLineDiag = Pens.DimGray;
					int uiDiagSpace = this.TV.uiDiagSpace;

					if (this.dir == Dir.DownLeft)
					{
						//g.DrawLine(Pens.Red, uix, uiy, uix - 20, uiy + 20);

						//s'il n'y a pas d'enfant on fait une ligne vide
						if (this.listChild.Count <= 0)
						{
							g.DrawLine(Pens.Red, uix, uiy, uix - 20, uiy + 20);
						}
						else
						{
							int ActualUiX = uix;
							int ActualUiY = uiy;
							int CurrentMaxWidth = (int)(g.MeasureString(this.UiName + "+", this.TV.uiItemFont).Width); //largeur maximale d'un objet fermé pour ne pas embarquer sur un autre arbre. cette valeur augmente pendant qu'on dessend dans les enfant.
							int index = 0;
							while (index < this.listChild.Count)
							{
								//on passe à l'enfant suivant
								g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - uiDiagSpace, ActualUiY + uiDiagSpace);
								ActualUiX -= uiDiagSpace;
								ActualUiY += uiDiagSpace;

								TreeObject to = this.listChild[index];
								int toNameWidth = 0; //largeur du nom de l'objet
								if (to.IsFolder) { toNameWidth = (int)(g.MeasureString(to.UiName + "+", this.TV.uiItemFont).Width); }
								else { toNameWidth = (int)(g.MeasureString(to.UiName, this.TV.uiItemFont).Width); }
								
								//si c'est un dossier ouvert
								if (to.IsFolder && to.IsOpen)
								{
									//ajoute le up height
									int toUpHeight = to.GetUpHeight(g);
									int Delta = (int)((float)toUpHeight / 1.4142f);
									g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - Delta, ActualUiY + Delta);
									ActualUiX -= Delta;
									ActualUiY += Delta;

									//dessine une "coche" qui indique plus clairement à l'user que cet enfant apartient à cette ligne
									g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + uiDiagSpace, ActualUiY);

									//maintenant fait dessiner l'élément enfant
									to.DrawFullAt(ActualUiX + uiDiagSpace, ActualUiY, g);

									////ajoute le down height
									//int toDownHeight = to.GetDownHeight(g);
									//Delta = (int)((float)toDownHeight / 1.4142f);
									//g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - Delta, ActualUiY + Delta);
									//ActualUiX -= Delta;
									//ActualUiY += Delta;

									CurrentMaxWidth = toNameWidth - uiDiagSpace;

								}
								else //si c'est un dossier fermé ou un fichier
								{

									//check si le nom est trop large
									if (toNameWidth > CurrentMaxWidth)
									{
										//si le nom est trop large on doit lui ajouter sa upheight necessaire pour qu'il ne le touche plus

										//ajoute le up height
										//int toUpHeight = to.GetUpHeight(g);
										//int Delta = (int)((float)toUpHeight / 1.4142f);
										int Delta = toNameWidth - CurrentMaxWidth;
										Delta /= 2;

										g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - Delta, ActualUiY + Delta);
										ActualUiX -= Delta;
										ActualUiY += Delta;

										//maintenant qu'on a dessendu, la largeur maximal augmente
										CurrentMaxWidth += 2 * Delta;

									}



									//dessine une "coche" qui indique plus clairement à l'user que cet enfant apartient à cette ligne
									g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + uiDiagSpace, ActualUiY);

									//maintenant fait dessiner l'élément enfant
									to.DrawFullAt(ActualUiX + uiDiagSpace, ActualUiY, g);
									

								}

								//next iteration
								index++;
								CurrentMaxWidth += 2 * uiDiagSpace;
							}

						}
					}
					else if (this.dir == Dir.DownRight)
					{
						//g.DrawLine(Pens.Red, uix, uiy, uix + 20, uiy + 20);

						//s'il n'y a pas d'enfant on fait une ligne vide
						if (this.listChild.Count <= 0)
						{
							g.DrawLine(Pens.Red, uix, uiy, uix + 20, uiy + 20);
						}
						else
						{
							int ActualUiX = uix;
							int ActualUiY = uiy;
							int index = 0;
							TreeObject LastTO = null;
							int LeftDownHeight = 0; //down height restant des élément supérieur
							while (index < this.listChild.Count)
							{
								//on passe à l'enfant suivant
								g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + uiDiagSpace, ActualUiY + uiDiagSpace);
								ActualUiX += uiDiagSpace;
								ActualUiY += uiDiagSpace;

								LeftDownHeight -= (int)((float)uiDiagSpace * 1.4142f); // *
								if (LeftDownHeight < 0) { LeftDownHeight = 0; }


								TreeObject to = this.listChild[index];

								//si c'est un dossier et qu'il est ouvert
								if (to.IsFolder && to.IsOpen)
								{
									//ajoute le down height du truc qui précède le dossier, s'il y en a un
									//if (LastTO != null)
									//{
									//	//ajoute le down height
									//	int lasttoDownHeight = LastTO.GetDownHeight(g);
									//	int Delta2 = (int)((float)lasttoDownHeight / 1.4142f);
									//	g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + Delta2, ActualUiY + Delta2);
									//	ActualUiX += Delta2;
									//	ActualUiY += Delta2;
									//}

									int Delta2 = (int)((float)LeftDownHeight / 1.4142f);
									g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + Delta2, ActualUiY + Delta2);
									ActualUiX += Delta2;
									ActualUiY += Delta2;
									LeftDownHeight = 0; //on le met à 0 pour être sûr que sa futur valeur sera


									//ajoute le up height
									int toUpHeight = to.GetUpHeight(g);
									int Delta = (int)((float)toUpHeight / 1.4142f);
									g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + Delta, ActualUiY + Delta);
									ActualUiX += Delta;
									ActualUiY += Delta;
								}


								//dessine une "coche" qui indique plus clairement à l'user que cet enfant apartient à cette ligne
								g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + uiDiagSpace, ActualUiY);

								//maintenant fait dessiner l'élément enfant
								to.DrawFullAt(ActualUiX + uiDiagSpace, ActualUiY, g);

								//ajoute le down height
								int toDownHeight = to.GetDownHeight(g) + (int)((float)uiDiagSpace * 1.4142f * 2f);
								if (toDownHeight > LeftDownHeight) { LeftDownHeight = toDownHeight; }
								else { }


								//Delta = (int)((float)toDownHeight / 1.4142f);
								//g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + Delta, ActualUiY + Delta);
								//ActualUiX += Delta;
								//ActualUiY += Delta;
								
								//next iteration
								index++;
								LastTO = to;
							}

						}
					}
					else if (this.dir == Dir.UpRight)
					{
						//g.DrawLine(Pens.Red, uix, uiy, uix + 20, uiy - 20);

						//s'il n'y a pas d'enfant on fait une ligne vide
						if (this.listChild.Count <= 0)
						{
							g.DrawLine(Pens.Red, uix, uiy, uix + 20, uiy - 20);
						}
						else
						{
							int ActualUiX = uix;
							int ActualUiY = uiy;
							int CurrentMaxWidth = (int)(g.MeasureString(this.UiName + "+", this.TV.uiItemFont).Width); //largeur maximale d'un objet fermé pour ne pas embarquer sur un autre arbre. cette valeur augmente pendant qu'on dessend dans les enfant.
							int index = 0;
							while (index < this.listChild.Count)
							{
								//on passe à l'enfant suivant
								g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + uiDiagSpace, ActualUiY - uiDiagSpace);
								ActualUiX += uiDiagSpace;
								ActualUiY -= uiDiagSpace;

								TreeObject to = this.listChild[index];
								int toNameWidth = 0; //largeur du nom de l'objet
								if (to.IsFolder) { toNameWidth = (int)(g.MeasureString(to.UiName + "+", this.TV.uiItemFont).Width); }
								else { toNameWidth = (int)(g.MeasureString(to.UiName, this.TV.uiItemFont).Width); }

								//si c'est un dossier ouvert
								if (to.IsFolder && to.IsOpen)
								{
									//ajoute le up height
									int toUpHeight = to.GetUpHeight(g);
									int Delta = (int)((float)toUpHeight / 1.4142f);
									g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + Delta, ActualUiY - Delta);
									ActualUiX += Delta;
									ActualUiY -= Delta;

									//dessine une "coche" qui indique plus clairement à l'user que cet enfant apartient à cette ligne
									g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - uiDiagSpace, ActualUiY);

									//maintenant fait dessiner l'élément enfant
									to.DrawFullAt(ActualUiX - uiDiagSpace, ActualUiY, g);


									//la largeur maximal des objet fermé à venir doit être reseté à partir d'ici
									CurrentMaxWidth = toNameWidth - uiDiagSpace;
								}
								else //c'est un dossier fermé ou un fichier
								{

									//check si le nom est trop large
									if (toNameWidth > CurrentMaxWidth)
									{
										//si le nom est trop large on doit lui ajouter sa upheight necessaire pour qu'il ne le touche plus

										//ajoute le up height
										//int toUpHeight = to.GetUpHeight(g);
										//int Delta = (int)((float)toUpHeight / 1.4142f);
										int Delta = toNameWidth - CurrentMaxWidth;
										Delta /= 2;

										g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + Delta, ActualUiY - Delta);
										ActualUiX += Delta;
										ActualUiY -= Delta;

										//maintenant qu'on a dessendu, la largeur maximal augmente
										CurrentMaxWidth += 2 * Delta;

									}



									//dessine une "coche" qui indique plus clairement à l'user que cet enfant apartient à cette ligne
									g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - uiDiagSpace, ActualUiY);

									//maintenant fait dessiner l'élément enfant
									to.DrawFullAt(ActualUiX - uiDiagSpace, ActualUiY, g);


								}

								////ajoute le up height
								//int toUpHeight = to.GetUpHeight(g);
								//int Delta = (int)((float)toUpHeight / 1.4142f);
								//g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + Delta, ActualUiY - Delta);
								//ActualUiX += Delta;
								//ActualUiY -= Delta;

								////dessine une "coche" qui indique plus clairement à l'user que cet enfant apartient à cette ligne
								//g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - uiDiagSpace, ActualUiY);

								////maintenant fait dessiner l'élément enfant
								//to.DrawFullAt(ActualUiX - uiDiagSpace, ActualUiY, g);

								////ajoute le down height
								//int toDownHeight = to.GetDownHeight(g);
								//Delta = (int)((float)toDownHeight / 1.4142f);
								//g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX + Delta, ActualUiY - Delta);
								//ActualUiX += Delta;
								//ActualUiY -= Delta;

								//next iteration
								index++;
								CurrentMaxWidth += 2 * uiDiagSpace;
							}

						}
					}
					else if (this.dir == Dir.UpLeft)
					{
						//g.DrawLine(Pens.Red, uix, uiy, uix - 20, uiy - 20);

						//s'il n'y a pas d'enfant on fait une ligne vide
						if (this.listChild.Count <= 0)
						{
							g.DrawLine(Pens.Red, uix, uiy, uix - 20, uiy - 20);
						}
						else
						{
							int ActualUiX = uix;
							int ActualUiY = uiy;
							int index = 0;
							TreeObject LastTO = null;
							int LeftDownHeight = 0; //down height restant des élément supérieur
							while (index < this.listChild.Count)
							{
								//on passe à l'enfant suivant
								g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - uiDiagSpace, ActualUiY - uiDiagSpace);
								ActualUiX -= uiDiagSpace;
								ActualUiY -= uiDiagSpace;

								LeftDownHeight -= (int)((float)uiDiagSpace * 1.4142f);
								if (LeftDownHeight < 0) { LeftDownHeight = 0; }


								TreeObject to = this.listChild[index];

								//si c'est un dossier et qu'il est ouvert
								if (to.IsFolder && to.IsOpen)
								{
									//ajoute le down height du truc qui précède le dossier, s'il y en a un
									//if (LastTO != null)
									//{
									//	//ajoute le down height
									//	int lasttoDownHeight = LastTO.GetDownHeight(g);
									//	int Delta2 = (int)((float)lasttoDownHeight / 1.4142f);
									//	g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - Delta2, ActualUiY - Delta2);
									//	ActualUiX -= Delta2;
									//	ActualUiY -= Delta2;
									//}
									int Delta2 = (int)((float)LeftDownHeight / 1.4142f);
									g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - Delta2, ActualUiY - Delta2);
									ActualUiX -= Delta2;
									ActualUiY -= Delta2;
									LeftDownHeight = 0; //on le met à 0 pour être sûr que sa futur valeur sera


									//ajoute le up height
									int toUpHeight = to.GetUpHeight(g);
									int Delta = (int)((float)toUpHeight / 1.4142f);
									g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - Delta, ActualUiY - Delta);
									ActualUiX -= Delta;
									ActualUiY -= Delta;
								}


								////ajoute le up height
								//int toUpHeight = to.GetUpHeight(g);
								//int Delta = (int)((float)toUpHeight / 1.4142f);
								//g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - Delta, ActualUiY - Delta);
								//ActualUiX -= Delta;
								//ActualUiY -= Delta;

								//dessine une "coche" qui indique plus clairement à l'user que cet enfant apartient à cette ligne
								g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - uiDiagSpace, ActualUiY);

								//maintenant fait dessiner l'élément enfant
								to.DrawFullAt(ActualUiX - uiDiagSpace, ActualUiY, g);
								
								//ajoute le down height
								int toDownHeight = to.GetDownHeight(g) + (int)((float)uiDiagSpace * 1.4142f * 2f);
								if (toDownHeight > LeftDownHeight) { LeftDownHeight = toDownHeight; }
								else { }
								

								//int toDownHeight = to.GetDownHeight(g);
								//Delta = (int)((float)toDownHeight / 1.4142f);
								//g.DrawLine(penLineDiag, ActualUiX, ActualUiY, ActualUiX - Delta, ActualUiY - Delta);
								//ActualUiX -= Delta;
								//ActualUiY -= Delta;

								//next iteration
								index++;
								LastTO = to;
							}

						}
					}
				}
			}



			//retourne le width relatif au parent
			public int GetWidth(Graphics g)
			{
				if (this.IsFolder)
				{
					if (!this.IsOpen)
					{
						//le dossier est fermé
						return (int)(g.MeasureString(this.UiName + "+", this.TV.uiItemFont).Width / 1.4142f);

					}
					else //le dossier est ouvert
					{
						//puisque le dossier est ouvert, il faut prendre en considération tout les contenu pour toute les direction
						if (this.dir == Dir.DownLeft || this.dir == Dir.UpRight)
						{
							//le width c'est le width du nom du dossier + toute les hauteur relative des truc à l'intérieur
							int rep = (int)(g.MeasureString(this.UiName + "+", this.TV.uiItemFont).Width / 1.4142f);
							rep += (int)((float)(this.TV.uiDiagSpace) * 1.4142f);

							int CurrentMaxWidth = (int)(g.MeasureString(this.UiName + "+", this.TV.uiItemFont).Width);

							foreach (TreeObject to in this.listChild)
							{
								int toNameWidth = 0; //largeur du nom de l'objet
								if (to.IsFolder) { toNameWidth = (int)(g.MeasureString(to.UiName + "+", this.TV.uiItemFont).Width); }
								else { toNameWidth = (int)(g.MeasureString(to.UiName, this.TV.uiItemFont).Width); }
								
								rep += (int)((float)(this.TV.uiDiagSpace) * 1.4142f);

								//si c'est un dossier ouvert
								if (to.IsFolder && to.IsOpen)
								{
									rep += to.GetUpHeight(g);
									CurrentMaxWidth = toNameWidth - this.TV.uiDiagSpace;
								}
								else //si c'est un dossier fermé ou un fichier
								{
									//on doit analyser si le nom de l'élément est trop grand ce qui fait baisser l'élément
									if (toNameWidth > CurrentMaxWidth)
									{
										int delta = (toNameWidth - CurrentMaxWidth) / 2;
										rep += (int)((float)delta * 1.4142f);
										CurrentMaxWidth += 2 * delta;
									}

								}

								//next iteration
								CurrentMaxWidth += 2 * this.TV.uiDiagSpace;
							}
							return rep;

						}
						else if (this.dir == Dir.DownRight || this.dir == Dir.UpLeft)
						{
							
							int rep = (int)(g.MeasureString(this.UiName + "+", this.TV.uiItemFont).Width / 1.4142f);
							rep += (int)((float)(this.TV.uiDiagSpace) * 1.4142f);

							int MinWidth = 0; //largeur minimal nécessaire pour tout contenir.

							TreeObject LastTO = null;
							int LastTODownHeight = 0;
							int LeftDownHeight = 0; //down height restant des élément supérieur
							foreach (TreeObject to in this.listChild)
							{
								rep += (int)((float)(this.TV.uiDiagSpace) * 1.4142f);
								
								LeftDownHeight -= (int)((float)(this.TV.uiDiagSpace) * 1.4142f);
								if (LeftDownHeight < 0) { LeftDownHeight = 0; }


								//si c'est un dossier ouvert
								if (to.IsFolder && to.IsOpen)
								{
									//if (LastTO != null)
									//{
									//	rep += LastTODownHeight;
									//}
									rep += LeftDownHeight;
									LeftDownHeight = 0;


									rep += to.GetUpHeight(g);
								}
								else //c'est un dossier fermé ou un fichier
								{

								}


								//ajoute le down height
								int toDownHeight = to.GetDownHeight(g) + (int)((float)(this.TV.uiDiagSpace) * 1.4142f * 2f);
								if (toDownHeight > LeftDownHeight) { LeftDownHeight = toDownHeight; }
								else { }


								//calcul le down height
								int toDownHeight2 = to.GetDownHeight(g);

								//on check si la width minimum requis pour contenir cet élément est plus grand que celui déjà enregistré
								int ActualMinWidth = rep + toDownHeight2;
								if (ActualMinWidth > MinWidth) { MinWidth = ActualMinWidth; }
								
								//next iteration
								LastTO = to;
								LastTODownHeight = toDownHeight2;
							}


							return MinWidth;
						}
						//else if (this.dir == Dir.UpRight)
						//{

						//}
						//else if (this.dir == Dir.UpLeft)
						//{
						//	int rep = (int)(g.MeasureString(this.UiName + "+", this.TV.uiItemFont).Width / 1.4142f);
						//	rep += (int)((float)(this.TV.uiDiagSpace) * 1.4142f);

						//	int MinWidth = 0; //largeur minimal nécessaire pour tout contenir.

						//	TreeObject LastTO = null;
						//	int LastTODownHeight = 0;
						//	foreach (TreeObject to in this.listChild)
						//	{
						//		rep += (int)((float)(this.TV.uiDiagSpace) * 1.4142f);

						//		//si c'est un dossier ouvert
						//		if (to.IsFolder && to.IsOpen)
						//		{
						//			if (LastTO != null)
						//			{
						//				rep += LastTODownHeight;
						//			}
						//			rep += to.GetUpHeight(g);
						//		}
						//		else //c'est un dossier fermé ou un fichier
						//		{

						//		}

						//		//calcul le down height
						//		int toDownHeight = to.GetDownHeight(g);

						//		//on check si la width minimum requis pour contenir cet élément est plus grand que celui déjà enregistré
						//		int ActualMinWidth = rep + toDownHeight + (int)((float)(this.TV.uiDiagSpace) * 1.4142f);
						//		if (ActualMinWidth > MinWidth) { MinWidth = ActualMinWidth; }

						//		//next iteration
						//		LastTO = to;
						//		LastTODownHeight = toDownHeight;
						//	}


						//	return MinWidth;
						//}



						//le width c'est le width du nom du dossier + toute les hauteur relative des truc à l'intérieur
						int rep2 = (int)(g.MeasureString(this.UiName + "+", this.TV.uiItemFont).Width / 1.4142f);
						rep2 += (int)((float)(this.TV.uiDiagSpace) * 1.4142f);

						foreach (TreeObject to in this.listChild)
						{
							rep2 += to.GetUpHeight(g);
							rep2 += to.GetDownHeight(g);
							rep2 += (int)((float)(this.TV.uiDiagSpace) * 1.4142f);
						}
						return rep2;
						
					}
				}
				else //alors this est un fichier
				{
					return (int)(g.MeasureString(this.UiName, this.TV.uiItemFont).Width / 1.4142f);
				}
			}

			//retourne le up height relatif au parent
			public int GetUpHeight(Graphics g)
			{
				if (this.Parent != null)
				{
					if (this.IsFolder) //dossier
					{
						if (this.IsOpen) //dossier ouvert
						{
							if (this.Parent.dir == Dir.DownLeft)
							{
								//on cherche le plus gros width à l'intérieur et on l'additionne à la hauteur du nom du dossier
								int BiggestWidth = 0;
								foreach (TreeObject to in this.listChild)
								{
									int towidth = to.GetWidth(g);
									if (towidth > BiggestWidth) { BiggestWidth = towidth; }
								}
								return (int)(g.MeasureString(this.UiName + "+", this.TV.uiItemFont).Width / 1.4142f) + BiggestWidth;
							}
							else if (this.Parent.dir == Dir.DownRight)
							{
								//on cherche le plus gros width à l'intérieur et on lui soustrait la hauteur du nom du dossier
								int BiggestWidth = 0;
								foreach (TreeObject to in this.listChild)
								{
									int towidth = to.GetWidth(g);
									if (towidth > BiggestWidth) { BiggestWidth = towidth; }
								}
								int rep = BiggestWidth - (int)(g.MeasureString(this.UiName + "+", this.TV.uiItemFont).Width / 1.4142f);
								//on make sure que la up height n'est pas négative
								if (rep < 0) { rep = 0; }
								return rep;
							}
							else if (this.Parent.dir == Dir.UpRight)
							{
								//on cherche le plus gros width à l'intérieur
								int BiggestWidth = 0;
								foreach (TreeObject to in this.listChild)
								{
									int towidth = to.GetWidth(g);
									if (towidth > BiggestWidth) { BiggestWidth = towidth; }
								}
								return (int)(g.MeasureString(this.UiName + "+", this.TV.uiItemFont).Width / 1.4142f) + BiggestWidth;
							}
							else if (this.Parent.dir == Dir.UpLeft)
							{
								//on cherche le plus gros width à l'intérieur et on lui soustrait la hauteur du nom du dossier
								int BiggestWidth = 0;
								foreach (TreeObject to in this.listChild)
								{
									int towidth = to.GetWidth(g);
									if (towidth > BiggestWidth) { BiggestWidth = towidth; }
								}
								int rep = BiggestWidth - (int)(g.MeasureString(this.UiName + "+", this.TV.uiItemFont).Width / 1.4142f);
								//on make sure que la up height n'est pas négative
								if (rep < 0) { rep = 0; }
								return rep;
							}
						}
						else //dossier fermé
						{
							if (this.Parent.dir == Dir.DownLeft)
							{
								return (int)(g.MeasureString(this.UiName + "+", this.TV.uiItemFont).Width / 1.4142f);
							}
							else if (this.Parent.dir == Dir.DownRight)
							{
								return 0;
							}
							else if (this.Parent.dir == Dir.UpRight)
							{
								return (int)(g.MeasureString(this.UiName + "+", this.TV.uiItemFont).Width / 1.4142f);
							}
							else if (this.Parent.dir == Dir.UpLeft)
							{
								return 0;
							}
						}
					}
					else //fichier
					{
						if (this.Parent.dir == Dir.DownLeft)
						{
							return (int)(g.MeasureString(this.UiName, this.TV.uiItemFont).Width / 1.4142f);
						}
						else if (this.Parent.dir == Dir.DownRight)
						{
							return 0;
						}
						else if (this.Parent.dir == Dir.UpRight)
						{
							return (int)(g.MeasureString(this.UiName, this.TV.uiItemFont).Width / 1.4142f);
						}
						else if (this.Parent.dir == Dir.UpLeft)
						{
							return 0;
						}
					}
				}
				else { return 0; }

				return 0;
			}

			//retourne le down height relatif au parent
			public int GetDownHeight(Graphics g)
			{
				if (this.Parent != null)
				{
					if (this.IsFolder) //dossier
					{
						if (this.Parent.dir == Dir.DownLeft)
						{
							return 0;
						}
						else if (this.Parent.dir == Dir.DownRight)
						{
							return (int)(g.MeasureString(this.UiName + "+", this.TV.uiItemFont).Width / 1.4142f);
						}
						else if (this.Parent.dir == Dir.UpRight)
						{
							return 0;
						}
						else if (this.Parent.dir == Dir.UpLeft)
						{
							return (int)(g.MeasureString(this.UiName + "+", this.TV.uiItemFont).Width / 1.4142f);
						}
					}
					else //fichier
					{
						if (this.Parent.dir == Dir.DownLeft)
						{
							return 0;
						}
						else if (this.Parent.dir == Dir.DownRight)
						{
							return (int)(g.MeasureString(this.UiName, this.TV.uiItemFont).Width / 1.4142f);
						}
						else if (this.Parent.dir == Dir.UpRight)
						{
							return 0;
						}
						else if (this.Parent.dir == Dir.UpLeft)
						{
							return (int)(g.MeasureString(this.UiName, this.TV.uiItemFont).Width / 1.4142f);
						}
					}
				}

				return 0;
			}



		}



		private void BuildTreeRoot()
		{
			TreeObject r = new TreeObject("This PC", this);
			r.tType = TOType.folder;
			r.dir = Dir.DownLeft; // DownLeft

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
					}
					else
					{
						this.ActualElement.Open();
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
						this.Root.DrawFullAt(ppMiddle.X + this.RootDrawPos.X, ppMiddle.Y + this.RootDrawPos.Y, g);
						g.Dispose();
						asdfimg.Dispose();

						

						//on réajuste la position graphique pour le le dossier revient au même endroit où il était
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
			
			g.FillPolygon(Brushes.SteelBlue, plArrowUp);
			g.FillPolygon(Brushes.CornflowerBlue, plArrowDown);
			g.FillPolygon(Brushes.DodgerBlue, plArrowMiddle);
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


		public TreeView4()
		{
			this.ImageBox = new PictureBox();
			this.ImageBox.BackColor = Color.Black;
			this.ImageBox.SizeChanged += new EventHandler(this.ImageBox_SizeChanged);
			this.ImageBox.MouseDown += new MouseEventHandler(this.ImageBox_MouseDown);
			this.ImageBox.MouseUp += new MouseEventHandler(this.ImageBox_MouseUp);
			this.ImageBox.MouseMove += new MouseEventHandler(this.ImageBox_MouseMove);

			this.CreateScroll();
			this.CreateArrow();

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


		private Font uiItemFont = new Font("consolas", 10f); // 10f
		private float uifItemTextHeight = 20f; // 8f ajusté lors de RefreshImage()

		private Pen uiHighlightPen = Pens.White; //pen utilisé pour dessiner un rectangle qui indique quel est l'élément actuellement focusé, ou juste "actuel"
		private Brush uiFolderBackBrush = new SolidBrush(Color.FromArgb(96, 96, 0)); //brush utilisé pour filler l'arrière plan du nom d'un dossier

		private int uiDiagSpace = 18; // 18 distance horizontale et vertical à parcourir pour passer immédiatement au prochain enfant en diagonalde



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


			//////dessine l'objet actuellement sélectionné
			//dessine le text. il faut ajouter le + ou le - si c'est un dossier
			//string strSelectedObjectText = this.ActualElement.UiName;
			//if (this.ActualElement.IsFolder)
			//{
			//	if (ActualElement.IsOpen)
			//	{
			//		strSelectedObjectText = "-" + strSelectedObjectText;
			//	}
			//	else { strSelectedObjectText = "+" + strSelectedObjectText; }
			//}
			////dessine le text de l'objet sélectionné, à sa place, au milieu
			//g.DrawString(strSelectedObjectText, this.uiItemFont, Brushes.White, fppMiddle.X, fppMiddle.Y - (fItemTextHeight / 2f));



			this.Root.DrawFullAt(ppMiddle.X + this.RootDrawPos.X, ppMiddle.Y + this.RootDrawPos.Y, g);


			//g.DrawImage(this.GetArrowAtAngle(45d), 0, 0);

			

			//////fin
			g.Dispose();
			if (this.ImageBox.Image != null) { this.ImageBox.Image.Dispose(); }
			this.ImageBox.Image = img;
			this.ImageBox.Refresh();
		}


		#endregion

	}
}

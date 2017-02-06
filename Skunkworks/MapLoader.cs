﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class MapLoader : MonoBehaviour {
	
	public Text Output;
	public int LevelToRetrieve;
	public const int TILE_SOLID= 0;
	public const int TILE_OPEN= 1;
	/*
Note the order of these 4 tiles are actually different in SHOCK. I swap them around in BuildTileMapShock for consistancy
*/
	public const int  TILE_DIAG_SE= 2;
	public const int  TILE_DIAG_SW =3;
	public 	const int  TILE_DIAG_NE= 4;
	public const int  TILE_DIAG_NW =5;

	public const int  TILE_SLOPE_N =6;
	public const int  TILE_SLOPE_S =7;
	public const int 	TILE_SLOPE_E =8;
	public const int  TILE_SLOPE_W =9;
	public const int  TILE_VALLEY_NW =10;
	public const int  TILE_VALLEY_NE= 11;
	public const int TILE_VALLEY_SE =12;
	public const int  TILE_VALLEY_SW= 13;
	public const int  TILE_RIDGE_SE= 14;
	public const int  TILE_RIDGE_SW= 15;
	public const int  TILE_RIDGE_NW= 16;
	public const int  TILE_RIDGE_NE =17;


	const int SLOPE_BOTH_PARALLEL= 0;
	const int SLOPE_BOTH_OPPOSITE= 1;
	const int SLOPE_FLOOR_ONLY =2;
	const int  SLOPE_CEILING_ONLY= 3;

	//Visible faces indices
	const int vTOP =0;
	const int vEAST =1;
	const int vBOTTOM= 2;
	const int vWEST= 3;
	const int vNORTH= 4;
	const int vSOUTH= 5;


	//BrushFaces
	const int fSELF =128;
	const int fCEIL= 64;
	const int fNORTH =32;
	const int fSOUTH =16;
	const int fEAST= 8;
	const int fWEST= 4;
	const int fTOP =2;
	const int fBOTTOM= 1;


	//**********************



	public TileInfo[,] LevelInfo=new TileInfo[64,64];
	public int[] texture_map = new int[256];
	public Material[] MaterialMasterList=new Material[260];

	//**********************

	public string Path;
	public int UW_CEILING_HEIGHT;
	public int CEILING_HEIGHT;


	void Start()
		{
		//Load all my materials
		for (int i =0; i<=MaterialMasterList.GetUpperBound(0);i++)
			{
			MaterialMasterList[i]=(Material)Resources.Load("UW1/Maps/Materials/uw1_" + i.ToString("d3"));
			}
		}


	//************************

	public bool BuildTileMapUW(string filePath, int game, int LevelNo)
		{
		// File pointer
		char[] lev_ark; 
		//char[] tmp_ark; 
		char[] tex_ark=new char[1]; 
		char[] tmp_ark=new char[1]; 
		int NoOfBlocks;
		long AddressOfBlockStart;
		long address_pointer;
		long textureAddress;
		long fileSize;
		short x;	
		short y;
		int i;
		int CeilingTexture=0;
		int textureMapSize=64;

		UW_CEILING_HEIGHT = ((128 >> 2) * 8 >>3);	//Shifts the scale of the level. Idea borrowed from abysmal

		switch (game)
		{
		case 1:	//UW1
			{
				CEILING_HEIGHT=UW_CEILING_HEIGHT;
				if (!DataLoader.ReadStreamFile(filePath, out lev_ark))
					{
					return false;
					}
				
				 // 0x7a;
				//Get the number of blocks in this file.
				NoOfBlocks =  (int)DataLoader.getValAtAddress(lev_ark,0,32);
				//Get the first map block
				AddressOfBlockStart =  DataLoader.getValAtAddress(lev_ark,(LevelNo * 4) + 2,32);	
				textureAddress =  DataLoader.getValAtAddress(lev_ark,((LevelNo+18) * 4) + 2 ,32);
				//long objectsAddress = AddressOfBlockStart + (64*64*4); //+ 1;
				address_pointer =0;
				break;
				}
		case 2:
				{
				//******************
				CEILING_HEIGHT=UW_CEILING_HEIGHT;

				textureMapSize = 70;	//0x86;
				if (!DataLoader.ReadStreamFile(filePath, out tmp_ark))
					{
					return false;
					}

				address_pointer=0;
				NoOfBlocks=(int)DataLoader.getValAtAddress(tmp_ark,0,32);	

				address_pointer=6;

				int compressionFlag=(int)DataLoader.getValAtAddress(tmp_ark,address_pointer + (NoOfBlocks*4) + (LevelNo*4) ,32);
				int isCompressed =(compressionFlag>>1) & 0x01;
				address_pointer=(LevelNo * 4) + 6;
				if ((int)DataLoader.getValAtAddress(tmp_ark,address_pointer,32)==0)
					{

					return false ;
					}
				if (isCompressed == 1)
					{
					int datalen=0;
					lev_ark = unpackUW2(tmp_ark,DataLoader.getValAtAddress(tmp_ark,address_pointer,32), ref datalen);
					address_pointer=address_pointer+4;
					AddressOfBlockStart=0;
					address_pointer=0;
					}
				else
					{
					int BlockStart = (int)DataLoader.getValAtAddress(tmp_ark, address_pointer, 32);
					int j=0;
					AddressOfBlockStart=0;
					lev_ark = new char[0x7c08];
					for (i = BlockStart; i < BlockStart + 0x7c08; i++)
						{
						lev_ark[j] = tmp_ark[i];
						j++;
						}
					}	


				//read in the textures
				//address_pointer=(LevelNo * 4) + 6 + (80*4);
				//tex_ark = tmp_ark;	//unpack(tmp_ark,getValAtAddress(tmp_ark,address_pointer,32));
				textureAddress=DataLoader.getValAtAddress(tmp_ark,(LevelNo * 4) + 6 + (80*4),32);	
				compressionFlag=(int)DataLoader.getValAtAddress(tmp_ark,(LevelNo * 4) + 6 + (80*4)+ (NoOfBlocks*4),32);
				isCompressed =(compressionFlag>>1) & 0x01;

				if (isCompressed == 1)
					{
					int datalen=0;
					tex_ark = unpackUW2(tmp_ark, textureAddress, ref datalen);
					textureAddress=-1;
					}
				break;
				}				

				//**************
		default:
			return false;
		}

		int offset=0;
		for (i = 0; i<textureMapSize; i++)	//256
			{//TODO: Only use this for texture lookups.
			switch (game)
			{
			case 1:
					{
					if (i<48)	//Wall and floor textures are int 16s
						{
						texture_map[i] =  (int)DataLoader.getValAtAddress(lev_ark, textureAddress + offset, 16); //(i * 2)
						offset=offset+2;

						}
					else if (i<=57)	//Wall and floor textures are int 16s
						{
						texture_map[i] =  (int)DataLoader.getValAtAddress(lev_ark, textureAddress + offset, 16)+210; //(i * 2)
						offset = offset + 2;

						}
					else
						{ //door textures are int 8s
						texture_map[i] = (int)DataLoader.getValAtAddress(lev_ark, textureAddress + offset, 8) ;//+210; //(i * 1)
						offset++;
						}
					if (i == 57)
						{
						CeilingTexture = texture_map[i];
						}

					break;
					}
				case 2:
					{
					if (textureAddress == -1)//Texture block was decompressed
						{
						textureAddress=0;
						if (i<64)
							{
							texture_map[i] =(int)DataLoader.getValAtAddress(tex_ark,offset ,16);//tmp //textureAddress+ //(i*2)
							offset = offset + 2;
							}
						else
							{//door textures
							texture_map[i] = (int)DataLoader.getValAtAddress(tex_ark, textureAddress + offset, 8);//tmp //textureAddress+//(i*2)
							offset++;
							}
						}
					else
						{
						if (i<64)
							{
							texture_map[i] =(int)DataLoader.getValAtAddress(tmp_ark,textureAddress+offset,16);//tmp //textureAddress+//(i*2)
							offset = offset + 2;
							}
						else
							{//door textures
							texture_map[i] = (int)DataLoader.getValAtAddress(tmp_ark, textureAddress + offset, 8);//tmp //textureAddress+//(i*2)
							offset++;
							}
						}

					if (i == 0xf)
						{
						CeilingTexture=texture_map[i];
						}
					break;
					}
				}
			}
		//Assign door textures to the object masters list
		//Depends on the correct values in the config!!
		//switch (game)
		//{
		//case 1:
			//TODO: Restore this 
			/*objectMasters[320].extraInfo = texture_map[58];
			objectMasters[321].extraInfo = texture_map[59];
			objectMasters[322].extraInfo = texture_map[60];
			objectMasters[323].extraInfo = texture_map[61];
			objectMasters[324].extraInfo = texture_map[62];
			objectMasters[325].extraInfo = texture_map[63];

			objectMasters[328].extraInfo = texture_map[58];
			objectMasters[329].extraInfo = texture_map[59];
			objectMasters[330].extraInfo = texture_map[60];
			objectMasters[331].extraInfo = texture_map[61];
			objectMasters[332].extraInfo = texture_map[62];
			objectMasters[333].extraInfo = texture_map[63];
			break;*/
		//}
		for (y=0; y<64;y++)
			{
			for (x=0; x<64;x++)
				{
				LevelInfo[x,y]=new TileInfo();
				LevelInfo[x,y].tileX = x;
				LevelInfo[x,y].tileY = y;
				LevelInfo[x,y].address = AddressOfBlockStart+address_pointer;
				long FirstTileInt = DataLoader.getValAtAddress(lev_ark,AddressOfBlockStart+(address_pointer+0),16);
				long SecondTileInt = DataLoader.getValAtAddress(lev_ark,AddressOfBlockStart+(address_pointer+2),16);
				address_pointer=address_pointer+4;

				LevelInfo[x,y].tileType = getTile(FirstTileInt) ;
				LevelInfo[x,y].floorHeight = getHeight(FirstTileInt) ;
				LevelInfo[x,y].floorHeight = ((LevelInfo[x,y].floorHeight <<3) >> 2)*8 >>3;	//Try and copy this shift from shock.

				LevelInfo[x,y].ceilingHeight = 0;//UW_CEILING_HEIGHT;	//constant for uw				
				LevelInfo[x,y].noOfNeighbours=0;
				LevelInfo[x,y].tileTested = 0;
				LevelInfo[x,y].TerrainChangeCount=0;
				LevelInfo[x,y].BullFrog = 0;

				LevelInfo[x,y].flags =(short)((FirstTileInt>>7) & 0x3);
				LevelInfo[x,y].noMagic =(short)( (FirstTileInt>>13) & 0x1);
				switch (game)
				{
				case 1:	//uw1
					LevelInfo[x,y].floorTexture = getFloorTex(lev_ark, textureAddress, FirstTileInt);
					if (LevelNo == 6)
						{//Tybals lair. Special case for the maze
						//int val = (FirstTileInt >> 10) & 0x0F;
						if (((FirstTileInt >> 10) & 0x0F) == 4)
							{//Maze floor
							LevelInfo[x,y].floorTexture = 278;
							}
						}
					LevelInfo[x,y].wallTexture = getWallTex(lev_ark, textureAddress, SecondTileInt);
					break;
				
				case 2:	//uw2
						{
						LevelInfo[x,y].floorTexture = getFloorTexUw2(tmp_ark, textureAddress, FirstTileInt);
						//int val = (FirstTileInt >>10) & 0x0F;
						LevelInfo[x,y].floorTexture = texture_map[(FirstTileInt >>10) & 0x0F];
						LevelInfo[x,y].wallTexture = getWallTexUw2(tmp_ark, textureAddress, SecondTileInt);	
						//(tileData & 0x3F
						LevelInfo[x,y].wallTexture= texture_map[SecondTileInt & 0x3F];
						break;
						}	

				}
				if (LevelInfo[x,y].floorTexture<0)
					{
					LevelInfo[x,y].floorTexture=0;
					}
				if (LevelInfo[x,y].wallTexture>=256)
					{
					LevelInfo[x,y].wallTexture =0;
					}					
				//UW only has a single ceiling texture so this is ignored.
				//LevelInfo[x,y].shockCeilingTexture = LevelInfo[x,y].floorTexture;					
				LevelInfo[x,y].shockCeilingTexture=CeilingTexture;
				//There is only one possible steepness in UW so I set it's properties to match a similar tile in shock.
				if (LevelInfo[x,y].tileType >=2)
					{
					LevelInfo[x,y].shockSteep = 1;
					LevelInfo[x,y].shockSteep = ((LevelInfo[x,y].shockSteep  <<3) >> 2)*8 >>3;	//Shift copied from shock
					LevelInfo[x,y].shockSlopeFlag = SLOPE_FLOOR_ONLY ;
					}


				//LevelInfo[x,y].isDoor = getDoors(FirstTileInt);

				//Different textures on solid tiles faces
				LevelInfo[x,y].North = LevelInfo[x,y].wallTexture;
				LevelInfo[x,y].South = LevelInfo[x,y].wallTexture;
				LevelInfo[x,y].East = LevelInfo[x,y].wallTexture;
				LevelInfo[x,y].West = LevelInfo[x,y].wallTexture;
				LevelInfo[x,y].Top = LevelInfo[x,y].floorTexture; 
				LevelInfo[x,y].Bottom = LevelInfo[x,y].floorTexture; 
				LevelInfo[x,y].Diagonal = LevelInfo[x,y].wallTexture;
				//First index of the linked list of objects.
				LevelInfo[x,y].indexObjectList = getObject(SecondTileInt);

				LevelInfo[x,y].Render=1;		
				LevelInfo[x,y].DimX=1;			
				LevelInfo[x,y].DimY=1;			
				LevelInfo[x,y].Grouped=0;	
				//LevelInfo[x,y].VisibleFaces = 63;
				for (int v = 0; v < 6; v++)
					{
					LevelInfo[x,y].VisibleFaces[v]=1;
					}
				//Restore this when texturesmasters is loaded.
				//LevelInfo[x,y].isWater = (textureMasters[LevelInfo[x,y].floorTexture].water == 1) && ((LevelInfo[x,y].tileType !=0) && (ENABLE_WATER==1));
				//LevelInfo[x,y].isLava = (textureMasters[LevelInfo[x,y].floorTexture].lava == 1) && ((LevelInfo[x,y].tileType != 0));
				LevelInfo[x,y].waterRegion= 0;
				LevelInfo[x,y].landRegion = 0;//including connected bridges.
				LevelInfo[x,y].lavaRegion = 0;

				//Set some easy tile visible settings
				switch (LevelInfo[x,y].tileType)
				{
				case TILE_SOLID:
					//Bottom and top are invisible
					LevelInfo[x,y].VisibleFaces[0] = 0;
					LevelInfo[x,y].VisibleFaces[2] = 0;
					break;
				default:
					//Bottom is invisible
					LevelInfo[x,y].VisibleFaces[2] = 0;
					break;
				}

				//Force off water to save on compile time during testing.
				//LevelInfo[x,y].isWater=0;
				//LevelInfo[x,y].TerrainChange=0;
				//LevelInfo[x,y].hasElevator=0;
				}
			}
		for (y=0; y<64;y++)
			{
			for (x=0; x<64;x++)
				{
				if (LevelInfo[x,y].tileType >= 0) //was just solid only. Note: If textures are all wrong it's probably caused here!
					{
					//assign it's north texture
					if (y<63)
						{LevelInfo[x,y].North =LevelInfo[x,y+1].wallTexture;}
					else
						{LevelInfo[x,y].North =-1;}
					//assign it's southern
					if (y>0)
						{LevelInfo[x,y].South  =LevelInfo[x,y-1].wallTexture;}
					else
						{LevelInfo[x,y].South =-1;}
					}
				//it's east
				if (x<63)
					{LevelInfo[x,y].East =LevelInfo[x+1,y].wallTexture;}
				else
					{LevelInfo[x,y].East =-1;}
				//assign it's West
				if (x>0)
					{LevelInfo[x,y].West =LevelInfo[x-1,y].wallTexture;}
				else
					{LevelInfo[x,y].West =-1;}				
				}
			if ((x<64) && (y<64))
				{
				LevelInfo[x,y].UpperEast = LevelInfo[x,y].East;
				LevelInfo[x,y].UpperWest = LevelInfo[x,y].West;
				LevelInfo[x,y].UpperNorth = LevelInfo[x,y].North;
				LevelInfo[x,y].UpperSouth = LevelInfo[x,y].South;
				LevelInfo[x,y].LowerEast = LevelInfo[x,y].East;
				LevelInfo[x,y].LowerWest = LevelInfo[x,y].West;
				LevelInfo[x,y].LowerNorth = LevelInfo[x,y].North;
				LevelInfo[x,y].LowerSouth = LevelInfo[x,y].South;
				}

			}
		return true;
		}



	//***********************

	int getTile(long tileData)
		{
		//gets tile data at bits 0-3 of the tile data
		return (int) (tileData & 0x0F);
		}

	int getHeight(long tileData)
		{//gets height data at bits 4-7 of the tile data
		return (int)(tileData & 0xF0) >> 4;
		}

	int getFloorTex(char[] buffer, long textureOffset, long tileData)
		{//gets floor texture data at bits 10-13 of the tile data
		int val = (int)(tileData >>10) & 0x0F;	//gets the index of the texture
		//look it up in texture block for it's absolute index for wxx.tr
		return (int)DataLoader.getValAtAddress(buffer,textureOffset+96+(val*2),16) +210;			//96 needed?
		//	return ((tileData >>10) & 0x0F);	//was	11
		}

	int getWallTex(char[] buffer, long textureOffset, long tileData)
		{
		//gets wall texture data at bits 0-5 (+16) of the tile data(2nd part)
		//return ((tileData >>17)& 0x3F);
		int val = (int)(tileData & 0x3F);	//gets the index of the texture
		return (int)DataLoader.getValAtAddress(buffer,textureOffset+(val*2),16);
		//return (tileData& 0x3F);
		}

	int getObject(long tileData)
		{
		//gets object data at bits 6-15 (+16) of the tile data(2nd part)
		return (int)tileData >> 6;
		}



	public void Retrieve()
		{

		for (int y=0; y<64;y++)
			{
			for (int x=0; x<64;x++)
				{
				LevelInfo[x,y]=new TileInfo();
				}
			}
		
		BuildTileMapUW(Path,2,LevelToRetrieve);

		string result="";
		for (int y=63; y>=0;y--)
			{
			for (int x=0; x<64;x++)
				{
				if (LevelInfo[x,y].tileType==0)
					{
					result+= "_";
					}
				else
					{
					result+= LevelInfo[x,y].tileType;
					}
				}
			result +="\n";
			}
		Output.text=result;
		}

	/*
	public void GenerateMesh()
		{
		for (int y=0; y<64;y++)
			{
			for (int x=0; x<64;x++)
				{
				LevelInfo[x,y]=new TileInfo();
				}
			}
		BuildTileMapUW(Path,1,LevelToRetrieve);
		for (int y=0; y<64;y++)
			{
			for (int x=0; x<64;x++)
				{
				//Create a mesh for each tile.
				if (LevelInfo[x,y].tileType!=0)
					{
					GameObject Tile = new GameObject("Tile"+x+"_"+y);
					Tile.transform.position = new Vector3(x*1.2f,0.0f, y*1.2f);
					Tile.transform.Rotate(-90.0f,-180.0f,0.0f);
					MeshFilter mf = Tile.AddComponent<MeshFilter>();
					MeshRenderer mr =Tile.AddComponent<MeshRenderer>();
					Mesh mesh = new Mesh();
					mesh.subMeshCount=5;//Should be no of visible faces

					Vector3[] newVertices=new Vector3[20];
					Vector2[] newUV=new Vector2[20];
					int[] newTriangles=new int[6];

					int dimX = LevelInfo[x,y].DimX;
					int dimY = LevelInfo[x,y].DimY;
					float floorHeight = LevelInfo[x,y].floorHeight * 0.15f;//Todo add in height adjustment

					//Add all the vertices into the mesh.
					//floor vertices
					newVertices[0]=  new Vector3(0.0f,0.0f, floorHeight);
					newVertices[1]=  new Vector3(1.2f*dimX,0.0f, floorHeight);
					newVertices[2]=  new Vector3(1.2f*dimX,1.2f*dimY, floorHeight);
					newVertices[3]=  new Vector3(0.0f,1.2f*dimY, floorHeight);

					//north wall vertices
					newVertices[4]=  new Vector3(0.0f,1.2f*dimY, 0.0f);
					newVertices[5]=  new Vector3(0f,1.2f*dimY, floorHeight);
					newVertices[6]=  new Vector3(1.2f*dimX,1.2f*dimY, floorHeight);
					newVertices[7]=  new Vector3(1.2f*dimX,1.2f*dimY, 0.0f);
				
					//west wall vertices
					newVertices[8]=  new Vector3(1.2f*dimX,1.2f*dimY, 0.0f);
					newVertices[9]=  new Vector3(1.2f*dimX,1.2f*dimY, floorHeight);
					newVertices[10]=  new Vector3(1.2f*dimX,0f, floorHeight);
					newVertices[11]=  new Vector3(1.2f*dimX,0f, 0.0f);

					//east wall vertices
					newVertices[12]=  new Vector3(0f,0f, 0.0f);
					newVertices[13]=  new Vector3(0f,0f, floorHeight);
					newVertices[14]=  new Vector3(0f,1.2f*dimY, floorHeight);
					newVertices[15]=  new Vector3(0f,1.2f*dimY, 0.0f);

					//south wall vertices
					newVertices[16]=  new Vector3(1.2f*dimX,0f, 0.0f);
					newVertices[17]=  new Vector3(1.2f*dimX,0f, floorHeight);
					newVertices[18]=  new Vector3(0f,0f, floorHeight);
					newVertices[19]=  new Vector3(0f,0f, 0.0f);

					//Adjust these by dimx values. These match the surfaces above
					newUV[0]=new Vector2(0.0f,0.0f);
					newUV[1]=new Vector2(0.0f,1.0f);
					newUV[2]=new Vector2(1.0f,1.0f);
					newUV[3]=new Vector2(1.0f,0.0f);
					newUV[4]=new Vector2(0.0f,0.0f);
					newUV[5]=new Vector2(0.0f,1.0f);
					newUV[6]=new Vector2(1.0f,1.0f);
					newUV[7]=new Vector2(1.0f,0.0f);
					newUV[8]=new Vector2(0.0f,0.0f);
					newUV[9]=new Vector2(0.0f,1.0f);
					newUV[10]=new Vector2(1.0f,1.0f);
					newUV[11]=new Vector2(1.0f,0.0f);
					newUV[12]=new Vector2(0.0f,0.0f);
					newUV[13]=new Vector2(0.0f,1.0f);
					newUV[14]=new Vector2(1.0f,1.0f);
					newUV[15]=new Vector2(1.0f,0.0f);
					newUV[16]=new Vector2(0.0f,0.0f);
					newUV[17]=new Vector2(0.0f,1.0f);
					newUV[18]=new Vector2(1.0f,1.0f);
					newUV[19]=new Vector2(1.0f,0.0f);

					mesh.vertices = newVertices;
					mesh.uv = newUV;


					//Change to match surface
					//Floor
					newTriangles[0]=0;
					newTriangles[1]=1;
					newTriangles[2]=2;
					newTriangles[3]=0;
					newTriangles[4]=2;
					newTriangles[5]=3;
					mesh.SetTriangles(newTriangles,0);

					//north wall
					newTriangles[0]=0+4;
					newTriangles[1]=1+4;
					newTriangles[2]=2+4;
					newTriangles[3]=0+4;
					newTriangles[4]=2+4;
					newTriangles[5]=3+4;
					mesh.SetTriangles(newTriangles,1);

					//west wall tris
					newTriangles[0]=0+8;
					newTriangles[1]=1+8;
					newTriangles[2]=2+8;
					newTriangles[3]=0+8;
					newTriangles[4]=2+8;
					newTriangles[5]=3+8;
					mesh.SetTriangles(newTriangles,2);

					//east wall tris
					newTriangles[0]=0+12;
					newTriangles[1]=1+12;
					newTriangles[2]=2+12;
					newTriangles[3]=0+12;
					newTriangles[4]=2+12;
					newTriangles[5]=3+12;
					mesh.SetTriangles(newTriangles,3);

					//east wall tris
					newTriangles[0]=0+16;
					newTriangles[1]=1+16;
					newTriangles[2]=2+16;
					newTriangles[3]=0+16;
					newTriangles[4]=2+16;
					newTriangles[5]=3+16;
					mesh.SetTriangles(newTriangles,4);


					//mesh.triangles = newTriangles;
					mesh.RecalculateNormals();
					//mr.materials=new Material[2];

					mr.materials= MatsToUse;

					//mr.material=mat;

					mf.mesh=mesh;


					}
				}
			}


		}*/

	//*************Export Tile Map ***********************//



	public void GenerateLevelFromTileMap(int game)
		{

		short skipCeil=1;


		//Clear out the children in the transform
			foreach (Transform child in this.gameObject.transform) {
				GameObject.Destroy(child.gameObject);
			}


		for (int y = 0; y <= 63; y++)
			{
			for (int x = 0; x <= 63; x++)
				{
				RenderTile(this.gameObject, game, x, y, LevelInfo[x,y], 0, 0, 0, skipCeil);
				RenderTile(this.gameObject, game, x, y, LevelInfo[x,y], 1, 0, 0, skipCeil);
				}
			}

		//Do a ceiling

		if (game != 2)
			{
			TileInfo tmp=new TileInfo();
			//Ceiling
			tmp.tileType = 1;
			tmp.Render = 1;
			tmp.isWater = 0;
			tmp.tileX = 0;
			tmp.tileY = 0;
			tmp.DimX = 64;
			tmp.DimY = 64;
			tmp.ceilingHeight = 0;
			tmp.floorTexture = LevelInfo[0,0].shockCeilingTexture;
			tmp.shockCeilingTexture = LevelInfo[0,0].shockCeilingTexture;
			tmp.East = LevelInfo[0,0].shockCeilingTexture;//CAULK;
			tmp.West = LevelInfo[0,0].shockCeilingTexture;//CAULK;
			tmp.North = LevelInfo[0,0].shockCeilingTexture;//CAULK;
			tmp.South = LevelInfo[0,0].shockCeilingTexture;//CAULK;
			tmp.VisibleFaces[vTOP] = 0;
			tmp.VisibleFaces[vEAST] = 0;
			tmp.VisibleFaces[vBOTTOM] = 1;
			tmp.VisibleFaces[vWEST] = 0;
			tmp.VisibleFaces[vNORTH] = 0;
			tmp.VisibleFaces[vSOUTH] = 0;
			// top,east,bottom,west,north,south
			RenderTile(this.gameObject, game, tmp.tileX, tmp.tileX, tmp, 0, 0, 1, 0);
			}

		}



	void RenderTile(GameObject parent, int game, int x, int y, TileInfo t, short Water, short invert, short skipFloor, short skipCeil)
		{
		//Picks the tile to render based on tile type/flags.
		switch (t.tileType)
		{
		case TILE_SOLID:	//0
				{	//solid
				RenderSolidTile(parent, x, y, t, Water);
				return;
				}
		case TILE_OPEN:		//1
				{//open
					if (skipFloor != 1) { RenderOpenTile( parent , x, y, t, Water, 0); }	//floor
					if ((skipCeil != 1)) { RenderOpenTile( parent , x, y, t, Water, 1); }	//ceiling	
				return;
				}
		case 2:
				{//diag se
					if (skipFloor != 1) { RenderDiagSETile( parent , x, y, t, Water, 0); }//floor
					if ((skipCeil != 1)) { RenderDiagSETile( parent , x, y, t, Water, 1); }
				return;
				}

		case 3:
				{	//diag sw
					if (skipFloor != 1) { RenderDiagSWTile( parent , x, y, t, Water, 0); }//floor
					if ((skipCeil != 1)) { RenderDiagSWTile( parent , x, y, t, Water, 1); }
				return;
				}

		case 4:
				{	//diag ne
					if (skipFloor != 1) { RenderDiagNETile( parent , x, y, t, Water, invert); }//floor
					if ((skipCeil != 1)) { RenderDiagNETile( parent , x, y, t, Water, 1); }
				return;
				}

		case 5:
				{//diag nw
					if (skipFloor != 1) { RenderDiagNWTile( parent , x, y, t, Water, invert); }//floor
					if ((skipCeil != 1)) { RenderDiagNWTile( parent , x, y, t, Water, 1); }
				return;
				}

		case TILE_SLOPE_N:	//6
				{//slope n
				switch (t.shockSlopeFlag)
				{
				case SLOPE_BOTH_PARALLEL:
						{
							if (skipFloor != 1) { RenderSlopeNTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderSlopeNTile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_BOTH_OPPOSITE:
						{
							if (skipFloor != 1) { RenderSlopeNTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderSlopeSTile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_FLOOR_ONLY:
						{
							if (skipFloor != 1) { RenderSlopeNTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderOpenTile( parent , x, y, t, Water, 1); }	//ceiling
						break;
						}
				case SLOPE_CEILING_ONLY:
						{
							if (skipFloor != 1) { RenderOpenTile( parent , x, y, t, Water, 0); }	//floor
						RenderSlopeNTile( parent , x, y, t, Water, 1);
						break;
						}
				}
				return;
				}
		case TILE_SLOPE_S: //slope s	7
				{
				switch (t.shockSlopeFlag)
				{
				case SLOPE_BOTH_PARALLEL:
						{
							if (skipFloor != 1) { RenderSlopeSTile( parent , x, y, t, Water, 0); }	//floor
						RenderSlopeSTile( parent , x, y, t, Water, 1);
						break;
						}
				case SLOPE_BOTH_OPPOSITE:
						{
							if (skipFloor != 1) { RenderSlopeSTile( parent , x, y, t, Water, 0); }	//floor
						RenderSlopeNTile( parent , x, y, t, Water, 1);
						break;
						}
				case SLOPE_FLOOR_ONLY:
						{
							if (skipFloor != 1) { RenderSlopeSTile( parent , x, y, t, Water, 0); }	//floor
							if ((skipCeil != 1)) { RenderOpenTile( parent , x, y, t, Water, 1); }	//ceiling
						break;
						}
				case SLOPE_CEILING_ONLY:
						{
							if (skipFloor != 1) { RenderOpenTile( parent , x, y, t, Water, 0); }	//floor
							if ((skipCeil != 1)) { RenderSlopeSTile( parent , x, y, t, Water, 1); }
						break;
						}
				}
				return;
				}
		case TILE_SLOPE_E:		//slope e 8	
				{
				switch (t.shockSlopeFlag)
				{
				case SLOPE_BOTH_PARALLEL:
						{
							if (skipFloor != 1) { RenderSlopeETile( parent , x, y, t, Water, 0); }//floor
						RenderSlopeETile( parent , x, y, t, Water, 1);
						break;
						}
				case SLOPE_BOTH_OPPOSITE:
						{
							if (skipFloor != 1) { RenderSlopeETile( parent , x, y, t, Water, 0); }//floor
						RenderSlopeWTile( parent , x, y, t, Water, 1);
						break;
						}
				case SLOPE_FLOOR_ONLY:
						{
							if (skipFloor != 1) { RenderSlopeETile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderOpenTile( parent , x, y, t, Water, 1); }	//ceiling
						break;
						}
				case SLOPE_CEILING_ONLY:
						{
							if (skipFloor != 1) { RenderOpenTile( parent , x, y, t, Water, 0); }	//floor
							if ((skipCeil != 1)) { RenderSlopeETile( parent , x, y, t, Water, 1); }
						break;
						}
				}
				return;
				}
		case TILE_SLOPE_W: 	//9
				{ //slope w
				switch (t.shockSlopeFlag)
				{
				case SLOPE_BOTH_PARALLEL:
						{
							if (skipFloor != 1) { RenderSlopeWTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderSlopeWTile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_BOTH_OPPOSITE:
						{
							if (skipFloor != 1) { RenderSlopeWTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderSlopeETile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_FLOOR_ONLY:
						{
							if (skipFloor != 1) { RenderSlopeWTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderOpenTile( parent , x, y, t, Water, 1); }	//ceiling
						break;
						}
				case SLOPE_CEILING_ONLY:
						{
							if (skipFloor != 1) { RenderOpenTile( parent , x, y, t, Water, 0); }	//floor
							if ((skipCeil != 1)) { RenderSlopeWTile( parent , x, y, t, Water, 1); }
						break;
						}
				}
				return;
				}
		case TILE_VALLEY_NW:
				{	//valleyNw(a)
				switch (t.shockSlopeFlag)
				{
				case SLOPE_BOTH_PARALLEL:
						{
							if (skipFloor != 1) { RenderValleyNWTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderValleyNWTile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_BOTH_OPPOSITE:
						{
							if (skipFloor != 1) { RenderValleyNWTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderValleySETile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_FLOOR_ONLY:
						{
							if (skipFloor != 1) { RenderValleyNWTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderOpenTile( parent , x, y, t, Water, 1); }	//ceiling
						break;
						}
				case SLOPE_CEILING_ONLY:
						{
							if (skipFloor != 1) { RenderOpenTile( parent , x, y, t, Water, 0); }	//floor
							if ((skipCeil != 1)) { RenderValleyNWTile( parent , x, y, t, Water, 1); }
						break;
						}
				}
				return;
				}
		case TILE_VALLEY_NE:
				{	//valleyne(b)
				switch (t.shockSlopeFlag)
				{
				case SLOPE_BOTH_PARALLEL:
						{
							if (skipFloor != 1) { RenderValleyNETile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderValleyNETile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_BOTH_OPPOSITE:
						{
							if (skipFloor != 1) { RenderValleyNETile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderValleySWTile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_FLOOR_ONLY:
						{
							if (skipFloor != 1) { RenderValleyNETile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderOpenTile( parent , x, y, t, Water, 1); }	//ceiling
						break;
						}
				case SLOPE_CEILING_ONLY:
						{
							if (skipFloor != 1) { RenderOpenTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderValleyNETile( parent , x, y, t, Water, 1); }
						break;
						}
				}
				return;
				}
		case TILE_VALLEY_SE:
				{	//valleyse(c)
				switch (t.shockSlopeFlag)
				{
				case SLOPE_BOTH_PARALLEL:
						{
							if (skipFloor != 1) { RenderValleySETile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderValleySETile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_BOTH_OPPOSITE:
						{
							if (skipFloor != 1) { RenderValleySETile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderValleyNWTile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_FLOOR_ONLY:
						{
							if (skipFloor != 1) { RenderValleySETile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderOpenTile( parent , x, y, t, Water, 1); }	//ceiling
						break;
						}
				case SLOPE_CEILING_ONLY:
						{
							if (skipFloor != 1) { RenderOpenTile( parent , x, y, t, Water, 0); }	//floor
							if ((skipCeil != 1)) { RenderValleySETile( parent , x, y, t, Water, 1); }
						break;
						}
				}
				return;
				}
		case TILE_VALLEY_SW:
				{	//valleysw(d)
				switch (t.shockSlopeFlag)
				{
				case SLOPE_BOTH_PARALLEL:
						{
							if (skipFloor != 1) { RenderValleySWTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderValleySWTile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_BOTH_OPPOSITE:
						{
							if (skipFloor != 1) { RenderValleySWTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderValleyNETile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_FLOOR_ONLY:
						{
							if (skipFloor != 1) { RenderValleySWTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderOpenTile( parent , x, y, t, Water, 1); }	//ceiling
						break;
						}
				case SLOPE_CEILING_ONLY:
						{
							if (skipFloor != 1) { RenderOpenTile( parent , x, y, t, Water, 0); }	//floor
							if ((skipCeil != 1)) { RenderValleySWTile( parent , x, y, t, Water, 1); }
						break;
						}
				}
				return;
				}
		case TILE_RIDGE_SE:
				{	//ridge se(f)
				switch (t.shockSlopeFlag)
				{
				case SLOPE_BOTH_PARALLEL:
						{
							if (skipFloor != 1) { RenderRidgeSETile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderRidgeSETile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_BOTH_OPPOSITE:
						{
							if (skipFloor != 1) { RenderRidgeSETile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderRidgeNWTile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_FLOOR_ONLY:
						{
							if (skipFloor != 1) { RenderRidgeSETile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderOpenTile( parent , x, y, t, Water, 1); }	//ceiling
						break;
						}
				case SLOPE_CEILING_ONLY:
						{
							if (skipFloor != 1) { RenderOpenTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderValleySETile( parent , x, y, t, Water, 1); }
						break;
						}
				}
				return;
				}
		case TILE_RIDGE_SW:
				{	//ridgesw(g)
				switch (t.shockSlopeFlag)
				{
				case SLOPE_BOTH_PARALLEL:
						{
							if (skipFloor != 1) { RenderRidgeSWTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderRidgeSWTile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_BOTH_OPPOSITE:
						{
							if (skipFloor != 1) { RenderRidgeSWTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderRidgeNETile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_FLOOR_ONLY:
						{
							if (skipFloor != 1) { RenderRidgeSWTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderOpenTile( parent , x, y, t, Water, 1); }	//ceiling
						break;
						}
				case SLOPE_CEILING_ONLY:
						{
							if (skipFloor != 1) { RenderOpenTile( parent , x, y, t, Water, 0); }	//floor
							if ((skipCeil != 1)) { RenderValleySWTile( parent , x, y, t, Water, 1); }
						break;
						}
				}
				return;
				}
		case TILE_RIDGE_NW:
				{	//ridgenw(h)
				switch (t.shockSlopeFlag)
				{
				case SLOPE_BOTH_PARALLEL:
						{
							if (skipFloor != 1) { RenderRidgeNWTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderRidgeNWTile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_BOTH_OPPOSITE:
						{
							if (skipFloor != 1) { RenderRidgeNWTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderRidgeSETile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_FLOOR_ONLY:
						{
							if (skipFloor != 1) { RenderRidgeNWTile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderOpenTile( parent , x, y, t, Water, 1); }	//ceiling
						break;
						}
				case SLOPE_CEILING_ONLY:
						{
							if (skipFloor != 1) { RenderOpenTile( parent , x, y, t, Water, 0); }	//floor
							if ((skipCeil != 1)) { RenderValleyNWTile( parent , x, y, t, Water, 1); }
						break;
						}
				}
				return;
				}
		case TILE_RIDGE_NE:
				{	//ridgene(i)
				switch (t.shockSlopeFlag)
				{
				case SLOPE_BOTH_PARALLEL:
						{
							if (skipFloor != 1) { RenderRidgeNETile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderRidgeNETile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_BOTH_OPPOSITE:
						{
							if (skipFloor != 1) { RenderRidgeNETile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderRidgeSWTile( parent , x, y, t, Water, 1); }
						break;
						}
				case SLOPE_FLOOR_ONLY:
						{
							if (skipFloor != 1) { RenderRidgeNETile( parent , x, y, t, Water, 0); }//floor
							if ((skipCeil != 1)) { RenderOpenTile( parent , x, y, t, Water, 1); }	//ceiling
						break;
						}
				case SLOPE_CEILING_ONLY:
						{
							if (skipFloor != 1) { RenderOpenTile( parent , x, y, t, Water, 0); }	//floor
							if ((skipCeil != 1)) { RenderValleyNETile( parent , x, y, t, Water, 1); }
						break;
						}
				}
				return;
			}
		}
	}

	void RenderCuboid(GameObject parent, int x, int y, TileInfo t, short Water, int Bottom, int Top, string TileName)
		{
		//Draw a cube with no slopes.
		int NumberOfVisibleFaces=0;
		//Get the number of faces
		for (int i=0; i<6;i++)
			{
			if (t.VisibleFaces[i]==1)
				{
				NumberOfVisibleFaces++;
				}
			}
			//Allocate enough verticea and UVs for the faces
			Vector3[] verts =new Vector3[NumberOfVisibleFaces*4];
			Vector2[] uvs =new Vector2[NumberOfVisibleFaces*4];
			float floorHeight=(float)(Top*0.15f);
			float baseHeight=(float)(Bottom*0.15f);
			float dimX = t.DimX;
			float dimY = t.DimY;

			//Now create the mesh
			GameObject Tile = new GameObject(TileName);
			Tile.transform.parent=parent.transform;
			Tile.transform.position = new Vector3(x*1.2f,0.0f, y*1.2f);

			Tile.transform.localRotation=Quaternion.Euler(0f,0f,0f);
			MeshFilter mf = Tile.AddComponent<MeshFilter>();
			MeshRenderer mr =Tile.AddComponent<MeshRenderer>();
			MeshCollider mc = Tile.AddComponent<MeshCollider>();
			mc.sharedMesh=null;
			Mesh mesh = new Mesh();
			mesh.subMeshCount=NumberOfVisibleFaces;//Should be no of visible faces

			Material[] MatsToUse=new Material[NumberOfVisibleFaces];
			//Now allocate the visible faces to triangles.
			int FaceCounter=0;//Tracks which number face we are now on.
		float PolySize= Top-Bottom;
		float uv0= (float)(Bottom*0.125f);
		float uv1=(PolySize / 8.0f) + (uv0);
			for (int i=0;i<6;i++)
				{
				if (t.VisibleFaces[i]==1)
					{
					switch(i)
						{
						case vTOP:
								{
								//Set the verts	
								
						MatsToUse[FaceCounter]=MaterialMasterList[FloorTexture(fSELF, t)];
							verts[0+ (4*FaceCounter)]=  new Vector3(0.0f, 0.0f,floorHeight);
							verts[1+ (4*FaceCounter)]=  new Vector3(0.0f, 1.2f*dimY, floorHeight);
							verts[2+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,1.2f*dimY, floorHeight);
							verts[3+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0.0f, floorHeight);
								
								//Allocate UVs
								uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,0.0f);
								uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,1.0f*dimY);
								uvs[2+ (4*FaceCounter)]=new Vector2(1.0f*dimX,1.0f*dimY);
								uvs[3+ (4*FaceCounter)]=new Vector2(1.0f*dimX,0.0f);

								break;
								}

						case vNORTH:
								{
								//north wall vertices
						MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fNORTH, t)];
						verts[0+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,1.2f*dimY, baseHeight);
						verts[1+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,1.2f*dimY, floorHeight);
						verts[2+ (4*FaceCounter)]=  new Vector3(0f,1.2f*dimY, floorHeight);
						verts[3+ (4*FaceCounter)]=  new Vector3(0f,1.2f*dimY, baseHeight);

						uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,uv0);
						uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,uv1);
						uvs[2+ (4*FaceCounter)]=new Vector2(dimX,uv1);
						uvs[3+ (4*FaceCounter)]=new Vector2(dimX,uv0);


								break;
								}

						case vWEST:
								{
								//west wall vertices
						MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fWEST, t)];
								verts[0+ (4*FaceCounter)]=  new Vector3(0f,1.2f*dimY, baseHeight);
								verts[1+ (4*FaceCounter)]=  new Vector3(0f,1.2f*dimY, floorHeight);
								verts[2+ (4*FaceCounter)]=  new Vector3(0f,0f, floorHeight);
								verts[3+ (4*FaceCounter)]=  new Vector3(0f,0f, baseHeight);
						uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,uv0);
						uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,uv1);
						uvs[2+ (4*FaceCounter)]=new Vector2(dimY,uv1);
						uvs[3+ (4*FaceCounter)]=new Vector2(dimY,uv0);

								break;
								}

						case vEAST:
								{
								//east wall vertices
						MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fEAST, t)];
						verts[0+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0f, baseHeight);
						verts[1+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0f, floorHeight);
						verts[2+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,1.2f*dimY, floorHeight);
						verts[3+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,1.2f*dimY, baseHeight);
						uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,uv0);
						uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,uv1);
						uvs[2+ (4*FaceCounter)]=new Vector2(dimY,uv1);
						uvs[3+ (4*FaceCounter)]=new Vector2(dimY,uv0);

								break;
								}

						case vSOUTH:
							{
						MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fSOUTH, t)];
							//south wall vertices
						verts[0+ (4*FaceCounter)]=  new Vector3(0f,0f, baseHeight);
						verts[1+ (4*FaceCounter)]=  new Vector3(0f,0f, floorHeight);
						verts[2+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0f, floorHeight);
						verts[3+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0f, baseHeight);
						uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,uv0);
						uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,uv1);
						uvs[2+ (4*FaceCounter)]=new Vector2(dimX,uv1);
						uvs[3+ (4*FaceCounter)]=new Vector2(dimX,uv0);

							break;
							}
				case vBOTTOM:
						{
						//bottom wall vertices
						MatsToUse[FaceCounter]=MaterialMasterList[FloorTexture(fCEIL, t)];
						verts[0+ (4*FaceCounter)]=  new Vector3(0f,1.2f*dimY, baseHeight);
						verts[1+ (4*FaceCounter)]=  new Vector3(0f,0f, baseHeight);
						verts[2+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0f, baseHeight);
						verts[3+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,1.2f*dimY, baseHeight);
						//Change default UVs
						uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,0.0f);
						uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,1.0f*dimY);
						uvs[2+ (4*FaceCounter)]=new Vector2(dimX,1.0f*dimY);
						uvs[3+ (4*FaceCounter)]=new Vector2(dimX,0.0f);
						break;
						}

					}
				FaceCounter++;
				}
			}

		//Apply the uvs and create my tris
		mesh.vertices = verts;
		mesh.uv = uvs;
		FaceCounter=0;
		int [] tris = new int[6];
		for (int i=0;i<6;i++)
			{
			if (t.VisibleFaces[i]==1)
				{
				tris[0]=0+(4*FaceCounter);
				tris[1]=1+(4*FaceCounter);
				tris[2]=2+(4*FaceCounter);
				tris[3]=0+(4*FaceCounter);
				tris[4]=2+(4*FaceCounter);
				tris[5]=3+(4*FaceCounter);
				mesh.SetTriangles(tris,FaceCounter);
				FaceCounter++;
				}
			}
		
		mr.materials= MatsToUse;//mats;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mf.mesh=mesh;
		mc.sharedMesh=mesh;
		}

	void RenderSlopedCuboid(GameObject parent, int x, int y, TileInfo t, short Water, int Bottom, int Top, int SlopeDir, int Steepness, int Floor, string TileName)
		{
		//Draws a cube with sloped tops

		float AdjustUpperNorth = 0f;
		float AdjustUpperSouth = 0f;
		float AdjustUpperEast = 0f;
		float AdjustUpperWest = 0f;

		float AdjustLowerNorth = 0f;
		float AdjustLowerSouth = 0f;
		float AdjustLowerEast = 0f;
		float AdjustLowerWest = 0f;

		if (Floor == 1)
			{
			switch (SlopeDir)
				{
				case TILE_SLOPE_N:
					AdjustUpperNorth = (float)Steepness*0.15f;
					break;
				case TILE_SLOPE_S:
					AdjustUpperSouth = (float)Steepness*0.15f;
					break;
				case TILE_SLOPE_E:
					AdjustUpperEast =(float)Steepness*0.15f;
					break;
				case TILE_SLOPE_W:
					AdjustUpperWest = (float)Steepness*0.15f;
					break;
				}
			}
		if (Floor == 0)
			{
			switch (SlopeDir)
			{
			case TILE_SLOPE_N:
				AdjustUpperNorth = -(float)Steepness*0.15f;
				break;
			case TILE_SLOPE_S:
				AdjustUpperSouth = -(float)Steepness*0.15f;
				break;
			case TILE_SLOPE_E:
				AdjustUpperEast =-(float)Steepness*0.15f;
				break;
			case TILE_SLOPE_W:
				AdjustUpperWest = -(float)Steepness*0.15f;
				break;
			}
			}

		//Draw a cube with no slopes.
		int NumberOfVisibleFaces=0;
		//Get the number of faces
		for (int i=0; i<6;i++)
			{
			if (t.VisibleFaces[i]==1)
				{
				NumberOfVisibleFaces++;
				}
			}
		//Allocate enough verticea and UVs for the faces
		Material[] MatsToUse=new Material[NumberOfVisibleFaces];
		Vector3[] verts =new Vector3[NumberOfVisibleFaces*4];
		Vector2[] uvs =new Vector2[NumberOfVisibleFaces*4];
		float floorHeight=(float)(Top*0.15f);
		float baseHeight=(float)(Bottom*0.15f);
		float dimX = t.DimX;
		float dimY = t.DimY;

		//Now create the mesh
		GameObject Tile = new GameObject(TileName);
		Tile.transform.parent=parent.transform;
		Tile.transform.position = new Vector3(x*1.2f,0.0f, y*1.2f);

		Tile.transform.localRotation=Quaternion.Euler(0f,0f,0f);
		MeshFilter mf = Tile.AddComponent<MeshFilter>();
		MeshRenderer mr =Tile.AddComponent<MeshRenderer>();
		MeshCollider mc = Tile.AddComponent<MeshCollider>();
		mc.sharedMesh=null;

		Mesh mesh = new Mesh();
		mesh.subMeshCount=NumberOfVisibleFaces;//Should be no of visible faces
		//Now allocate the visible faces to triangles.
		int FaceCounter=0;//Tracks which number face we are now on.
		float PolySize= Top-Bottom;
		float uv0= (float)(Bottom*0.125f);
		float uv1=(PolySize / 8.0f) + (uv0);
		for (int i=0;i<6;i++)
			{
			if (t.VisibleFaces[i]==1)
				{
				switch(i)
				{
				case vTOP:
						{
						//Set the verts	
						MatsToUse[FaceCounter]=MaterialMasterList[FloorTexture(fSELF, t)];

						verts[0+ (4*FaceCounter)]=  new Vector3(0.0f, 0.0f,floorHeight+AdjustUpperWest+AdjustUpperSouth);
						verts[1+ (4*FaceCounter)]=  new Vector3(0.0f, 1.2f*dimY, floorHeight+AdjustUpperWest+AdjustUpperNorth);
						verts[2+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,1.2f*dimY, floorHeight+AdjustUpperNorth+AdjustUpperEast);
						verts[3+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0.0f, floorHeight+AdjustUpperSouth+AdjustUpperEast);

						//Allocate UVs
						uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,1.0f*dimY);
						uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,0.0f);
						uvs[2+ (4*FaceCounter)]=new Vector2(1.0f*dimX,0.0f);
						uvs[3+ (4*FaceCounter)]=new Vector2(1.0f*dimX,1.0f*dimY);

						break;
						}

				case vNORTH:
						{
						//north wall vertices
						MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fNORTH, t)];

						verts[0+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,1.2f*dimY, baseHeight +AdjustLowerNorth+AdjustLowerEast);
						verts[1+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,1.2f*dimY, floorHeight+AdjustUpperNorth+AdjustUpperEast);
						verts[2+ (4*FaceCounter)]=  new Vector3(0f,1.2f*dimY, floorHeight+AdjustUpperNorth+AdjustUpperWest);
						verts[3+ (4*FaceCounter)]=  new Vector3(0f,1.2f*dimY, baseHeight+AdjustLowerNorth+AdjustLowerWest);

						uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,uv0);
						uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,uv1);
						uvs[2+ (4*FaceCounter)]=new Vector2(dimX,uv1);
						uvs[3+ (4*FaceCounter)]=new Vector2(dimX,uv0);


						break;
						}

				case vWEST:
						{
						//west wall vertices
						MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fWEST, t)];
						verts[0+ (4*FaceCounter)]=  new Vector3(0f,1.2f*dimY, baseHeight+AdjustLowerWest+AdjustLowerNorth);
						verts[1+ (4*FaceCounter)]=  new Vector3(0f,1.2f*dimY, floorHeight+AdjustUpperWest+AdjustUpperNorth);
						verts[2+ (4*FaceCounter)]=  new Vector3(0f,0f, floorHeight+AdjustUpperWest+AdjustUpperSouth);
						verts[3+ (4*FaceCounter)]=  new Vector3(0f,0f, baseHeight+AdjustLowerWest+AdjustLowerSouth);
						uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,uv0);
						uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,uv1);
						uvs[2+ (4*FaceCounter)]=new Vector2(dimY,uv1);
						uvs[3+ (4*FaceCounter)]=new Vector2(dimY,uv0);

						break;
						}

				case vEAST:
						{
						//east wall vertices
						MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fEAST, t)];
						verts[0+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0f, baseHeight+AdjustLowerEast+AdjustLowerSouth);
						verts[1+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0f, floorHeight+AdjustUpperEast+AdjustUpperSouth);
						verts[2+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,1.2f*dimY, floorHeight+AdjustUpperEast+AdjustUpperNorth);
						verts[3+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,1.2f*dimY, baseHeight+AdjustLowerEast+AdjustLowerNorth);
						uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,uv0);
						uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,uv1);
						uvs[2+ (4*FaceCounter)]=new Vector2(dimY,uv1);
						uvs[3+ (4*FaceCounter)]=new Vector2(dimY,uv0);

						break;
						}

				case vSOUTH:
						{
						//south wall vertices
						MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fSOUTH, t)];
						verts[0+ (4*FaceCounter)]=  new Vector3(0f,0f, baseHeight+AdjustLowerSouth+AdjustLowerWest);
						verts[1+ (4*FaceCounter)]=  new Vector3(0f,0f, floorHeight+AdjustUpperSouth+AdjustUpperWest);
						verts[2+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0f, floorHeight+AdjustUpperSouth+AdjustUpperEast);
						verts[3+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0f, baseHeight+AdjustLowerSouth+AdjustLowerEast);
						uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,uv0);
						uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,uv1);
						uvs[2+ (4*FaceCounter)]=new Vector2(dimX,uv1);
						uvs[3+ (4*FaceCounter)]=new Vector2(dimX,uv0);

						break;
						}
				case vBOTTOM:
						{
						//bottom wall vertices
						MatsToUse[FaceCounter]=MaterialMasterList[FloorTexture(fCEIL, t)];
						//TODO:Get the lower face adjustments for this (shock only)
						verts[0+ (4*FaceCounter)]=  new Vector3(0f,1.2f*dimY, baseHeight);
						verts[1+ (4*FaceCounter)]=  new Vector3(0f,0f, baseHeight);
						verts[2+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0f, baseHeight);
						verts[3+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,1.2f*dimY, baseHeight);
						//Change default UVs
						uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,0.0f);
						uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,1.0f*dimY);
						uvs[2+ (4*FaceCounter)]=new Vector2(dimX,1.0f*dimY);
						uvs[3+ (4*FaceCounter)]=new Vector2(dimX,0.0f);
						break;
						}

				}
				FaceCounter++;
				}
			}

		//Apply the uvs and create my tris
		mesh.vertices = verts;
		mesh.uv = uvs;
		FaceCounter=0;
		int [] tris = new int[6];
		for (int i=0;i<6;i++)
			{
			if (t.VisibleFaces[i]==1)
				{
				tris[0]=0+(4*FaceCounter);
				tris[1]=1+(4*FaceCounter);
				tris[2]=2+(4*FaceCounter);
				tris[3]=0+(4*FaceCounter);
				tris[4]=2+(4*FaceCounter);
				tris[5]=3+(4*FaceCounter);
				mesh.SetTriangles(tris,FaceCounter);
				FaceCounter++;
				}
			}

		mr.materials=MatsToUse;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mf.mesh=mesh;
		mc.sharedMesh=mesh;

		}
	

	void RenderSolidTile(GameObject parent, int x, int y, TileInfo t, short Water)
		{
		if (t.Render == 1)
			{
			if (t.isWater == Water)
				{
				string TileName = "Tile_" + x.ToString("D2") + "_" + y.ToString("D2");
				RenderCuboid( parent, x, y, t, Water, -2, CEILING_HEIGHT + 1, TileName);
				}
			}
		}



	void RenderOpenTile(GameObject parent, int x, int y, TileInfo t, short Water, short invert)
		{
			if (t.Render == 1){
			string TileName = "";
			if (t.isWater == Water)
				{
				if (invert == 0)
					{
					//Bottom face 
					if ((t.hasElevator == 0))
						{
						if (t.BullFrog >0)
							{
							TileName = "Tile_" + x.ToString("D2") + "_" + y.ToString("D2");
							RenderCuboid(parent,x, y, t, Water, -16, t.floorHeight, TileName);
							}
						else
							{
							TileName = "Tile_" + x.ToString("D2") + "_" + y.ToString("D2");
							RenderCuboid(parent, x, y, t, Water, -2, t.floorHeight, TileName);
							}
						}
					else
						{
						TileName = "Tile_" + x.ToString("D2") + "_" + y.ToString("D2");
						RenderCuboid(parent, x, y, t, Water, -CEILING_HEIGHT, t.floorHeight, TileName);
						}
					}
				else
					{
					//Ceiling version of the tile
					TileName = "Tile_" + x.ToString("D2") + "_" + y.ToString("D2");
					RenderCuboid( parent, x, y, t, Water, CEILING_HEIGHT - t.ceilingHeight, CEILING_HEIGHT + 1, TileName);
					}
				}
			}
		}


	void RenderDiagSETile(GameObject parent, int x, int y, TileInfo t, short Water, short invert)
		{
		string TileName = "";
		int BLeftX; int BLeftY; int BLeftZ; int TLeftX; int TLeftY; int TLeftZ; int TRightX; int TRightY; int TRightZ;

		if (t.Render == 1)
			{
			if (invert == 0)
				{

				if (Water != 1)
					{
					//the wall part
					TileName = "Wall_" + x.ToString("D2") + "_" + y.ToString("D2");
					RenderDiagSEPortion(parent, -2, CEILING_HEIGHT + 1, t, TileName);

					}
				if (t.isWater == Water)
					{
					//it's floor
					//RenderDiagNWPortion( -2, t.floorHeight, t,"DiagNW1");
					short PreviousNorth = t.VisibleFaces[vNORTH];
					short PreviousWest = t.VisibleFaces[vWEST];
					t.VisibleFaces[vNORTH] = 0;
					t.VisibleFaces[vWEST] = 0;
					RenderOpenTile( parent , x, y, t, Water, 0);
					t.VisibleFaces[vNORTH] = PreviousNorth;
					t.VisibleFaces[vWEST] = PreviousWest;
					}
				}
			else
				{//it's ceiling
				//RenderDiagNWPortion( CEILING_HEIGHT - t.ceilingHeight, CEILING_HEIGHT + 1, t, "DiagNW2a");
				RenderOpenTile( parent , x, y, t, Water, 1);
				}
			}
		return;
		}


	///
	void RenderDiagSWTile(GameObject parent,int x, int y, TileInfo t, short Water, short invert)
		{
		string TileName = "";
		if (t.Render == 1)
			{
			if (invert == 0)
				{
				if (Water != 1)
					{
					//Its wall
					TileName = "Wall_" + x.ToString("D2") + "_" + y.ToString("D2");
					RenderDiagSWPortion(parent, -2, CEILING_HEIGHT + 1, t, TileName);
					}
				if (t.isWater == Water)
					{
					//it's floor
					//RenderDiagNEPortion( -2, t.floorHeight, t,"TileNe1");
					short PreviousNorth = t.VisibleFaces[vNORTH];
					short PreviousEast = t.VisibleFaces[vEAST];
					t.VisibleFaces[vNORTH] = 0;
					t.VisibleFaces[vEAST] = 0;
					RenderOpenTile( parent , x, y, t, Water, 0);
					t.VisibleFaces[vNORTH] = PreviousNorth;
					t.VisibleFaces[vEAST] = PreviousEast;
					}
				}
			else
				{
				//its' ceiling.
				//RenderDiagNEPortion( CEILING_HEIGHT - t.ceilingHeight, CEILING_HEIGHT + 1, t, "TileNe2");
				RenderOpenTile( parent , x, y, t, Water, 1);
				}
			}
		return;
		}



	void RenderDiagNETile(GameObject parent,int x, int y, TileInfo t, short Water, short invert)
		{
		string TileName = "";
			if (t.Render == 1){
			if (invert == 0)
				{

				if (Water != 1)
					{
					TileName = "Wall_" + x.ToString("D2") + "_" + y.ToString("D2");
					RenderDiagNEPortion(parent, -2, CEILING_HEIGHT + 1, t, TileName);
					}
				if (t.isWater == Water)
					{
					//it's floor
					//RenderDiagSWPortion( -2, t.floorHeight, t, "DiagSW2");
					short PreviousSouth = t.VisibleFaces[vSOUTH];
					short PreviousWest = t.VisibleFaces[vWEST];
					t.VisibleFaces[vSOUTH] = 0;
					t.VisibleFaces[vWEST] = 0;
					RenderOpenTile( parent , x, y, t, Water, 0);
					t.VisibleFaces[vSOUTH] = PreviousSouth;
					t.VisibleFaces[vWEST] = PreviousWest;
					}
				}
			else
				{//it's ceiling
				//RenderDiagSWPortion( CEILING_HEIGHT - t.ceilingHeight, CEILING_HEIGHT + 1, t, "DiagSE3");
				RenderOpenTile( parent , x, y, t, Water, 1);
				}
			}
		return;
		}



	void RenderDiagNWTile(GameObject parent,int x, int y, TileInfo t, short Water, short invert)
		{
		string TileName="";
		if (t.Render == 1)
			{
			if (invert == 0)
				{
				if (Water != 1)
					{
					//It's wall.
					TileName = "Wall_" + x.ToString("D2") + "_" + y.ToString("D2");
					RenderDiagNWPortion(parent, -2, CEILING_HEIGHT + 1, t, TileName);
					}


				if (t.isWater == Water)
					{
					//it's floor
					//RenderDiagSEPortion( -2, t.floorHeight, t, "DiagSE2");
					short PreviousSouth = t.VisibleFaces[vSOUTH];
					short PreviousEast = t.VisibleFaces[vEAST];
					t.VisibleFaces[vSOUTH] = 0;
					t.VisibleFaces[vEAST] = 0;
					RenderOpenTile( parent , x, y, t, Water, 0);
					t.VisibleFaces[vSOUTH] = PreviousSouth;
					t.VisibleFaces[vEAST] = PreviousEast;
					}
				}
			else
				{//it's ceiling
				//RenderDiagSEPortion( CEILING_HEIGHT - t.ceilingHeight, CEILING_HEIGHT + 1, t, "DiagSE3");
				RenderOpenTile( parent , x, y, t, Water, 1);
				}
			}
		return;
		}




	////
	/// 
	/// 


	void RenderSlopeNTile(GameObject parent,int x, int y, TileInfo t, short Water, short invert)
		{
		string TileName="";
			if (t.Render == 1){
			if (invert == 0)
				{
				if (t.isWater == Water)
					{
					//A floor
					TileName = "Tile_" + x.ToString("D2") + "_" + y.ToString("D2");
					RenderSlopedCuboid(parent, x, y, t, Water, -2, t.floorHeight, TILE_SLOPE_N, t.shockSteep, 1, TileName);
					}
				}
			else
				{
				//It's invert
				TileName = "Ceiling_" + x.ToString("D2") + "_" + y.ToString("D2");
				RenderSlopedCuboid(parent, x, y, t, Water, CEILING_HEIGHT - t.ceilingHeight, CEILING_HEIGHT + 1, TILE_SLOPE_N, t.shockSteep, 0, TileName);
				}
			}
		return;
		}

	void RenderSlopeSTile(GameObject parent,int x, int y, TileInfo t, short Water, short invert)
		{
		string TileName="";
			if (t.Render == 1){
			if (invert == 0)
				{
				if (t.isWater == Water)
					{
					//A floor
					TileName = "Tile_" + x.ToString("D2") + "_" + y.ToString("D2");
					RenderSlopedCuboid(parent, x, y, t, Water, -2, t.floorHeight, TILE_SLOPE_S, t.shockSteep, 1, TileName);
					}
				}
			else
				{
				//It's invert
				TileName = "Ceiling_" + x.ToString("D2") + "_" + y.ToString("D2");
				RenderSlopedCuboid(parent, x, y, t, Water, CEILING_HEIGHT - t.ceilingHeight, CEILING_HEIGHT + 1, TILE_SLOPE_S, t.shockSteep, 0, TileName);
				}
			}
		return;
		}

	void RenderSlopeWTile(GameObject parent,int x, int y, TileInfo t, short Water, short invert)
		{
		string TileName="";
			if (t.Render == 1){
			if (invert == 0)
				{
				if (t.isWater == Water)
					{
					//A floor
					TileName = "Tile_" + x.ToString("D2") + "_" + y.ToString("D2");
					RenderSlopedCuboid(parent, x, y, t, Water, -2, t.floorHeight, TILE_SLOPE_W, t.shockSteep, 1, TileName);
					}
				}
			else
				{
				//It's invert
				TileName = "Ceiling_" + x.ToString("D2") + "_" + y.ToString("D2");
				RenderSlopedCuboid(parent, x, y, t, Water, CEILING_HEIGHT - t.ceilingHeight, CEILING_HEIGHT + 1, TILE_SLOPE_W, t.shockSteep, 0, TileName);
				}
			}
		return;
		}

	void RenderSlopeETile(GameObject parent,int x, int y, TileInfo t, short Water, short invert)
		{
		string TileName="";
			if (t.Render == 1){
			if (invert == 0)
				{
				if (t.isWater == Water)
					{
					//A floor
					TileName = "Tile_" + x.ToString("D2") + "_" + y.ToString("D2");
					RenderSlopedCuboid(parent, x, y, t, Water, -2, t.floorHeight, TILE_SLOPE_E, t.shockSteep, 1, TileName);
					}
				}
			else
				{
				//It's invert
				TileName = "Ceiling_" + x.ToString("D2") + "_" + y.ToString("D2");
				RenderSlopedCuboid(parent, x, y, t, Water, CEILING_HEIGHT - t.ceilingHeight, CEILING_HEIGHT + 1, TILE_SLOPE_E, t.shockSteep, 0, TileName);
				}
			}
		return;
		}


	void RenderValleyNWTile(GameObject parent,int x, int y, TileInfo t, short Water, short invert)
		{
		int originalTile = t.tileType;
		t.tileType = TILE_SLOPE_N;
		RenderSlopeNTile( parent , x, y, t, Water, invert);
		t.tileType = TILE_SLOPE_W;
		RenderSlopeWTile( parent , x, y, t, Water, invert);
		t.tileType = originalTile;
		return;
		}


	void RenderValleyNETile(GameObject parent,int x, int y, TileInfo t, short Water, short invert)
		{
		int originalTile = t.tileType;
		t.tileType = TILE_SLOPE_E;
		RenderSlopeETile( parent , x, y, t, Water, invert);
		t.tileType = TILE_SLOPE_N;
		RenderSlopeNTile( parent , x, y, t, Water, invert);
		t.tileType = originalTile;
		return;
		}

	void RenderValleySWTile(GameObject parent,int x, int y, TileInfo t, short Water, short invert)
		{
		int originalTile = t.tileType;
		t.tileType = TILE_SLOPE_W;
		RenderSlopeWTile( parent , x, y, t, Water, invert);
		t.tileType = TILE_SLOPE_S;
		RenderSlopeSTile( parent , x, y, t, Water, invert);
		t.tileType = originalTile;
		return;
		}

	void RenderValleySETile(GameObject parent,int x, int y, TileInfo t, short Water, short invert)
		{
		int originalTile = t.tileType;
		t.tileType = TILE_SLOPE_E;
		RenderSlopeETile( parent , x, y, t, Water, invert);
		t.tileType = TILE_SLOPE_S;
		RenderSlopeSTile( parent , x, y, t, Water, invert);
		t.tileType = originalTile;
		return;
		}




	void RenderRidgeNWTile(GameObject parent, int x, int y, TileInfo t, short Water, short invert)
		{

		if (t.Render == 1)
			{
			if (invert == 0)

				{//consists of a slope n and a slope w
				if (t.isWater == Water)
					{
					RenderSlopeNTile( parent , x, y, t, Water, invert);
					RenderSlopeWTile( parent , x, y, t, Water, invert);
					}
				}
			else
				{
				//made of upper slope e and upper slope s

				RenderSlopeETile( parent , x, y, t, Water, invert);
				RenderSlopeSTile( parent , x, y, t, Water, invert);
				}

			}
		return;
		}

	void RenderRidgeNETile(GameObject parent, int x, int y, TileInfo t, short Water, short invert)
		{
		//consists of a slope n and a slope e

			if (t.Render == 1){
				if (invert == 0){
				if (t.isWater == Water)
					{
					RenderSlopeNTile( parent , x, y, t, Water, invert);
					RenderSlopeETile( parent , x, y, t, Water, invert);
					}
				}
			else
				{//invert is south and west slopes
				RenderSlopeSTile( parent , x, y, t, Water, invert);
				RenderSlopeWTile( parent , x, y, t, Water, invert);
				}
			}

		return;
		}

	void RenderRidgeSWTile(GameObject parent, int x, int y, TileInfo t, short Water, short invert)
		{
		//consists of a slope s and a slope w
		if (t.Render == 1)
		if (invert == 0)
			{
				{
				if (t.isWater == Water)
					{
					RenderSlopeSTile( parent , x, y, t, Water, invert);
					RenderSlopeWTile( parent , x, y, t, Water, invert);
					}
				}
			}
		else
			{	//invert is n and w slopes
			//render a ceiling version of this tile
			RenderSlopeNTile( parent , x, y, t, Water, invert);
			RenderSlopeWTile( parent , x, y, t, Water, invert);

			}
		return;
		}

	void RenderRidgeSETile(GameObject parent, int x, int y, TileInfo t, short Water, short invert)
		{
		//consists of a slope s and a slope e
		//done

		if (t.Render == 1)
			{
			if (invert == 0)
				{
				if (t.isWater == Water)
					{
					RenderSlopeSTile( parent , x, y, t, Water, invert);
					RenderSlopeETile( parent , x, y, t, Water, invert);
					}
				}
			else
				{//invert is n w
				//render a ceiling version of this tile
				//top and bottom faces move up
				RenderSlopeNTile( parent , x, y, t, Water, invert);
				RenderSlopeWTile( parent , x, y, t, Water, invert);
				}
			}
		return;
		}
	



	void RenderDiagSEPortion(GameObject parent,int Bottom, int Top, TileInfo t, string TileName)
		{
		//Does a thing.
		//Draws 3 meshes. Outward diagonal wall. Back and side if visible.


		int NumberOfVisibleFaces=1;//Will always have the diag.
		//Get the number of faces
		for (int i=0; i<6;i++)
			{
			if ((i==vNORTH) || (i==vWEST))
				{
				if (t.VisibleFaces[i]==1)
					{
					NumberOfVisibleFaces++;
					}
				}
			}
		//Allocate enough verticea and UVs for the faces
		Material[] MatsToUse=new Material[NumberOfVisibleFaces];
		Vector3[] verts =new Vector3[NumberOfVisibleFaces*4];
		Vector2[] uvs =new Vector2[NumberOfVisibleFaces*4];
		float floorHeight=(float)(Top*0.15f);
		float baseHeight=(float)(Bottom*0.15f);
		float dimX = t.DimX;
		float dimY = t.DimY;

		//Now create the mesh
		GameObject Tile = new GameObject(TileName);
		Tile.transform.parent=parent.transform;
		Tile.transform.position = new Vector3(t.tileX*1.2f,0.0f, t.tileY*1.2f);

		Tile.transform.localRotation=Quaternion.Euler(0f,0f,0f);
		MeshFilter mf = Tile.AddComponent<MeshFilter>();
		MeshRenderer mr =Tile.AddComponent<MeshRenderer>();
		MeshCollider mc = Tile.AddComponent<MeshCollider>();
		mc.sharedMesh=null;

		Mesh mesh = new Mesh();
		mesh.subMeshCount=NumberOfVisibleFaces;//Should be no of visible faces

		//Now allocate the visible faces to triangles.
		int FaceCounter=0;//Tracks which number face we are now on.
		MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fSELF, t)];
		float PolySize= Top-Bottom;
		float uv0= (float)(Bottom*0.125f);
		float uv1=(PolySize / 8.0f) + (uv0);
		//Set the diagonal face first

		verts[0]=  new Vector3(0f,0f, baseHeight);
		verts[1]=  new Vector3(0f,0f, floorHeight);
		verts[2]=  new Vector3(-1.2f,1.2f, floorHeight);
		verts[3]=  new Vector3(-1.2f,1.2f, baseHeight);

		uvs[0]=new Vector2(0.0f,uv0);
		uvs[1]=new Vector2(0.0f,uv1);
		uvs[2]=new Vector2(1,uv1);
		uvs[3]=new Vector2(1,uv0);
		FaceCounter++;

		for (int i=0;i<6;i++)
			{
			if ((t.VisibleFaces[i]==1) && ((i==vNORTH) || (i==vWEST)))
				{//Will only render north or west if needed.
				switch(i)
				{
				case vNORTH:
						{
						//north wall vertices
						MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fNORTH, t)];
						verts[0+ (4*FaceCounter)]=  new Vector3(-1.2f,1.2f, baseHeight);
						verts[1+ (4*FaceCounter)]=  new Vector3(-1.2f,1.2f, floorHeight);
						verts[2+ (4*FaceCounter)]=  new Vector3(0f,1.2f, floorHeight);
						verts[3+ (4*FaceCounter)]=  new Vector3(0f,1.2f, baseHeight);

						uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,uv0);
						uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,uv1);
						uvs[2+ (4*FaceCounter)]=new Vector2(1,uv1);
						uvs[3+ (4*FaceCounter)]=new Vector2(1,uv0);

						break;
						}

				case vWEST:
						{
						//west wall vertices
						MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fWEST, t)];
						verts[0+ (4*FaceCounter)]=  new Vector3(0f,1.2f, baseHeight);
						verts[1+ (4*FaceCounter)]=  new Vector3(0f,1.2f, floorHeight);
						verts[2+ (4*FaceCounter)]=  new Vector3(0f,0f, floorHeight);
						verts[3+ (4*FaceCounter)]=  new Vector3(0f,0f, baseHeight);
						uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,uv0);
						uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,uv1);
						uvs[2+ (4*FaceCounter)]=new Vector2(1,uv1);
						uvs[3+ (4*FaceCounter)]=new Vector2(1,uv0);

						break;
						}

				}
				FaceCounter++;
				}
			}

		//Apply the uvs and create my tris
		mesh.vertices = verts;
		mesh.uv = uvs;
		int [] tris = new int[6];
		//Tris for diagonal.

		tris[0]=0;
		tris[1]=1;
		tris[2]=2;
		tris[3]=0;
		tris[4]=2;
		tris[5]=3;
		mesh.SetTriangles(tris,0);
		FaceCounter=1;

		for (int i=0;i<6;i++)
			{
			if ((i==vNORTH) || (i==vWEST))
				{
				if (t.VisibleFaces[i]==1)
					{
					tris[0]=0+(4*FaceCounter);
					tris[1]=1+(4*FaceCounter);
					tris[2]=2+(4*FaceCounter);
					tris[3]=0+(4*FaceCounter);
					tris[4]=2+(4*FaceCounter);
					tris[5]=3+(4*FaceCounter);
					mesh.SetTriangles(tris,FaceCounter);
					FaceCounter++;
					}
				}
			}

		mr.materials= MatsToUse;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mf.mesh=mesh;
		mc.sharedMesh=mesh;
		return;
		}

	void RenderDiagSWPortion(GameObject parent,int Bottom, int Top, TileInfo t, string TileName)
		{
		//Does a thing.
		//Does a thing.
		//Draws 3 meshes. Outward diagonal wall. Back and side if visible.
		int NumberOfVisibleFaces=1;//Will always have the diag.
		//Get the number of faces
		for (int i=0; i<6;i++)
			{
			if ((i==vNORTH) || (i==vEAST))
				{
				if (t.VisibleFaces[i]==1)
					{
					NumberOfVisibleFaces++;
					}
				}
			}
		//Allocate enough verticea and UVs for the faces
		Material[] MatsToUse=new Material[NumberOfVisibleFaces];
		Vector3[] verts =new Vector3[NumberOfVisibleFaces*4];
		Vector2[] uvs =new Vector2[NumberOfVisibleFaces*4];
		float floorHeight=(float)(Top*0.15f);
		float baseHeight=(float)(Bottom*0.15f);
		float dimX = t.DimX;
		float dimY = t.DimY;

		//Now create the mesh
		GameObject Tile = new GameObject(TileName);
		Tile.transform.parent=parent.transform;
		Tile.transform.position = new Vector3(t.tileX*1.2f,0.0f, t.tileY*1.2f);

		Tile.transform.localRotation=Quaternion.Euler(0f,0f,0f);
		MeshFilter mf = Tile.AddComponent<MeshFilter>();
		MeshRenderer mr =Tile.AddComponent<MeshRenderer>();
		MeshCollider mc = Tile.AddComponent<MeshCollider>();
		mc.sharedMesh=null;

		Mesh mesh = new Mesh();
		mesh.subMeshCount=NumberOfVisibleFaces;//Should be no of visible faces

		//Now allocate the visible faces to triangles.
		int FaceCounter=0;//Tracks which number face we are now on.

		MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fSELF, t)];

		float PolySize= Top-Bottom;
		float uv0= (float)(Bottom*0.125f);
		float uv1=(PolySize / 8.0f) + (uv0);
		//Set the diagonal face first
		verts[0]=  new Vector3(0f,1.2f, baseHeight);
		verts[1]=  new Vector3(0f,1.2f, floorHeight);
		verts[2]=  new Vector3(-1.2f,0f, floorHeight);
		verts[3]=  new Vector3(-1.2f,0f, baseHeight);

		uvs[0]=new Vector2(0.0f,uv0);
		uvs[1]=new Vector2(0.0f,uv1);
		uvs[2]=new Vector2(1,uv1);
		uvs[3]=new Vector2(1,uv0);
		FaceCounter++;

		for (int i=0;i<6;i++)
			{
			if ((t.VisibleFaces[i]==1) && ((i==vNORTH) || (i==vEAST)))
				{//Will only render north or west if needed.
				switch(i)
				{
				case vNORTH:
						{
						//north wall vertices
						MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fNORTH, t)];
						verts[0+ (4*FaceCounter)]=  new Vector3(-1.2f,1.2f, baseHeight);
						verts[1+ (4*FaceCounter)]=  new Vector3(-1.2f,1.2f, floorHeight);
						verts[2+ (4*FaceCounter)]=  new Vector3(0f,1.2f, floorHeight);
						verts[3+ (4*FaceCounter)]=  new Vector3(0f,1.2f, baseHeight);

						uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,uv0);
						uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,uv1);
						uvs[2+ (4*FaceCounter)]=new Vector2(1,uv1);
						uvs[3+ (4*FaceCounter)]=new Vector2(1,uv0);

						break;
						}

				case vEAST:
						{
						//east wall vertices
						MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fEAST, t)];
						verts[0+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0f, baseHeight);
						verts[1+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0f, floorHeight);
						verts[2+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,1.2f*dimY, floorHeight);
						verts[3+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,1.2f*dimY, baseHeight);
						uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,uv0);
						uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,uv1);
						uvs[2+ (4*FaceCounter)]=new Vector2(dimY,uv1);
						uvs[3+ (4*FaceCounter)]=new Vector2(dimY,uv0);

						break;
						}

				}
				FaceCounter++;
				}
			}

		//Apply the uvs and create my tris
		mesh.vertices = verts;
		mesh.uv = uvs;
		int [] tris = new int[6];
		//Tris for diagonal.

		tris[0]=0;
		tris[1]=1;
		tris[2]=2;
		tris[3]=0;
		tris[4]=2;
		tris[5]=3;
		mesh.SetTriangles(tris,0);
		FaceCounter=1;

		for (int i=0;i<6;i++)
			{
			if ((i==vNORTH) || (i==vEAST))
				{
				if (t.VisibleFaces[i]==1)
					{
					tris[0]=0+(4*FaceCounter);
					tris[1]=1+(4*FaceCounter);
					tris[2]=2+(4*FaceCounter);
					tris[3]=0+(4*FaceCounter);
					tris[4]=2+(4*FaceCounter);
					tris[5]=3+(4*FaceCounter);
					mesh.SetTriangles(tris,FaceCounter);
					FaceCounter++;
					}
				}
			}

		mr.materials= MatsToUse;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mf.mesh=mesh;
		mc.sharedMesh=mesh;
		return;
		}

	void RenderDiagNWPortion(GameObject parent,int Bottom, int Top, TileInfo t, string TileName)
		{
		//Does a thing.
		//Does a thing.
		//Draws 3 meshes. Outward diagonal wall. Back and side if visible.


		int NumberOfVisibleFaces=1;//Will always have the diag.
		//Get the number of faces
		for (int i=0; i<6;i++)
			{
			if ((i==vSOUTH) || (i==vEAST))
				{
				if (t.VisibleFaces[i]==1)
					{
					NumberOfVisibleFaces++;
					}
				}
			}
		//Allocate enough verticea and UVs for the faces
		Material[] MatsToUse=new Material[NumberOfVisibleFaces];
		Vector3[] verts =new Vector3[NumberOfVisibleFaces*4];
		Vector2[] uvs =new Vector2[NumberOfVisibleFaces*4];
		float floorHeight=(float)(Top*0.15f);
		float baseHeight=(float)(Bottom*0.15f);
		float dimX = t.DimX;
		float dimY = t.DimY;

		//Now create the mesh
		GameObject Tile = new GameObject(TileName);
		Tile.transform.parent=parent.transform;
		Tile.transform.position = new Vector3(t.tileX*1.2f,0.0f, t.tileY*1.2f);

		Tile.transform.localRotation=Quaternion.Euler(0f,0f,0f);
		MeshFilter mf = Tile.AddComponent<MeshFilter>();
		MeshRenderer mr =Tile.AddComponent<MeshRenderer>();
		MeshCollider mc = Tile.AddComponent<MeshCollider>();
		mc.sharedMesh=null;

		Mesh mesh = new Mesh();
		mesh.subMeshCount=NumberOfVisibleFaces;//Should be no of visible faces
		//Now allocate the visible faces to triangles.
		int FaceCounter=0;//Tracks which number face we are now on.
		float PolySize= Top-Bottom;
		float uv0= (float)(Bottom*0.125f);
		float uv1=(PolySize / 8.0f) + (uv0);
		//Set the diagonal face first
		MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fSELF, t)];

		verts[0]=  new Vector3(-1.2f,1.2f, baseHeight);
		verts[1]=  new Vector3(-1.2f,1.2f, floorHeight);
		verts[2]=  new Vector3(0f,0f, floorHeight);
		verts[3]=  new Vector3(0f,0f, baseHeight);

		uvs[0]=new Vector2(0.0f,uv0);
		uvs[1]=new Vector2(0.0f,uv1);
		uvs[2]=new Vector2(1,uv1);
		uvs[3]=new Vector2(1,uv0);
		FaceCounter++;

		for (int i=0;i<6;i++)
			{
			if ((t.VisibleFaces[i]==1) && ((i==vNORTH) || (i==vWEST)))
				{//Will only render north or west if needed.
				switch(i)
				{
				case vEAST:
						{
						//east wall vertices
						MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fEAST, t)];
						verts[0+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0f, baseHeight);
						verts[1+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0f, floorHeight);
						verts[2+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,1.2f*dimY, floorHeight);
						verts[3+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,1.2f*dimY, baseHeight);
						uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,uv0);
						uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,uv1);
						uvs[2+ (4*FaceCounter)]=new Vector2(dimY,uv1);
						uvs[3+ (4*FaceCounter)]=new Vector2(dimY,uv0);

						break;
						}

				case vSOUTH:
						{
						//south wall vertices
						MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fSOUTH, t)];
						verts[0+ (4*FaceCounter)]=  new Vector3(0f,0f, baseHeight);
						verts[1+ (4*FaceCounter)]=  new Vector3(0f,0f, floorHeight);
						verts[2+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0f, floorHeight);
						verts[3+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0f, baseHeight);
						uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,uv0);
						uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,uv1);
						uvs[2+ (4*FaceCounter)]=new Vector2(dimX,uv1);
						uvs[3+ (4*FaceCounter)]=new Vector2(dimX,uv0);

						break;
						}

				}
				FaceCounter++;
				}
			}

		//Apply the uvs and create my tris
		mesh.vertices = verts;
		mesh.uv = uvs;
		int [] tris = new int[6];
		//Tris for diagonal.

		tris[0]=0;
		tris[1]=1;
		tris[2]=2;
		tris[3]=0;
		tris[4]=2;
		tris[5]=3;
		mesh.SetTriangles(tris,0);
		FaceCounter=1;

		for (int i=0;i<6;i++)
			{
			if ((i==vSOUTH) || (i==vEAST))
				{
				if (t.VisibleFaces[i]==1)
					{
					tris[0]=0+(4*FaceCounter);
					tris[1]=1+(4*FaceCounter);
					tris[2]=2+(4*FaceCounter);
					tris[3]=0+(4*FaceCounter);
					tris[4]=2+(4*FaceCounter);
					tris[5]=3+(4*FaceCounter);
					mesh.SetTriangles(tris,FaceCounter);
					FaceCounter++;
					}
				}
			}

		mr.materials= MatsToUse;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mf.mesh=mesh;
		mc.sharedMesh=mesh;
		return;
		}

	void RenderDiagNEPortion(GameObject parent,int Bottom, int Top, TileInfo t, string TileName)
		{
		//Does a thing.
		//Does a thing.
		//Draws 3 meshes. Outward diagonal wall. Back and side if visible.


		int NumberOfVisibleFaces=1;//Will always have the diag.
		//Get the number of faces
		for (int i=0; i<6;i++)
			{
			if ((i==vSOUTH) || (i==vWEST))
				{
				if (t.VisibleFaces[i]==1)
					{
					NumberOfVisibleFaces++;
					}
				}
			}
		//Allocate enough verticea and UVs for the faces
		Material[] MatsToUse=new Material[NumberOfVisibleFaces];
		Vector3[] verts =new Vector3[NumberOfVisibleFaces*4];
		Vector2[] uvs =new Vector2[NumberOfVisibleFaces*4];

		float floorHeight=(float)(Top*0.15f);
		float baseHeight=(float)(Bottom*0.15f);
		float dimX = t.DimX;
		float dimY = t.DimY;

		//Now create the mesh
		GameObject Tile = new GameObject(TileName);
		Tile.transform.parent=parent.transform;
		Tile.transform.position = new Vector3(t.tileX*1.2f,0.0f, t.tileY*1.2f);

		Tile.transform.localRotation=Quaternion.Euler(0f,0f,0f);
		MeshFilter mf = Tile.AddComponent<MeshFilter>();
		MeshRenderer mr =Tile.AddComponent<MeshRenderer>();
		MeshCollider mc = Tile.AddComponent<MeshCollider>();
		mc.sharedMesh=null;

		Mesh mesh = new Mesh();
		mesh.subMeshCount=NumberOfVisibleFaces;//Should be no of visible faces

		//Now allocate the visible faces to triangles.
		int FaceCounter=0;//Tracks which number face we are now on.
		float PolySize= Top-Bottom;
		float uv0= (float)(Bottom*0.125f);
		float uv1=(PolySize / 8.0f) + (uv0);
		//Set the diagonal face first
		MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fSELF, t)];
		verts[0]=  new Vector3(-1.2f,0f, baseHeight);
		verts[1]=  new Vector3(-1.2f,0f, floorHeight);
		verts[2]=  new Vector3(0f,1.2f, floorHeight);
		verts[3]=  new Vector3(0f,1.2f, baseHeight);

		uvs[0]=new Vector2(0.0f,uv0);
		uvs[1]=new Vector2(0.0f,uv1);
		uvs[2]=new Vector2(1,uv1);
		uvs[3]=new Vector2(1,uv0);
		FaceCounter++;

		for (int i=0;i<6;i++)
			{
			if ((t.VisibleFaces[i]==1) && ((i==vNORTH) || (i==vWEST)))
				{//Will only render north or west if needed.
				switch(i)
				{
				case vSOUTH:
						{
						//south wall vertices
						MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fSOUTH, t)];
						verts[0+ (4*FaceCounter)]=  new Vector3(0f,0f, baseHeight);
						verts[1+ (4*FaceCounter)]=  new Vector3(0f,0f, floorHeight);
						verts[2+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0f, floorHeight);
						verts[3+ (4*FaceCounter)]=  new Vector3(-1.2f*dimX,0f, baseHeight);
						uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,uv0);
						uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,uv1);
						uvs[2+ (4*FaceCounter)]=new Vector2(dimX,uv1);
						uvs[3+ (4*FaceCounter)]=new Vector2(dimX,uv0);

						break;
						}

				case vWEST:
						{
						//west wall vertices
						MatsToUse[FaceCounter]=MaterialMasterList[WallTexture(fWEST, t)];
						verts[0+ (4*FaceCounter)]=  new Vector3(0f,1.2f, baseHeight);
						verts[1+ (4*FaceCounter)]=  new Vector3(0f,1.2f, floorHeight);
						verts[2+ (4*FaceCounter)]=  new Vector3(0f,0f, floorHeight);
						verts[3+ (4*FaceCounter)]=  new Vector3(0f,0f, baseHeight);
						uvs[0+ (4*FaceCounter)]=new Vector2(0.0f,uv0);
						uvs[1 +(4*FaceCounter)]=new Vector2(0.0f,uv1);
						uvs[2+ (4*FaceCounter)]=new Vector2(1,uv1);
						uvs[3+ (4*FaceCounter)]=new Vector2(1,uv0);

						break;
						}

				}
				FaceCounter++;
				}
			}

		//Apply the uvs and create my tris
		mesh.vertices = verts;
		mesh.uv = uvs;
		int [] tris = new int[6];
		//Tris for diagonal.

		tris[0]=0;
		tris[1]=1;
		tris[2]=2;
		tris[3]=0;
		tris[4]=2;
		tris[5]=3;
		mesh.SetTriangles(tris,0);
		FaceCounter=1;

		for (int i=0;i<6;i++)
			{
			if ((i==vSOUTH) || (i==vWEST))
				{
				if (t.VisibleFaces[i]==1)
					{
					tris[0]=0+(4*FaceCounter);
					tris[1]=1+(4*FaceCounter);
					tris[2]=2+(4*FaceCounter);
					tris[3]=0+(4*FaceCounter);
					tris[4]=2+(4*FaceCounter);
					tris[5]=3+(4*FaceCounter);
					mesh.SetTriangles(tris,FaceCounter);
					FaceCounter++;
					}
				}
			}

		mr.materials= MatsToUse;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mf.mesh=mesh;
		mc.sharedMesh=mesh;
		return;
		}





	/////

	public void CleanUp(int game)
		{
		int x; int y;

			for (x=0;x<64;x++){
				for (y=0;y<64;y++){
				//lets test this tile for visibility
				//A tile is invisible if it only touches other solid tiles and has no objects or does not have a terrain change.
					if ((LevelInfo[x,y].tileType ==0) && (LevelInfo[x,y].indexObjectList == 0)  && (LevelInfo[x,y].TerrainChange == 0)){
					switch (y)
					{
					case 0:	//bottom row
						switch (x)
						{
						case 0:	//bl corner
							if ((LevelInfo[x + 1,y].tileType == 0) && (LevelInfo[x,y + 1].tileType == 0)
								&& (LevelInfo[x + 1,y].TerrainChange == 0) && (LevelInfo[x,y+1].TerrainChange == 0))
								{LevelInfo[x,y].Render =0 ; ;break;}
								else {LevelInfo[x,y].Render =1 ;break;}
						case 63://br corner
							if ((LevelInfo[x - 1,y].tileType == 0) && (LevelInfo[x,y + 1].tileType == 0)
								&& (LevelInfo[x - 1,y].TerrainChange == 0) && (LevelInfo[x,y+1].TerrainChange == 0))
								{LevelInfo[x,y].Render =0 ;break;}
								else {LevelInfo[x,y].Render =1 ;break;}
						default: // invert t
							if ((LevelInfo[x + 1,y].tileType == 0) && (LevelInfo[x,y + 1].tileType == 0) && (LevelInfo[x + 1,y].tileType == 0) 
								&& (LevelInfo[x+1,y].TerrainChange == 0) && (LevelInfo[x,y+1].TerrainChange == 0) && (LevelInfo[x+1,y].TerrainChange == 0))
								{LevelInfo[x,y].Render =0 ;break;}
								else {LevelInfo[x,y].Render =1 ;break;}
						}
						break;
					case 63: //Top row
						switch (x)
						{
						case 0:	//tl corner
							if ((LevelInfo[x + 1,y].tileType == 0) && (LevelInfo[x,y - 1].tileType == 0) 
								&& (LevelInfo[x + 1,y].TerrainChange == 0) && (LevelInfo[x,y-1].TerrainChange == 0))
								{LevelInfo[x,y].Render =0 ;break;}
								else {LevelInfo[x,y].Render =1 ;break;}
						case 63://tr corner
							if ((LevelInfo[x - 1,y].tileType == 0) && (LevelInfo[x,y - 1].tileType == 0) 
								&& (LevelInfo[x - 1,y].TerrainChange == 0) && (LevelInfo[x,y-1].TerrainChange == 0))
								{LevelInfo[x,y].Render =0 ;break;}
								else {LevelInfo[x,y].Render =1 ;break;}
						default: //  t
							if ((LevelInfo[x + 1,y].tileType == 0) && (LevelInfo[x,y - 1].tileType == 0) && (LevelInfo[x - 1,y].tileType == 0) 
								&& (LevelInfo[x + 1,y].TerrainChange == 0) && (LevelInfo[x,y - 1].TerrainChange == 0) && (LevelInfo[x-1,y].TerrainChange == 0))
								{LevelInfo[x,y].Render =0;break;}
								else {LevelInfo[x,y].Render =1 ;break;}
						}	
						break;
					default: //
						switch (x)
						{
						case 0:		//left edge
							if ((LevelInfo[x,y + 1].tileType == 0) && (LevelInfo[x + 1,y].tileType == 0) && (LevelInfo[x,y - 1].tileType == 0) 
								&& (LevelInfo[x,y+1].TerrainChange == 0) && (LevelInfo[x+1,y].TerrainChange == 0) && (LevelInfo[x,y-1].TerrainChange == 0))
								{LevelInfo[x,y].Render =0;break;}
								else {LevelInfo[x,y].Render =1 ;break;}
						case 63:	//right edge
							if ((LevelInfo[x,y + 1].tileType == 0) && (LevelInfo[x - 1,y].tileType == 0) && (LevelInfo[x,y - 1].tileType == 0) 
								&& (LevelInfo[x,y+1].TerrainChange == 0) && (LevelInfo[x-1,y].TerrainChange == 0) && (LevelInfo[x,y-1].TerrainChange == 0))
								{LevelInfo[x,y].Render =0 ;break;}
								else {LevelInfo[x,y].Render =1 ;break;}
						default:		//+
							if ((LevelInfo[x,y + 1].tileType == 0) && (LevelInfo[x + 1,y].tileType == 0) && (LevelInfo[x,y - 1].tileType == 0) && (LevelInfo[x - 1,y].tileType == 0) 
								&& (LevelInfo[x,y + 1].TerrainChange == 0) && (LevelInfo[x + 1,y].TerrainChange == 0) && (LevelInfo[x,y - 1].TerrainChange == 0) && (LevelInfo[x-1,y].TerrainChange == 0))
								{LevelInfo[x,y].Render =0; break;}
								else {LevelInfo[x,y].Render =1 ;break;}
						}
						break;
					}
					}
				}
			}






		//return;
		//if (game == SHOCK) {return;}
		int j=1 ;
		//Now lets combine the solids along particular axis
			for (x=0;x<63;x++){
				for (y=0;y<63;y++){
				if  ((LevelInfo[x,y].Grouped ==0))
					{
					j=1;
					while ((LevelInfo[x,y].Render == 1) && (LevelInfo[x,y + j].Render == 1) && (LevelInfo[x,y + j].Grouped == 0) && (LevelInfo[x,y + j].BullFrog == 0))		//&& (LevelInfo[x,y].tileType ==0) && (LevelInfo[x,y+j].tileType ==0)
						{
						//combine these two if they match and they are not already part of a group
							if (DoTilesMatch(LevelInfo[x,y],LevelInfo[x,y+j])){
							LevelInfo[x,y+j].Render =0;
							LevelInfo[x,y+j].Grouped =1;
							LevelInfo[x,y].Grouped =1;
							//LevelInfo[x,y].DimY++;
							j++;
							}
						else
							{
							break;
							}

						}
					LevelInfo[x,y].DimY =(short)( LevelInfo[x,y].DimY +j-1);
					j=1;
					}
				}
			}
		j=1;

		////Now lets combine solids along the other axis
			for (y=0;y<63;y++){
				for (x=0;x<63;x++){
				if  ((LevelInfo[x,y].Grouped ==0))
					{
					j=1;
					while ((LevelInfo[x,y].Render == 1) && (LevelInfo[x + j,y].Render == 1) && (LevelInfo[x + j,y].Grouped == 0) && (LevelInfo[x + j,y].BullFrog == 0))		//&& (LevelInfo[x,y].tileType ==0) && (LevelInfo[x,y+j].tileType ==0)
						{
						//combine these two if they  match and they are not already part of a group
							if (DoTilesMatch(LevelInfo[x,y],LevelInfo[x+j,y])){
							LevelInfo[x+j,y].Render =0;
							LevelInfo[x+j,y].Grouped =1;
							LevelInfo[x,y].Grouped =1;
							//LevelInfo[x,y].DimY++;
							j++;
							}
						else
							{
							break;
							}

						}
					LevelInfo[x,y].DimX =(short)( LevelInfo[x,y].DimX +j-1);
					j=1;
					}
				}
			}

		//Clear invisible faces on solid tiles. 
		//TODO:Support all 64x64 tiles
			for (y = 0; y<=63; y++){
				for (x = 0; x<=63; x++){
				if ((LevelInfo[x,y].tileType == TILE_SOLID))
					{
					int dimx = LevelInfo[x,y].DimX;
					int dimy = LevelInfo[x,y].DimY;

					if (x == 0)
						{
						LevelInfo[x,y].VisibleFaces[vWEST]=0;
						}
					if (x == 63)
						{
						LevelInfo[x,y].VisibleFaces[vEAST] = 0;
						}
					if (y == 0 )
						{
						LevelInfo[x,y].VisibleFaces[vSOUTH] = 0;
						}

					if (y == 63)
						{
						LevelInfo[x,y].VisibleFaces[vNORTH] = 0;
						}
					if ((x+dimx <= 63) && (y+dimy <= 63))
						{
						if ((LevelInfo[x + dimx,y].tileType == TILE_SOLID) && (LevelInfo[x + dimx,y].TerrainChange == 0) && (LevelInfo[x,y].TerrainChange == 0))//Tile to the east is a solid
							{
							LevelInfo[x,y].VisibleFaces[vEAST] = 0;
							LevelInfo[x + dimx,y].VisibleFaces[vWEST] = 0;
							}
						if ((LevelInfo[x,y + dimy].tileType == TILE_SOLID) && (LevelInfo[x,y].TerrainChange == 0) && (LevelInfo[x,y + dimy].TerrainChange == 0))//TIle to the north is a solid
							{
							LevelInfo[x,y].VisibleFaces[vNORTH] = 0;
							LevelInfo[x,y + dimy].VisibleFaces[vSOUTH] = 0;
							}
						}
					}
				}
			}

		//Clear invisible faces on diagonals
			for (y = 1; y < 63; y++){
				for (x = 1; x < 63; x++){
				switch (LevelInfo[x,y].tileType)
				{
				case TILE_DIAG_NW:
						{
						if ((LevelInfo[x,y - 1].tileType == TILE_SOLID) && (LevelInfo[x,y-1].TerrainChange == 0))
							{
							LevelInfo[x,y].VisibleFaces[vSOUTH]=0;
							LevelInfo[x,y - 1].VisibleFaces[vNORTH]=0;
							}
						if ((LevelInfo[x + 1,y].tileType == TILE_SOLID) && (LevelInfo[x + 1,y].TerrainChange==0))
							{
							LevelInfo[x,y].VisibleFaces[vEAST] = 0;
							LevelInfo[x + 1,y].VisibleFaces[vWEST]=0;
							}
						}
					break;
				case TILE_DIAG_NE:
						{
						if ((LevelInfo[x,y - 1].tileType == TILE_SOLID) && (LevelInfo[x,y - 1].TerrainChange == 0))
							{
							LevelInfo[x,y].VisibleFaces[vSOUTH] = 0;
							LevelInfo[x,y - 1].VisibleFaces[vNORTH] = 0;
							}
						if ((LevelInfo[x - 1,y].tileType == TILE_SOLID) && (LevelInfo[x - 1,y].TerrainChange == 0))
							{
							LevelInfo[x,y].VisibleFaces[vWEST] = 0;
							LevelInfo[x - 1,y].VisibleFaces[vEAST] = 0;
							}
						}
					break;
				case TILE_DIAG_SE:
						{
						if ((LevelInfo[x,y + 1].tileType == TILE_SOLID) && (LevelInfo[x,y + 1].TerrainChange == 0))
							{
							LevelInfo[x,y].VisibleFaces[vNORTH] = 0;
							LevelInfo[x,y + 1].VisibleFaces[vSOUTH] = 0;
							}
						if ((LevelInfo[x - 1,y].tileType == TILE_SOLID) && (LevelInfo[x - 1,y].TerrainChange == 0))
							{
							LevelInfo[x,y].VisibleFaces[vWEST] = 0;
							LevelInfo[x - 1,y].VisibleFaces[vEAST] = 0;
							}
						}
					break;
				case TILE_DIAG_SW:
						{
						if ((LevelInfo[x,y + 1].tileType == TILE_SOLID) && (LevelInfo[x,y + 1].TerrainChange == 0))
							{
							LevelInfo[x,y].VisibleFaces[vNORTH] = 0;
							LevelInfo[x,y + 1].VisibleFaces[vSOUTH] = 0;
							}
						if ((LevelInfo[x + 1,y].tileType == TILE_SOLID) && (LevelInfo[x + 1,y].TerrainChange == 0))
							{
							LevelInfo[x,y].VisibleFaces[vEAST] = 0;
							LevelInfo[x + 1,y].VisibleFaces[vWEST] = 0;
							}
						}
					break;
				}

				}

			}

			for (y = 1; y < 63; y++){
				for (x = 1; x < 63; x++){
				if ((LevelInfo[x,y].tileType == TILE_OPEN) && (LevelInfo[x,y].TerrainChange == 0) && (LevelInfo[x,y].BullFrog == 0))
					{
					if (
						((LevelInfo[x + 1,y].tileType == TILE_OPEN) && (LevelInfo[x + 1,y].TerrainChange == 0) && (LevelInfo[x + 1,y].BullFrog == 0) && (LevelInfo[x + 1,y].floorHeight >= LevelInfo[x,y].floorHeight))
						||
						(LevelInfo[x + 1,y].tileType == TILE_SOLID) && (LevelInfo[x + 1,y].TerrainChange == 0) && (LevelInfo[x + 1,y].BullFrog == 0)
					)
						{
						LevelInfo[x,y].VisibleFaces[vEAST]=0;
						}


					if (
						((LevelInfo[x - 1,y].tileType == TILE_OPEN) && (LevelInfo[x - 1,y].TerrainChange == 0) && (LevelInfo[x - 1,y].BullFrog == 0) && (LevelInfo[x - 1,y].floorHeight >= LevelInfo[x,y].floorHeight))
						||
						(LevelInfo[x - 1,y].tileType == TILE_SOLID) && (LevelInfo[x - 1,y].TerrainChange == 0) && (LevelInfo[x - 1,y].BullFrog == 0)
					)
						{
						LevelInfo[x,y].VisibleFaces[vWEST] = 0;
						}


					if (
						((LevelInfo[x,y + 1].tileType == TILE_OPEN) && (LevelInfo[x,y + 1].TerrainChange == 0) && (LevelInfo[x,y + 1].BullFrog == 0) && (LevelInfo[x,y + 1].floorHeight >= LevelInfo[x,y].floorHeight))
						||
						(LevelInfo[x,y + 1].tileType == TILE_SOLID) && (LevelInfo[x,y + 1].TerrainChange == 0) && (LevelInfo[x,y + 1].BullFrog == 0)
					)
						{
						LevelInfo[x,y].VisibleFaces[vNORTH] = 0;
						}

					if (
						((LevelInfo[x,y - 1].tileType == TILE_OPEN) && (LevelInfo[x,y - 1].TerrainChange == 0) && (LevelInfo[x,y - 1].BullFrog == 0) && (LevelInfo[x,y - 1].floorHeight >= LevelInfo[x,y].floorHeight))
						||
						(LevelInfo[x,y - 1].tileType == TILE_SOLID) && (LevelInfo[x,y - 1].TerrainChange == 0) && (LevelInfo[x,y - 1].BullFrog == 0)
					)
						{
						LevelInfo[x,y].VisibleFaces[vSOUTH] = 0;
						}
					}

				}
			}
		//Make sure solids & opens are still consistently visible.
			for (y = 1; y < 63; y++){
				for (x = 1; x < 63; x++){

				if ((LevelInfo[x,y].tileType == TILE_SOLID) || (LevelInfo[x,y].tileType == TILE_OPEN))
					{
					int dimx = LevelInfo[x,y].DimX;
					int dimy = LevelInfo[x,y].DimY;
					if (dimx>1)
						{//Make sure the ends are set properly
						LevelInfo[x,y].VisibleFaces[vEAST] = LevelInfo[x + dimx - 1,y].VisibleFaces[vEAST];
						}
					if (dimy>1)
						{
						LevelInfo[x,y].VisibleFaces[vNORTH] = LevelInfo[x,y + dimy - 1].VisibleFaces[vNORTH];
						}

					//Check along each axis
					for (int i = 0; i < LevelInfo[x,y].DimX; i++)
						{
						if (LevelInfo[x + i,y].VisibleFaces[vNORTH] == 1)
							{
							LevelInfo[x,y].VisibleFaces[vNORTH]=1;
							}
						if (LevelInfo[x + i,y].VisibleFaces[vSOUTH] == 1)
							{
							LevelInfo[x,y].VisibleFaces[vSOUTH] = 1;
							}
						}

					for (int i = 0; i < LevelInfo[x,y].DimY; i++)
						{
						if (LevelInfo[x,y+i].VisibleFaces[vEAST] == 1)
							{
							LevelInfo[x,y].VisibleFaces[vEAST] = 1;
							}
						if (LevelInfo[x,y+i].VisibleFaces[vWEST] == 1)
							{
							LevelInfo[x,y].VisibleFaces[vWEST] = 1;
							}
						}

					}
				}
			}
			for ( y = 0; y <= 63; y++){
				LevelInfo[0,y].VisibleFaces[vEAST]=1;
				LevelInfo[63,y].VisibleFaces[vWEST] = 1;
			}
			for ( x = 0; x <= 63; x++){
				LevelInfo[x,0].VisibleFaces[vNORTH] = 1;
				LevelInfo[x,63].VisibleFaces[vSOUTH] = 1;
			}
		}



	bool DoTilesMatch(TileInfo t1, TileInfo t2)
		{//TODO:Tiles have a lot more properties now.
		//if ((t1.tileType >1) || (t1.hasElevator==1) || (t1.TerrainChange ==1) ||  (t2.hasElevator==1) || (t2.TerrainChange ==1) || (t1.isWater ==1) || (t2.isWater ==1)){	//autofail no none solid/open/special.
			if ((t1.tileType >1) || (t1.hasElevator == 1) || (t1.TerrainChange == 1) || (t2.hasElevator == 1) || (t2.TerrainChange == 1)){	//autofail no none solid/open/special.
			return false;
			}
		else
			{
			if ((t1.tileType==0) && (t2.tileType == 0))	//solid
				{
				return ((t1.wallTexture==t2.wallTexture) && (t1.West == t2.West) && (t1.South == t2.South) && (t1.East == t2.East) && (t1.North == t2.North) && (t1.UseAdjacentTextures == t2.UseAdjacentTextures));
				}
			else
				{
				return (t1.shockCeilingTexture == t2.shockCeilingTexture)
					&& (t1.floorTexture == t2.floorTexture) 
					&& (t1.floorHeight == t2.floorHeight)
					&& (t1.ceilingHeight == t2.ceilingHeight )
					&& (t1.DimX==t2.DimX) && (t1.DimY == t2.DimY ) 
					&& (t1.wallTexture == t2.wallTexture)
					&& (t1.tileType == t2.tileType ) 
					&& (t1.isDoor==0) && (t2.isDoor ==0);//
				}
			}
		}



	int WallTexture(int face, TileInfo t)
		{
		int wallTexture;
		int ceilOffset = 0;
		wallTexture = t.wallTexture;
		switch (face)
		{
		case fSOUTH:
			wallTexture = t.South;
			break;
		case fNORTH:
			wallTexture = t.North;
			break;
		case fEAST:
			wallTexture = t.East;
			break;
		case fWEST:
			wallTexture = t.West;
			break;
		}
		if ((wallTexture<0) || (wallTexture >512))
			{
			wallTexture = 0;
			}
		return wallTexture;
		}

	int FloorTexture(int face, TileInfo t)
		{
		int floorTexture;

		if (face == fCEIL)
			{
			floorTexture = t.shockCeilingTexture;
			}
		else
			{
			floorTexture = t.floorTexture;
			}

		if ((floorTexture<0) || (floorTexture >512))
			{
			floorTexture = 0;
			}
		return floorTexture;
		}



	public static char[] unpackUW2(char[] tmp, long address_pointer, ref int datalen)
		{

		//Robbed and changed slightly from the Labyrinth of Worlds implementation project.
		//This decompresses UW2 blocks.
		long	len = (int)DataLoader.getValAtAddress(tmp,address_pointer,32);	//lword(base);
		long block_address=address_pointer;
		char[] buf = new char[len+100];
		char[] up = buf;
		long upPtr=0;
		long bufPtr=0;
		datalen = 0;
		address_pointer += 4;

		while(upPtr < len)
			{
			int		bits = tmp[address_pointer++];
			for(int r=0; r<8; r++)
				{
				if((bits & 1)==1)
					{
					//printf("transfer %d\ at %d\n", byte(base),base);
					up[upPtr++] = tmp[address_pointer++];
					datalen = datalen+1;
					}
				else
					{
					int	o = tmp[address_pointer++];
					int	c = tmp[address_pointer++];

					o |= (c&0xF0)<<4;
					c = (c&15) + 3;
					o = o+18;
					if(o > (upPtr-bufPtr))
						o -= 0x1000;
					while(o < (upPtr-bufPtr-0x1000))
						o += 0x1000;

					while(c-- >0)
						{
						if (o<0)
							{
							up[upPtr++]= tmp[tmp.GetUpperBound(0)+ o++];
							}
						else
							{
							up[upPtr++]= buf[o++];
							}
						//Output.text=upPtr.ToString() + " = " + o.ToString();
	

						datalen = datalen+1;
						}
					}
				bits >>= 1;
				}
			}

		return buf;
		}



	int getFloorTexUw2(char[] buffer, long textureOffset, long tileData)
		{//gets floor texture data at bits 10-13 of the tile data
		long val = (tileData >>10) & 0x0F;	//gets the index of the texture
		//look it up in texture block for it's absolute index for wxx.tr
		return (int)DataLoader.getValAtAddress(buffer,textureOffset+(val*2),16);	
		//	return ((tileData >>10) & 0x0F);	//was	11
		}

	int getWallTexUw2(char[]  buffer, long textureOffset, long tileData)
		{
		//gets wall texture data at bits 0-5 (+16) of the tile data(2nd part)
		//return ((tileData >>17)& 0x3F);
		long val = (tileData & 0x3F);	//gets the index of the texture
		return (int)DataLoader.getValAtAddress(buffer,textureOffset+(val*2),16);
		//return (tileData& 0x3F);
		}



}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace UnderworldEditor
{
    class ArtUI
    {

        public static void setPixelAtLocation(BitmapUW currentimg, PictureBox img, int x, int y, int newpixel)
        {
            if (currentimg.artdata == null) { return; }
            if (img.Image == null) { return; }
            currentimg.artdata.ImageCache[currentimg.ImageNo].image.SetPixel(x, y, currentimg.ImagePalette.ColorAtPixel((byte)newpixel, false));
            currentimg.Modified = true;
            switch (main.CurrentImage.ImageType)
            {
                case BitmapUW.ImageTypes.Texture:
                    {
                        TextureLoader texdata = (TextureLoader)currentimg.artdata;
                        switch (main.curgame)
                        {
                            case main.GAME_UW1:
                                if (currentimg.ImageNo < 210)
                                {
                                    texdata.texturebufferW[currentimg.FileOffset + y * 64 + x] = (char)newpixel;
                                }
                                else
                                {
                                    texdata.texturebufferF[currentimg.FileOffset + y * 32 + x] = (char)newpixel;
                                }
                                currentimg.artdata.Modified = true;
                                break;
                        }
                        break;
                    }
                case BitmapUW.ImageTypes.Byt:               
                    {
                        switch (main.curgame)
                        {
                            case main.GAME_UW1:
                                {
                                    BytLoader byt = (BytLoader)currentimg.artdata;
                                    byt.ImageFileData[currentimg.FileOffset + y * (currentimg.image.Width) + x] = (char)newpixel;
                                    currentimg.artdata.Modified = true;
                                    break;
                                }
                        }
                        break;
                    }
                case BitmapUW.ImageTypes.EightBitUncompressed:
                    {
                        GRLoader gr = (GRLoader)currentimg.artdata;
                        gr.ImageFileData[currentimg.FileOffset + y * (currentimg.image.Width) + x] = (char)newpixel;
                        currentimg.artdata.Modified = true;
                        break;
                    }
                default:
                    {
                        
                    // img.Image = main.CurrentImage.image;
                        break;
                    }
            }
            main.CurrentImage = currentimg.artdata.ImageCache[currentimg.ImageNo];
            img.Image = main.CurrentImage.image;

        }

        public static void SaveBytDataUW1(char[] artfile, string filename)
        {
            Util.WriteStreamFile(main.basepath + filename, artfile);
        }

        public static void SaveTextureData(char[] artfile, bool IsUW1Wall)
        {
            switch (main.curgame)
            {
                case main.GAME_UW1:
                    if (IsUW1Wall)
                    {
                        Util.WriteStreamFile(main.basepath + "\\data\\W64.TR", artfile);
                    }
                    else
                    {
                        Util.WriteStreamFile(main.basepath + "\\data\\F32.TR", artfile);
                    }                    
                    break;
                case main.GAME_UW2:
                    Util.WriteStreamFile(main.basepath + "\\data\\T64.TR", artfile);
                    break;
            }           
        }

    }//endclass
}
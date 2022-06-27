using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShemeMaster
{
    public static class MountingTypeLibrary
    {

        static Dictionary<int, string> mountingTypeE = new Dictionary<int, string>()
        {
           { 18, "1,5"},
            { 25, "2,5"},
            { 34, "4"},
            { 43, "6"},
            { 60, "10"},
            { 80, "16"},
            { 101, "25"},
            { 126, "35"},
            { 153, "50"},
            { 196, "70"},
            { 238, "95"},
            { 276, "120"},
            { 319, "150"},
            { 364, "185"},
            { 430, "240"},
            { 497, "300"}
        };

        static Dictionary<int, string> mountingType1PhA2 = new Dictionary<int, string>()
        {
            { 14, "1,5"},
            { 18, "2,5"},
            { 25, "4"},
            { 32, "6"},
            { 43, "10"},
            { 57, "16"},
            { 75, "25"},
            { 92, "35"},
            { 110, "50"},
            { 139, "70"},
            { 167, "95"},
            { 192, "120"},
            { 219, "150"},
            { 248, "185"},
            { 291, "240"},
            { 334, "300"}
        };

        static Dictionary<int, string> mountingType1PhB2 = new Dictionary<int, string>()
        {
           { 16, "1,5"},
            { 23, "2,5"},
            { 30, "4"},
            { 38, "6"},
            { 52, "10"},
            { 69, "16"},
            { 90, "25"},
            { 111, "35"},
            { 133, "50"},
            { 168, "70"},
            { 201, "95"},
            { 232, "120"},
            { 258, "150"},
            { 294, "185"},
            { 344, "240"},
            { 394, "300"}
        };

        static Dictionary<int, string> mountingType1PhC = new Dictionary<int, string>()
        {
            { 19, "1,5"},
            { 27, "2,5"},
            { 36, "4"},
            { 46, "6"},
            { 63, "10"},
            { 85, "16"},
            { 112, "25"},
            { 138, "35"},
            { 168, "50"},
            { 213, "70"},
            { 258, "95"},
            { 299, "120"},
            { 344, "150"},
            { 392, "185"},
            { 461, "240"},
            { 530, "300"}
        };

        static Dictionary<int, string> mountingType1PhD1 = new Dictionary<int, string>()
        {
           { 22, "1,5"},
            { 29, "2,5"},
            { 37, "4"},
            { 46, "6"},
            { 60, "10"},
            { 78, "16"},
            { 99, "25"},
            { 119, "35"},
            { 140, "50"},
            { 173, "70"},
            { 204, "95"},
            { 231, "120"},
            { 261, "150"},
            { 292, "185"},
            { 336, "240"},
            { 379, "300"}
        };

        static Dictionary<int, string> mountingType3PhA2 = new Dictionary<int, string>()
        {
           { 13, "1,5"},
            { 17, "2,5"},
            { 23, "4"},
            { 29, "6"},
            { 39, "10"},
            { 52, "16"},
            { 68, "25"},
            { 83, "35"},
            { 99, "50"},
            { 125, "70"},
            { 150, "95"},
            { 172, "120"},
            { 196, "150"},
            { 223, "185"},
            { 261, "240"},
            { 298, "300"}
        };

        static Dictionary<int, string> mountingType3PhB2 = new Dictionary<int, string>()
        {
           { 15, "1,5"},
            { 20, "2,5"},
            { 27, "4"},
            { 34, "6"},
            { 46, "10"},
            { 62, "16"},
            { 80, "25"},
            { 99, "35"},
            { 118, "50"},
            { 149, "70"},
            { 179, "95"},
            { 206, "120"},
            { 225, "150"},
            { 255, "185"},
            { 297, "240"},
            { 339, "300"}
        };

        static Dictionary<int, string> mountingType3PhC = new Dictionary<int, string>()
        {
           { 17, "1,5"},
            { 24, "2,5"},
            { 32, "4"},
            { 41, "6"},
            { 57, "10"},
            { 76, "16"},
            { 96, "25"},
            { 119, "35"},
            { 144, "50"},
            { 184, "70"},
            { 223, "95"},
            { 259, "120"},
            { 299, "150"},
            { 341, "185"},
            { 403, "240"},
            { 464, "300"}
        };

        static Dictionary<int, string> mountingType3PhD1 = new Dictionary<int, string>()
        {
           { 18, "1,5"},
            { 24, "2,5"},
            { 30, "4"},
            { 38, "6"},
            { 50, "10"},
            { 64, "16"},
            { 82, "25"},
            { 98, "35"},
            { 116, "50"},
            { 143, "70"},
            { 169, "95"},
            { 192, "120"},
            { 217, "150"},
            { 243, "185"},
            { 280, "240"},
            { 316, "300"}
        };

        public static Dictionary<string, Dictionary<int, string>> Data = new Dictionary<string, Dictionary<int, string>>
        {
            { "E-1", mountingTypeE},
            { "E-3", mountingTypeE},
            { "A2-1", mountingType1PhA2},
            { "B2-1", mountingType1PhB2},
            { "C-1", mountingType1PhC},
            { "D1-1", mountingType1PhD1},
            { "A2-3", mountingType3PhA2},
            { "B2-3", mountingType3PhB2},
            { "C-3", mountingType3PhC},
            { "D1-3", mountingType3PhD1},
        };
        //public static Dictionary<string, Dictionary<int, string>> Data = new Dictionary<string, Dictionary<int, string>>
        //{
        //    { "E230 В", mountingTypeE},
        //    { "E400 В", mountingTypeE},
        //    { "A2230 В", mountingType1PhA2},
        //    { "B2230 В", mountingType1PhB2},
        //    { "C230 В", mountingType1PhC},
        //    { "D1230 В", mountingType1PhD1},
        //    { "A2400 В", mountingType3PhA2},
        //    { "B2400 В", mountingType3PhB2},
        //    { "C400 В", mountingType3PhC},
        //    { "D1400 В", mountingType3PhD1},
        //};
    }
}

// LoadingIndicator.cs
// Copyright Karel Kroeze, 2018-2018

namespace SteamWorkshopUploader
{
    public class LoadingIndicator
    {
        private int state;

        public override string ToString()
        {
            switch ( state++ )
            {
                case 0:
                    return "|";
                case 1:
                    return "/";
                case 2:
                    return "-";
                case 3:
                    return "\\";
                case 4:
                    return "|";
                case 5:
                    return "/";
                case 6:
                    return "-";
                default:
                    state = 0;
                    return "\\";
            }
        }
    }
}
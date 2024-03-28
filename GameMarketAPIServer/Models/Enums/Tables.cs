﻿namespace GameMarketAPIServer.Models.Enums
{
    public enum CRUD
    {
        Create,
        Read,
        Update,
        Delete,
        UpSert
    };
    public enum Tables
    {
        //xbox
        XboxUserProfiles,
        XboxGameTitles,
        XboxGameBundles,
        XboxProductIds,
        XboxTitleDetails,
        XboxMarketDetails,
        XboxGameGenres,
        XboxGroupData,
        XboxTitleDevices,
        XboxProductPlatforms,
        //steam
        SteamAppIDs,
        SteamAppDetails,
        SteamAppDevelopers,
        SteamAppPublishers,
        SteamPackages,
        SteamPackageDetails,
        SteamPackageIDs,
        SteamAppPlatforms,
        SteamAppGenres,


        //gamemarket
        GameMarketGameTitles,
        GameMarketXboxLink,
        GameMarketSteamLink
        //other
    };


    public static class TablesExtension
    {
        public static string To_String(this Tables table)
        {
            switch (table)
            {

                //xbox tables
                case Tables.XboxGameGenres: return "GameGenres";
                case Tables.XboxGameTitles: return "GameTitles";
                case Tables.XboxUserProfiles: return "UserProfiles";
                case Tables.XboxProductIds: return "ProductIds";
                case Tables.XboxGameBundles: return "GameBundles";
                case Tables.XboxTitleDetails: return "TitleDetails";
                case Tables.XboxMarketDetails: return "MarketDetails";
                case Tables.XboxGroupData: return "GroupData";
                case Tables.XboxTitleDevices: return "titleDevices";
                case Tables.XboxProductPlatforms: return "productPlatforms";

                //steam tables
                case Tables.SteamAppIDs: return "appIDs";
                case Tables.SteamAppDetails: return "AppDetails";
                case Tables.SteamAppPublishers: return "AppPublishers";
                case Tables.SteamAppDevelopers: return "AppDevelopers";
                case Tables.SteamAppGenres: return "AppGenres";
                case Tables.SteamPackageIDs: return "PackageIDs";
                case Tables.SteamPackageDetails: return "PackageDetails";
                case Tables.SteamPackages: return "Packages";
                case Tables.SteamAppPlatforms: return "AppPlatforms";


                case Tables.GameMarketGameTitles: return "GameTitles";
                case Tables.GameMarketXboxLink: return "XboxLink";
                case Tables.GameMarketSteamLink: return "SteamLink";

                default: return "";
            }
        }

        public static int toInt(this Tables table)
        {

            return (int)table;
        }
    }
}
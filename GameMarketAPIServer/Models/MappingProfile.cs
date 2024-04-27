using AutoMapper;
using GameMarketAPIServer.Services;
using static GameMarketAPIServer.Models.DataBaseSchemas;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace GameMarketAPIServer.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //CreateMap<XboxSchema.UserProfileTable, CompositeTableData>();
            //For Xbox ParseGameTitle
            CreateMap<XboxProduct, XboxSchema.TitleDetailTable>()
                .ForMember(d => d.modernTitleID, opt => opt.MapFrom(src => src.alternateIds.First(ai => ai.idType == "XboxTitleId").value))
                .ForMember(d => d.GameBundles, opt => opt.MapFrom(src => src.marketProperties
                    .SelectMany(mp => mp.relatedProducts).Where(rp => rp.relationshipType == "Bundle")
                    .Select(rp => new XboxSchema.GameBundleTable(rp.relatedProductId, src.productId))));


            //for Xbox ParsePlayerHistory
            CreateMap<XboxTitle, XboxSchema.GameTitleTable>()
                .ForMember(des => des.isGamePass, opt => opt.MapFrom(src => src.gamePass.isGamePass))
                .ForMember(des => des.TitleDevices, opt => opt.MapFrom(src => src.devices
                    .Select(device => new XboxSchema.TitleDeviceTable(src.modernTitleId, device)).ToList()))
                .ForMember(des => des.titleName, opt => opt.MapFrom(src => src.name));

            CreateMap<SteamApp, SteamSchema.AppIDsTable>()
                .ForMember(des => des.appID, opt => opt.MapFrom(src => src.appID));

        }

        public static XboxSchema.MarketDetailTable MapXboxProductToMarketDetail(XboxProduct product)
        {
            var table = new XboxSchema.MarketDetailTable();

            table.productID = product.productId;

            foreach (var localProp in product.localizedProperties)
            {
                table.developerName = localProp.developerName;
                table.publisherName = localProp.publisherName;
                table.productTitle = localProp.productTitle;

                if (localProp.Images != null)
                    foreach (var image in localProp.Images)
                    {
                        //find the poster image
                        if (image == null) continue;
                        if (image.imagePurpose == "Poster")
                        {
                            table.posterImage = image.uri;
                        }
                    }
                //might add description later
                foreach (var marketProperty in product.marketProperties)
                {
                    if (DateTime.TryParse(marketProperty.OriginalReleaseDate, out var origRelease))
                    {
                        table.releaseDate = origRelease;
                    }
                }

                foreach (var displaySkuAvail in product.displaySkuAvailabilities)
                {
                    if (table.purchasable) break;

                    int validAvails = 0;

                    foreach (var availability in displaySkuAvail.availabilities)
                    {
                        // table.purchasable = false;
                        //find the date for each sku;
                        //if no date range is give, idk what to do with it.
                        if (availability.conditions.startDate == null || availability.conditions.endDate == null) continue;


                        //Get at the first date.
                        if (table.startDate == DateTime.MinValue)
                            if (DateTime.TryParse(availability.conditions.startDate, out var start))
                                table.startDate = start;

                        if (table.endDate == DateTime.MinValue)
                            if (DateTime.TryParse(availability.conditions.endDate, out var end))
                                table.endDate = end;

                        //if you cant purchase it, this avail is pointless
                        //There are some gamepass games that cant be purchased though, I will have to deal with that later.
                        if (!availability.actions.Contains("Purchase") && !table.purchasable)
                        {
                            continue;
                        }
                        else
                        {
                            //most likely a gamepass game or some other service
                            if (availability.remediationRequired == true)
                            {
                                continue;
                            }
                            //This sku should be purchasable
                            else
                            {

                                if (DateTime.TryParse(availability.conditions.startDate, out var start))
                                    table.startDate = start;

                                if (DateTime.TryParse(availability.conditions.endDate, out var end))
                                    table.endDate = end;

                                //The current sale is ongoing.
                                if (DateTime.UtcNow > table.startDate && DateTime.UtcNow < table.endDate)
                                {
                                    //The current sku is purchasable
                                    if (availability.orderManagementData.price.currencyCode != null)
                                        table.currencyCode = availability.orderManagementData.price.currencyCode;

                                    if (++validAvails > 1)
                                    {
                                        Console.WriteLine("multiple skus valid??");
                                    }
                                    table.purchasable = true;
                                    table.listPrice = availability.orderManagementData.price.listPrice;
                                    table.msrp = availability.orderManagementData.price.msrp;

                                    if (availability.conditions.clientConditions.allowedPlatforms != null)
                                        foreach (var platform in availability.conditions.clientConditions.allowedPlatforms)
                                            //table.platforms.Add(platform.platformName);
                                            table.ProductPlatforms.Add(new XboxSchema.ProductPlatformTable(product.productId, platform.platformName));
                                    //Only finding the first skew
                                    break;
                                }

                            }
                        }
                    }
                }


            }


            return table;
        }

        public static SteamSchema.AppDetailsTable MapSteamAppDetails(SteamAppData appData)
        {
            var table = new SteamSchema.AppDetailsTable();

            table.appID = appData.steam_appid;
            table.appName = appData.name;
            table.appType = appData.type;

            table.isFree = appData.is_free;

            if (!table.isFree)
            {
                table.msrp = appData.price_overview.initial;
                table.listPrice = appData.price_overview.final;
            }

            //add genres
            if (appData.genres != null && appData.genres.Any())
            {
                foreach (var genre in appData.genres)
                {
                    if (genre.description == "") continue;
                    table.Genres.Add(new SteamSchema.AppGenresTable(table.appID, genre.description) { AppDetails = table });
                }
            }
            if (appData.dlc != null && appData.dlc.Any())
            {
                foreach (var dlc in appData.dlc.ToHashSet())
                {
                    if (dlc == 0) continue;
                    table.DLCs.Add(new SteamSchema.AppDLCTable(table.appID, dlc) { AppDetails = table });
                }
            }
            if (appData.packages != null && appData.packages.Any())
            {
                foreach (var package in appData.packages.ToHashSet())
                {
                    if (package == 0) continue;
                    table.Packeges.Add(new SteamSchema.PackagesTable(table.appID, package) { AppDetails = table });
                }
            }

            //need to add dlcs later


            //Add Developers
            if (appData.developers != null && appData.developers.Any())
            {
                foreach (var developer in appData.developers.ToHashSet())
                {
                    if (developer == "" || developer == null) continue;
                    table.Developers.Add(new SteamSchema.AppDevelopersTable(table.appID, developer) { AppDetails = table });
                }
            }
            //publishers
            if (appData.publishers != null && appData.publishers.Any())
            {
                foreach (var publisher in appData.publishers.ToHashSet())
                {
                    if (publisher == "" || publisher == null) continue;
                    table.Publishers.Add(new SteamSchema.AppPublishersTable(table.appID, publisher) { AppDetails = table });
                }
            }
            //add the platforms
            if (appData.platforms.windows)
            {
                table.Platforms.Add(new SteamSchema.AppPlatformsTable(table.appID, "Windows") { AppDetails = table });
            }
            if (appData.platforms.mac)
            {
                table.Platforms.Add(new SteamSchema.AppPlatformsTable(table.appID, "Mac") { AppDetails = table });
            }
            if (appData.platforms.linux)
            {
                table.Platforms.Add(new SteamSchema.AppPlatformsTable(table.appID, "Linux") { AppDetails = table });
            }


            return table;
        }


        public static GameMarketSchema.GameTitleTable MapTitleTalbe(GameMarketAPIServer.Services.GameMarketTitle marketTitle)
        {
            var table = new GameMarketSchema.GameTitleTable();

            table.gameID = marketTitle.gameID;
            table.gameTitle = marketTitle.titleName;
            table.Developers = marketTitle.developers
                .Select(g => new GameMarketSchema.DeveloperTable(table.gameID, g) { GameTitle = table })
                .ToList();
            table.Publishers = marketTitle.publishers
                .Select(g => new GameMarketSchema.PublisherTable(table.gameID, g) { GameTitle = table })
                .ToList();
            if (marketTitle.platformIds != null && marketTitle.platformIds.Any())
            {
                if (marketTitle.platformIds.ContainsKey(Database_structure.Xbox))
                {
                    table.XboxLinks = marketTitle.platformIds[Database_structure.Xbox]
                    .Select(g => new GameMarketSchema.XboxLinkTable(table.gameID, g) { GameTitle = table })
                    .ToList();
                }
                if (marketTitle.platformIds.ContainsKey(Database_structure.Steam))
                {
                    table.SteamLinks = marketTitle.platformIds[Database_structure.Steam]
                                .Select(g => new GameMarketSchema.SteamLinkTable(table.gameID, Convert.ToUInt32(g)) { GameTitle = table })
                                .ToList();
                }
            }
            return table;
        }
    }
}
